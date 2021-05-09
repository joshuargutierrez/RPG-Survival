using CBGames.Core;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CBGames.Core.CoreDelegates;

namespace CBGames.UI
{
    [Serializable]
    public class SceneOption
    {
        public string sceneName = "";
        public Sprite sceneSprite = null;

        public SceneOption(string inputName, Sprite inputSprite)
        {
            this.sceneName = inputName;
            this.sceneSprite = inputSprite;
        }
    }
    [Serializable]
    public class RoomListWrapper
    {
        public List<SceneOption> wrapper = new List<SceneOption>();
    }

    [AddComponentMenu("CB GAMES/Core/UI Core Logic")]
    public partial class UICoreLogic : MonoBehaviour
    {
        #region Delegates
        [Tooltip("Delegate. Called when any player on a team is added or removed.")]
        public BasicDelegate teamsUpdated;
        [Tooltip("Delegate. Called when the voice view is updated.")]
        public BasicDelegate voiceViewUpdated;
        #endregion

        #region Properties
        #region Session Settings
        [Tooltip("If you don't set the scene to load this is the default level index it will use.")]
        [SerializeField] protected int defaultLevelIndex = 0;
        [Tooltip("The complete list of players that are selectable to the end user.")]
        public GameObject[] selectablePlayers = new GameObject[] { };
        [Tooltip("UnityEvent. Called only once in the `OnStart` method of this component.")]
        public UnityEvent OnStart = new UnityEvent();
        [Tooltip("UnityEvent. Called everytime when a unity scene is first loaded.")]
        public SceneEvent OnSceneLoaded = new SceneEvent();
        #endregion

