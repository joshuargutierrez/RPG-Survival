using CBGames.Core;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Connection Status")]
    public class TrackConnectionStatus : MonoBehaviour
    {
        [Tooltip("The Text objects to set their string values to be whatever " +
            "the current connection status is of the NetworkManager.")]
        [SerializeField] protected Text[] texts = new Text[] { };
        [Tooltip("UnityEvent. Called when you successfully connect to the Photon Lobby.")]
        [SerializeField] protected UnityEvent OnConnectedToLobby = new UnityEvent();
        [Tooltip("UnityEvent. Called when you successfully connect to a photon room.")]
        [SerializeField] protected UnityEvent OnConnectedToRoom = new UnityEvent();

        protected bool _firedLobbyEvents = false;
        protected bool _firedRoomEvents = false;

        /// <summary>
        /// Resets the lobby/room unity event fired status'
        /// </summary>
        protected virtual void OnEnable()
        {
            _firedLobbyEvents = false;
            _firedRoomEvents = false;
        }

        /// <summary>
        /// Will set the `texts` values to be whatever the NetworkManager connection
        /// status is dynamically. Will also only fire the `OnConnectedToLobby`
        /// and `OnConnectedToRoom` once based on their fired status'.
        /// </summary>
        protected virtual void Update()
        {
            if (NetworkManager.networkManager)
            {
                SetText(NetworkManager.networkManager._connectStatus);
            }
            if (PhotonNetwork.IsConnected)
            {
                if (_firedLobbyEvents == false && PhotonNetwork.InLobby)
                {
                    OnConnectedToLobby.Invoke();
                }
                if (_firedRoomEvents == false && PhotonNetwork.InRoom)
                {
                    OnConnectedToRoom.Invoke();
                }
            }
        }

        /// <summary>
        /// Set the string value of all the `texts` according to whatever
        /// the input value is.
        /// </summary>
        /// <param name="inputText">string type, the connection status to set.</param>
        protected virtual void SetText(string inputText)
        {
            foreach (Text text in texts)
            {
                text.text = inputText;
            }
        }
    }
}