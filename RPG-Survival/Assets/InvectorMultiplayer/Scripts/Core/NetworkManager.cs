using UnityEngine;
using Photon.Pun;               //to acces Photon features
using Photon.Realtime;          //to access Photon callbacks
using UnityEngine.Events;       //to call actions on various states
using System.Collections.Generic;
using CBGames.Inspector;
using UnityEngine.SceneManagement;
using System.Collections;
using Invector.vCharacterController;
using System.Text.RegularExpressions;
using CBGames.Player;
using Photon.Voice.Unity;
using CBGames.UI;
using System.Linq;
using Invector.vItemManager;
using CBGames.Objects;
using Invector;
using System.Reflection;
using static CBGames.Core.CoreDelegates;

namespace CBGames.Core
{
    #region  Define UnityEvents
    [System.Serializable]
    public class PlayerEvent : UnityEvent<Photon.Realtime.Player> { }
    
    [System.Serializable]
    public class LobbyEvents
    {
        public UnityEvent _onJoinedLobby;
        public UnityEvent _onLeftLobby;
    }
    [System.Serializable]
    public class RoomEvents
    {
        public UnityEvent _onJoinedRoom;
        public UnityEvent _onLeftRoom;
        public UnityEvent _OnCreatedRoom;
        public StringUnityEvent _onCreateRoomFailed;
        public StringUnityEvent _onJoinRoomFailed;
        public UnityEvent _onReconnect;
    }
    [System.Serializable]
    public class PlayerEvents
    {
        public PlayerEvent _onPlayerEnteredRoom;
        public PlayerEvent _onPlayerLeftRoom;
    }
    [System.Serializable]
    public class OtherEvents
    {
        public UnityEvent _onMasterClientSwitched;
        public StringUnityEvent _onDisconnected;
        public UnityEvent _onConnectedToMaster;
        public StringUnityEvent _onFailedToConnectToPhoton;
        public StringUnityEvent _onConnectionFail;
    }
    #endregion

    #region Scene Update Database Classes
    [HideInInspector] [SerializeField] public enum ObjectActionEnum { Create, Delete, Update };
    
    [System.Serializable]
    public class ObjectAction
    {
        public string name = "";
        public string sceneName = "";
        public string resourcePrefab = "";
        public Vector3 position = Vector3.zero;
        public ObjectActionEnum action = ObjectActionEnum.Update;

        public string methodToCall = "";
        public string[] methodArgs;

        public ObjectAction(string inputName, string inputsceneName, string inputResourcePrefab, 
            Vector3 inputposition, ObjectActionEnum inputAction, string inputMethodToCall, string[] args)
        {
            this.name = inputName;
            this.sceneName = inputsceneName;
            this.resourcePrefab = inputResourcePrefab;
            this.position = inputposition;
            this.action = inputAction;
            this.methodToCall = inputMethodToCall;
            this.methodArgs = args;
        }

    }
    [System.Serializable]
    public class CallPlayerFunction
    {
        public string methodName = "";
        public string methodArg;

        public CallPlayerFunction(string inputMethodName, string inputMethodArgs)
        {
            this.methodName = inputMethodName;
            this.methodArg = inputMethodArgs;
        }
    }
    #endregion

    [AddComponentMenu("CB GAMES/Core/Network Manager")]
    public partial class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region Editor Variables
        [HideInInspector] public bool e_show_unv = false;
        [HideInInspector] public bool e_show_player = false;
        [HideInInspector] public bool e_show_spawn = false;
        [HideInInspector] public bool e_show_debug = false;
        [HideInInspector] public bool e_show_network = false;
        #endregion

        #region Modifiables
        [Tooltip("The version to connect with. Incompatible versions will not connect with each other.")]
        public string gameVersion = "1.0";
        [Tooltip("The max number of player per room. When a room is full, it can't be joined by new players, and so a new room will be created.")]
        [SerializeField] public byte maxPlayerPerRoom = 4;
        [Tooltip("The _prefab that will be spawned in when a player successfully connects.")]
        public GameObject playerPrefab = null;
        [Tooltip("The point where the player will start when they have successfully connected or if no other available spawn point is specified.")]
        public Transform defaultSpawnPoint = null;
        [Tooltip("Will find all transforms with this tag and treat it as a spawn point. If no tag is specified will only us what is placed in the default spawn point tag.")]
        [TagSelector] public string spawnPointsTag = "SpawnPoint";
        [Tooltip("Shows the current connection process. This is great for UI to reference and use.")]
        public string _connectStatus = "";
        [Tooltip("Automatically sync all connected clients scenes. Make sure everyone is always on the same scene together.")]
        public bool syncScenes = false;
        [Tooltip("Save state between scenes. When you re-enter the scene replay all of the actions on the objects with syncScenes enabled. " +
            "This allows for persistant dropped items, picked up items, interacted objects, etc. between unity scenes/photon rooms.")]
        public bool replayScenes = true;
        [Tooltip("The scene database that holds all of the information about all of the given scenes.")]
        public SceneDatabase database = null;
        [Tooltip("The voice recorder that will be set on owning players Primary Recorder slot. This must be set if using voice chat.")]
        public Recorder voiceRecorder;
        [Tooltip("If you want to log everything to the console that the network manager is doing at runtime.")]
        public bool debugging = false;
        [Tooltip("If you want to display a visual window of the current settings found in the network manager at runtime.")]
        public bool displayDebugWindow = false;
        [Tooltip("Actions to trigger on events that happen in the lobby. Contains the following UnityEvents: " +
            "_onJoinedLobby, _onLeftLobby")]
        public LobbyEvents lobbyEvents;
        [Tooltip("Actions to trigger on events that happen in the room. Contains the following UnityEvents: " +
            " _onJoinedRoom, _onLeftRoom, _OnCreatedRoom, _onCreateRoomFailed, _onJoinRoomFailed, _onReconnect")]
        public RoomEvents roomEvents;
        [Tooltip("Actions to trigger on events that happen with actions according to each player. This contains " +
            "the following UnityEvent parameters: _onPlayerEnteredRoom, _onPlayerLeftRoom")]
        public PlayerEvents playerEvents;
        [Tooltip("Unity events to call based on Misc Network events. This contains the following UnityEvent parameters: " +
            "_onMasterClientSwitched, _onDisconnected, _onConnectedToMaster, _onFailedToConnectToPhoton, _onConnectionFail")]
        public OtherEvents otherEvents;
        [Tooltip("Says if you're currently connecting to the Photon Server, Lobby, or Room. (false after connection is made)")]
        [HideInInspector] public bool _connecting = false;
        [Tooltip("The name of the team you're currently on. If the team name is never set either manually or " +
            "via the \"SetTeamName\" function everyone will be able to damage each other. (Free-For-All) " +
            "\n\nWARNING: DO NOT SET THIS MANUALLY! " +
            "The only reason to set this manually is if you are testing something out related to same " +
            "team mechanics.")]
        public string teamName = "";
        [Tooltip("The spawn point name that will represent the initial starting point for the team " +
            "If more than one of the spawn points are named with this same name then it will randomly " +
            "choose between those points and spawn the team member at that location.")]
        [SerializeField]
        public DictionaryOfStringAndString initalTeamSpawnPointNames = new DictionaryOfStringAndString();
        [Tooltip("Allow team members to damage each other.")]
        public bool allowTeamDamaging = false;
        [Tooltip("If you want to immediately spawn the player into the scene when joining the lobby. " +
            "If not you need to have a UI that sets this to true before going to the next scene.")]
        public bool autoSpawnPlayer = true;
        [Tooltip("Automatically attempt to reconnect to the last room you were in if you get disconnected.")]
        public bool reconnect = false;
        [Tooltip("How many attempts to reconnect to the room if 'reconnect' is true.")]
        [SerializeField] protected int connect_attempts = 3;
        [Tooltip("On a successfull reconnect, do you want to spawn the character back at the location " +
            "you last were at or just at a random spawn in point?")]
        [SerializeField] public bool spawnAtSaved = true;
        #endregion

        #region Public Delegates
        /// <summary>
        /// This delegate is called whenever you are trying to reconnect to a room after you disconnected from it.
        /// </summary>
        public BasicDelegate OnReconnectingToRoom;