        #region Sound Options
        [Tooltip("The audio source that will play your menu music.")]
        [SerializeField] protected AudioSource musicSource = null;
        [Tooltip("The audio source that will play your menu sounds.")]
        [SerializeField] protected AudioSource soundSource = null;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseEnter\" function.")]
        [SerializeField] protected AudioClip mouseEnter = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"mouseEnter\" sound.")]
        [SerializeField] protected float mouseEnterVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseExit\" function.")]
        [SerializeField] protected AudioClip mouseExit = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"mouseExit\" sound.")]
        [SerializeField] protected float mouseExitVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseClick\" function.")]
        [SerializeField] protected AudioClip mouseClick = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"mouseClick\" sound.")]
        [SerializeField] protected float mouseClickVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerFinalClick\" function.")]
        [SerializeField] protected AudioClip finalClick = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"finalClick\" sound.")]
        [SerializeField] protected float finalClickVolume = 0.5f;
        [Tooltip("How loud to set your audio to.")]
        [SerializeField] protected float startVolume = 0.5f;
        [Tooltip("When first loading the UI, fade in the audio.")]
        [SerializeField] protected bool fadeInAudio = false;
        [Tooltip("If fading out your music how to fast to fade out. Higher values will fade out faster.")]
        [Range(0.01f, 5.0f)]
        [SerializeField] protected float fadeInSpeed = 0.25f;
        #endregion

        #region Player Settings
        [Tooltip("UnityEvent. Called when entering a name for your player fails.")]
        public StringUnityEvent OnNameEnterFailed;
        [Tooltip("UnityEvent. Called when entering a name for your player succeeds.")]
        public UnityEvent OnNameEnterSuccess;
        #endregion

        #region Room Settings
        [Tooltip("A list of `SceneOptions` to choose from when you want to implement " +
            "a way for players to select the scene they would like to join.")]
        [SerializeField] public List<SceneOption> sceneList = new List<SceneOption>();
        [Tooltip("UnityEvent. Called when creating a photon room fails.")]
        public StringUnityEvent OnCreateRoomFailed;
        [Tooltip("UnityEvent. Called when creating a photon room succeeds.")]
        public UnityEvent OnCreateRoomSuccess;
        [Tooltip("UnityEvent. Called right after you attempt to join the photon lobby.")]
        public UnityEvent OnWaitToJoinPhotonRoomsLobby;
        [Tooltip("Called when you successfully create a new session.")]
        public UnityEvent OnStartSession;
        #endregion

        #region Generic Network
        [Tooltip("UnityEvent. Called when you were disconnected from the photon server and" +
            "are now attempting to reconnect to the last room you were in.")]
        public UnityEvent OnReconnecting;
        [Tooltip("UnityEvent. Called when you receive a network error.")]
        public StringUnityEvent OnNetworkError;
        [Tooltip("Log everything that happens to the unity console.")]
        [SerializeField] protected bool debugging = false;
        #endregion

        #region Loading Page
        [Tooltip("The Parent Object that holds all of the loading page transforms.")]
        public GameObject loadingParent = null;
        [Tooltip("The image that will display when loading the screen.")]
        public List<Sprite> loadingImages = new List<Sprite>();
        [Tooltip("How long to display each image before fading to the next one.")]
        [SerializeField] protected float loadingImageDisplayTime = 8.0f;
        [Tooltip("How fast to fade the images in and out.")]
        [SerializeField] protected float loadingPageFadeSpeed = 0.25f;
        [Tooltip("The title text that will be set when the loading screen is displayed")]
        public string loadingTitle = "";
        [Tooltip("The description text that will be set when the loading screen is displayed.")]
        public List<string> loadingDesc = new List<string>();
        [Tooltip("The image that will be displaying your main loading sprite")]
        [SerializeField] protected Image mainLoadingImage = null;
        [Tooltip("The text object that will be used to display your loading title text.")]
        [SerializeField] protected Text loadingTitleText = null;
        [Tooltip("The text object that will be used to display your loading description text.")]
        [SerializeField] protected Text loadingDescText = null;
        [Tooltip("The loading bar.")]
        [SerializeField] protected Image loadingBar = null;
        [Tooltip("UnityEvent. Called when you have just started loading a unity scene.")]
        public UnityEvent OnStartLoading;
        [Tooltip("UnityEvent. Called when you have completely loaded a unity scene.")]
        public UnityEvent OnCompleteLevelLoading;
        #endregion

        #region Countdown
        [Tooltip("UnityEvent. Called when you receive a start countdown PhotonEvent.")]
        public FloatUnityEvent OnCountdownStarted;
        [Tooltip("UnityEvent. Called when you receive a stop countdown PhotonEvent.")]
        public UnityEvent OnCountdownStopped;
        #endregion

        #region Misc
        [Tooltip("UnityEvent. Called when you receive an update round time PhotonEvent.")]
        public FloatUnityEvent OnReceiveRoundTime;
        [Tooltip("UnityEvent. Called when you want to reset the UI back to it's default state.")]
        public UnityEvent OnResetEverything;
        #endregion

        #region Internal Only
        protected bool _roomNameChecking = true;
        protected bool _fadeOutAudio = false;
        protected bool _trackLoadingBar = false;
        protected bool _cycleLoadingPage = false;
        protected bool _loadFading = false;
        protected bool _loadIsFadingOut = false;
        protected string _playerName = "";
        protected string _roomName = "";
        protected string _roomPassword = "";
        protected Dictionary<string, RoomInfo> _rooms = new Dictionary<string, RoomInfo>();
        protected Dictionary<string, bool> _playersReady = new Dictionary<string, bool>();
        protected Dictionary<string, string> _teamData = new Dictionary<string, string>();
        protected Dictionary<string, int> _playerVoiceChatViews = new Dictionary<string, int>();
        protected Dictionary<int, int> _sceneVotes = new Dictionary<int, int>();
        protected int _myPrevVote = -1;
        protected List<SceneOption> _randomSceneList = new List<SceneOption>();
        protected string _savedLevelName = "";
        protected int _savedLevelIndex = 0;
        protected int _selectedLoadImageIndex = 0;
        protected int _selectedDescTextIndex = 0;
        protected int _maxVote = 0;
        protected int _setPlayerIndex = 0;
        protected List<GameObject> _playerUIs = new List<GameObject>();
        protected float _loadingFadeTimer = 0.0f;
        protected Color _tempLoadColor = new Color();
        protected Color _tempLoadTextColor = new Color();
        #endregion

        #region Editor Only
        [HideInInspector] public bool e_events_oneTime = false;
        [HideInInspector] public bool e_events_loading = false;
        [HideInInspector] public bool e_events_naming = false;
        [HideInInspector] public bool e_events_errors = false;
        [HideInInspector] public bool e_events_room = false;
        [HideInInspector] public bool e_show_events = false;
        [HideInInspector] public bool e_show_loading = false;
        [HideInInspector] public bool e_show_audio = false;
        [HideInInspector] public bool e_show_core = false;
        [HideInInspector] public bool e_show_countdown = false;
        [HideInInspector] public bool e_show_misc = false;
        #endregion
        #endregion

        #region Initializations
        /// <summary>
        /// Sets the music volume, Adds the `AddNewPlayerToPlayerReadyList` and 
        /// `RemovePlayerToPlayerReadyList` functions to the `OnPlayerJoinedCurrentRoom` and
        /// `OnPlayerLeftCurrentRoom` delegates of the `NetworkManager` component. Also sets
        /// up the `SceneLoaded` function to be called when a new scene is loaded.
        /// </summary>
        protected virtual void Start()
        {
            musicSource.volume = (fadeInAudio == false) ? startVolume : 0;
            _savedLevelIndex = defaultLevelIndex;
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            _savedLevelName = sceneData.Find(x => x.index == _savedLevelIndex).sceneName;
            OnStart.Invoke();
            PhotonNetwork.NetworkingClient.EventReceived += RecievedPhotonEvent;
            SceneManager.sceneLoaded += SceneLoaded;
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom += AddNewPlayerToPlayerReadyList;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom += RemovePlayerToPlayerReadyList;
            NetworkManager.networkManager.OnReconnectingToRoom += ReconnectingToLastRoom;
            CreateRandomSceneList();
        }

        protected virtual void OnDestroy()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= RecievedPhotonEvent;
            SceneManager.sceneLoaded -= SceneLoaded;
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom -= AddNewPlayerToPlayerReadyList;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom -= RemovePlayerToPlayerReadyList;
            NetworkManager.networkManager.OnReconnectingToRoom -= ReconnectingToLastRoom;
        }


        /// <summary>
        /// Called when you want to reset the UI back to its default state. Will clear all
        /// parameters and attempt to reset this component back to its starting values. Also
        /// calls `OnResetEverything` UnityEvent for additional user-defined actions.
        /// </summary>
        public virtual void ResetEverything()
        {
            if (debugging == true) Debug.Log("Resetting all data...");
            _roomName = "";
            _roomPassword = "";
            _rooms.Clear();
            _playersReady.Clear();
            _teamData.Clear();
            _sceneVotes.Clear();
            _playersReady.Clear();
            _randomSceneList.Clear();
            _playerVoiceChatViews.Clear();
            _savedLevelName = "";
            _savedLevelIndex = 0;
            _selectedLoadImageIndex = 0;
            _selectedDescTextIndex = 0;
            _maxVote = 0;
            _playerUIs.Clear();
            SetPlayer(0);
            SceneManager.sceneLoaded -= SceneLoaded;
            PhotonNetwork.NetworkingClient.EventReceived -= RecievedPhotonEvent;
            NetworkManager.networkManager.OnPlayerJoinedCurrentRoom -= AddNewPlayerToPlayerReadyList;
            NetworkManager.networkManager.OnPlayerLeftCurrentRoom -= RemovePlayerToPlayerReadyList;
            NetworkManager.networkManager.OnReconnectingToRoom -= ReconnectingToLastRoom;
            OnResetEverything.Invoke();
            Start();
        }
        #endregion

        #region Heartbeat
        /// <summary>
        /// Controls music volumes, loading bars, load images, loading titles,
        /// and loading descriptions.
        /// </summary>
        protected virtual void Update()
        {
            if (fadeInAudio == true)
            {
                musicSource.volume += Time.deltaTime * fadeInSpeed;
                if (musicSource.volume >= startVolume)
                {
                    musicSource.volume = startVolume;
                    fadeInAudio = false;
                }
            }
            if (_fadeOutAudio == true)
            {
                musicSource.volume -= Time.deltaTime * fadeInSpeed;
                if (musicSource.volume <= 0)
                {
                    musicSource.volume = 0;
                    _fadeOutAudio = false;
                }
            }
            if (_trackLoadingBar == true)
            {
                loadingBar.fillAmount = PhotonNetwork.LevelLoadingProgress;
                if (PhotonNetwork.LevelLoadingProgress == 1)
                {
                    if (debugging == true) Debug.Log("Completed Leveling Loading.");
                    OnCompleteLevelLoading.Invoke();
                    EnableLoadingPage(false);
                }
            }
            if (_cycleLoadingPage == true)
            {
                if (_loadFading == false)
                {
                    _loadingFadeTimer += Time.deltaTime;
                    if (_loadingFadeTimer >= loadingImageDisplayTime)
                    {
                        _loadIsFadingOut = (mainLoadingImage.color.a == 0) ? false : true;
                        _loadFading = true;
                    }
                }
                if (_loadFading == true)
                {
                    _tempLoadColor = mainLoadingImage.color;
                    _tempLoadTextColor = loadingDescText.color;
                    if (_loadIsFadingOut == false)
                    {
                        _tempLoadColor.a += Time.deltaTime * loadingPageFadeSpeed;
                        _tempLoadTextColor.a += Time.deltaTime * loadingPageFadeSpeed;
                    }
                    else
                    {
                        _tempLoadColor.a -= Time.deltaTime * loadingPageFadeSpeed;
                        _tempLoadTextColor.a -= Time.deltaTime * loadingPageFadeSpeed;
                    }
                    mainLoadingImage.color = _tempLoadColor;
                    loadingDescText.color = _tempLoadTextColor;
                    if (_tempLoadColor.a <= 0 || _tempLoadColor.a >= 1)
                    {
                        _loadFading = false;
                        _tempLoadColor.a = (_tempLoadColor.a <= 0) ? 0 : 1;
                        mainLoadingImage.color = _tempLoadColor;
                        if (mainLoadingImage.color.a == 0)
                        {
                            _selectedLoadImageIndex = (loadingImages.Count <= (_selectedLoadImageIndex + 1)) ? 0 : _selectedLoadImageIndex + 1;
                            mainLoadingImage.sprite = loadingImages[_selectedLoadImageIndex];

                            _selectedDescTextIndex = (loadingDesc.Count <= (_selectedDescTextIndex + 1)) ? 0 : _selectedDescTextIndex + 1;
                            loadingDescText.text = loadingDesc[_selectedDescTextIndex];
                        }
                    }
                }
            }
        }
        #endregion

        #region Sound/Music Events
        /// <summary>
        /// Will start the audio clip from the beginning again on the `musicSource`.
        /// </summary>
        public virtual void RestartMusic()
        {
            musicSource.Stop();
            musicSource.Play();
        }

        /// <summary>
        /// Stops playing the audio clip on the `musicSource`
        /// </summary>
        public virtual void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Starts playing the audio clip on the `musicSource`.
        /// </summary>
        public virtual void PlayMusic()
        {
            musicSource.Play();
        }

        /// <summary>
        /// Disables/Enables the `musicSource` component based on the input value.
        /// </summary>
        /// <param name="isEnabled">bool type, Enable or Disable `musicSource` component.</param>
        public virtual void EnableMusic(bool isEnabled)
        {
            if (debugging == true) Debug.Log("Enable Music: " + isEnabled);
            musicSource.enabled = isEnabled;
        }

        /// <summary>
        /// Fade the `musicSource` in or out based on the input value. True = Fade Out, False = Fade In
        /// </summary>
        /// <param name="fadeOut">bool type, fade music in or out?</param>
        public virtual void FadeMusic(bool fadeOut)
        {
            if (debugging == true) Debug.Log("Fade music: " + fadeOut);
            if (musicSource.volume < startVolume && fadeOut == false)
            {
                fadeInAudio = true;
                _fadeOutAudio = false;
            }
            else if (musicSource.volume > 0 && fadeOut == true)
            {
                fadeInAudio = false;
                _fadeOutAudio = true;
            }
        }

        /// <summary>
        /// Set the music to fade to the selected volume next time you trigger a fade music.
        /// </summary>
        /// <param name="fadeToVolume">float type, the volume to set the music to fade to</param>
        public virtual void SetFadeToVolume(float fadeToVolume)
        {
            startVolume = fadeToVolume;
        }

        /// <summary>
        /// Play the `mouseEnter` audio clip from the `soundSource`
        /// </summary>
        public virtual void PlayMouseEnter()
        {
            if (soundSource == null || mouseEnter == null) return;
            soundSource.clip = mouseEnter;
            soundSource.volume = mouseEnterVolume;
            soundSource.Play();
        }

        /// <summary>
        /// Play the `mouseExit` audio clip from the `soundSource`
        /// </summary>
        public virtual void PlayMouseExit()
        {
            if (soundSource == null || mouseExit == null) return;
            soundSource.clip = mouseExit;
            soundSource.volume = mouseExitVolume;
            soundSource.Play();
        }

        /// <summary>
        /// Play the `mouseClick` audio clip from the `soundSource`
        /// </summary>
        public virtual void PlayMouseClick()
        {
            if (soundSource == null || mouseClick == null) return;
            soundSource.clip = mouseClick;
            soundSource.volume = mouseClickVolume;
            soundSource.Play();
        }

        /// <summary>
        /// Play the `finalClick` audio clip from the `soundSource`
        /// </summary>
        public virtual void PlayFinalClick()
        {
            if (soundSource == null || finalClick == null) return;
            soundSource.clip = finalClick;
            soundSource.volume = finalClickVolume;
            soundSource.Play();
        }

        /// <summary>
        /// Set the `musicSource` loop value.
        /// </summary>
        /// <param name="setLooping">bool type, loop the `musicSource` or not?</param>
        public virtual void LoopMusic(bool setLooping)
        {
            musicSource.loop = setLooping;
        }

        /// <summary>
        /// Immediately set the `musicSource` volume level.
        /// </summary>
        /// <param name="volume">float type, the volume level to set the music to</param>
        public virtual void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
        }

        /// <summary>
        /// Set the `musicSource` audio clip.
        /// </summary>
        /// <param name="clip">AudioClip type, the clip to set the `musicSource` to.</param>
        public virtual void SetMusicAudio(AudioClip clip)
        {
            musicSource.clip = clip;
        }
        #endregion

        #region Player Settings
        #region Player - Kick
        /// <summary>
        /// Calls the `VisualKickPlayer` IEnumerator wit the `userId`.
        /// </summary>
        /// <param name="userId">string type, the photon user id to boot from the photon room.</param>
        public virtual void KickPlayer(string userId)
        {
            StartCoroutine(VisualKickPlayer(userId));
        }

        /// <summary>
        /// Calls the `CB_EVENT_KICKPLAYER` PhotonEvent. Which will force the owner of photon user id
        /// to leave the photon room.
        /// </summary>
        /// <param name="userId">string type, the owner's user id to boot from the photon room.</param>
        IEnumerator VisualKickPlayer(string userId)
        {
            object[] data = new object[] { PhotonNetwork.LocalPlayer.UserId };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_KICKPLAYER photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_KICKPLAYER, data, options, SendOptions.SendReliable);
            yield return new WaitForSeconds(0.1f);
            NetworkManager.networkManager.KickPlayer(userId);
        }
        #endregion

        #region Player - Set PlayerPrefab
        /// <summary>
        /// Get the `playerPrefab` from the network manager.
        /// </summary>
        /// <returns>GameObject of the player that you have set in the `NetworkManager`</returns>
        public virtual GameObject GetSetPlayer()
        {
            return NetworkManager.networkManager.playerPrefab;
        }

        /// <summary>
        /// Returns the current selected player index that will be used to select a player later.
        /// </summary>
        /// <returns></returns>
        public virtual int GetSetPlayerIndex()
        {
            return _setPlayerIndex;
        }

        /// <summary>
        /// Will set the player according to the input index. Will choose the player to set as the 
        /// `playerPrefab` in the NetworkManager from the `selectablePlayers` list.
        /// </summary>
        /// <param name="index">int type, the index to choose from the `selectablePlayers` list.</param>
        public virtual void SetPlayer(int index)
        {
            _setPlayerIndex = index;
            if (debugging == true) Debug.Log("Set Network Manager player prefab from index: " + index);
            if (index < 0 || index >= selectablePlayers.Length) return;
            NetworkManager.networkManager.playerPrefab = selectablePlayers[index];
        }
        #endregion

        #region Player - Name
        /// <summary>
        /// Calls the `OnNameEnterFailed` or `OnNameEnterSuccess` UnityEvents if the player name
        /// that was saved previously passes the tests or not. If it passes the tests it will 
        /// send this player name to the NetworkManager's `SetPlayerName` function.
        /// </summary>
        public virtual void SubmitSavedPlayerName()
        {
            if (debugging == true) Debug.Log("Attempting to set the saved players network name: " + _playerName);
            if (string.IsNullOrEmpty(_playerName))
            {
                OnNameEnterFailed.Invoke("You must enter a name to continue.");
            }
            else if (_playerName.Contains(":"))
            {
                OnNameEnterFailed.Invoke("Name contains ':' which is not allowed.");
            }
            else if (_playerName.Contains("_"))
            {
                OnNameEnterFailed.Invoke("Name contains '_' which is not allowed.");
            }
            else
            {
                NetworkManager.networkManager.SetPlayerName(_playerName);
                OnNameEnterSuccess.Invoke();
            }
        }

        /// <summary>
        /// Saves the input string as a future player name that will be used by the \
        /// `SubmitSavedPlayerName` function.
        /// </summary>
        /// <param name="playerName">string type, the player name to potentially use.</param>
        public virtual void SavePlayerName(string playerName)
        {
            if (debugging == true) Debug.Log("Save player name: " + playerName);
            _playerName = playerName;
        }
        #endregion

        #region Player - Team
        /// <summary>
        /// Calls the `CB_EVENT_TEAMCHANGE` PhotonEvent. This will set your photon user id
        /// to be on the team name that matches the input value.
        /// </summary>
        /// <param name="teamName">string type, what team to join.</param>
        public virtual void SetMyTeamName(string teamName = "")
        {
            if (debugging == true) Debug.Log("Setting My Team Name...");
            NetworkManager.networkManager.teamName = teamName;
            if (!string.IsNullOrEmpty(teamName))
            {
                AddToTeamData(PhotonNetwork.LocalPlayer.UserId, teamName);
                object[] data = new object[] { PhotonNetwork.LocalPlayer.UserId, teamName };
                RaiseEventOptions options = new RaiseEventOptions
                {
                    CachingOption = EventCaching.AddToRoomCache,
                    Receivers = ReceiverGroup.Others
                };
                if (debugging == true) Debug.Log("Raising CB_EVENT_TEAMCHANGE photon event...");
                PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_TEAMCHANGE, data, options, SendOptions.SendReliable);
            }
        }

        /// <summary>
        /// Returns the string value that is stored in the `NetworkManager`'s `teamName` parameter.
        /// </summary>
        /// <returns>string value in the `teamName` parameter of the NetworkManager component</returns>
        public virtual string GetMyTeamName()
        {
            return NetworkManager.networkManager.teamName;
        }

        /// <summary>
        /// Finds the team name that the photon user id is a part of.
        /// </summary>
        /// <param name="userId">string type, the photon user id</param>
        /// <returns>the string team name that the user id is a part of</returns>
        public virtual string GetUserTeamName(string userId)
        {
            return (_teamData.ContainsKey(userId)) ? _teamData[userId] : "";
        }

        /// <summary>
        /// Returns the entire team data dictionary. This dictionary is a list of 
        /// users ids and the teams those ids are on.
        /// </summary>
        /// <returns>Dictionary of team data</returns>
        public virtual Dictionary<string, string> GetTeamData()
        {
            return _teamData;
        }

        /// <summary>
        /// Adds the user id to the team name. If that user id is already in the 
        /// dictionary it will move that user id to the new team name.
        /// </summary>
        /// <param name="userId">string type, Photon user id</param>
        /// <param name="teamName">string type, name of the team to join</param>
        public virtual void AddToTeamData(string userId, string teamName)
        {
            if (debugging == true) Debug.Log("Adding: " + userId + " to team: " + teamName);
            if (_teamData.ContainsKey(userId))
            {
                _teamData[userId] = teamName;
            }
            else
            {
                _teamData.Add(userId, teamName);
            }
        }

        /// <summary>
        /// Erases the entire team data dictionary.
        /// </summary>
        public virtual void ClearTeamData()
        {
            if (debugging == true) Debug.Log("Clear saved team Data");
            _teamData.Clear();
        }
        #endregion

        #region Player - Ready
        /// <summary>
        /// Calls the `CB_EVENT_READYUP` PhotonEvent. This event will make your 
        /// user id be marked as ready or not based on the input value.
        /// </summary>
        /// <param name="isReady">bool type, you're ready?</param>
        public virtual void SendReadyState(bool isReady)
        {
            if (debugging == true) Debug.Log("Sending ready state: " + isReady);
            object[] data = new object[] { isReady, PhotonNetwork.LocalPlayer.UserId };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising Photon event: CB_EVENT_READYUP");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_READYUP, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Erases the entire dictionary of what players are ready.
        /// </summary>
        public virtual void ClearPlayerReadyDict()
        {
            if (debugging == true) Debug.Log("Clearing player ready list.");
            _playersReady.Clear();
        }

        /// <summary>
        /// Returns the dictionary of users ids and if they're ready or not
        /// </summary>
        /// <returns>player ready dictionary</returns>
        public virtual Dictionary<string, bool> GetPlayersReadyDict()
        {
            return _playersReady;
        }

        /// <summary>
        /// Checks to see if that user id is marked as ready by looking it up
        /// in the player ready dictionary.
        /// </summary>
        /// <param name="userId">string type, the photon user id</param>
        /// <returns>true if the player is marked as ready in the player ready dictionary, otherwise false.</returns>
        public virtual bool PlayerIsReady(string userId)
        {
            return (_playersReady.ContainsKey(userId)) ? _playersReady[userId] : false;
        }

        /// <summary>
        /// Checks all the user ids in the player ready dictionary and sees if they
        /// have been marked as ready or not.
        /// </summary>
        /// <returns>True if all user ids in the player ready dictionary are makrs as ready, otherwise false</returns>
        public virtual bool AllPlayersReady()
        {
            int readyCount = 0;
            foreach (bool player_ready in _playersReady.Values)
            {
                readyCount = (player_ready == true) ? readyCount + 1 : readyCount;
            }
            return readyCount == PhotonNetwork.CurrentRoom.PlayerCount;
        }

        /// <summary>
        /// Returns true if the user id is already in the player read dictionary.
        /// </summary>
        /// <param name="userId">string type, photon user id</param>
        /// <returns>true if that id is in the player ready dictionary, otherwise false.</returns>
        public virtual bool PlayerInReadyDict(string userId)
        {
            return _playersReady.ContainsKey(userId);
        }

        /// <summary>
        /// Takes a Photon.Realtime.Player input and extracts the user id of that
        /// player and adds it to the player ready dictionary as not ready. If that
        /// player's user id already exists in the dictionary it will make it be 
        /// marked as not ready.
        /// </summary>
        /// <param name="player">Photon.Realtime.Player type, player to put into the dictionary</param>
        public virtual void AddNewPlayerToPlayerReadyList(Photon.Realtime.Player player)
        {
            if (debugging == true) Debug.Log("Add new player to ready list: " + player.NickName);
            if (_playersReady.ContainsKey(player.UserId))
            {
                _playersReady[player.UserId] = false;
            }
            else
            {
                _playersReady.Add(player.UserId, false);
            }
        }

        /// <summary>
        /// Takes a Photon.Realtime.Player input and extracts the user id of that
        /// player, finds that user id in the player ready dictionary and removes 
        /// it from the dictionary.
        /// </summary>
        /// <param name="player">Photon.Realtime.Player type, the player to remove</param>
        public virtual void RemovePlayerToPlayerReadyList(Photon.Realtime.Player player)
        {
            if (debugging == true) Debug.Log("Remove player from ready list: " + player.NickName);
            if (_playersReady.ContainsKey(player.UserId))
            {
                _playersReady.Remove(player.UserId);
            }
        }
        #endregion
        #endregion

        #region Photon Lobby Events
        /// <summary>
        /// Calls the instanced `NetworkManager`'s `JoinLobby` function.
        /// </summary>
        public virtual void JoinLobby()
        {
            if (debugging == true) Debug.Log("Join default lobby...");
            NetworkManager.networkManager.JoinLobby();
        }
        #endregion

        #region Photon Room Settings
        /// <summary>
        /// Callback method. This is called whenever you're attempting to reconnect to a 
        /// photon room.
        /// </summary>
        public virtual void ReconnectingToLastRoom()
        {
            OnReconnecting.Invoke();
        }

        /// <summary>
        /// Stores the room name input for future use into the `_roomName` internal variable.
        /// </summary>
        /// <param name="roomName">string type, the name of the photon room to save.</param>
        public virtual void SaveRoomName(string roomName)
        {
            if (debugging == true) Debug.Log("Save room name: " + roomName);
            _roomName = roomName;
        }

        /// <summary>
        /// Calls the instanceds `NetworkManager`'s `JoinRoom` function with the input value 
        /// and immediately calls the `OnWaitToJoinPhotonRoomsLobby` UnityEvent.
        /// </summary>
        /// <param name="roomname">string type, the name of the photon room to join</param>
        public virtual void JoinRoom(string roomname)
        {
            if (debugging == true) Debug.Log("Join Room: " + roomname);
            NetworkManager.networkManager.JoinRoom(roomname);
            OnWaitToJoinPhotonRoomsLobby.Invoke();
        }

        /// <summary>
        /// Calls the instance'd `NetworkManager`'s `JoinRandomRoom` function with the 
        /// `_roomName` internal variable that was saved from the `SaveRoomName` function.
        /// Also uses the saved `_roomPassword` is the input value is true. Immediately 
        /// calls the `OnWaitToJoinPhotonRoomsLobby` UnityEvent.
        /// </summary>
        /// <param name="useSavedPassword">bool type, use the previously set room password or join without a password?</param>
        public virtual void JoinSavedRoomName(bool useSavedPassword)
        {
            if (debugging == true) Debug.Log("Join Saved Room: " + _roomName);
            if (useSavedPassword == true)
            {
                NetworkManager.networkManager.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable {
                    { RoomProperty.RoomName, _roomName },
                    { RoomProperty.Password, _roomPassword },
                    { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                });
            }
            else
            {
                NetworkManager.networkManager.JoinRoom(_roomName);
            }
            OnWaitToJoinPhotonRoomsLobby.Invoke();
        }

        /// <summary>
        /// Calls the instance'd `NetworkManager`'s `JoinRandomRoom` function with the previous
        /// set internal variables `_roomName` and `_roomPassword` parameters.
        /// </summary>
        public virtual void JoinPrivateRoom()
        {
            if (debugging == true) Debug.Log("Join Private Room: " + _roomName + " With Password: " + _roomPassword);
            NetworkManager.networkManager.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable {
                { RoomProperty.RoomName, _roomName },
                { RoomProperty.Password, _roomPassword },
                { RoomProperty.RoomType, RoomProperty.PrivateRoomType }
            });
        }

        /// <summary>
        /// Calls the `WaitForRandomJoin` IEnumerator.
        /// </summary>
        public virtual void JoinRandomPublicRoomOrCreateOne()
        {
            StartCoroutine(WaitForRandomJoin());
        }

        /// <summary>
        /// Joins the Photon Lobby and attempts to find an open room that isn't at
        /// capacity yet. If one isn't found it will generate a new room with a 
        /// random hash room name. 
        /// </summary>
        IEnumerator WaitForRandomJoin()
        {
            if (debugging == true) Debug.Log("Waiting to connect to lobby...");
            yield return new WaitUntil(() => PhotonNetwork.NetworkClientState == ClientState.JoinedLobby);
            if (debugging == true) Debug.Log("Connected To Lobby!");
            yield return new WaitForSeconds(0.05f);
            bool exists = false;
            if (NetworkManager.networkManager.cachedRoomList.Count < 1)
            {
                if (debugging == true) Debug.Log("No games exist...");
                exists = false;
            }
            else
            {
                if (debugging == true) Debug.Log("Checking for existing games...");
                foreach (RoomInfo room in NetworkManager.networkManager.cachedRoomList.Values)
                {
                    foreach (DictionaryEntry item in room.CustomProperties)
                    {
                        Debug.Log(item.Key + " = " + item.Value);
                    }
                    if (room.CustomProperties.ContainsKey(RoomProperty.RoomType))
                    {
                        if ((string)room.CustomProperties[RoomProperty.RoomType] == RoomProperty.PublicRoomType &&
                            room.PlayerCount < NetworkManager.networkManager.maxPlayerPerRoom)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
            }
            if (exists == true)
            {
                if (debugging == true) Debug.Log("Joining a found room...");
                NetworkManager.networkManager.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable {
                    { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                });
            }
            else
            {
                if (debugging == true) Debug.Log("No rooms found, creating a new random room...");
                _roomName = "";
                _roomPassword = "";
                EnableRoomNameChecking(false);
                CreateSessionWithSavedRoomName(false);
            }
        }

        /// <summary>
        /// Saves the input string into the `_roomPassword` internal variable for
        /// later use.
        /// </summary>
        /// <param name="password">string type, the password you want to include when joining/creating rooms.</param>
        public virtual void SaveRoomPassword(string password)
        {
            _roomPassword = password;
        }

        /// <summary>
        /// Clears the `_roomPassword` internal variable.
        /// </summary>
        public virtual void ClearSavedRoomPassword()
        {
            _roomPassword = "";
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `SetRoomIsOpen` function with whatever 
        /// value you have specified in this function.
        /// </summary>
        /// <param name="isJoinable">bool type, make this photon room joinable?</param>
        public virtual void SetRoomIsJoinable(bool isJoinable)
        {
            if (debugging == true) Debug.Log("Set room joinable state to: " + isJoinable);
            NetworkManager.networkManager.SetRoomIsOpen(isJoinable);
        }

        /// <summary>
        /// Make this photon room visible or not. Calls the `NetworkManager`'s 
        /// `SetRoomVisibility` function with the input value specified here.
        /// </summary>
        /// <param name="isVisible">bool type, make the photon room be in the list when people list rooms.</param>
        public virtual void SetRoomVisibility(bool isVisible)
        {
            if (debugging == true) Debug.Log("Set room visibility to: " + isVisible);
            NetworkManager.networkManager.SetRoomVisibility(isVisible);
        }

        /// <summary>
        /// Calls the `CB_EVENT_STARTCOUNTDOWN` PhotonEvent, which will trigger the
        /// countdown to start. This will trigger the `OnCountdownStarted` UnityEvent
        /// for all players that receive this event, including yourself.
        /// </summary>
        /// <param name="amount">float type, countdown amount</param>
        public virtual void SendStartCountdown(float amount)
        {
            object[] data = new object[] { true, amount };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_STARTCOUNTDOWN photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_STARTCOUNTDOWN, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Calls the `CB_EVENT_STARTCOUNTDOWN` PhotonEvent which will make all players, 
        /// including yourself, stop the countdown process. This all triggers the 
        /// `OnCountdownStopped` UnityEvent for everyone, including yourself.
        /// </summary>
        public virtual void SendStopCountdown()
        {
            object[] data = new object[] { false, 0 };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_STARTCOUNTDOWN photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_STARTCOUNTDOWN, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Calls the `CB_EVENT_SCENEVOTE` PhotonEvent which will add one to all players
        /// `_sceneVotes` at the index key you have specified. 
        /// </summary>
        /// <param name="sceneIndex">int type, the scene index to vote for. </param>
        public virtual void SendSceneVote(int sceneIndex)
        {
            if (_myPrevVote != sceneIndex)
            {
                object[] data = new object[] { _myPrevVote, sceneIndex };
                RaiseEventOptions options = new RaiseEventOptions
                {
                    CachingOption = EventCaching.AddToRoomCache,
                    Receivers = ReceiverGroup.All
                };
                if (debugging == true) Debug.Log("Raising CB_EVENT_SCENEVOTE photon event...");
                PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_SCENEVOTE, data, options, SendOptions.SendReliable);

                _myPrevVote = sceneIndex;
            }
        }

        /// <summary>
        /// Returns the entire `_sceneVotes` dictionary which contains all the votes
        /// that people have currently cast for a scene index.
        /// </summary>
        /// <param name="sceneIndex">int type, the scene index to see how many votes it has</param>
        /// <returns></returns>
        public virtual int GetSceneVotes(int sceneIndex)
        {
            return (_sceneVotes.ContainsKey(sceneIndex)) ? _sceneVotes[sceneIndex] : 0;
        }

        /// <summary>
        /// Clears the `_sceneVotes` dictionary so it no longer has ANY votes in it.
        /// </summary>
        public virtual void ClearSceneVoteList()
        {
            _maxVote = 0;
            _myPrevVote = -1;
            _sceneVotes.Clear();
        }

        /// <summary>
        /// Populates the `_randomSceneList` list. This is used for players to cast votes
        /// as to what scene they would like to play at.
        /// </summary>
        public virtual void CreateRandomSceneList()
        {
            _randomSceneList.Clear();
            List<SceneOption> temp = new List<SceneOption>();
            temp.AddRange(sceneList);
            foreach (SceneOption option in sceneList)
            {
                SceneOption item = temp[UnityEngine.Random.Range(0, temp.Count)];
                _randomSceneList.Add(item);
                temp.Remove(item);
            }
        }

        /// <summary>
        /// Receives as scene list and sets it to the `_randomSceneList` list.
        /// </summary>
        /// <param name="randomRoomList"></param>
        public virtual void SetRandomSceneList(List<SceneOption> randomRoomList)
        {
            _randomSceneList = randomRoomList;
        }

        /// <summary>
        /// Calls the `CB_EVENT_RANDOMSCENELIST` PhotonEvent which will make 
        /// everyone set their `_randomSceneList` list to the value that you
        /// currently have set in your own `_randomSceneList` list.
        /// </summary>
        public virtual void SendCreatedRandomSceneList()
        {
            RoomListWrapper container = new RoomListWrapper();
            container.wrapper = _randomSceneList;
            object[] data = new object[] { JsonUtility.ToJson(container) };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_RANDOMSCENELIST photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_RANDOMSCENELIST, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Returns an amount of random `SceneOption`'s from the `_randomSceneList` list.
        /// </summary>
        /// <param name="value">int type, number of `SceneOption`'s to return</param>
        /// <returns>Specified number of `SceneOption`'s</returns>
        public virtual SceneOption GetRandomSceneNumber(int value)
        {
            if (_randomSceneList.Count <= value)
            {
                return new SceneOption("", null);
            }
            else
            {
                return _randomSceneList[value];
            }
        }

        /// <summary>
        /// Extracts the `SceneOption` value from the `sceneList` list at the specified index.
        /// </summary>
        /// <param name="value">int type, the index to pull from</param>
        /// <returns>A `SceneOption` value</returns>
        public virtual SceneOption GetSceneNumber(int value)
        {
            return sceneList[value];
        }

        /// <summary>
        /// Populates the internal variable `_rooms` based on the `cachedRoomList` value in
        /// the `NetworkManager` component.
        /// </summary>
        public virtual void SaveRoomList()
        {
            if (debugging == true) Debug.Log("Save Room List: " + NetworkManager.networkManager.cachedRoomList.Count);
            _rooms = NetworkManager.networkManager.cachedRoomList;
        }

        /// <summary>
        /// Returns the internal variable `_rooms`. This is just a list of potential photon
        /// rooms.
        /// </summary>
        /// <returns>Dictionary of photon rooms</returns>
        public virtual Dictionary<string, RoomInfo> GetRoomList()
        {
            SaveRoomList();
            return _rooms;
        }

        /// <summary>
        /// Turn on valid room name checking. This makes it so rooms names can't have invalid
        /// characters or be blank. If this is on and rooms have these it will throw an error.
        /// </summary>
        /// <param name="isEnabled">bool type, ensure valid room names.</param>
        public virtual void EnableRoomNameChecking(bool isEnabled)
        {
            if (debugging == true) Debug.Log("Set room name checking: " + isEnabled);
            _roomNameChecking = isEnabled;
        }

        /// <summary>
        /// Create the initial photon room that will become the session name for your session.
        /// If you want to create a password protected room pass in true. Also will perform
        /// checks on the room name to make sure it's valid if you have enabled the 
        /// `_roomNameChecking` variable to be true. Calls the `NetworkManager`'s `CreateRoom`
        /// function to create the Photon room. Finally calls `OnCreateRoomSuccess` UnityEvent
        /// if the room creation was successfull, or calls `OnCreateRoomFailed` if it was not.
        /// </summary>
        /// <param name="useSavedPassword"></param>
        public virtual void CreateSessionWithSavedRoomName(bool useSavedPassword = false)
        {
            _roomName = _roomName.Trim();
            if (debugging == true) Debug.Log("Create Session With Saved Room Name: \"" + _roomName + "\"...");
            bool canCreate = true;
            if (_roomNameChecking == true)
            {
                if (string.IsNullOrEmpty(_roomName))
                {
                    OnCreateRoomFailed.Invoke("You must specify a room name to create a session.");
                    canCreate = false;
                }
                else if (_roomName.Contains("_"))
                {
                    OnCreateRoomFailed.Invoke("You cannot create a room with the '_' symbol in it.");
                    canCreate = false;
                }
                else if (_roomName.Contains(":"))
                {
                    OnCreateRoomFailed.Invoke("You cannot create a room with the ':' symbol in it.");
                    canCreate = false;
                }
            }
            if (PhotonNetwork.InLobby && canCreate == true)
            {
                foreach (KeyValuePair<string, RoomInfo> room in NetworkManager.networkManager.cachedRoomList)
                {
                    if (Regex.Replace(room.Key, "_.*", "") == _roomName)
                    {
                        OnCreateRoomFailed.Invoke("A session with that name already exists, choose another.");
                        return;
                    }
                }
                if (useSavedPassword == true)
                {
                    NetworkManager.networkManager.CreateRoom(
                        _roomName,
                        customRoomProperties: new ExitGames.Client.Photon.Hashtable {
                            { RoomProperty.Password, _roomPassword },
                            { RoomProperty.RoomName, _roomName },
                            { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                        },
                        exposePropertiesToLobby: new string[]
                        {
                            RoomProperty.Password,
                            RoomProperty.RoomName,
                            RoomProperty.RoomType
                        }
                    );
                }
                else
                {
                    NetworkManager.networkManager.CreateRoom(
                        _roomName,
                        customRoomProperties: new ExitGames.Client.Photon.Hashtable {
                            { RoomProperty.Password, _roomPassword },
                            { RoomProperty.RoomName, _roomName },
                            { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                        },
                        exposePropertiesToLobby: new string[]
                        {
                            RoomProperty.Password,
                            RoomProperty.RoomName,
                            RoomProperty.RoomType
                        }
                     );
                }
                OnCreateRoomSuccess.Invoke();
            }
            else if (canCreate == true)
            {
                if (useSavedPassword == true)
                {
                    NetworkManager.networkManager.CreateRoom(
                        _roomName,
                        customRoomProperties: new ExitGames.Client.Photon.Hashtable {
                            { RoomProperty.Password, _roomPassword },
                            { RoomProperty.RoomName, _roomName },
                            { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                        },
                        exposePropertiesToLobby: new string[] {
                            RoomProperty.Password,
                            RoomProperty.RoomName,
                            RoomProperty.RoomType
                        }
                    );
                }
                else
                {
                    NetworkManager.networkManager.CreateRoom(
                        _roomName,
                        customRoomProperties: new ExitGames.Client.Photon.Hashtable {
                            { RoomProperty.Password, _roomPassword },
                            { RoomProperty.RoomName, _roomName },
                            { RoomProperty.RoomType, RoomProperty.PublicRoomType }
                        },
                        exposePropertiesToLobby: new string[] {
                            RoomProperty.Password,
                            RoomProperty.RoomName,
                            RoomProperty.RoomType
                        }
                     );
                }
                OnCreateRoomSuccess.Invoke();
            }
        }

        /// <summary>
        /// Attempts to create a photon room with the saved `_roomName` and `_roomPassword`
        /// variables. Calls the `OnCreateRoomFailed` or `OnCreateRoomSuccess` UnityEvents
        /// if the room creation was successfull or not.
        /// </summary>
        public virtual void CreatePrivateSession()
        {
            if (debugging == true) Debug.Log("Create Session With Saved Room Name: " + _roomName + "...");
            if (string.IsNullOrEmpty(_roomName))
            {
                OnCreateRoomFailed.Invoke("You must specify a room name to create a session.");
            }
            else if (_roomName.Contains("_"))
            {
                OnCreateRoomFailed.Invoke("You cannot create a room with the '_' symbol in it.");
            }
            else if (_roomName.Contains(":"))
            {
                OnCreateRoomFailed.Invoke("You cannot create a room with the ':' symbol in it.");
            }
            NetworkManager.networkManager.CreateRoom(
                _roomName,
                customRoomProperties: new ExitGames.Client.Photon.Hashtable {
                    { RoomProperty.Password, _roomPassword },
                    { RoomProperty.RoomName, _roomName },
                    { RoomProperty.RoomType, RoomProperty.PrivateRoomType }
                },
                exposePropertiesToLobby: new string[]
                {
                    RoomProperty.Password,
                    RoomProperty.RoomName,
                    RoomProperty.RoomType
                }
            );
            OnCreateRoomSuccess.Invoke();
        }

        /// <summary>
        /// Calls the `CB_EVENT_ROUNDTIME` PhotonEvent to set the round time to whatever number is
        /// passed into this function for everyone in the photon room.
        /// </summary>
        /// <param name="roundTime">float type, the amount of time to set the round to</param>
        public virtual void SendRoundTime(float roundTime)
        {
            object[] data = new object[] { roundTime };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_ROUNDTIME photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_ROUNDTIME, data, options, SendOptions.SendReliable);
        }
        #endregion

        #region Generic Network Events
        /// <summary>
        /// Calls the `NetworkManager`'s `Disconnect` function to disconnect from the 
        /// photon room.
        /// </summary>
        public virtual void Disconnect()
        {
            ResetEverything();
            if (debugging == true) Debug.Log("Disconnect from Photon...");
            NetworkManager.networkManager.Disconnect();
        }

        /// <summary>
        /// Calls the `OnNetworkError` UnityEvent with the passed in string
        /// </summary>
        /// <param name="errorMessage">string type, the error message to display</param>
        public virtual void NetworkErrorOccured(string errorMessage)
        {
            if (debugging == true) Debug.Log("Recieved Network Error: " + errorMessage);
            OnNetworkError.Invoke(errorMessage);
        }
        #endregion

        #region Photon Events
        /// <summary>
        /// Called when ANY photon event is received. Will find out what type of event it is
        /// and call that function. (EX: If receiving `CB_EVENT_STARTSESSION` photon event it 
        /// will call the `PhotonEvent_STARTSESSION` function)
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void RecievedPhotonEvent(EventData obj)
        {
            try
            {
                if (debugging == true) Debug.Log("Receiving Photon Event: " + obj.Code + " ...");
                if (obj.CustomData == null || obj.CustomData.GetType() != typeof(object[])) return;
                object[] data = (object[])obj.CustomData;
                switch (obj.Code)
                {
                    case PhotonEventCodes.CB_EVENT_READYUP:
                        PhotonEvent_READUP(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_TEAMCHANGE:
                        PhotonEvent_TEAMCHANGE(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_STARTSESSION:
                        PhotonEvent_STARTSESSION(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_AUTOSPAWN:
                        PhotonEvent_AUTOSPAWN(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_KICKPLAYER:
                        PhotonEvent_KICKPLAYER(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_MAPCHANGE:
                        PhotonEvent_MAPCHANGE(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_RANDOMSCENELIST:
                        PhotonEvent_RANDOMSCENELIST(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_SCENEVOTE:
                        PhotonEvent_SCENEVOTE(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_STARTCOUNTDOWN:
                        PhotonEvent_STARTCOUNTDOWN(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_ROUNDTIME:
                        PhotonEvent_ROUNDTIME(data);
                        break;
                    case PhotonEventCodes.CB_EVENT_VOICEVIEW:
                        PhotonEvent_VOICEVIEW(data);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (debugging == true) Debug.Log(ex);
            }
        }

        /// <summary>
        /// Makes the user id ready up. This is a received PhotonEvent message.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_READUP(object[] data)
        {
            if (debugging == true) Debug.Log("Received: CB_EVENT_READYUP");
            bool isReady = (bool)data[0];
            string userId = (string)data[1];
            if (debugging == true) Debug.Log("READY: " + isReady + ", USERID: " + userId);
            if (_playersReady.ContainsKey(userId))
            {
                _playersReady[userId] = isReady;
            }
            else
            {
                _playersReady.Add(userId, isReady);
            }
        }

        /// <summary>
        /// Makes a user id change to a new team. This is a received PhotonEvent message.
        /// Also executes the `teamsUpdated` delegate.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_TEAMCHANGE(object[] data)
        {
            if (debugging == true) Debug.Log("Received: CB_EVENT_TEAMCHANGE");
            string userId = (string)data[0];
            string teamName = (string)data[1];
            if (debugging == true) Debug.Log("USERID: " + userId + ", TEAMNAME: " + teamName);
            AddToTeamData(userId, teamName);
            if (teamsUpdated != null)
            {
                teamsUpdated.Invoke();
            }
        }

        /// <summary>
        /// Makes everyone start the session and this will make it so the `RecievedPhotonEvent`
        /// function will not be called anymore when receiving events from the network.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_STARTSESSION(object[] data)
        {
            if (debugging == true) Debug.Log("Received: CB_EVENT_STARTMATCH");
            PhotonNetwork.NetworkingClient.EventReceived -= RecievedPhotonEvent;
            bool starting = (bool)data[0];
            if (starting == true)
            {
                OnStartSession.Invoke();
            }
        }

        /// <summary>
        /// Calls the `SendEnableAutoSpawn` function. This is a received Photon Event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_AUTOSPAWN(object[] data)
        {
            if (debugging == true) Debug.Log("Received: CB_EVENT_AUTOSPAWN");
            bool spawnEnabled = (bool)data[0];
            if (debugging == true) Debug.Log("AUTO_SPAWN_ENABLED: " + spawnEnabled);
            SendEnableAutoSpawn(spawnEnabled);
        }

        /// <summary>
        /// Makes the receiver of this event leave the photon room with an error message.
        /// This is a received photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_KICKPLAYER(object[] data)
        {
            NetworkErrorOccured("You have been kicked from the room");
        }

        /// <summary>
        /// Makes the receiver change the saved leve name and index. This is a received
        /// photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_MAPCHANGE(object[] data)
        {
            _savedLevelName = (string)data[0];
            _savedLevelIndex = (int)data[1];
        }

        /// <summary>
        /// Makes the receiver set their `_randomSceneList` variable to whatever is received.
        /// This is a received photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_RANDOMSCENELIST(object[] data)
        {
            RoomListWrapper container = JsonUtility.FromJson<RoomListWrapper>((string)data[0]);
            foreach (SceneOption option in container.wrapper)
            {
                option.sceneSprite = sceneList.Find(x => x.sceneName == option.sceneName).sceneSprite;
            }
            _randomSceneList = container.wrapper;
        }

        /// <summary>
        /// Adds or removes a vote to the scene vote index. This is a received photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_SCENEVOTE(object[] data)
        {
            int prevScene = (int)data[0];
            int currentScene = (int)data[1];
            if (prevScene != -1)
            {
                _sceneVotes[prevScene] -= 1;
            }
            if (_sceneVotes.ContainsKey(currentScene))
            {
                _sceneVotes[currentScene] += 1;
            }
            else
            {
                _sceneVotes.Add(currentScene, 1);
            }
            if (PhotonNetwork.IsMasterClient == true)
            {
                string winning_level_name = _savedLevelName;
                int max_vote = 0;
                List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
                foreach (KeyValuePair<int, int> item in _sceneVotes)
                {
                    if (item.Value > _maxVote)
                    {
                        winning_level_name = sceneData.Find(x => x.index == item.Key).sceneName;
                        max_vote = item.Value;
                    }
                }
                if (winning_level_name != _savedLevelName)
                {
                    _savedLevelIndex = sceneData.Find(x => x.sceneName == winning_level_name).index;
                    SendSceneChangeInfo(_savedLevelName, _savedLevelIndex);
                }
            }
        }

        /// <summary>
        /// Makes the receiver call the `OnCountdownStarted` or `OnCountdownStopped` UnityEvents
        /// based on the received value. This is a received photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_STARTCOUNTDOWN(object[] data)
        {
            bool start_countdown = (bool)data[0];
            float countdown_amount = (float)data[1];
            if (start_countdown)
            {
                OnCountdownStarted.Invoke(countdown_amount);
            }
            else
            {
                OnCountdownStopped.Invoke();
            }
        }

        /// <summary>
        /// Sets the round time to be whatever is received. This is a received photon event.
        /// This also calls the `OnReceiveRoundTime` UnityEvent.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_ROUNDTIME(object[] data)
        {
            float round_time = (float)data[0];
            if (debugging == true) Debug.Log("Received Round Time Update: " + round_time);
            OnReceiveRoundTime.Invoke(round_time);
        }

        /// <summary>
        /// Sets the voice view id for the user id for later referencing. Also calls the 
        /// `voiceViewUpdated` delegate. This is a received photon event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void PhotonEvent_VOICEVIEW(object[] data)
        {
            string UserId = (string)data[0];
            int ViewId = (int)data[1];
            if (debugging == true) Debug.Log("Recieved new VOICEVIEW, UserId: " + UserId + ", ViewID: " + ViewId);
            if (_playerVoiceChatViews.ContainsKey(UserId))
            {
                if (debugging == true) Debug.Log("Updating, UserId: " + UserId + ", To ViewID: " + ViewId);
                _playerVoiceChatViews[UserId] = ViewId;
            }
            else
            {
                if (debugging == true) Debug.Log("Adding new UserId: " + UserId + ", With ViewID: " + ViewId);
                _playerVoiceChatViews.Add(UserId, ViewId);
            }
            if (voiceViewUpdated != null) voiceViewUpdated.Invoke();
        }
        #endregion

        #region Session Events
        /// <summary>
        /// Calls the `CB_EVENT_STARTSESSION` photon event. Which makes everyone start the 
        /// photon session. Only the master client can call this method.
        /// </summary>
        public virtual void SendStartSession()
        {
            if (PhotonNetwork.IsMasterClient == false) return;
            if (debugging == true) Debug.Log("Sending start session...");
            object[] data = new object[] { true };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCacheGlobal,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising Photon Event: CB_EVENT_STARTMATCH");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_STARTSESSION, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Calls the `CB_EVENT_AUTOSPAWN` Photon Event. This will make it so autospawn is
        /// enabled when a new Unity scene is loaded.
        /// </summary>
        /// <param name="enableSpawn">bool type, enable auto spawn?</param>
        public virtual void SendEnableAutoSpawn(bool enableSpawn)
        {
            if (debugging == true) Debug.Log("Sending Enable Auto Spawn: " + enableSpawn);
            object[] data = new object[] { enableSpawn };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising Photon Event: CB_EVENT_AUTOSPAWN");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_AUTOSPAWN, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Returns the previously saved level name
        /// </summary>
        /// <returns>the previously saved level name</returns>
        public virtual string GetSavedSceneToLoadName()
        {
            return _savedLevelName;
        }

        /// <summary>
        /// Calls the `CB_EVENT_MAPCHANGE` photon event. Sets the scene name and index
        /// for all players in the photon room.
        /// </summary>
        /// <param name="sceneName">string type, the scene name to set to</param>
        /// <param name="index">int type, the index of this scene name</param>
        protected virtual void SendSceneChangeInfo(string sceneName, int index)
        {
            object[] data = new object[] { sceneName, index };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCache,
                Receivers = ReceiverGroup.Others
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_MAPCHANGE photon event...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_MAPCHANGE, data, options, SendOptions.SendReliable);
        }

        /// <summary>
        /// Calls `SendSceneChangeInfo` function with the extracted index of the 
        /// scene name that you input and the scene name.
        /// </summary>
        /// <param name="levelName">string type, the scene name to have everyone set their map choice to</param>
        public virtual void SaveSceneToLoad(string levelName)
        {
            if (debugging == true) Debug.Log("Save Scene To Load: " + levelName);
            _savedLevelName = levelName;
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            _savedLevelIndex = sceneData.Find(x => x.sceneName == _savedLevelName).index;
            SendSceneChangeInfo(_savedLevelName, _savedLevelIndex);
        }

        /// <summary>
        /// Calls `SendSceneChangeInfo` with the extracts name of the scene index
        /// that you input and the index.
        /// </summary>
        /// <param name="levelIndex"></param>
        public virtual void SaveSceneToLoad(int levelIndex)
        {
            if (debugging == true) Debug.Log("Save Scene To Load Index: " + levelIndex);
            _savedLevelIndex = levelIndex;
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            _savedLevelName = sceneData.Find(x => x.index == _savedLevelIndex).sceneName;
            SendSceneChangeInfo(_savedLevelName, _savedLevelIndex);
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `NetworkLoadLevel` function with the previously
        /// saved level name's index.
        /// </summary>
        public virtual void LoadSavedLevel()
        {
            if (debugging == true) Debug.Log("Load the saved level: " + _savedLevelName);
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            if (!string.IsNullOrEmpty(_savedLevelName))
            {
                NetworkManager.networkManager.NetworkLoadLevel(sceneData.Find(x => x.sceneName == _savedLevelName).index);
            }
            else
            {
                NetworkManager.networkManager.NetworkLoadLevel(_savedLevelIndex, sendEveryone: false);
            }
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `NetworkLoadLevel` function with the sendEveryone variable 
        /// as true. It uses the previously saved level name's index.
        /// </summary>
        public virtual void EveryoneLoadSavedLevel()
        {
            if (debugging == true) Debug.Log("Every load the saved level: " + _savedLevelName);
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            if (!string.IsNullOrEmpty(_savedLevelName))
            {
                NetworkManager.networkManager.NetworkLoadLevel(sceneData.Find(x => x.sceneName == _savedLevelName).index);
            }
            else
            {
                NetworkManager.networkManager.NetworkLoadLevel(_savedLevelIndex);
            }
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `NetworkLoadLevel` with the `sendEveryone` value as true.
        /// It will load the unity scene based on the input value's index.
        /// </summary>
        /// <param name="levelName">string type, the scene name to find the index for</param>
        public virtual void EveryoneLoadLevel(string levelName)
        {
            if (debugging == true) Debug.Log("Every load level: " + levelName);
            OnStartSession.Invoke();
            List<DatabaseScene> sceneData = NetworkManager.networkManager.database.storedScenesData;
            NetworkManager.networkManager.NetworkLoadLevel(sceneData.Find(x => x.sceneName == levelName).index);
        }

        /// <summary>
        /// Calls the `NetworkManager`'s `NetworkLoadLevel with the `sendEveryone` value as true.
        /// It will load the unity scene based on the index you pass in.
        /// </summary>
        /// <param name="levelIndex">int type, the scene index to load</param>
        public virtual void EveryoneLoadLevel(int levelIndex)
        {
            if (debugging == true) Debug.Log("Every load level index: " + levelIndex);
            OnStartSession.Invoke();
            NetworkManager.networkManager.NetworkLoadLevel(levelIndex);
        }

        /// <summary>
        /// Sets the `NetworkManager`'s `autoSpawnPlayer` value to whatever input value you use.
        /// </summary>
        /// <param name="setActive">bool type, make the players auto spawn?</param>
        public virtual void EnablePlayerAutoSpawn(bool setActive)
        {
            if (debugging == true) Debug.Log("Enable Player Auto Spawn: " + setActive);
            NetworkManager.networkManager.autoSpawnPlayer = setActive;
        }
        #endregion

        #region Loading Pages
        /// <summary>
        /// Display the loading page UI or not.
        /// </summary>
        /// <param name="isEnabled">bool type, show the loading page?</param>
        public virtual void EnableLoadingPage(bool isEnabled)
        {
            loadingParent.SetActive(isEnabled);
            if (isEnabled == true)
            {
                if (debugging == true) Debug.Log("Start Level Loading...");
                ResetLoadingBar();

                _cycleLoadingPage = (loadingImages.Count > 1 || loadingDesc.Count > 1) ? true : false;
                _selectedLoadImageIndex = 0;
                _selectedDescTextIndex = 0;

                _tempLoadColor = mainLoadingImage.color;
                _tempLoadTextColor = loadingDescText.color;
                _tempLoadColor.a = 1;
                _tempLoadTextColor.a = 1;
                mainLoadingImage.color = _tempLoadColor;
                loadingDescText.color = _tempLoadTextColor;

                mainLoadingImage.sprite = (loadingImages.Count > 0) ? loadingImages[0] : mainLoadingImage.sprite;
                loadingTitleText.text = loadingTitle;
                loadingDescText.text = (loadingDesc.Count > 0) ? loadingDesc[0] : loadingDescText.text;

                mainLoadingImage.gameObject.SetActive(true);
                loadingTitleText.gameObject.SetActive(true);
                loadingDescText.gameObject.SetActive(true);
                StartCoroutine(WaitForSceneToStartLoading());
                OnStartLoading.Invoke();
            }
            else
            {
                if (debugging == true) Debug.Log("End Level Loading.");
                mainLoadingImage.gameObject.SetActive(false);
                loadingTitleText.gameObject.SetActive(false);
                loadingDescText.gameObject.SetActive(false);
                _trackLoadingBar = false;
                _cycleLoadingPage = false;
            }
        }
        IEnumerator WaitForSceneToStartLoading()
        {
            yield return new WaitUntil(() => PhotonNetwork.LevelLoadingProgress < 1);
            _trackLoadingBar = true;
        }

        /// <summary>
        /// Sets the `loadingBar` to zero.
        /// </summary>
        public virtual void ResetLoadingBar()
        {
            if (debugging == true) Debug.Log("Reset Loading Bar.");
            loadingBar.fillAmount = 0;
        }
        #endregion

        #region Application Options
        /// <summary>
        /// Quit the unity application.
        /// </summary>
        public virtual void QuitGame()
        {
            Application.Quit();
        }
        #endregion

        #region Chatbox
        /// <summary>
        /// Make the chatbox slide in or out. True = Slide out(visible), False = Slide in(not visible)
        /// </summary>
        /// <param name="isEnabled">bool type, True = Slide Out</param>
        public virtual void EnableChatboxSlideOut(bool isEnabled)
        {
            if (debugging == true) Debug.Log("Enable ChatBox GameObject: " + isEnabled);
            ChatBox chatbox = FindObjectOfType<ChatBox>();
            if (chatbox)
            {
                chatbox.EnableChat(isEnabled);
            }
        }

        /// <summary>
        /// Make the chatbox inactive or not. True = Can see chatbox, False = Can NOT see chatbox
        /// </summary>
        /// <param name="isEnabled">bool type, make the chatbox visible or not?</param>
        public virtual void EnableChatBoxVisibility(bool isEnabled)
        {
            if (debugging == true) Debug.Log("Enable Visual ChatBox: " + isEnabled);
            ChatBox chatbox = FindObjectOfType<ChatBox>();
            if (chatbox)
            {
                chatbox.EnableVisualBox(isEnabled);
            }
        }
        #endregion

        #region Invector Sources
        /// <summary>
        /// Find all of the UI's that invector provides and enable them or not.
        /// </summary>
        /// <param name="isEnabled">bool type, disable or enable the Invector provided UIs?</param>
        public virtual void EnableSavedPlayerUI(bool isEnabled)
        {
            if (debugging == true) Debug.Log("Enable Saved Player UI: " + isEnabled);
            foreach (GameObject ui in _playerUIs)
            {
                ui.SetActive(isEnabled);
            }
        }

        /// <summary>
        /// Find all the objects tagged with `PlayerUI` and save them for later 
        /// manipulation.
        /// </summary>
        public virtual void SavePlayerUIs()
        {
            if (debugging == true) Debug.Log("Save Player UIs");
            GameObject[] invectorUI = GameObject.FindGameObjectsWithTag("PlayerUI");
            _playerUIs.AddRange(invectorUI);
        }

        /// <summary>
        /// Remove all saved player UIs from local manipulation.
        /// </summary>
        public virtual void ClearPlayerUIs()
        {
            if (debugging == true) Debug.Log("Remove all saved player uis.");
            _playerUIs.Clear();
        }

        /// <summary>
        /// Rebuilds the saved player UIs list.
        /// </summary>
        public virtual void RefreshPlayerUIs()
        {
            ClearPlayerUIs();
            SavePlayerUIs();
        }
        #endregion

        #region Mouse/Keyboard Settings
        /// <summary>
        /// Allow the mouse to be moved around or not.
        /// </summary>
        /// <param name="isEnabled">bool type, true = can move, false = cannot move</param>
        public virtual void EnableMouseMovement(bool isEnabled)
        {
            Cursor.lockState = (isEnabled == true) ? CursorLockMode.None : CursorLockMode.Locked;
        }

        /// <summary>
        /// Hide or show the mouse.
        /// </summary>
        /// <param name="isVisible">bool type, true = show, false = hide</param>
        public virtual void EnableMouseVisibility(bool isVisible)
        {
            Cursor.visible = isVisible;
        }

        /// <summary>
        /// Calls the `MouseSelectHandle` IEnumerator
        /// </summary>
        /// <param name="target"></param>
        public virtual void MouseSelect(GameObject target)
        {
            StartCoroutine(MouseSelectHandle(target));
        }

        /// <summary>
        /// Sets the target the mouse will select based on the input target.
        /// </summary>
        /// <param name="target">GameObject type, the target for the mouse to select</param>
        IEnumerator MouseSelectHandle(GameObject target)
        {
            if (this.enabled)
            {
                yield return new WaitForEndOfFrame();
                MouseSelectTarget(target);
            }
        }

        /// <summary>
        /// Have the mouse select the target gameobect
        /// </summary>
        /// <param name="target">Gameobject type, the gameobject for the mouse to select</param>
        protected virtual void MouseSelectTarget(GameObject target)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
        }
        #endregion

        #region Scenes
        /// <summary>
        /// Callback method. This is called when a new Unity scene is loaded.
        /// This calls the `OnSceneLoaded` UnityEvent.
        /// </summary>
        /// <param name="scene">Scene type, the Scene that was loaded</param>
        /// <param name="mode">LoadSceneMode type, how this scene was loaded</param>
        protected virtual void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoaded.Invoke(scene);
        }
        #endregion

        #region Voice Chat
        /// <summary>
        /// Get the voice view of the passed in photon user id.
        /// </summary>
        /// <param name="UserId">string type, the photon user id</param>
        /// <returns>The photon voice view of the user id</returns>
        public virtual int GetPlayerVoiceView(string UserId)
        {
            return (_playerVoiceChatViews.ContainsKey(UserId)) ? _playerVoiceChatViews[UserId] : 999999;
        }

        /// <summary>
        /// Calls the `CB_EVENT_VOICEVIEW` PhotonEvent. This eventually will execute the 
        /// `voiceViewUpdated` delegate.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ViewId"></param>
        public virtual void SendUpdateVoiceView(string UserId, int ViewId)
        {
            object[] data = new object[] { UserId, ViewId };
            RaiseEventOptions options = new RaiseEventOptions
            {
                CachingOption = EventCaching.AddToRoomCacheGlobal,
                Receivers = ReceiverGroup.All
            };
            if (debugging == true) Debug.Log("Raising CB_EVENT_VOICEVIEW photon event with data { " + UserId + ", " + ViewId + " }...");
            PhotonNetwork.RaiseEvent(PhotonEventCodes.CB_EVENT_VOICEVIEW, data, options, SendOptions.SendReliable);
        }
        #endregion
    }
}