using CBGames.Core;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Menus/Player List/Player List Object")]
    public class PlayerListObject : MonoBehaviour
    {
        [Tooltip("Hidden variable. The user id of this player.")]
        [HideInInspector] public string userId = null;
        [Tooltip("Hidden variable. The scene name this player is currently in.")]
        [HideInInspector] public string sceneName = "Unknown";
        [Tooltip("The Text component that is responsible for displaying the player's name.")]
        [SerializeField] protected Text playerNameText = null;
        [Tooltip("The Text component that is responsible for displaying the Unity scene the player is currently in.")]
        [SerializeField] protected Text location = null;
        [Tooltip("The Text component that is responsible for displaying if this player is the MasterClient or not.")]
        [SerializeField] protected Text ownerText = null;
        [Tooltip("The Text component that is responsible for displaying if the player is marked as ready or not.")]
        [SerializeField] protected Text readyText = null;
        [Tooltip("The Image component that is specific to this player.")]
        [SerializeField] protected Image playerImage = null;
        [Tooltip("The Image component to display if this player is the MasterClient.")]
        [SerializeField] protected Image showIfOwner = null;
        [Tooltip("The GameObject to SetActive if the player is marked as ready.")]
        [SerializeField] protected GameObject readyImage = null;
        [Tooltip("The GameObject to SetActive if the player is marked as not ready.")]
        [SerializeField] protected GameObject notReadyImage = null;
        [Tooltip("Will hide the Text component that is responsible for displaying " +
            "the player's location if that location currently is not set.")]
        [SerializeField] protected bool hideLocationIfNotSet = false;

        protected bool isReady = false;
        protected UICoreLogic logic = null;

        /// <summary>
        /// Sets this component values with the `InstantiationData` for this component. Also
        /// adds the `RecievedPhotonEvent` function to the Photon's `EventReceived` delegate
        /// so it will be called anytime a PhotonEvent is received.
        /// </summary>
        protected virtual void Start()
        {
            logic = NetworkManager.networkManager.GetComponentInChildren<UICoreLogic>();
            if (GetComponent<PhotonView>())
            {
                object[] data = GetComponent<PhotonView>().InstantiationData;
                if (data != null)
                {
                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                    {
                        if (player.UserId == (string)data[0])
                        {
                            SetPlayerContents(player);
                            SetReadyState(logic.PlayerIsReady(player.UserId));
                        }
                    }
                    Transform parentToSet = StaticMethods.FindTargetChild((int[])data[1], logic.transform);
                    transform.SetParent(parentToSet);
                    transform.localScale = new Vector3(1, 1, 1);
                    transform.position = Vector3.zero;
                }
            }
            PhotonNetwork.NetworkingClient.EventReceived += RecievedPhotonEvent;
        }

        /// <summary>
        /// Removes the `RecievedPhotonEvent` function from Photon's `EventReceived` delegate.
        /// </summary>
        protected virtual void OnDestroy()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= RecievedPhotonEvent;
        }

        /// <summary>
        /// Sets all the Text and Image values based on the input `info`
        /// </summary>
        /// <param name="info">PlayerListInfo type, all the data about this player</param>
        public virtual void SetContents(PlayerListInfo info)
        {
            userId = info.userId;
            playerNameText.text = userId.Split(':')[0];
            if (location != null)
            {
                sceneName = "Unknown Location";
                if (info.sceneIndex > 9999 && hideLocationIfNotSet == true)
                {
                    location.gameObject.SetActive(false);
                }
                else
                {
                    location.gameObject.SetActive(true);
                    if (info.sceneIndex == -1)
                    {
                        sceneName = "Lobby";
                    }
                    else if (info.sceneIndex < NetworkManager.networkManager.database.storedScenesData.Count)
                    {
                        sceneName = NetworkManager.networkManager.database.storedScenesData.Find(x => x.index == info.sceneIndex).sceneName;
                    }
                }
                location.text = sceneName;
            }
        }

        /// <summary>
        /// Sets the Text and Images based on the input values.
        /// </summary>
        /// <param name="player">Photon.Realtime.Player type, the </param>
        /// <param name="isOwnerText">string type, the string value to display if this is a MasterClient</param>
        /// <param name="nonOwnerText">string type, the string value to display if this is NOT a MasterClient</param>
        public virtual void SetPlayerContents(Photon.Realtime.Player player, string isOwnerText = "ROOM OWNER", string nonOwnerText = "")
        {
            userId = player.UserId;
            if (playerNameText != null)
            {
                playerNameText.text = player.NickName;
            }
            if (ownerText != null)
            {
                ownerText.text = (player.IsMasterClient == true) ? isOwnerText : nonOwnerText;
            }
            if (showIfOwner != null)
            {
                showIfOwner.gameObject.SetActive(player.IsMasterClient);
            }
        }

        /// <summary>
        /// The Image to set as the player image based on the input `image`.
        /// </summary>
        /// <param name="image">Sprite type, the image to set for the player.</param>
        public virtual void SetPlayerImage(Sprite image)
        {
            if (playerImage != null)
            {
                playerImage.sprite = image;
            }
        }

        /// <summary>
        /// Make this player be marked as ready.
        /// </summary>
        /// <param name="inputIsReady">bool type, this player is ready?</param>
        public virtual void SetReadyState(bool inputIsReady)
        {
            isReady = inputIsReady;
            if (readyImage != null)
            {
                readyImage.SetActive(isReady);
            }
            if (notReadyImage != null)
            {
                notReadyImage.SetActive(!isReady);
            }
            if (readyText != null)
            {
                readyText.text = (isReady == true) ? "READY" : "NOT READY";
            }
        }

        /// <summary>
        /// Returns if this player is ready or not.
        /// </summary>
        /// <returns>True or False, player is ready</returns>
        public virtual bool GetReadyState()
        {
            return isReady;
        }

        /// <summary>
        /// Callback method. Called whenever a PhotonEvent is Received. If the event code matches
        /// `CB_EVENT_READYUP` it sets the ready stat of this player if the userId that is received
        /// matches this one.
        /// </summary>
        /// <param name="obj">EventData type, the object holding the received event data</param>
        protected virtual void RecievedPhotonEvent(EventData obj)
        {
            if (obj.Code == PhotonEventCodes.CB_EVENT_READYUP)
            {
                object[] data = (object[])obj.CustomData;
                bool isReady = (bool)data[0];
                string receivedUserId = (string)data[1];
                if (userId == receivedUserId)
                {
                    SetReadyState(isReady);
                }
            }
        }
    }
}