        /// <summary>
        /// This delegate is called whenever another player joins the photon room. This isn't called 
        /// when you first join on yourself.
        /// </summary>
        /// <param name="player">player parameter is Photon.Realtime.Player type. This is the player that is joining.</param>
        public PlayerDelegate OnPlayerJoinedCurrentRoom;
        
        /// <summary>
        /// This delegate is called whenever another player leaves the photon room. This isn't called 
        /// when you leave the photon room on yourself.
        /// </summary>
        /// <param name="player">player parameter is Photon.Realtime.Player type. This is the player that is leaving.</param>
        public PlayerDelegate OnPlayerLeftCurrentRoom;

        /// <summary>
        /// This delegate is called whenever another player is kicked from the photon room.
        /// </summary>
        /// <param name="player">player parameter is Photon.Realtime.Player type. This is the player that was just kicked from the photon room.</param>
        public PlayerDelegate OnPlayerKicked;

        /// <summary>
        /// This delegate is called whenever the master client left and a new master client is selected.
        /// </summary>
        /// <param name="player">player parameter is Photon.Realtime.Player type. This is the player was selected as the master client.</param>
        public PlayerDelegate OnClientHostSwitched;

        /// <summary>
        /// This delegate is called whenever you disconnect from the photon room.
        /// </summary>
        /// <param name="cause">cause parameter is Photon.Realtime.DisconnectCause type. This is a basic disconnect error that is returned.</param>
        public DisconnectCauseDelegate OnDisconnectedFromPhoton;

        /// <summary>
        /// This delegate is called when something causes the connection to fail(after it was established), followed by a call to OnDisconnectedFromPhoton().
        /// If the server could not be reached in the first place, OnFailedToConnectToPhoton is called instead.The reason for the error is provided as StatusCode.
        /// </summary>
        /// <param name="cause">cause parameter is Photon.Realtime.DisconnectCause type. This is a basic connection error/cause that is returned.</param>
        public DisconnectCauseDelegate OnConnectionFailed;

        /// <summary>
        /// This delegate is called when something causes the connection to the Photon master server to fail.
        /// </summary>
        /// <param name="cause">cause parameter is Photon.Realtime.DisconnectCause type. This is a basic connection error/cause that is returned.</param>
        public DisconnectCauseDelegate OnFailToConnectToPhoton;

        /// <summary>
        /// This delegate is called whenever you failed to connect to a photon room.
        /// </summary>
        /// <param name="returnCode">The status code</param>
        /// <param name="message">A basic error message.</param>
        public RoomFailure OnJoinRoomFailure;

        /// <summary>
        /// This delegate is called whenever you failed to create to a photon room.
        /// </summary>
        /// <param name="returnCode">The status code</param>
        /// <param name="message">A basic error message.</param>
        public RoomFailure OnCreateRoomFailure;

        /// <summary>
        /// This delegate is called a update to the photom room list is received.
        /// </summary>
        /// <param name="roomList">roomList is type Dictionary<string, RoomInfo>. A list of currently found photon rooms.</param>
        public RoomListUpdate OnUpdatedRoomList;

        /// <summary>
        /// This delegate is called when you successfully connect to the photon master server.
        /// </summary>
        public BasicDelegate OnConnectedToMasterPhotonServer;

        /// <summary>
        /// This delegate is called when you successfully connect to the photon lobby.
        /// </summary>
        public BasicDelegate OnJoinedPhotonLobby;

        /// <summary>
        /// This delegate is called when you successfully disconnect from the photon lobby.
        /// </summary>
        public BasicDelegate OnLeftPhotonLobby;

        /// <summary>
        /// This delegate is called when you successfully connect to the photon room.
        /// </summary>
        public BasicDelegate OnJoinedPhotonRoom;

        /// <summary>
        /// This delegate is called when you successfully disconnect from the photon room.
        /// </summary>
        public BasicDelegate OnLeftPhotonRoom;

        /// <summary>
        /// This delegate is called when you successfully create a the photon room.
        /// </summary>
        public BasicDelegate OnCreatedPhotonRoom;

        /// <summary>
        /// This delegate is called when you successfully connected to the ChatBox's data channel.
        /// </summary>
        public StringDelegate OnJoinedChatDataChannel;
        #endregion

        #region Internal Use Variables
        public Vector3 spawnAtLoc = Vector3.zero;
        public Quaternion spawnAtRot = Quaternion.identity;
        public static NetworkManager networkManager = null;
        protected bool isReconnecting = false;
        protected int _reconnectAttempt = 0;
        protected bool _inDataChannel = false;
        protected bool _playedSceneDatabase = false;
        protected bool _inventoryLoading = false;
        protected bool _lockModifySceneDatabase = false;
        Dictionary<string, List<ObjectAction>> _modifySceneDatabase = new Dictionary<string, List<ObjectAction>>();
        protected string chatDataChannel = null;
        protected ChatBox chatbox = null;
        [Tooltip("The current photon room list if in the photon lobby. This is updated via the " +
            "\"UpdateCachedRoomList\" function which in turn is called via the \"OnRoomListUpdate\" " +
            "function.")]
        public Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
        protected bool _joinLobby = false;
        protected bool _joinRoom = false;
        protected bool _createRoom = false;
        protected string _roomName = "";
        protected bool _inRoom = false;
        protected string _spawnAtPoint = null;
        protected int _levelIndexToLoad = -1;
        protected string _sceneName = "";
        protected TypedLobby _targetLobbyType = TypedLobby.Default;
        protected PlayerData _cachedData;
        protected bool _useGlobalNaming = true;
        [Tooltip("The current index of the Unity scene that your currently in. This is used for internal " +
            "logic in the NetworkManager and ChatBox.")]
        public int _currentLevelIndex = -1;

        public object InventoryDataFile { get; private set; }

        #endregion

