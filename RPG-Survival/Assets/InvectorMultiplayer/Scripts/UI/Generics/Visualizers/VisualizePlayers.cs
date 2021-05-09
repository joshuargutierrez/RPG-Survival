using CBGames.Core;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Visualizers/Visualize Players")]
    public class VisualizePlayers : MonoBehaviour
    {
        [Tooltip("The parent object of these newly spawned UI elements")]
        [SerializeField] protected Transform parentObj = null;
        [Tooltip("Spawn this object when the found player is the owner of the room.")]
        [SerializeField] protected GameObject ownerPlayer = null;
        [Tooltip("Spawn this object when the found player is NOT the owner of the room.")]
        [SerializeField] protected GameObject otherPlayer = null;
        [Tooltip("If you want to only see the players that are on a certain team. " +
            "If blank this will just display everyone.")]
        [SerializeField] protected string teamName = "";
        [Tooltip("If the child objects have a PhotonView component that you would like to allocate " +
            "a unique view id to that belongs to the owning player, only the master client does the " +
            "allocation. EX: Each child has a chat icon that only displays when the owner is speaking. " +
            "This would require a PhotonView id to be allocated.")]
        [SerializeField] protected bool allocateViewIds = false;
        [Tooltip("(Optional) Random sound to play when a player joins.")]
        [SerializeField] protected AudioClip[] joinedSound = new AudioClip[] { };
        [Tooltip("(Optional) Random sound to play when a player leaves.")]
        [SerializeField] protected AudioClip[] leftSound = new AudioClip[] { };
        [Tooltip("(Optional) What AudioSource to play the leave/join sounds.")]
        [SerializeField] protected AudioSource soundSource = null;
        [Tooltip("By default a player enters a game without having his team set. This will automatically " +
            "try to evenly choose a team for the entering player.\n\n" +
            "IMPORTANT NOTE: You should only ever have this boolean enabled on one of these components at a time. " +
            "Otherwise the enabled components will fight with each other.")]
        [SerializeField] protected bool autoSetTeamIfNotSet = false;
        [Tooltip("Enable this if you want to have verbose logging into the console to help you debug things.")]
        [SerializeField] protected bool debugging = false;

        protected int _prevPlayerCount = 0;
        protected UICoreLogic logic;
        protected bool _playJoinedSound = false;
        protected bool _playLeftSound = false;

        protected virtual void Start()
        {
            parentObj = (parentObj == null) ? transform : parentObj;
        }

        /// <summary>
        /// Will add the `WaitRefresh` to be called with the `teamsUpdated` and `voiceViewUpdated` delegates.
        /// </summary>
        protected virtual void OnEnable()
        {
            _prevPlayerCount = 0;
            logic = FindObjectOfType<UICoreLogic>();
            logic.teamsUpdated += WaitRefresh;
            logic.voiceViewUpdated += WaitRefresh;
        }

        /// <summary>
        /// Will remove the `WaitRefresh` from being called with the `teamsUpdated` and `voiceViewUpdated` delegates.
        /// </summary>
        protected virtual void OnDisable()
        {
            logic.teamsUpdated -= WaitRefresh;
            logic.voiceViewUpdated -= WaitRefresh;
        }

        /// <summary>
        /// Will dynamically instantiate the `ownerPlayer` if you or the `otherPlayer` gameobjects when a player
        /// joins the photon room, based on if they're the master client or not. Also plays a sound (if one is set) 
        /// when a player joins and leave the room. If the `autoSetTeamIfNotSet` is true and the player joining 
        /// doesn't already have a team set (from joining previously) then this will be called to automatically 
        /// select a team for them.
        /// </summary>
        protected virtual void Update()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && _prevPlayerCount != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (_prevPlayerCount < PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    _playJoinedSound = true;
                }
                else if (_prevPlayerCount > PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    _playLeftSound = true;
                }
                _prevPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                if (autoSetTeamIfNotSet == true)
                {
                    AutoSelectTeam();
                }
                WaitRefresh();
                if (_playJoinedSound == true)
                {
                    _playJoinedSound = false;
                    PlayJoinedSound();
                }
                else if (_playLeftSound == true)
                {
                    _playLeftSound = false;
                    PlayLeftSound();
                }
            }
        }

        #region Sound Settings
        /// <summary>
        /// Plays a random `joinedSound` at the `soundSource`.
        /// </summary>
        protected virtual void PlayJoinedSound()
        {
            if (joinedSound.Length > 0 && soundSource)
            {
                soundSource.clip = joinedSound[Random.Range(0, joinedSound.Length)];
                soundSource.Play();
            }
        }

        /// <summary>
        /// Plays a random `leftSound` at the `soundSource`.
        /// </summary>
        protected virtual void PlayLeftSound()
        {
            if (leftSound.Length > 0 && soundSource)
            {
                soundSource.clip = leftSound[Random.Range(0, joinedSound.Length)];
                soundSource.Play();
            }
        }
        #endregion

        #region Team Selection
        /// <summary>
        /// Will automatically try to evenly add playersto a select team. Loops over all players, skipping ones
        /// that already have a team set and captures all the players that don't. Then it will set each captured
        /// player into a team one by one, evenly distributing them.
        /// </summary>
        protected virtual void AutoSelectTeam()
        {
            if (debugging == true) Debug.Log("Auto selecting team...");
            string myUserId = PhotonNetwork.LocalPlayer.UserId;
            if (logic.PlayerInReadyDict(myUserId)) return;
            Dictionary<string,string> teamData = logic.GetTeamData();
            foreach (KeyValuePair<int, Photon.Realtime.Player> target_player in PhotonNetwork.CurrentRoom.Players)
            {
                if (!teamData.ContainsKey(target_player.Value.UserId) && myUserId == target_player.Value.UserId)
                {
                    Dictionary<string, int> teamCounts = new Dictionary<string, int>();
                    foreach(KeyValuePair<string, string> item in NetworkManager.networkManager.initalTeamSpawnPointNames)
                    {
                        teamCounts.Add(item.Key, 0);
                    }
                    foreach (KeyValuePair<string, string> team in teamData)
                    {
                        if (teamCounts.ContainsKey(team.Value))
                        {
                            teamCounts[team.Value] += 1;
                        }
                        else
                        {
                            teamCounts.Add(team.Value, 1);
                        }
                    }
                    string myTeamName = "";
                    if (teamCounts.Count > 0)
                    {
                        myTeamName = teamCounts.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                    }
                    else
                    {
                        myTeamName = teamName;
                    }
                    if (debugging == true) Debug.Log("Auto setting team: " + myTeamName);
                    logic.SetMyTeamName(myTeamName);
                }
            }
        }
        #endregion

        #region Refresh View
        /// <summary>
        /// Calls the `WaitToRefresh` IEnumerator
        /// </summary>
        public virtual void WaitRefresh()
        {
            StartCoroutine(WaitToRefresh());
        }

        /// <summary>
        /// Calls `DestroyChildObjects` and `SpawnChildObjects` functions.
        /// </summary>
        /// <returns></returns>
        protected virtual System.Collections.IEnumerator WaitToRefresh()
        {
            yield return new WaitForEndOfFrame();
            DestroyChildObjects();
            yield return new WaitForEndOfFrame();
            SpawnChildObjects();
        }

        /// <summary>
        /// Destroys all child gameobjects of this gameobject that this component is attached to.
        /// </summary>
        protected virtual void DestroyChildObjects()
        {
            if (debugging == true) Debug.Log("Destroy child objects...");
            foreach (Transform child in parentObj)
            {
                if (allocateViewIds == true)
                {
                    if (child.GetComponent<PhotonView>())
                    {
                        PhotonNetwork.LocalCleanPhotonView(child.GetComponent<PhotonView>());
                    }
                }
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Spawns a new child gameobject for the passed in player.
        /// </summary>
        /// <param name="target_player">KeyValuePair<int, Photon.Realtime.Player> type, the connected player dictionary</param>
        protected virtual void SpawnChild(KeyValuePair<int, Photon.Realtime.Player> target_player)
        {
            if (string.IsNullOrEmpty(teamName) || teamName == logic.GetUserTeamName(target_player.Value.UserId))
            {
                if (debugging == true) Debug.Log("Spawning local player object: " + target_player.Value.NickName);
                GameObject newChild = null;
                if (target_player.Value.IsMasterClient == true)
                {
                    newChild = Instantiate(ownerPlayer);
                }
                else
                {
                    newChild = Instantiate(otherPlayer);
                }
                newChild.transform.SetParent(parentObj);
                newChild.transform.localScale = new Vector3(1, 1, 1);
                newChild.transform.position = Vector3.zero;

                if (newChild.GetComponent<PlayerListObject>())
                {
                    newChild.GetComponent<PlayerListObject>().SetPlayerContents(target_player.Value);
                    newChild.GetComponent<PlayerListObject>().SetReadyState(logic.PlayerIsReady(target_player.Value.UserId));
                }
                if (allocateViewIds == true)
                {
                    if (newChild.GetComponent<PhotonView>())
                    {
                        if (logic.GetPlayerVoiceView(target_player.Value.UserId) == 999999 && PhotonNetwork.IsMasterClient == true)
                        {
                            logic.SendUpdateVoiceView(target_player.Value.UserId, PhotonNetwork.AllocateViewID(target_player.Value.ActorNumber));
                        }
                        else if (logic.GetPlayerVoiceView(target_player.Value.UserId) != 999999)
                        {
                            newChild.GetComponent<PhotonView>().ViewID = logic.GetPlayerVoiceView(target_player.Value.UserId);
                            newChild.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;
                            newChild.GetComponent<PhotonView>().TransferOwnership(target_player.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loops over all the players in the current room and calls `SpawnChild` on each one.
        /// </summary>
        protected virtual void SpawnChildObjects()
        {
            if (debugging == true) Debug.Log("Current Player Count: " + PhotonNetwork.CurrentRoom.Players.Count);
            foreach (KeyValuePair<int, Photon.Realtime.Player> target_player in PhotonNetwork.CurrentRoom.Players)
            { 
                SpawnChild(target_player);
            }
        }

        /// <summary>
        /// Can be used by other classes to set the `otherPlayer` gameobject to be spawned.
        /// </summary>
        /// <param name="newOtherPlayer">GameObject type, the gameobject to spawn when the player connecting is not the master client</param>
        public virtual void SetOtherPlayerGO(GameObject newOtherPlayer)
        {
            otherPlayer = newOtherPlayer;
        }

        /// <summary>
        /// Can be used by other classes to set the `newOwnerPlayer` gameobject to be spawned.
        /// </summary>
        /// <param name="newOtherPlayer">GameObject type, the gameobject to spawn when the player connecting is the master client</param>
        public virtual void SetOwnerPlayerGO(GameObject newOwnerPlayer)
        {
            ownerPlayer = newOwnerPlayer;
        }
        #endregion
    }
}