using CBGames.Core;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Objects/Call Network Events")]
    [RequireComponent(typeof(PhotonView))]
    public class CallNetworkEvents : MonoBehaviour
    {
        #region Parameters
        [System.Serializable]
        public class NetworkGameObjectInvoke : UnityEvent<GameObject> { }
        [System.Serializable]
        public class NetworkFloatInvoke : UnityEvent<float> { }

        [Tooltip("Do you want these actions to persist between scenes for all players or have it " +
            "be a call that will only execute for players currently in this scene?")]
        [SerializeField] protected bool syncCrossScenes = true;
        [Tooltip("This is only important if \"syncCrossScenes\" is true. \n\nThe object you want to track " +
            "the position of. When trying to sync this item across the scene it uses it's name and this " +
            "holder position to try and figure out what object to update.")]
        [SerializeField] protected Transform holder = null;

        #region No Input Invokes
        [SerializeField] private UnityEvent NetworkInvoke1 = new UnityEvent();
        [SerializeField] private UnityEvent NetworkInvoke2 = new UnityEvent();
        [SerializeField] private UnityEvent NetworkInvoke3 = new UnityEvent();
        [SerializeField] private UnityEvent NetworkInvoke4 = new UnityEvent();
        [SerializeField] private UnityEvent NetworkInvoke5 = new UnityEvent();
        #pragma warning disable 0414
        [HideInInspector] [SerializeField] private bool showNetworkInvoke1 = false;
        [HideInInspector] [SerializeField] private bool showNetworkInvoke2 = false;
        [HideInInspector] [SerializeField] private bool showNetworkInvoke3 = false;
        [HideInInspector] [SerializeField] private bool showNetworkInvoke4 = false;
        [HideInInspector] [SerializeField] private bool showNetworkInvoke5 = false;
        #pragma warning restore 0414
        #endregion

        #region GameObjectInvokes
        [SerializeField] private NetworkGameObjectInvoke NetworkGameObjectInvoke1 = new NetworkGameObjectInvoke();
        [SerializeField] private NetworkGameObjectInvoke NetworkGameObjectInvoke2 = new NetworkGameObjectInvoke();
        [SerializeField] private NetworkGameObjectInvoke NetworkGameObjectInvoke3 = new NetworkGameObjectInvoke();
        [SerializeField] private NetworkGameObjectInvoke NetworkGameObjectInvoke4 = new NetworkGameObjectInvoke();
        #pragma warning disable 0414
        [HideInInspector] [SerializeField] private bool showGameObjectInvoke1 = false;
        [HideInInspector] [SerializeField] private bool showGameObjectInvoke2 = false;
        [HideInInspector] [SerializeField] private bool showGameObjectInvoke3 = false;
        [HideInInspector] [SerializeField] private bool showGameObjectInvoke4 = false;
        #pragma warning restore 0414
        #endregion

        #region Single Invokes
        [SerializeField] private NetworkFloatInvoke NetworkSingleInvoke1 = new NetworkFloatInvoke();
        #pragma warning disable 0414
        [HideInInspector] [SerializeField] private bool showSingleInvoke1 = false;
        #pragma warning restore 0414
        #endregion

        #endregion

        #region No Input UnityEvents
        /// <summary>
        /// This will call the `InvokeEvent` function with 1
        /// </summary>
        public virtual void CallNetworkInvoke1()
        {
            InvokeEvent(1);
        }

        /// <summary>
        /// This will call the `InvokeEvent` function with 2
        /// </summary>
        public virtual void CallNetworkInvoke2()
        {
            InvokeEvent(2);
        }

        /// <summary>
        /// This will call the `InvokeEvent` function with 3
        /// </summary>
        public virtual void CallNetworkInvoke3()
        {
            InvokeEvent(3);
        }

        /// <summary>
        /// This will call the `InvokeEvent` function with 4
        /// </summary>
        public virtual void CallNetworkInvoke4()
        {
            InvokeEvent(4);
        }

        /// <summary>
        /// This will call the `InvokeEvent` function with 5
        /// </summary>
        public virtual void CallNetworkInvoke5()
        {
            InvokeEvent(5);
        }

        /// <summary>
        /// This will invoke one of the no input UnityEvents on every connected
        /// players copy.
        /// </summary>
        /// <param name="number">int type, What no input UnityEvent number to invoke</param>
        protected virtual void InvokeEvent(int number = 0)
        {
            if (number == 0) return;
            if (syncCrossScenes == true)
            {
                GetComponent<PhotonView>().RPC("InvokeNoInputEvent", RpcTarget.AllBuffered, number);
            }
            else
            {
                GetComponent<PhotonView>().RPC("InvokeNoInputEvent", RpcTarget.All, number);
            }
        }
        #endregion

        #region GameObject UnityEvents
        /// <summary>
        /// Calls `InvokeGameObjectEvent` with the input gameobject and 1
        /// </summary>
        /// <param name="target">GameObject type, What GameObject to invoke these unity events with</param>
        public virtual void CallGameObjectInvoke1(GameObject target)
        {
            InvokeGameObjectEvent(target, 1);
        }

        /// <summary>
        /// Calls `InvokeGameObjectEvent` with the input gameobject and 2
        /// </summary>
        /// <param name="target">GameObject type, What GameObject to invoke these unity events with</param>
        public virtual void CallGameObjectInvoke2(GameObject target)
        {
            InvokeGameObjectEvent(target, 2);
        }

        /// <summary>
        /// Calls `InvokeGameObjectEvent` with the input gameobject and 3
        /// </summary>
        /// <param name="target">GameObject type, What GameObject to invoke these unity events with</param>
        public virtual void CallGameObjectInvoke3(GameObject target)
        {
            InvokeGameObjectEvent(target, 3);
        }

        /// <summary>
        /// Calls `InvokeGameObjectEvent` with the input gameobject and 4
        /// </summary>
        /// <param name="target">GameObject type, What GameObject to invoke these unity events with</param>
        public virtual void CallGameObjectInvoke4(GameObject target)
        {
            InvokeGameObjectEvent(target, 4);
        }
        protected virtual void InvokeGameObjectEvent(GameObject target = null, int number = 0)
        {
            if (!target.GetComponent<PhotonView>() || number == 0) return;
            if (syncCrossScenes == true)
            {
                GetComponent<PhotonView>().RPC("GameObjectInvoke", RpcTarget.AllBuffered, target.GetComponent<PhotonView>().ViewID, number);
            }
            else
            {
                GetComponent<PhotonView>().RPC("GameObjectInvoke", RpcTarget.All, target.GetComponent<PhotonView>().ViewID, number);
            }
        }
        #endregion

        #region Float UnityEvents
        /// <summary>
        /// Calls `InvokeSingleEvent` function with a value value and number 1
        /// </summary>
        /// <param name="value">float type, the value to pass to the UnityEvents</param>
        public virtual void CallFloatInvoke1(float value)
        {
            InvokeSingleEvent(value, 1);
        }

        /// <summary>
        /// Invokes The float unity events across the network with the specified number and value.
        /// </summary>
        /// <param name="value">float type, the float value to pass to these UnityEvents</param>
        /// <param name="number">The unityevent number to invoke</param>
        protected virtual void InvokeSingleEvent(float value, int number = 0)
        {
            if (number == 0) return;
            if (syncCrossScenes == true)
            {
                GetComponent<PhotonView>().RPC("FloatInvoke", RpcTarget.AllBuffered, value, number);
            }
            else
            {
                GetComponent<PhotonView>().RPC("FloatInvoke", RpcTarget.All, value, number);
            }
        }
        #endregion

        #region Scene Update Requests
        /// <summary>
        /// Designed to invoke the RPC call based on the input number
        /// </summary>
        /// <param name="input"></param>
        public virtual void SceneUpdateNoInputInvokeEvent(string[] input)
        {
            string number = input[0];
            switch(number)
            {
                case "1":
                    NetworkInvoke1.Invoke();
                    break;
                case "2":
                    NetworkInvoke2.Invoke();
                    break;
                case "3":
                    NetworkInvoke3.Invoke();
                    break;
                case "4":
                    NetworkInvoke4.Invoke();
                    break;
                case "5":
                    NetworkInvoke5.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Designed to invoke the RPC call for the GameObject input type 
        /// based on your input number
        /// </summary>
        /// <param name="input"></param>
        public virtual void SceneUpdateGameObjectInvokeEvent(string[] input)
        {
            string viewId = input[0];
            string number = input[1];
            PhotonView view = PhotonView.Find(int.Parse(viewId));
            if (view)
            {
                switch(number)
                {
                    case "1":
                        NetworkGameObjectInvoke1.Invoke(view.gameObject);
                        break;
                    case "2":
                        NetworkGameObjectInvoke2.Invoke(view.gameObject);
                        break;
                    case "3":
                        NetworkGameObjectInvoke3.Invoke(view.gameObject);
                        break;
                    case "4":
                        NetworkGameObjectInvoke4.Invoke(view.gameObject);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Designed to invoke the RPC call for the float input type
        /// based on your input number
        /// </summary>
        /// <param name="input"></param>
        public virtual void SceneUpdateFloatInvokeEvent(string[] input)
        {
            string floatValue = input[0];
            string number = input[1];
            switch (number)
            {
                case "1":
                    NetworkSingleInvoke1.Invoke(float.Parse(floatValue));
                    break;
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        protected virtual void InvokeNoInputEvent(int number)
        {
            if (syncCrossScenes == true && PhotonNetwork.IsMasterClient == true &&
                NetworkManager.networkManager.GetChabox() &&
                NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel())
            {
                ObjectAction objectInfo = new ObjectAction(
                    holder.name.Replace("(Clone)", ""),
                    SceneManager.GetActiveScene().name,
                    null,
                    holder.position.Round(),
                    ObjectActionEnum.Update,
                    "SceneUpdateNoInputInvokeEvent",
                    new string[1] { number.ToString() }
                );
                NetworkManager.networkManager.GetChabox().BroadcastData(
                    NetworkManager.networkManager.GetChatDataChannel(),
                    objectInfo
                );
            }
            if (number == 1)
            {
                NetworkInvoke1.Invoke();
            }
            else if (number == 2)
            {
                NetworkInvoke2.Invoke();
            }
            else if (number == 3)
            {
                NetworkInvoke3.Invoke();
            }
            else if (number == 4)
            {
                NetworkInvoke4.Invoke();
            }
            else if (number == 5)
            {
                NetworkInvoke5.Invoke();
            }
        }
        [PunRPC]
        protected virtual void GameObjectInvoke(int viewId, int number)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (syncCrossScenes == true && PhotonNetwork.IsMasterClient == true &&
                NetworkManager.networkManager.GetChabox() &&
                NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel())
            {
                ObjectAction objectInfo = new ObjectAction(
                    holder.name.Replace("(Clone)", ""),
                    SceneManager.GetActiveScene().name,
                    null,
                    holder.position.Round(),
                    ObjectActionEnum.Update,
                    "SceneUpdateGameObjectInvokeEvent",
                    new string[2] { viewId.ToString(), number.ToString() }
                );
                NetworkManager.networkManager.GetChabox().BroadcastData(
                    NetworkManager.networkManager.GetChatDataChannel(),
                    objectInfo
                );
            }
            if (view)
            {
                if (number == 1)
                {
                    NetworkGameObjectInvoke1.Invoke(view.gameObject);
                }
                else if(number == 2)
                {
                    NetworkGameObjectInvoke2.Invoke(view.gameObject);
                }
                else if (number == 3)
                {
                    NetworkGameObjectInvoke3.Invoke(view.gameObject);
                }
                else if (number == 4)
                {
                    NetworkGameObjectInvoke4.Invoke(view.gameObject);
                }
            }
        }
        [PunRPC]
        protected virtual void FloatInvoke(float value, int number)
        {
            if (syncCrossScenes == true && PhotonNetwork.IsMasterClient == true &&
                NetworkManager.networkManager.GetChabox() &&
                NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel())
            {
                ObjectAction objectInfo = new ObjectAction(
                    holder.name.Replace("(Clone)", ""),
                    SceneManager.GetActiveScene().name,
                    null,
                    holder.position.Round(),
                    ObjectActionEnum.Update,
                    "SceneUpdateFloatInvokeEvent",
                    new string[2] { value.ToString(), number.ToString() }
                );
                NetworkManager.networkManager.GetChabox().BroadcastData(
                    NetworkManager.networkManager.GetChatDataChannel(),
                    objectInfo
                );
            }
            if (number == 1)
            {
                NetworkSingleInvoke1.Invoke(value);
            }
        }
        #endregion
    }
}