        #region Spawning Logic
        /// <summary>
        /// Set the name of the team you are a member of. This is for your local player only and is not designed
        /// to work with Network players.
        /// </summary>
        /// <param name="inputName">The team name to set.</param>
        public virtual void SetTeamName(string inputName)
        {
            teamName = inputName;
            foreach(SyncPlayer player in FindObjectsOfType<SyncPlayer>())
            {
                if (player.transform.GetComponent<PhotonView>().IsMine == true)
                {
                    player.teamName = inputName;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Returns a random transform from the scene that is tagged with the spawn point tag.
        /// </summary>
        /// <returns>Transform that is tagged with the specified spawn point tag.</returns>
        public virtual Transform GetRandomSpawnPoint()
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(spawnPointsTag);
            if (spawnPoints.Length < 1)
                return defaultSpawnPoint;
            else
                return spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform;
        }

        /// <summary>
        /// Get a spawn point that is specific for the specified team.
        /// </summary>
        /// <param name="teamName">The specific team named spawn point to look for.</param>
        /// <returns>Transform that is tagged with spawn point tag but is also named according to your team definition.
        /// If nothing like this is found then GetRandomSpawnPoint() is called.</returns>
        public virtual Transform GetTeamSpawnPoint(string teamName)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(spawnPointsTag);
            List<Transform> availableSpawnPoints = new List<Transform>();
            foreach(GameObject spawnPoint in spawnPoints)
            {
                if (spawnPoint.name.Trim() == initalTeamSpawnPointNames[teamName])
                {
                    availableSpawnPoints.Add(spawnPoint.transform);
                }
            }
            if (availableSpawnPoints.Count == 1)
            {
                return availableSpawnPoints[0];
            }
            else if (availableSpawnPoints.Count > 0)
            {
                return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
            }
            else
            {
                return GetRandomSpawnPoint();
            }
        }
        #endregion

        #region LocalMethods
        /// <summary>
        /// This is used to make sure this is the only NetworkManager in the scene as well as setup scene loading 
        /// delegates
        /// </summary>
        protected virtual void Awake()
        {
            if (networkManager == null)
            {
                networkManager = this;
                DontDestroyOnLoad(this.gameObject);
                this.gameObject.name = gameObject.name + " Instance";
                SceneManager.sceneLoaded += NewSceneLoaded;
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
            PhotonNetwork.AutomaticallySyncScene = syncScenes; //Automatically load scenes together (make sure everyone is always on the same scene)
        }
        /// <summary>
        /// This is used to find the chatbox in the scene. Used in start to give awake enough time to destroy 
        /// duplicate Chatbox's if any.
        /// </summary>
        protected virtual void Start()
        {
            chatbox = (FindObjectOfType<ChatBox>()) ? FindObjectOfType<ChatBox>() : null;
        }
        #endregion

        #region Room/Scene Logic
        /// <summary>
        /// This is called via "OnRoomListUpdate" function. This will update the list of available rooms 
        /// and store it in the cachedRoomList parameter. Only usable when in the Photon Lobby
        /// </summary>
        /// <param name="roomList">List of current photon rooms.</param>
        protected virtual void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            cachedRoomList.Clear();
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
            if (OnUpdatedRoomList != null) OnUpdatedRoomList.Invoke(cachedRoomList);
        }
        
        /// <summary>
        /// Will format the string to be session name compliant.
        /// </summary>
        /// <param name="inputName">The name that you want to make sure is session name formatted.</param>
        /// <returns>The session name.</returns>
        public virtual string GetGlobalRoomName(string inputName)
        {
            return Regex.Replace(inputName, "_.+", ""); ;
        }

        /// <summary>
        /// Gets the current session name that your in and combines it with the Unity scene that 
        /// your in to make it (session name)_(unity scene)
        /// </summary>
        /// <returns>A string formatted like: (session name)_(unity scene)</returns>
        public virtual string GetIndividualRoomName()
        {
            return GetGlobalRoomName(_roomName) + "_" + _sceneName;
        }

        /// <summary>
        /// Set the spawn at point then trigger the "NewSceneLoaded" function.
        /// </summary>
        /// <param name="pointName">The spawn point name to set</param>
        public virtual void SetSpawnAtPoint(string pointName)
        {
            _spawnAtPoint = pointName;
            SceneManager.sceneLoaded += NewSceneLoaded;
        }
        
        /// <summary>
        /// Get the scene name that the network manager thinks it's in.
        /// </summary>
        /// <returns>The network manager scene name string</returns>
        public virtual string GetCurrentSceneName()
        {
            return _sceneName;
        }
        
        /// <summary>
        /// Get the scene index that the network manager thinks it's in.
        /// </summary>
        /// <returns>Scene index, int</returns>
        public virtual int GetCurrentSceneIndex()
        {
            return _currentLevelIndex;
        }

        /// <summary>
        /// Loads a Unity Scene and triggers creating or joining a room based on the 
        /// GetIndividualRoomName function's return. Can send one person or everyone 
        /// together. If everyone only the MasterClient can do this. Calling this also
        /// saves the current settings of your player and restores them in the new 
        /// unity scene when it has spawned your player there.
        /// </summary>
        /// <param name="level">The index of the unity scene to load</param>
        /// <param name="spawnAtPoint">The string of the spawn point to find and spawn your character at</param>
        /// <param name="sendEveryone">Send everyone in the current photon room with you to the new scene.</param>
        public virtual void NetworkLoadLevel(int level, string spawnAtPoint = null, bool sendEveryone = false)
        {
            _currentLevelIndex = level;
            _spawnAtPoint = spawnAtPoint;
            if (debugging) Debug.Log("Starting NetworkLoadLevel - SendEveryone = " + sendEveryone);

            PhotonNetwork.AutomaticallySyncScene = sendEveryone;
            _sceneName = database.storedScenesData.Find(x => x.index == level).sceneName;
            if (debugging) Debug.Log("Attempting to travel to: " + _sceneName);
            SavePlayer(GetYourPlayer());

            _connectStatus = "Loading level...";
            if (debugging == true) Debug.Log(_connectStatus);
            if (sendEveryone == true && PhotonNetwork.IsMasterClient)
            {
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                PhotonNetwork.IsMessageQueueRunning = false;
                PhotonNetwork.LoadLevel(level);
            }
            else
            {
                _roomName = GetIndividualRoomName();
                _levelIndexToLoad = level;
                if (IsInRoom())
                {
                    LeaveRoom();
                }
                else if (cachedRoomList.ContainsKey(_roomName))
                {
                    JoinRoom(_roomName);
                }
                else
                {
                    CreateRoom(_roomName, new RoomOptions() { MaxPlayers = maxPlayerPerRoom, PublishUserId = true, IsVisible = false, IsOpen = true });
                }
            }
        }

        /// <summary>
        /// Callback method that is called every time a new scene is loaded. Will 
        /// potentially spawn your character in the new scene. It also replays the 
        /// items that were dropped/picked up in this scene as well as other network
        /// actions.
        /// </summary>
        /// <param name="scene">The scene that is getting loaded.</param>
        /// <param name="mode">LoadSceneMode that is loading this scene.</param>
        public virtual void NewSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (replayScenes == true)
            {
                _playedSceneDatabase = false;
                ReplaySceneDatabase(false, false);
            }
            if (_levelIndexToLoad != -1)
            {
                if (debugging == true) Debug.Log("New Scene Loaded!");
                if (autoSpawnPlayer == true)
                {
                    vGameController gc = FindObjectOfType<vGameController>();
                    BindingFlags allBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    if (isReconnecting && spawnAtSaved == true)
                    {
                        GameObject player = NetworkInstantiatePrefab(playerPrefab.name, spawnAtLoc, spawnAtRot, 0);
                        if (gc)
                        {
                            gc.GetType().GetField("currentController", allBindings).SetValue(gc, player.GetComponent<vThirdPersonController>());
                        }
                        LoadPlayerData(player.name.Replace("(Clone)", "").Replace("Instance", ""), player.GetComponent<vThirdPersonController>(), false, true);
                    }
                    else if (_spawnAtPoint == null)
                    {
                        GameObject player = null;
                        GameObject[] targets = GameObject.FindGameObjectsWithTag("SpawnPoint");
                        GameObject target = targets[Random.Range(0, targets.Length)];
                        player = NetworkInstantiatePrefab(playerPrefab.name, target.transform.position, target.transform.rotation, 0);
                        if (gc)
                        {
                            gc.GetType().GetField("currentController", allBindings).SetValue(gc, player.GetComponent<vThirdPersonController>());
                        }
                        LoadPlayerData(player.name.Replace("(Clone)","").Replace("Instance",""), player.GetComponent<vThirdPersonController>(), false, true);
                    }
                    else
                    {
                        foreach (GameObject go in GameObject.FindGameObjectsWithTag("LoadPoint"))
                        {
                            if (go.name == _spawnAtPoint)
                            {
                                if (debugging == true) Debug.Log("Network Instantiating player prefab");
                                GameObject player = NetworkInstantiatePrefab(playerPrefab.name, go.transform.position, go.transform.rotation, 0);
                                if (gc)
                                {
                                    gc.GetType().GetField("currentController", allBindings).SetValue(gc, player.GetComponent<vThirdPersonController>());
                                }
                                LoadPlayerData(player.name.Replace("(Clone)", "").Replace("Instance", ""), player.GetComponent<vThirdPersonController>(), false, true);
                                _spawnAtPoint = null;
                                break;
                            }
                        }
                    }
                }
                _levelIndexToLoad = -1;
                PhotonNetwork.IsMessageQueueRunning = true;
            }
        }
        #endregion

        #region Chat Logic
        /// <summary>
        /// Get the current chatbox's data channel.
        /// </summary>
        /// <returns>Chatbox's data channel that you're subscribed to.</returns>
        public virtual string GetChatDataChannel()
        {
            return chatDataChannel;
        }
        
        /// <summary>
        /// Get the chatbox component.
        /// </summary>
        /// <returns>The chatbox component</returns>
        public virtual ChatBox GetChabox()
        {
            return chatbox;
        }
        
        /// <summary>
        /// Set if you are currently joined to the chatbox's data channel or not
        /// </summary>
        /// <param name="value">True or False, are you in the chatbox's data channel?</param>
        public virtual void SetInDataChannel(bool value)
        {
            _inDataChannel = value;
        }
        #endregion

