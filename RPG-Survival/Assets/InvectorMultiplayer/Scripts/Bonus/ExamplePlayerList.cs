// Where Do I Put This?
// Add this to the same gameobject that holds the NetworkManager component
//
// What does this do?
// This is used to keep a running list of the players currently in the scene.
// As well as all those player vs player kill counts and death counts.
//

using CBGames.Core;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/Bonus/Player List")]
    public class ExamplePlayerList : MonoBehaviour
    {
        #region Variables
        public class PlayerListData
        {
            public string userId = "";
            public string name = "";
            public int kills = 0;
            public int deaths = 0;

            public PlayerListData(string input_id, string input_name)
            {
                this.userId = input_id;
                this.name = input_name;
                this.kills = 0;
                this.deaths = 0;
            }
        }
        protected List<PlayerListData> data = new List<PlayerListData>();
        #endregion

        #region Core Logic For List Manipulation
        public virtual void UpdateList()
        {
            AutoAddNewPlayers();
            AutoRemoveLeftPlayers();
        }
        public virtual void AutoAddNewPlayers()
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (data.Find(x => x.userId == player.UserId) == null)
                {
                    data.Add(new PlayerListData(
                        player.UserId,
                        player.NickName
                    ));
                }
            }
        }
        public virtual void AutoRemoveLeftPlayers()
        {
            List<PlayerListData> temp = new List<PlayerListData>();
            temp = data;
            foreach (PlayerListData item in data)
            {
                if (!PhotonNetwork.PlayerList.Any(x => x.UserId == item.userId))
                {
                    temp.Remove(item);
                }
            }
            data = temp;
        }
        public virtual void AddPlayer(Photon.Realtime.Player player)
        {
            if (data.Find(x => x.userId == player.UserId) == null)
            {
                data.Add(new PlayerListData(
                    player.UserId,
                    player.NickName
                ));
            }
        }
        public virtual void RemovePlayer(Photon.Realtime.Player player)
        {
            if (data.Find(x => x.userId == player.UserId) != null)
            {
                data.Remove(data.Find(x => x.userId == player.UserId));
            }
        }
        public virtual void AddKill(string userId)
        {
            // You can override the SyncPlayer.cs file to include a function like "Dead"
            // public void Dead()
            //{
            //  if (lastDamageSender != null && lastDamageSender.GetComponent<PhotonView>())
            //  {
            //      FindObjectOfType<ExamplePlayerList>().AddKill(lastDamageSender.GetComponent<PhotonView>().Owner.UserId);
            //      FindObjectOfType<ExamplePlayerList>().AddDeath(PhotonNetwork.LocalPlayer.UserId);
            //  }
            //}
            PlayerListData item = data.Find(x => x.userId == userId);
            if (item != null)
            {
                item.kills += 1;
            }
        }
        public virtual void AddDeath(string userId)
        {
            // You can override the SyncPlayer.cs file to include a function like "Dead"
            // public void Dead()
            //{
            //  if (lastDamageSender != null && lastDamageSender.GetComponent<PhotonView>())
            //  {
            //      FindObjectOfType<ExamplePlayerList>().AddKill(lastDamageSender.GetComponent<PhotonView>().Owner.UserId);
            //      FindObjectOfType<ExamplePlayerList>().AddDeath(PhotonNetwork.LocalPlayer.UserId);
            //  }
            //}
            PlayerListData item = data.Find(x => x.userId == userId);
            if (item != null)
            {
                item.deaths += 1;
            }
        }
        public virtual List<PlayerListData> GetList()
        {
            return data;
        }
        public virtual PlayerListData GetPlayerData(Photon.Realtime.Player player)
        {
            return data.Find(x => x.userId == player.UserId);
        }
        public virtual PlayerListData GetPlayerData(string userId)
        {
            return data.Find(x => x.userId == userId);
        }
        public virtual int GetPlayerDeaths(string userId)
        {
            return GetPlayerData(userId).deaths;
        }
        public virtual int GetPlayerKills(string userId)
        {
            return GetPlayerData(userId).kills;
        }
        public virtual int GetPlayerDeaths(Photon.Realtime.Player player)
        {
            return GetPlayerData(player).deaths;
        }
        public virtual int GetPlayerKills(Photon.Realtime.Player player)
        {
            return GetPlayerData(player).kills;
        }
        public virtual void ClearList()
        {
            data.Clear();
        }
        #endregion

        #region Auto Update Logic
        protected virtual void Start()
        {
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom += AddPlayer;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom += RemovePlayer;
            NetworkManager.networkManager.OnJoinedPhotonRoom += UpdateList;
            NetworkManager.networkManager.OnLeftPhotonRoom += ClearList;
        }
        protected virtual void OnDisable()
        {
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom -= AddPlayer;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom -= RemovePlayer;
            NetworkManager.networkManager.OnJoinedPhotonRoom -= UpdateList;
            NetworkManager.networkManager.OnLeftPhotonRoom -= ClearList;
        }
        protected virtual void OnDestroy()
        {
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom -= AddPlayer;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom -= RemovePlayer;
            NetworkManager.networkManager.OnJoinedPhotonRoom -= UpdateList;
            NetworkManager.networkManager.OnLeftPhotonRoom -= ClearList;
        }
        #endregion
    }
}