using CBGames.Core;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Testing/Auto Connect To Room")]
    public class AutoConnect : MonoBehaviour
    {
        [Tooltip("The photon room to auto connect to")]
        public string roomToAutoConnectTo = "MyTestingRoom";

        /// <summary>
        /// Calls the `AutoStart` IEnumerator
        /// </summary>
        void Start()
        {
            StartCoroutine(AutoStart());
        }

        /// <summary>
        /// Will automatically join the photon server, lobby, and finally the photon
        /// room name that you specify in the `roomToAutoConnectTo` parameter.
        /// 
        /// This is used exclusively for testing builds quickly without having to 
        /// go through a UI interface.
        /// </summary>
        IEnumerator AutoStart()
        {
            NetworkManager.networkManager.JoinLobby();
            yield return new WaitUntil(() => PhotonNetwork.InLobby);
            RoomOptions options = new RoomOptions()
            {
                MaxPlayers = 10,
                PublishUserId = true,
                IsVisible = true,
                IsOpen = true,
                CleanupCacheOnLeave = true
            };
            NetworkManager.networkManager.JoinOrCreateRoom(roomToAutoConnectTo, options);
        }
    }
}