        #region Saving/Loading
        /// <summary>
        /// Get the vThirdPersonController that you own, not the networked players.
        /// </summary>
        /// <returns>vThirdPersonController component</returns>
        public virtual vThirdPersonController GetYourPlayer()
        {
            foreach (vThirdPersonController controller in FindObjectsOfType<vThirdPersonController>())
            {
                if (controller.gameObject.GetComponent<PhotonView>().IsMine == true)
                {
                    return controller;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the PlayerData that was saved previously. Used for loading saved 
        /// player stats, inventory, etc.
        /// </summary>
        /// <returns>cached player data</returns>
        public virtual PlayerData GetCachedPlayerData()
        {
            return _cachedData;
        }

        /// <summary>
        /// Save your players stats, inventory, etc.
        /// </summary>
        /// <param name="player">The vThirdPersonController that you own and want to save.</param>
        public virtual void SavePlayer(vThirdPersonController player)
        {
            if (debugging) Debug.Log("Saving player data");
            if (player)
            {
                _cachedData = SaveLogic.SavePlayerData(player);
            }
            else if (debugging)
            {
                Debug.Log("No passed in player, skipping player save.");
            }
        }

        /// <summary>
        /// Load the player data into the target vThirdPersonController.
        /// </summary>
        /// <param name="playerName">The player name the data is cached under</param>
        /// <param name="player">The vThirdPersonController you want to load the data into</param>
        /// <param name="useSavedNickname">If you don't want to overwrite the saved player name</param>
        /// <param name="loadFromCache">If you want to load what the NetworkManager already knows about or load it from the saved binary file</param>
        /// <param name="overrideStartMaxHealth">If you want to set the health to whatever is loaded</param>
        public virtual void LoadPlayerData(string playerName, vThirdPersonController player = null, bool useSavedNickname = true, bool loadFromCache = false, bool overrideStartMaxHealth = false)
        {
            if (debugging == true) Debug.Log("Beginning to load player data...");
            if (loadFromCache == false)
            {
                if (debugging == true) Debug.Log("Loading player data from saved file.");
                _cachedData = SaveLogic.LoadPlayerData(playerName);
            }
            else if (debugging == true) Debug.Log("Loading player data from cache.");
            if (_cachedData == null) return;
            if (useSavedNickname == true)
            {
                if (debugging == true) Debug.Log("Setting network nickname to: " + _cachedData.characterName);
                PhotonNetwork.NickName = _cachedData.characterName;
            }
            if (player)
            {
                if (debugging == true) Debug.Log("Setting player data...");

                #region Health
                int setHealth = (overrideStartMaxHealth == true) ? _cachedData.health.max : Mathf.RoundToInt(_cachedData.health.current);
                _cachedData.health.current = setHealth;
                player.ChangeHealth(setHealth);
                player.maxHealth = _cachedData.health.max;
                player.healthRecovery = _cachedData.health.recovery;
                player.healthRecoveryDelay = _cachedData.health.recoveryDelay;
                #endregion

                #region Stamina
                player.maxStamina = _cachedData.stamina.max;
                player.staminaRecovery = _cachedData.stamina.recovery;
                player.sprintStamina = _cachedData.stamina.sprint;
                player.jumpStamina = _cachedData.stamina.jump;
                player.rollStamina = _cachedData.stamina.roll;
                #endregion

                #region Roll
                player.rollSpeed = _cachedData.roll.speed;
                player.rollRotationSpeed = _cachedData.roll.rotationSpeed;
                player.rollUseGravity = _cachedData.roll.useGravity;
                player.rollUseGravityTime = _cachedData.roll.useGravityTime;
                player.timeToRollAgain = _cachedData.roll.timeToRollAgain;
                #endregion

                #region Jump
                player.jumpTimer = _cachedData.jump.timer;
                player.jumpHeight = _cachedData.jump.height;
                player.airSpeed = _cachedData.jump.airSpeed;
                player.airSmooth = _cachedData.jump.airSmooth;
                player.extraGravity = _cachedData.jump.extraGravity;
                player.limitFallVelocity = _cachedData.jump.limitFallVelocity;
                player.ragdollVelocity = _cachedData.jump.ragdollVelocity;
                player.fallMinHeight = _cachedData.jump.fallDamageMinHeight;
                player.fallMinVerticalVelocity = _cachedData.jump.fallDamageMinVerticalVelocity;
                player.fallDamage = _cachedData.jump.fallDamage;
                #endregion

                #region Ground
                player.groundMinDistance = _cachedData.ground.minDistance;
                player.groundMaxDistance = _cachedData.ground.maxDistance;
                player.slideDownVelocity = _cachedData.ground.slideDownVelocity;
                player.slideSidewaysVelocity = _cachedData.ground.slideSidewaysVelocity;
                player.slidingEnterTime = _cachedData.ground.slideEnterTime;
                player.stepOffsetMaxHeight = _cachedData.ground.stepOffsetMaxHeight;
                player.stepOffsetMinHeight = _cachedData.ground.stepOffsetMinHeight;
                player.stepOffsetDistance = _cachedData.ground.stepOffsetDistance;
                #endregion

                #region Inventory
                if (player.gameObject.GetComponent<vItemManager>())
                {
                    StartCoroutine(LoadItemsIntoInventory(player));
                }
                #endregion
            }
        }
        IEnumerator LoadItemsIntoInventory(vThirdPersonController player)
        {
            if (_inventoryLoading == true) yield return null;
            _inventoryLoading = true;
            PlayerData.InventoryWrapper topKey = _cachedData.ConvertBackToWrapper(_cachedData.inventory.items);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            player.gameObject.GetComponent<vItemManager>().items = topKey.wrapper.items;
            player.gameObject.GetComponent<vItemManager>().inventory.OnReloadGame();
            yield return new WaitForEndOfFrame();
            foreach (PlayerData.InventoryPlacement placement in topKey.wrapper.placement)
            {
                if (placement.item != null)
                {
                    player.gameObject.GetComponent<vItemManager>().EquipItemToEquipSlot(placement.areaIndex, placement.slotIndex, placement.item, true);
                }
            }
            _inventoryLoading = false;
        }
        #endregion

        #region Network Actions Logic
        /// <summary>
        /// Are you currently in a photon room?
        /// </summary>
        /// <returns>True if you're already in a photon room.</returns>
        public virtual bool IsInRoom()
        {
            return _inRoom;
        }
        
        /// <summary>
        /// Are you currently in a photon lobby?
        /// </summary>
        /// <returns>True if you're already in a photon lobby</returns>
        public virtual bool IsInLobby()
        {
            return PhotonNetwork.InLobby;
        }

        /// <summary>
        /// Set the players network name, if nothing is supplied will auto name as 'Un-named Player'
        /// </summary>
        /// <param name="name">The name you want to be known as over the network</param>
        public virtual void SetPlayerName(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            PhotonNetwork.NickName = (name == "") ? "Un-named Player" : name;
        }

        /// <summary>
        /// Disconnect from the PUN master server. Will be dropped from any lobby or room.
        /// </summary>
        public virtual void Disconnect()
        {
            _connectStatus = "Disconnecting...";
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// Attempts to join the selected room. If it doesn't exist it will create it.
        /// </summary>
        /// <param name="roomName">The name of the room to join/create</param>
        /// <param name="options">Options for the room, in case it does not exist yet. Else these values are ignored.</param>
        /// <param name="customRoomProperties">The room properties to potentially create this room with</param>
        /// <param name="exposePropertiesToLobby">The custom properties you want to allow other users to see</param>
        /// <param name="expectedUsers">Optional list of users (by UserId) who are expected to join this game and who you want to block a slot for.</param>
        public virtual void JoinOrCreateRoom(string roomName, RoomOptions options = null, ExitGames.Client.Photon.Hashtable customRoomProperties = null, string[] exposePropertiesToLobby = null, string[] expectedUsers = null)
        {
            _connecting = true;
            _roomName = (_useGlobalNaming) ? GetGlobalRoomName(roomName) : roomName;
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InLobby == false)
            {
                _createRoom = true;
                JoinLobby();
            }
            else
            {
                _connectStatus = "Attempting to Join or Create \"" + _roomName + "\" room...";
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                if (options == null)
                {
                    options = new RoomOptions()
                    {
                        MaxPlayers = maxPlayerPerRoom,
                        PublishUserId = true,
                        IsVisible = true,
                        IsOpen = true,
                        CleanupCacheOnLeave = true,
                        CustomRoomProperties = customRoomProperties,
                        CustomRoomPropertiesForLobby = exposePropertiesToLobby
                    };
                }
                PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default, expectedUsers);
            }
        }

        /// <summary>
        /// Create a room with the target name for other players to join in the default lobby. Will connect to master server and default lobby if not already connected.
        /// </summary>
        /// <param name="roomName">The name of the room to make</param>
        /// <param name="options">Options for the room, in case it does not exist yet. Else these values are ignored.</param>
        /// <param name="customRoomProperties">The room properties to potentially create this room with</param>
        /// <param name="exposePropertiesToLobby">The custom properties you want to allow other users to see</param>
        public virtual void CreateRoom(string roomName, RoomOptions options = null, ExitGames.Client.Photon.Hashtable customRoomProperties = null, string[] exposePropertiesToLobby = null)
        {
            _connecting = true;
            _roomName = (_useGlobalNaming) ? GetGlobalRoomName(roomName) : roomName;
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InLobby == false)
            {
                _createRoom = true;
                JoinLobby();
            }
            else
            {
                _connectStatus = "Creating \""+_roomName+"\" room...";
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                if (options == null)
                {
                    options = new RoomOptions() {
                        MaxPlayers = maxPlayerPerRoom,
                        PublishUserId = true,
                        IsVisible = true,
                        IsOpen = true,
                        CleanupCacheOnLeave = true,
                        CustomRoomProperties = customRoomProperties,
                        CustomRoomPropertiesForLobby = exposePropertiesToLobby
                    };
                }
                PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
            }
        }
        
        /// <summary>
        /// Makes the caller leave the room they are connected to but stay connected to default lobby and master server.
        /// </summary>
        public virtual void LeaveRoom()
        {
            _connectStatus = "Attempting to leave room...";
            if (debugging == true) Debug.Log(_connectStatus);
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// Attempt to join a random room in your connected lobby. Will join master server and defaul lobby if not already connected prior to joining.
        /// </summary>
        /// <param name="expectedRoomProperties">The room properties that need to exist for you to join the room</param>
        public virtual void JoinRandomRoom(ExitGames.Client.Photon.Hashtable expectedRoomProperties = null)
        {
            _connecting = true;
            _roomName = expectedRoomProperties[RoomProperty.RoomName].ToString();
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InLobby == false)
            {
                _joinRoom = true; //will join the room after connecting to master server and joining lobby.
                JoinLobby(); // will connect to server and join lobby.
            }
            else
            {
                _connectStatus = "Attempting to join a random room... ";
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                if (expectedRoomProperties == null)
                {
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    PhotonNetwork.JoinRandomRoom(expectedRoomProperties, maxPlayerPerRoom);
                }
            }
        }

        /// <summary>
        /// Join a room with name in your connected lobby. Will join master server and defaul lobby if not already connected prior to joining.
        /// </summary>
        /// <param name="roomName">The name of the photon room to join</param>
        public virtual void JoinRoom(string roomName)
        {
            _connecting = true;
            _roomName = roomName;
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InLobby == false)
            {
                _joinRoom = true;
                JoinLobby();
            }
            else
            {
                _connectStatus = "Attempting to join room: " + roomName;
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                PhotonNetwork.JoinRoom(roomName);
                _joinRoom = false;
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
            }
        }

