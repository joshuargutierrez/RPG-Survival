using UnityEngine;
using Photon.Pun;
using Invector.vItemManager;
using System.Collections;
using UnityEngine.Events;
using Invector.vCharacterController.vActions;
using CBGames.Core;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Objects/Sync Item Collection")]
    [RequireComponent(typeof(PhotonView))]
    [DisallowMultipleComponent]
    public class SyncItemCollection : MonoBehaviour
    {
        #region Parameters
        [Tooltip("Whether or not you want this to be a globally updated item. (EX: If a player " +
            "in scene 1 collects this then when other players enter this scene they will see " +
            "this as already collected.)")]
        [SerializeField] protected bool syncCrossScenes = true;
        [Tooltip("This is only important if \"syncCrossScenes\" is true. \n\nThe object you want to track " +
            "the position of. When trying to sync this item across the scene it uses it's name and this " +
            "holder position to try and figure out what object to update.")]
        [SerializeField] protected Transform holder = null;
        [Tooltip("Only matters if \"Sync Cross Scenes\" is true.\n\n" +
            "Enable if this object is dynamically added to the scene. Will instantiate/destroy this object across " +
            "Unity Scenes and for all networked players.")]
        [SerializeField] protected bool syncCreateDestroy = false;
        [Tooltip("Only matters if \"Sync Cross Scenes\" is true.\n\n" +
            "The prefab name in the resources folder that corresponds to this object. Must be exact in " +
            "order to spawn from the resources folder.")]
        [SerializeField] protected string resourcesPrefab = null;
        [Tooltip("If false, will check to make sure this has been instantiated with item data via the network call. " +
            "If not it will destroy itself in favor of another ItemCollection. Enable this ONLY if you DON'T dynamically " +
            "add items to the ItemCollection component.")]
        [SerializeField] protected bool skipStartCheck = false;
        [Tooltip("This should be copied exactly from vItemCollection.")]
        public float onPressActionDelay;

        [Tooltip("UnityEvent, called when you press the action button.")]
        public UnityEvent OnPressActionInput;
        [Tooltip("UnityEvent, called when you press the action button with a gameobject.")]
        public OnDoActionWithTarget onPressActionInputWithTarget;
        [Tooltip("UnityEvent, call when you first enter a room and the `DoScenesReplay` " +
            "from the NetworkManager indicates this object has had actions performed on it " +
            "by another player previously.")]
        public UnityEvent OnSceneEnterUpdate;

        protected vItemCollection ic = null;
        protected bool collected = false;
        [Tooltip("The list of items this object has.")]
        [SerializeField] List<ItemReference> references = new List<ItemReference>();
        #endregion

        #region Initializations
        protected virtual void Awake()
        {
            ic = (ic == null) ? GetComponent<vItemCollection>() : ic;
        }
        /// <summary>
        /// Wehn first started will check to see if this has been instantiated 
        /// from the owning player with item data. If not it will destroy this 
        /// object because it was instantiated from Invector and not photon. 
        /// This is to prevent duplicates appearing for the player that 
        /// originally dropped this.
        /// </summary>
        protected virtual void Start()
        {
            if (skipStartCheck == false)
            {
                object[] data = GetComponent<PhotonView>().InstantiationData;
                if (PhotonNetwork.InRoom == true && data == null && syncCreateDestroy == true)
                {
                    Destroy(gameObject);
                }
                else if (PhotonNetwork.InRoom == true && syncCreateDestroy == true)
                {
                    ItemWrapper wrapper = JsonUtility.FromJson<ItemWrapper>(data[0] as string);
                    GetComponent<vItemCollection>().items = wrapper.items;
                }
            }
        }
        #endregion

        #region SYNC Create/Destroys
        /// <summary>
        /// Takes a list of `ItemReference`'s and serializes that list and returns it.
        /// </summary>
        /// <param name="items">List<ItemReference> type, the list of items you want to serialize.</param>
        /// <returns>A serialized version of the input list</returns>
        protected virtual string[] SerializeItems(List<ItemReference> items)
        {
            string[] data = new string[1];
            ItemWrapper wrapper = new ItemWrapper(items);
            data[0] = JsonUtility.ToJson(wrapper);
            return data;
        }

        /// <summary>
        /// Takes a serialized List<ItemReference> and converts it back to a list
        /// from a string.
        /// </summary>
        /// <param name="serializedItems"></param>
        /// <returns>returns the original List<ItemReference></returns>
        public virtual List<ItemReference> ConvertBackToItemRefs(string[] serializedItems)
        {
            ItemWrapper wrapper = JsonUtility.FromJson<ItemWrapper>(serializedItems[0]);
            return wrapper.items;
        }

        /// <summary>
        /// Calls the `UpdateScenesDatabase` IEnumerator function. This will send the item
        /// update information over the ChatBox data channel so others entering this 
        /// scene will see the updates to this list of items.
        /// </summary>
        /// <param name="items">List<ItemReference> type, The list of items to set on this itemCollection</param>
        public virtual void UpdateDatabase(List<ItemReference> items)
        {
            StartCoroutine(UpdateScenesDatabase(items));
        }

        /// <summary>
        /// Send this item and its item list to all other players in the session so when
        /// they join this scene they will have an up to date accurate item in the
        /// scene. Done via sending this information over the data channel on the chatbox.
        /// </summary>
        /// <param name="items">List<ItemReference> type, the list of items to add to this object</param>
        protected virtual IEnumerator UpdateScenesDatabase(List<ItemReference> items)
        {
            yield return new WaitUntil(() => NetworkManager.networkManager.GetChabox() && NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel() == true);
            ObjectAction objectInfo = new ObjectAction(
                holder.name.Replace("(Clone)", ""),
                SceneManager.GetActiveScene().name,
                resourcesPrefab,
                holder.position.Round(),
                ObjectActionEnum.Create,
                null,
                SerializeItems(items)
            );
            NetworkManager.networkManager.GetChabox().BroadcastData(
                NetworkManager.networkManager.GetChatDataChannel(),
                objectInfo
            );
        }
        #endregion

        #region SYNC Updates
        /// <summary>
        /// Calls the `NetworkSceneCollected` RPC for everyone. This calls the `E_OnPressAction` 
        /// function for everyone.
        /// </summary>
        public virtual void SceneCollected()
        {
            GetComponent<PhotonView>().RPC("NetworkSceneCollected", RpcTarget.AllBuffered);
        }

        /// <summary>
        /// Enables/Disables the `vItemCollection` component on this object.
        /// </summary>
        /// <param name="isEnabled"></param>
        public virtual void EnableVItemCollection(bool isEnabled)
        {
            GetComponent<vItemCollection>().enabled = isEnabled;
        }
        #endregion

        #region SYNC Update/Create/Delete
        /// <summary>
        /// This is designed to be called from the vItemCollection OnPress* UnityEvent.
        /// This calls `NetworkCollect` RPC and broadcasts the collect command via the
        /// Chatbox to all those in the session (via `ChatBoxBroadCastCollect` function).
        /// </summary>
        public virtual void Collect()
        {
            if (collected == false)
            {
                collected = true;
                GetComponent<PhotonView>().RPC("NetworkCollect", RpcTarget.OthersBuffered);
                ChatBoxBroadCastCollect();
                StartCoroutine(E_OnPressAction(null, true));
            }
        }

        /// <summary>
        /// This is designed to be called from the vItemCollection OnPress* UnityEvent.
        /// This calls `NetworkCollect` RPC and broadcasts the collect command via the
        /// Chatbox to all those in the session (via `ChatBoxBroadCastCollect` function).
        /// </summary>
        public virtual void Collect(GameObject target = null)
        {
            if (collected == false)
            {
                collected = true;
                if (string.IsNullOrEmpty(ic.playAnimation) == false)
                {
                    GetComponent<PhotonView>().RPC("NetworkPlayerPlayAnimation", RpcTarget.Others, target.GetComponent<PhotonView>().ViewID);
                }
                if (GetComponent<PhotonView>().ViewID != 0)
                {
                    GetComponent<PhotonView>().RPC("NetworkCollect", RpcTarget.OthersBuffered, target.GetComponent<PhotonView>().ViewID);
                }
                ChatBoxBroadCastCollect();
                StartCoroutine(E_OnPressAction(target, true));
            }
        }    
        
        /// <summary>
        /// Sends data via the data channel in the chatbox to everyone in the session to
        /// tell them that this item has been collected when they entire this unity scene.
        /// </summary>
        public virtual void ChatBoxBroadCastCollect()
        {
            if (syncCrossScenes == true && NetworkManager.networkManager.GetChabox() && 
                NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel())
            {
                if (syncCreateDestroy == true)
                {
                    ObjectAction objectInfo = new ObjectAction(
                        holder.name.Replace("(Clone)", ""),
                        SceneManager.GetActiveScene().name,
                        resourcesPrefab,
                        holder.position.Round(),
                        ObjectActionEnum.Delete,
                        null, null
                    );
                    NetworkManager.networkManager.GetChabox().BroadcastData(
                        NetworkManager.networkManager.GetChatDataChannel(),
                        objectInfo
                    );
                }
                else
                {
                    ObjectAction objectInfo = new ObjectAction(
                        holder.name,
                        SceneManager.GetActiveScene().name,
                        resourcesPrefab,
                        holder.position,
                        ObjectActionEnum.Update,
                        "SceneCollected",
                        null
                    );
                    NetworkManager.networkManager.GetChabox().BroadcastData(
                        NetworkManager.networkManager.GetChatDataChannel(),
                        objectInfo
                    );
                }
            }
        }

        /// <summary>
        /// Used to call the `NetworkDestroySelf` RPC, which destroys this object for 
        /// everyone currently in the scene and everyone going to enter this scene.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sendNetwork"></param>
        /// <returns></returns>
        protected virtual IEnumerator E_OnPressAction(GameObject target = null, bool sendNetwork = true)
        {
            if (onPressActionDelay > 0) yield return new WaitForSeconds(onPressActionDelay);
            OnPressActionInput.Invoke();
            if (target != null)
            {
                onPressActionInputWithTarget.Invoke(target);
            }
            if (sendNetwork == true)
            {
                if (syncCreateDestroy == true)
                {
                    if (PhotonNetwork.IsMasterClient == true)
                    {
                        PhotonNetwork.Destroy(this.gameObject);
                    }
                    else
                    {
                        GetComponent<PhotonView>().RPC("NetworkDestroySelf", RpcTarget.MasterClient);
                    }
                }
            }
            else if (GetComponent<vItemCollection>().destroyAfter == true)
            {
                yield return new WaitForSeconds(GetComponent<vItemCollection>().destroyDelay);
                if (gameObject)
                {
                    Destroy(gameObject);
                }
            }
        }
        #endregion

/*        #region Shooter Template
        public virtual void NetworkArrowViewDestroy()
        {
            Transform you = NetworkManager.networkManager.GetYourPlayer().transform;
            you.GetComponent<PhotonView>().RPC(
                "DestroyArrow",
                RpcTarget.Others,
                GetComponent<ArrowView>().viewId
            );
        }
        #endregion*/

        #region NetworkEvents
        public virtual Transform GetHolder()
        {
            return holder;
        }

        [PunRPC]
        protected virtual void NetworkPlayerPlayAnimation(int viewId)
        {
            PhotonView pv = PhotonView.Find(viewId);
            if (pv && pv.gameObject)
            {
                pv.gameObject.GetComponent<Animator>().Play(ic.playAnimation, ic.animatorActionState);
            }
        }
        [PunRPC]
        protected virtual void NetworkCollect(int viewId)
        {
            if (collected == true) return;
            collected = true;
            PhotonView pv = PhotonView.Find(viewId);
            if (pv && pv.gameObject)
            {
                StartCoroutine(E_OnPressAction(pv.gameObject, false));
            }
            else
            {
                StartCoroutine(E_OnPressAction(null, false));
            }
        }
        [PunRPC]
        protected virtual void NetworkCollect()
        {
            if (collected == true) return;
            collected = true;
            StartCoroutine(E_OnPressAction(null, false));
        }
        [PunRPC]
        protected virtual void NetworkDestroySelf()
        {
            if (PhotonNetwork.IsMasterClient == true)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
        [PunRPC]
        protected virtual void NetworkSceneCollected()
        {
            if (collected == false)
            {
                collected = true;
                OnSceneEnterUpdate.Invoke();
            }
        }
        #endregion
    }
}
