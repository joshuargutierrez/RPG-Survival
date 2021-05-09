using CBGames.Core;
using Invector.vCharacterController;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Objects/Scene Transition")]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(PhotonView))]
    public class SceneTransition : MonoBehaviour
    {
        #region Modifiables 
        [Tooltip("When entring this trigger whether or not to automatically move to the new scene or not.")]
        [SerializeField] protected bool autoTravel = false;
        [Tooltip("The button that must be pressed when inside this trigger to travel the new scene.")]
        [SerializeField] protected string buttonToTravel = "E";
        [Tooltip("The gameobjects to active/deactive when entering/exiting this trigger.")]
        [SerializeField] protected GameObject[] activeOnEnter = null;
        [Tooltip("The name of the scene to load. This must be an exact spelling according to what is in the build settings.")]
        [SerializeField] protected string LoadSceneName = "";
        [Tooltip("The name of the LoadPoint object to spawn at in the desired scene. This naming must be exact.")]
        [SerializeField] protected string SpawnAtPoint = "";
        [Tooltip("Whether to send everyone when traveling or to just send the person entering.")]
        [SerializeField] public bool sendAllTogether = true;
        [Tooltip("The scene database that holds a list of all scenes and LoadPoint objects in those scenes.")]
        [SerializeField] private SceneDatabase database;
        [Tooltip("UnityEvent that is called when the owner enters this trigger, not a networked player.")]
        [SerializeField] private UnityEvent OnOwnerEnterTrigger = null;
        [Tooltip("UnityEvent that is called when the owner exits this trigger, not the networked player.")]
        [SerializeField] private UnityEvent OnOwnerExitTrigger = null;
        [Tooltip("UnityEvent, travel has been called, but this event fires right before.")]
        [SerializeField] private UnityEvent BeforeTravel = null;
        [Tooltip("UnityEvent, called when ANY player enter the trigger, owner or networked player")]
        [SerializeField] private UnityEvent OnAnyPlayerEnterTrigger = null;
        [Tooltip("UnityEvent, called when ANY player exits the trigger, owner or networked player")]
        [SerializeField] private UnityEvent OnAnyPlayerExitTrigger = null;
        #endregion

        #region Internal Variables
        protected bool acceptingInput = false;
        protected Component comp;
        protected Color gizmoColor = new Color(1f, 0.92f, 0.016f, 0.3f);
        protected Color gizmoError = Color.red;
        protected GameObject targetPlayer;
        #endregion
        
        /// <summary>
        /// Used for automation. Sets the `database` variable with the input sceneDatabase.
        /// </summary>
        /// <param name="input">SceneDatabase type, the sceneDatabase to have this component reference</param>
        public virtual void SetDatabase(SceneDatabase input)
        {
            database = input;
        }

        /// <summary>
        /// Will do actions only when a vThirdPersonController enters the trigger and nothing else.
        /// </summary>
        /// <param name="other">Collider type, only vThirdPersonController players fire this</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<vThirdPersonController>())
            {
                OnAnyPlayerEnterTrigger.Invoke();
                if (other.GetComponent<PhotonView>() && other.GetComponent<PhotonView>().IsMine)
                {
                    OnOwnerEnterTrigger.Invoke();
                    if (autoTravel && other.GetComponent<PhotonView>())
                    {
                        targetPlayer = other.gameObject;
                        Travel();
                        return;
                    }
                    else
                    {
                        acceptingInput = true;
                        foreach (GameObject go in activeOnEnter)
                        {
                            go.SetActive(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Will do actions only when a vThirdPersonController leaves the trigger and nothing else.
        /// </summary>
        /// <param name="other">Collider type, only vThirdPersonController players fire this</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<vThirdPersonController>())
            {
                OnAnyPlayerExitTrigger.Invoke();
                if (other.GetComponent<PhotonView>() && other.GetComponent<PhotonView>().IsMine)
                {
                    OnOwnerExitTrigger.Invoke();
                    if (autoTravel) return;
                    targetPlayer = other.gameObject;
                    acceptingInput = false;
                    foreach (GameObject go in activeOnEnter)
                    {
                        go.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// If a owning vThirdPersonController is in the trigger and presses the `buttonToTravel`
        /// it will call the `Travel` function.
        /// </summary>
        protected virtual void Update()
        {
            if (acceptingInput && Input.GetButtonDown(buttonToTravel.ToString()))
            {
                Travel();
            }
        }

        /// <summary>
        /// Calls `SendToScene` RPC for everyone or yourself based on the `sendAllTogether` 
        /// variable. Finds the scene to load from the `database` and calls `NetworkLoadLevel`
        /// from the NetworkManager component.
        /// </summary>
        public virtual void Travel()
        {
            acceptingInput = false;
            if (sendAllTogether)
            {
                GetComponent<PhotonView>().RPC("SendToScene", RpcTarget.All, SpawnAtPoint);
            }
            else
            {
                BeforeTravel.Invoke();
                NetworkManager.networkManager.NetworkLoadLevel(database.storedScenesData.Find(x => x.sceneName == LoadSceneName).index, SpawnAtPoint, sendAllTogether);
            }
        }

        [PunRPC]
        protected virtual void SendToScene(string entrancePoint)
        {
            sendAllTogether = false;
            Travel();
        }

        //Draw trigger box
        protected virtual void OnDrawGizmos()
        {
            if (GetComponent<BoxCollider>())
            {
                if (gameObject.GetComponent<BoxCollider>().isTrigger && gameObject.layer == 2 && database)
                {
                    Gizmos.color = gizmoColor;
                }
                else
                {
                    Gizmos.color = gizmoError;
                }
                gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
                gameObject.GetComponent<BoxCollider>().size = Vector3.one;

                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }
    }
}