        /// <summary>
        /// Leave your currently connected lobby but stay connect to the master server.
        /// </summary>
        public virtual void LeaveLobby()
        {
            if (PhotonNetwork.IsConnected == true)
            {
                if (PhotonNetwork.InLobby == true)
                {
                    _connectStatus = "Leaving lobby...";
                    PhotonNetwork.LeaveLobby();
                }
            }
        }

        /// <summary>
        /// Join the default lobby. If not connected will connect to master server first. If already in lobby, will do nothing.
        /// </summary>
        /// <param name="lobbyType">A typed lobby to join (must have name and type).</param>
        public virtual void JoinLobby(TypedLobby lobbyType = null)
        {
            if (lobbyType == null) lobbyType = TypedLobby.Default;
            _connecting = true;
            _connectStatus = "Attempting to join the lobby...";
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            if (PhotonNetwork.IsConnected == true)
            {
                if (PhotonNetwork.InLobby == false)
                {
                    PhotonNetwork.JoinLobby(lobbyType);
                    _joinLobby = false;
                }
            }
            else
            {
                _connectStatus = "Not yet connected to master server.";
                if (debugging == true) Debug.Log(_connectStatus);
                _joinLobby = true;
                _targetLobbyType = lobbyType;
                ConnectToMasterServer();
            }
        }

        /// <summary>
        /// Connect to the PUN master server.
        /// </summary>
        public virtual void ConnectToMasterServer()
        {
            _connecting = true;
            _connectStatus = "Attempting to join the master server...";
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// Returns the numbers of players connected to this room
        /// </summary>
        /// <returns>The number of players currently in the photon room</returns>
        public virtual int GetPlayerCount()
        {
            return PhotonNetwork.CurrentRoom.PlayerCount;
        }

        /// <summary>
        /// Spawn a object from the "Resources" folder on every instance of the game over the network that is owned by a particular player
        /// </summary>
        /// <param name="prefabName">The name of the prefab in the resource folder</param>
        /// <param name="position">Vector3 position in the scene to spawn the object</param>
        /// <param name="rotation">Quaternion rotation in the scene to spawn the object</param>
        /// <param name="group">The network group to do this for</param>
        /// <param name="data">Additional data to spawn the object with</param>
        /// <returns>spawned Gameobject from the resource folder</returns>
        public virtual GameObject NetworkInstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation, byte group=0, object[] data=null)
        {
            if (debugging == true) Debug.Log("Network Instantiating the prefab: "+prefabName);
            GameObject target = PhotonNetwork.Instantiate(prefabName, position, rotation, group, data) as GameObject;
            
            return target;
        }

        /// <summary>
        /// Spawn a object from the "Resources" folder on every instance of the game over the network that is owned by the scene, no player.
        /// </summary>
        /// <param name="prefabName">The name of the prefab in the resource folder</param>
        /// <param name="position">Vector3 position in the scene to spawn the object</param>
        /// <param name="rotation">Quaternion rotation in the scene to spawn the object</param>
        /// <param name="group">The network group to do this for</param>
        /// <param name="data">Additional data to spawn the object with</param>
        /// <returns>spawned Gameobject from the resource folder</returns>
        public virtual GameObject NetworkInstantiatePersistantPrefab(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
        {
            if (debugging == true) Debug.Log("Network Instantiating persistant prefab: " + prefabName);
            GameObject target = PhotonNetwork.InstantiateRoomObject(prefabName, position, rotation, group, data);

            return target;
        }
        #endregion

        #region Photon Callback Methods
        protected virtual void OnApplicationQuit()
        {
            Disconnect();
        }

        #region PlayerEvents
        /// <summary>
        /// Boots a player from a photon room.
        /// </summary>
        /// <param name="playerToKick">The Photon.Realtime.Player to kick from the photon room</param>
        public virtual void KickPlayer(Photon.Realtime.Player playerToKick)
        {
            PhotonNetwork.CloseConnection(playerToKick);
            if (OnPlayerKicked != null) OnPlayerKicked.Invoke(playerToKick);
        }

        /// <summary>
        /// Boots a player from a photon room.
        /// </summary>
        /// <param name="userId">The UserId to kick from the photon room.</param>
        public virtual void KickPlayer(string userId)
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (userId == player.UserId)
                {
                    KickPlayer(player);
                    break;
                }
            }
        }

        /// <summary>
        /// Callback method. Called when another player enters the photon room. Isn't called for yourself.
        /// </summary>
        /// <param name="newPlayer">The Photon.Realtime.Player that just entered the room.</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            playerEvents._onPlayerEnteredRoom.Invoke(newPlayer);
            vThirdPersonController you = GetYourPlayer();
            if (you != null)
            {
                Vector3 position = you.gameObject.transform.position;
                Quaternion rotation = you.gameObject.transform.rotation;
                you.gameObject.GetComponent<PhotonView>().RPC("SetPositionRotation", RpcTarget.Others, JsonUtility.ToJson(rotation), JsonUtility.ToJson(position));
                //TO DO: "you" also need to update visuals for currently equipped weapons
            }
            if (OnPlayerJoinedCurrentRoom != null) OnPlayerJoinedCurrentRoom(newPlayer);
            base.OnPlayerEnteredRoom(newPlayer);
        }

        /// <summary>
        /// Callback method. Called when a player leaves a room. Isn't called for yourself.
        /// </summary>
        /// <param name="otherPlayer">The Photon.Realtime.Player that just left</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            playerEvents._onPlayerLeftRoom.Invoke(otherPlayer);
            if (OnPlayerLeftCurrentRoom != null) OnPlayerLeftCurrentRoom(otherPlayer);
            base.OnPlayerLeftRoom(otherPlayer);
        }
        #endregion

        #region MasterClient Events
        /// <summary>
        /// Callback method. Called when you connect to the photon master server.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            _connecting = false;
            _connectStatus = "Connected to the master server.";
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            base.OnConnectedToMaster();
            otherEvents._onConnectedToMaster.Invoke();
            if (OnConnectedToMasterPhotonServer != null) OnConnectedToMasterPhotonServer.Invoke();
            if (_joinLobby == true)
            {
                JoinLobby(_targetLobbyType);
            }
            else if (_joinRoom == true)
            {
                if (PhotonNetwork.InLobby == false)
                {
                    JoinLobby(_targetLobbyType);
                }
                else if (_roomName == "")
                {
                    JoinRandomRoom();
                }
                else
                {
                    JoinRoom(_roomName);
                }
            }
            else if (_createRoom == true)
            {
                CreateRoom(_roomName);
            }
        }

        /// <summary>
        /// Callback method. Callwed when the photon master client is switched.
        /// </summary>
        /// <param name="newMasterClient">The Photon.Realtime.Player that is now the new master client</param>
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            if (debugging == true)
            {
                Debug.Log("Switched Master Client");
            }
            otherEvents._onMasterClientSwitched.Invoke();
            if (OnClientHostSwitched != null) OnClientHostSwitched.Invoke(newMasterClient);
        }
        #endregion

        #region Fails/Disconnects
        /// <summary>
        /// Callback method. Called when you disconnect from a photon room. Calls the 
        /// otherEvents._onDisconnected UnityEvent and OnDisconnectedFromPhoton delegate.
        /// </summary>
        /// <param name="cause">DisconnectCause with a basic error message.</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            _connecting = false;
            _joinLobby = false;
            _joinRoom = false;
            _createRoom = false;
            _connectStatus = "Disconnected: " + cause;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            if (_reconnectAttempt > connect_attempts && reconnect == true || reconnect == false)
            {
                _levelIndexToLoad = -1;
                _useGlobalNaming = true;
                reconnect = false;
                isReconnecting = false;
                _reconnectAttempt = 0;
                base.OnDisconnected(cause);
                otherEvents._onDisconnected.Invoke("Disconnected: " + cause.ToString());
                if (OnDisconnectedFromPhoton != null) OnDisconnectedFromPhoton.Invoke(cause);
            }
            else
            {
                _reconnectAttempt += 1;
                isReconnecting = true;
                _connectStatus = "Reconnecting...";
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                if (OnReconnectingToRoom != null) OnReconnectingToRoom.Invoke();
                roomEvents._onReconnect.Invoke();
                JoinOrCreateRoom(_roomName);
            }
        }

        /// <summary>
        /// Callback Method. Called when you fail to connect to a Photon Room/Lobby. 
        /// Calls the otherEvents._onConnectionFail UnityEvent and OnConnectionFailed
        /// delegate.
        /// </summary>
        /// <param name="cause">DisconnectCause with a basic error message.</param>
        public virtual void OnConnectionFail(DisconnectCause cause)
        {
            if (reconnect == false || reconnect == true && _reconnectAttempt > connect_attempts)
            {
                isReconnecting = false;
                _connecting = false;
                _createRoom = false;
                _joinRoom = false;
                _levelIndexToLoad = -1;
                _joinLobby = false;
                _useGlobalNaming = true;
                _connectStatus = "Failed to connect, reason: " + cause;
                if (debugging == true)
                {
                    Debug.Log(_connectStatus);
                }
                otherEvents._onConnectionFail.Invoke("Connection Failed: " + cause.ToString());
                if (OnConnectionFailed != null) OnConnectionFailed.Invoke(cause);
            }
            else
            {
                isReconnecting = true;
                _reconnectAttempt += 1;
                _connectStatus = "Reconnecting...";
                if (OnReconnectingToRoom != null) OnReconnectingToRoom.Invoke();
                roomEvents._onReconnect.Invoke();
                JoinOrCreateRoom(_roomName);
            }
        }

        /// <summary>
        /// Callback method. Called when you fail to connect to the photon master server.
        /// Calls the otherEvents._onFailedToConnectToPhoton UnityEvent and OnFailToConnectToPhoton
        /// delegate.
        /// </summary>
        /// <param name="cause">DisconnectCause with a basic error message.</param>
        public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            _connectStatus = "Failed to connect to the master server: " + cause;
            isReconnecting = false;
            _connecting = false;
            _createRoom = false;
            _joinRoom = false;
            _levelIndexToLoad = -1;
            _joinLobby = false;
            _useGlobalNaming = true;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            otherEvents._onFailedToConnectToPhoton.Invoke("Failed To Connect to Master Server: " + cause.ToString());
            if (OnFailToConnectToPhoton != null) OnFailToConnectToPhoton.Invoke(cause);
        }

        /// <summary>
        /// Callback method. Called when you fail to join a random photon room. 
        /// Calls the roomEvents._onJoinRoomFailed UnityEvent and OnJoinRoomFailure
        /// delegate.
        /// </summary>
        /// <param name="returnCode">The error code</param>
        /// <param name="message">Simple error message</param>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            _connecting = false;
            _createRoom = false;
            _joinRoom = false;
            _levelIndexToLoad = -1;
            _joinLobby = false;
            _connectStatus = "Failed to find a room, with error: (" + returnCode + ") " + message;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            base.OnJoinRandomFailed(returnCode, message);
            roomEvents._onJoinRoomFailed.Invoke("Failed to join room: "+returnCode.ToString()+" - "+message);
            if (OnJoinRoomFailure != null) OnJoinRoomFailure.Invoke(returnCode, message);
        }

        /// <summary>
        /// Callback method. Called when you fail to join a specified room. Calls the 
        /// OnJoinRoomFailure delegate and roomEvents._onJoinRoomFailed UnityEvent. 
        /// If the return code is 32758 (timeout) it will attempt to recreate the room.
        /// </summary>
        /// <param name="returnCode">The error code</param>
        /// <param name="message">Simple error message</param>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            _connecting = false;
            _joinRoom = false;
            _connectStatus = "Failed to join room: " + returnCode + " - " + message;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            if (returnCode.Equals(32758) && _levelIndexToLoad != -1)
            {
                _connectStatus = "Timeout seemed to have occured, attempting to re-create the room.";
                if (debugging == true) Debug.Log("Timeout seemed to have occured, attempting to re-create the room.");
                CreateRoom(GetIndividualRoomName());
            }
            else
            {
                _levelIndexToLoad = -1;
                roomEvents._onJoinRoomFailed.Invoke("Failed to join room: " + returnCode.ToString() + " - " + message);
            }
            base.OnJoinRoomFailed(returnCode, message);
            if (OnJoinRoomFailure != null) OnJoinRoomFailure.Invoke(returnCode, message);
        }

        /// <summary>
        /// Callback method. called when you fail to create a photon room. This also triggers the 
        /// roomEvents._onCreateRoomFailed UnityEvent and the OnCreateRoomFailure delegate.
        /// </summary>
        /// <param name="returnCode">The error code</param>
        /// <param name="message">Simple error message</param>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            _connecting = false;
            _createRoom = false;
            _connectStatus = "Failed to create a room: (" + returnCode + ") " + message;
            _useGlobalNaming = true;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            base.OnCreateRoomFailed(returnCode, message);
            if (OnCreateRoomFailure != null) OnCreateRoomFailure.Invoke(returnCode, message);
            if (_levelIndexToLoad != -1)
            {
                JoinRoom(_roomName);
            }
            else
            {
                roomEvents._onCreateRoomFailed.Invoke("Failed to create a room: " + returnCode.ToString() + " - " + message);
            }
        }
        #endregion

        #region Room Events
        /// <summary>
        /// Set the current photon room as open and joinable
        /// </summary>
        /// <param name="isOpen">True or false, Make the room open or not</param>
        public virtual void SetRoomIsOpen(bool isOpen)
        {
            PhotonNetwork.CurrentRoom.IsOpen = isOpen;
        }
        
        /// <summary>
        /// The current photon room will be listed or not
        /// </summary>
        /// <param name="isVisible">Room should be listed?</param>
        public virtual void SetRoomVisibility(bool isVisible)
        {
            PhotonNetwork.CurrentRoom.IsVisible = isVisible;
        }

        /// <summary>
        /// Subscribe to the ChatBox's data channel based on the GetGlobalRoomName
        /// function.
        /// </summary>
        protected virtual void JoinChatDataChannel()
        {
            if (_inDataChannel == false)
            {
                chatDataChannel = GetGlobalRoomName(PhotonNetwork.CurrentRoom.Name) + "_data";
                if (debugging == true) Debug.Log("CHATBOX - Join data channel: " + chatDataChannel);
                chatbox.SubscribeToChannel(chatDataChannel);
                if (OnJoinedChatDataChannel != null) OnJoinedChatDataChannel.Invoke(chatDataChannel);
            }
        }

        /// <summary>
        /// Callback method. Calls JoinChatDataChannel and NetworkInstantiatePrefab functions.
        /// Calls OnJoinedPhotonRoom delegate and roomEvents._onJoinedRoom UnityEvent.
        /// </summary>
        public override void OnJoinedRoom()
        {
            _connecting = false;
            _joinRoom = false;
            _useGlobalNaming = false;
            _reconnectAttempt = 0;
            isReconnecting = false;
            _connectStatus = "Successfully joined a room: \""+_roomName+"\"";
            JoinChatDataChannel();
            if (_levelIndexToLoad != -1)
            {
                PhotonNetwork.LoadLevel(_levelIndexToLoad);
            }
            else if (playerPrefab != null && autoSpawnPlayer == true)
            {
                Transform spawnPoint;
                if (string.IsNullOrEmpty(teamName))
                {
                    spawnPoint = GetRandomSpawnPoint();
                }
                else
                {
                    spawnPoint = GetTeamSpawnPoint(teamName);
                }
                if (isReconnecting && spawnAtSaved)
                {
                    NetworkInstantiatePrefab(playerPrefab.name, spawnAtLoc, spawnAtRot, 0);
                }
                else
                {
                    NetworkInstantiatePrefab(playerPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);
                }
            }
            roomEvents._onJoinedRoom.Invoke();
            _inRoom = true;
            base.OnJoinedRoom();
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            if (OnJoinedPhotonRoom != null) OnJoinedPhotonRoom.Invoke();
        }

        /// <summary>
        /// Callback method. Called when you have left a photon room. Calls the JoinRoom 
        /// function or CreateRoom function if switching Unity scenes. Calls 
        /// roomEvents._onLeftRoom UnityEvent and OnLeftPhotonRoom delegate.
        /// </summary>
        public override void OnLeftRoom()
        {
            _connecting = false;
            _joinRoom = false;
            _connectStatus = "Left Room: "+_roomName;
            roomEvents._onLeftRoom.Invoke();
            base.OnLeftRoom();
            if (OnLeftPhotonRoom != null) OnLeftPhotonRoom.Invoke();
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            _inRoom = false;
            if (_levelIndexToLoad != -1)
            {
                if (cachedRoomList.ContainsKey(_roomName))
                {
                    JoinRoom(_roomName);
                }
                else
                {
                    CreateRoom(_roomName, new RoomOptions() { MaxPlayers = maxPlayerPerRoom, PublishUserId = true, IsVisible = false, IsOpen = true });
                }
            }
        }

        /// <summary>
        /// Callback method. Called when you have successfully create a photon room. Calls
        /// JoinChatDataChannel function, roomEvents._OnCreatedRoom UnityEvent, 
        /// and OnCreatedPhotonRoom delegate.
        /// </summary>
        public override void OnCreatedRoom()
        {
            _connecting = false;
            _createRoom = false;
            _connectStatus = "Successfully created room \"" + _roomName +"\"";
            JoinChatDataChannel();
            base.OnCreatedRoom();
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            roomEvents._OnCreatedRoom.Invoke();
            if (_useGlobalNaming == true) _useGlobalNaming = false;
            if (OnCreatedPhotonRoom != null) OnCreatedPhotonRoom.Invoke();
        }

        /// <summary>
        /// Callback method. Called when in a photon lobby and the number of 
        /// photon rooms changes. Calls UpdateCachedRoomList.
        /// </summary>
        /// <param name="roomList"></param>
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            if (debugging == true)
            {
                Debug.Log("Updated room list, count: " + roomList.Count);
            }
            UpdateCachedRoomList(roomList);
        }
        #endregion

        #region Lobby Events
        /// <summary>
        /// Callback method. Called when successfully joined the photon lobby.
        /// Calls lobbyEvents._onJoinedLobby UnityEvent and OnJoinedPhotonLobby 
        /// delegate. Will also call JoinRandomRoom, JoinRoom, or CreateRoom based 
        /// on if you're switching UnityScenes and attempting to join a Photon 
        /// room or not.
        /// </summary>
        public override void OnJoinedLobby()
        {
            _connecting = false;
            _joinLobby = false;
            _connectStatus = "Succesfully joined the server lobby: DefaultLobby=" + PhotonNetwork.CurrentLobby.IsDefault;
            if (debugging == true)
            {
                Debug.Log(_connectStatus);
            }
            base.OnJoinedLobby();
            lobbyEvents._onJoinedLobby.Invoke();
            if (OnJoinedPhotonLobby != null) OnJoinedPhotonLobby.Invoke();
            if (_joinRoom == true)
            {
                if (_roomName == "")
                {
                    JoinRandomRoom();
                }
                else
                {
                    JoinRoom(_roomName);
                }
            }
            else if (_createRoom == true && isReconnecting == false)
            {
                CreateRoom(_roomName);
            }
            else if (isReconnecting == true)
            {
                JoinOrCreateRoom(_roomName);
            }
        }

        /// <summary>
        /// Callback method. Called when you have succesfully left the Photon Lobby. 
        /// Calls lobbyEvents._onLeftLobby UnityEvent and OnLeftPhotonLobby delegate.
        /// </summary>
        public override void OnLeftLobby()
        {
            _connectStatus = "Left Lobby.";
            _joinLobby = false;
            base.OnLeftLobby();
            lobbyEvents._onLeftLobby.Invoke();
            if (OnLeftPhotonLobby != null) OnLeftPhotonLobby.Invoke();
        }
        #endregion
        #endregion

        #region Scene Persistance Logic
        /// <summary>
        /// Triggers the WaitForReplay IEnumerator which triggers DoSceneReplay function.
        /// </summary>
        /// <param name="createsOnly">Only perform the creates actions? (ex: Dropped items)</param>
        /// <param name="updatesOnly">Only perform the updates actions? (ex: opened doors/pressed buttons)</param>
        public virtual void ReplaySceneDatabase(bool createsOnly = false, bool updatesOnly = false)
        {
            if (_playedSceneDatabase == true) return;
            _playedSceneDatabase = true;
            StartCoroutine(WaitForReplay(createsOnly, updatesOnly));
        }
        IEnumerator WaitForReplay(bool createsOnly = false, bool updatesOnly = false)
        {
            if (_levelIndexToLoad == -1) yield return null;
            yield return new WaitUntil(() => _levelIndexToLoad == SceneManager.GetActiveScene().buildIndex);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            DoSceneReplay(createsOnly, updatesOnly);
        }

        /// <summary>
        /// Performs all the actions on this scene that have been stored in the scene database. Actions
        /// are received via the Chatbox's data channel. When someone opens a door, presses a button, 
        /// drops an item, picks up an item, etc. In another scene that information is recieved by everyone 
        /// in the session via the ChatBox's data channel. This function is responsible to replay all 
        /// of those actions for a particular scene when you enter the unity scene. That way it is in 
        /// sync with all of the other players that are currently already in the scene. This also deals 
        /// with keeping Unity scenes persistant between loads.
        /// </summary>
        /// <param name="createsOnly">Only perform the creates actions? (ex: Dropped items)</param>
        /// <param name="updatesOnly">Only perform the updates actions? (ex: opened doors/pressed buttons)</param>
        private void DoSceneReplay(bool createsOnly = false, bool updatesOnly = false)
        {
            _lockModifySceneDatabase = true;
            int levelIndex = (_levelIndexToLoad != -1) ? _levelIndexToLoad : SceneManager.GetActiveScene().buildIndex;
            if (!_modifySceneDatabase.ContainsKey(database.storedScenesData.Find(x => x.index == levelIndex).name))
            {
                _lockModifySceneDatabase = false;
                return;
            }

            bool _itemCreated = false;
            foreach (ObjectAction item in _modifySceneDatabase[database.storedScenesData.Find(x => x.index == levelIndex).name])
            {
                foreach (GameObject target in Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Replace("(Clone)","") == item.name.Replace("(Clone)","")))
                {
                    if (target.transform.position.Round() == item.position.Round())
                    {
                        if (item.action == ObjectActionEnum.Update && createsOnly == false)
                        {
                            try
                            {
                                if (debugging == true) Debug.Log("Sending \"" + item.methodToCall + "\" command to: " + item.name + ", at position: " + item.position);
                                if (item.methodArgs != null && item.methodArgs.Length > 0)
                                {
                                    target.BroadcastMessage(item.methodToCall, item.methodArgs);
                                }
                                else
                                {
                                    target.BroadcastMessage(item.methodToCall);
                                }
                                break;
                            }
                            catch
                            {
                                break;
                            }
                        }
                        else if (updatesOnly == false && item.action == ObjectActionEnum.Create && PhotonNetwork.IsMasterClient == true)
                        {
                            if (item.methodToCall == null && item.methodArgs != null &&
                                target.GetComponent<vItemCollection>() && target.GetComponent<SyncItemCollection>() &&
                                !ItemListTheSame(target.GetComponent<vItemCollection>().items, target.GetComponent<SyncItemCollection>().ConvertBackToItemRefs(item.methodArgs)))
                            {
                                if (debugging == true) Debug.Log("Creating object in scene: " + item.name + ", at position: " + item.position);
                                _itemCreated = true;
                                break;
                            }
                        }
                    }
                }
                if (updatesOnly == false && item.action == ObjectActionEnum.Create && PhotonNetwork.IsMasterClient == true && _itemCreated == false)
                {
                    CreateItem(item);
                }
                _itemCreated = false;
            }
            _lockModifySceneDatabase = false;
        }

        /// <summary>
        /// Used by the DoSceneReplay funcion only, do not modify. This is responsible for creating 
        /// items in the Unity scene.
        /// </summary>
        /// <param name="item">The ObjectAction item to create.</param>
        private void CreateItem(ObjectAction item)
        {
            object[] data = (string.IsNullOrEmpty(item.methodToCall)) ? BuildDataArray(item.methodArgs) : null;
            GameObject instantiatedObject = NetworkInstantiatePersistantPrefab(item.resourcePrefab, item.position, Quaternion.identity, 0, data);
            if (!string.IsNullOrEmpty(item.methodToCall))
            {
                if (item.methodArgs != null)
                {
                    if (debugging == true) Debug.Log("Calling method with " + item.methodArgs.Length + " arguments: " + item.methodToCall);
                    instantiatedObject.BroadcastMessage(item.methodToCall, item.methodArgs);
                }
                else
                {
                    if (debugging == true) Debug.Log("Calling method without arguments: " + item.methodToCall);
                    instantiatedObject.BroadcastMessage(item.methodToCall);
                }
            }
        }

        /// <summary>
        /// Used by the DoSceneReplay function only, do not modify. This is responsible for building the information array.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private object[] BuildDataArray(string[] args = null)
        {
            object[] data;
            if (args != null && args.Length > 0)
            {
                List<object> temp = new List<object>();
                for (int i = 0; i < args.Length; i++)
                {
                    temp.Add(args[i]);
                }
                data = temp.ToArray();
            }
            else
            {
                data = null;
            }
            return data;
        }

        /// <summary>
        /// The is used by the DoSceneReplay function, do not modify. This is used to make sure that the 
        /// item list on a particular item is the same for the entering player.
        /// </summary>
        /// <param name="original">The current item list</param>
        /// <param name="incoming">The proposed update list</param>
        /// <returns>True or False if the item list is currently the same.</returns>
        private bool ItemListTheSame(List<ItemReference> original, List<ItemReference> incoming)
        {
            bool same = true;
            foreach(ItemReference item in original)
            {
                if (incoming.Find(x => x.id == item.id) == null)
                {
                    same = false;
                    break;
                }
                else
                {
                    incoming.Remove(incoming.Find(x => x.id == item.id));
                }
            }
            return same;
        }

        /// <summary>
        /// This is used to add/remove actions to be performed by the DoSceneReplay function. The scene database
        /// is responsible for playing actions for a player that is entering a Unity scene for the first time.
        /// This function will add or remove actions to the scene database.
        /// </summary>
        /// <param name="hashData">The data that defines what action to do and if to remove or add it</param>
        public virtual void UpdateSceneDatabase(ObjectAction hashData)
        {
            if (debugging == true) Debug.Log("Recieved update action type: " + hashData.action);
            if (_lockModifySceneDatabase == true)
            {
                if (debugging == true) Debug.Log("Modify Scene Database locked, skipping add.");
                return;
            }
            if (!_modifySceneDatabase.ContainsKey(hashData.sceneName))
            {
                if (debugging == true) Debug.Log("Database added new scene: " + hashData.sceneName);
                _modifySceneDatabase.Add(hashData.sceneName, new List<ObjectAction>());
            }
            if (hashData.action != ObjectActionEnum.Delete)
            { 
                if (_modifySceneDatabase[hashData.sceneName].Find(x => x.position.Round() == hashData.position.Round() && x.name == hashData.name.Replace("(Clone)", "") && x.action == hashData.action) == null)
                {
                    if (debugging == true) Debug.Log("Adding - (SceneName: " + hashData.sceneName + ", ItemName:" + hashData.name + ", Position:" + hashData.position + ", Method: "+hashData.methodToCall+", Arguments Count: "+hashData.methodArgs.Length + ",  Action: " + hashData.action);
                    _modifySceneDatabase[hashData.sceneName].Add(hashData);
                }
            }
            else if (hashData.action == ObjectActionEnum.Delete)
            {
                ObjectAction foundAction = _modifySceneDatabase[hashData.sceneName].Find(x =>
                x.name == hashData.name &&
                x.position == hashData.position &&
                x.action == ObjectActionEnum.Create);
                if (foundAction != null)
                {
                    if (debugging == true) Debug.Log("Removing - (SceneName: " + hashData.sceneName + ", ItemName:" + hashData.name + ", Position:" + hashData.position + ", Method: " + hashData.methodToCall + ", Arguments Count: " + hashData.methodArgs.Length + ",  Action: " + hashData.action);
                    _modifySceneDatabase[hashData.sceneName].Remove(foundAction);
                }
                else
                {
                    Debug.Log("Item doesnt exists in the scenes database, no reason to perform a delete, skipping...");
                }
            }
            else if (debugging == true)
            {
                Debug.Log("Item already exists in the scenes database, skipping...");
            }
        }
        #endregion

        #region Debugging
        private void OnGUI()
        {
            if (displayDebugWindow == true)
            {
                GUI.Box(new Rect(10, 10, 250, 200), "Network Manager Settings");

                //Connection Status
                GUI.Label(new Rect(12, 25, 200, 25), "Connection Status:");
                GUI.Label(new Rect(200, 25, 400, 50), _connectStatus);

                //Is In Room
                GUI.Label(new Rect(12, 50, 200, 25), "Is In Room");
                GUI.Label(new Rect(200, 50, 100, 25), IsInRoom().ToString());
                
                //Room Name
                GUI.Label(new Rect(12, 75, 200, 25), "Room Name");
                GUI.Label(new Rect(200, 75, 400, 50), _roomName);

                //Is In Lobby
                GUI.Label(new Rect(12, 100, 200, 25), "Is In Lobby");
                GUI.Label(new Rect(200, 100, 100, 25), PhotonNetwork.InLobby.ToString());

                //Connected To Master Server
                GUI.Label(new Rect(12, 125, 200, 25), "Connected To Master Server");
                GUI.Label(new Rect(200, 125, 100, 25), PhotonNetwork.IsConnected.ToString());

                //Connected To Master Server
                GUI.Label(new Rect(12, 150, 200, 25), "# Of Players In Room");
                GUI.Label(new Rect(200, 150, 100, 25), PhotonNetwork.PlayerList.Length.ToString());

                //Is Master Client
                GUI.Label(new Rect(12, 175, 200, 25), "Is Master Client");
                GUI.Label(new Rect(200, 175, 100, 25), PhotonNetwork.IsMasterClient.ToString());
            }
        }
        #endregion
    }
}