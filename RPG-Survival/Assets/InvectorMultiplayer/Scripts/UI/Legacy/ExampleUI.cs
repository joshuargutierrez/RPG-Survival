#pragma warning disable 0649

using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CBGames.Core;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Text.RegularExpressions;
using Invector;
using Invector.vCharacterController;
using System.Reflection;
using UnityEngine.Events;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Legacy/Example UI")]
    public class ExampleUI : MonoBehaviour
    {
        #region Modifiable Variables
        [Tooltip("The index of the scene that will act as your room's lobby. You can find this index in the Build Settings when you specify what scenes to build in your project.")]
        public int lobbyIndex = 0;
        
        #region Pages
        [Header("Pages")]
        [Tooltip("The gameobject that holds everything, just in case you need to disable everything.")]
        public GameObject PanelPage;
        [Tooltip("The gameobject that holds all the child elements that make up the main page.")]
        public GameObject MainPage;
        [Tooltip("The gameobject that holds all the child elements that make up the host game page.")]
        public GameObject HostGamePage;
        [Tooltip("The gameobject that holds all the child elements that make up the join game page.")]
        public GameObject JoinGamePage;
        [Tooltip("The gameobject that holds all the child elements to set a players name.")]
        public GameObject PlayerNamePage;
        [Tooltip("The page to display when you are connected to a lobby of a room (NOT A PHOTON LOBBY).")]
        public GameObject LobbyPage;
        [Tooltip("The gameobject that holds all the child elements to allow a player to select their character(prefab).")]
        public GameObject PlayerPrefabSelectPage;
        [Tooltip("The gameobject to display a player card.")]
        public GameObject PlayerCardPage;
        [Tooltip("The gameobject to enable when the player attempts to input an invalid name.")]
        public GameObject NameErrorPage;
        [Tooltip("The gameobject to enable when you wish to display a network error.")]
        public GameObject NetworkErrorPage;
        [Tooltip("The page to display when loading a scene.")]
        public GameObject LoadingPage;
        [Tooltip("The page that is displayed when there is more than one available room to join.")]
        public GameObject ChooseRoomPage;

        UnityEvent<RoomInfo> OnJoinGameRoom;
        #endregion

        #region Specific Elements
        [Space(10)]
        [Header("Specific Needed Elements")]
        [Tooltip("The loading page title text to display.")]
        public Text LoadingTitleText;
        [Tooltip("The loading page description text to display.")]
        public Text LoadingDescriptionText;
        [Tooltip("The loading page preview image to display.")]
        public Image LoadingPreviewImage;
        [Tooltip("The loading bar to display when loading a page.")]
        public Slider LoadingBar;
        [Tooltip("The text to set to display the recently joined player.")]
        public Text PlayerCardText;
        [Tooltip("The animation component to call to player slide in/out animations.")]
        public Animation PlayerCardAnimation;
        [Tooltip("The gameobject that is the title image.")]
        public GameObject titleImage;
        [Tooltip("The text element that will be used to display any network errors/events.")]
        public Text NetworkErrorText;
        [Tooltip("The input field element that will be used to set the player name.")]
        public InputField NameInputField;
        [Tooltip("The input field element that will be used to capture the room name to start.")]
        public InputField RoomNameInput;
        [Tooltip("The transform element in your UI that will be used as a parent object for your room button.")]
        public GameObject AvailableRooms;
        [Tooltip("The transform element in your UI that will be used as a parent object for your available scenes buttons. This is a child element of your \"ChooseRoomPage\"")]
        public GameObject AvailableScenes;
        [Tooltip("The gameobject that holds the \"Create Room\" button.")]
        public GameObject createRoomButton;
        [Tooltip("A prefab element that will be spawned in when a room is found from the server.")]
        public GameObject roomButton;
        [Tooltip("String elements that will be populated by the network manager connection status.")]
        public Text[] connectionStatus;
        #endregion

        #region Audio Settings
        [Space(10)]
        [Header("Audio")]
        [Tooltip("The audio source that will play your menu music.")]
        [SerializeField] private AudioSource musicSource;
        [Tooltip("The audio source that will play your menu sounds.")]
        [SerializeField] private AudioSource soundSource;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseEnter\" function.")]
        [SerializeField] private AudioClip mouseEnter = null;
        [Range(0,1)]
        [Tooltip("How loud to play the \"mouseEnter\" sound.")]
        [SerializeField] private float mouseEnterVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseExit\" function.")]
        [SerializeField] private AudioClip mouseExit = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"mouseExit\" sound.")]
        [SerializeField] private float mouseExitVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerMouseClick\" function.")]
        [SerializeField] private AudioClip mouseClick = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"mouseClick\" sound.")]
        [SerializeField] private float mouseClickVolume = 0.5f;
        [Tooltip("The sound that will be played when you call the \"PlayerFinalClick\" function.")]
        [SerializeField] private AudioClip finalClick = null;
        [Range(0, 1)]
        [Tooltip("How loud to play the \"finalClick\" sound.")]
        [SerializeField] private float finalClickVolume = 0.5f;
        [Tooltip("How loud to set your audio to.")]
        [SerializeField] private float startVolume = 0.5f;
        [Tooltip("When first loading the UI, fade in the audio.")]
        [SerializeField] private bool fadeInAudio = false;
        [Tooltip("If fading out your music how to fast to fade out. Higher values will fade out faster.")]
        [Range(0.01f, 5.0f)]
        [SerializeField] private float fadeOutSpeed = 0.25f;
        #endregion

        #region Visual Elements
        [Space(10)]
        [Header("Visual Elements")]
        [Tooltip("You must be in the lobby for the \"Create Room\" button to be displayed.")]
        [SerializeField] private bool lobbyOnlyRoomCreate = true;
        [Tooltip("How long to display the loading images on the loading page before begining the fade/out in process.")]
        [SerializeField] private float loadImageDisplayTime = 5.0f;
        [Tooltip("How fast to fade in and out the images on the loading page.")]
        [SerializeField] private float LoadingPreviewImageTransitionSpeed = 0.5f;
        [Tooltip("When fading effect this object. If effect all children is selected it will effect this object and all of its children.")]
        [SerializeField] private GameObject[] fadeObjects = null;
        [Tooltip("When fading do you want to effect the parent object and all of its children?")]
        [SerializeField] private bool effectChildren = true;
        [Tooltip("Fade in your UI when first loading it.")]
        [SerializeField] private bool fadeInUI = true;
        [Tooltip("How fast to fade in your UI. Higher values will fade in faster.")]
        [Range(0.01f, 5.0f)]
        [SerializeField] private float uiFadeSpeed = 1.0f;
        [Tooltip("How long to show the player card before sliding out.")]
        [Range(2.0f, 30.0f)]
        [SerializeField] private float playerCardDisplayTime = 10.0f;
        #endregion

        #region Player Options
        [Space(10)]
        [Header("Player Options")]
        [Tooltip("All of the possible players that can be selected. Used in combination with " +
            "the \"SelectPlayer\" function in this component. Call this function BEFORE attempting " +
            "to spawn your player as the playerPrefab on the NetworkManager is set from this " +
            "function.")]
        public GameObject[] players = null;
        [Tooltip("The gameobject to set active after successfully entering your player name. " +
            "If not supplied will travel to PlayerPrefabPage")]
        public GameObject successNameInputTravelTo = null;
        #endregion
        #endregion

        #region Private Variables
        private bool _isLobbyOwner = false;
        private bool _isWaitingForRefresh = false;
        private bool _cycleLoadingPreviewImages = false;
        private List<Sprite> LoadingPreviewImages = new List<Sprite>();
        private List<string> _loadingDescriptionText = new List<string>();
        private string prev_status = "";
        private Dictionary<string, RoomInfo> roomList = new Dictionary<string, RoomInfo>();
        private bool _fadeOut = false;
        private bool _fadeIn = true;
        private List<Image> _images = new List<Image>();
        private List<Text> _texts = new List<Text>();
        private float _uiOpacity = 0;
        private float _playerDisplayedTime = 0.0f;
        private List<Image> _loadImage = new List<Image>();
        private List<Text> _loadPageTexts = new List<Text>();
        private List<Image> _loadDoneImage = new List<Image>();
        private float _loadImageOpacity = 1.0f;
        private bool _loadFadeIn = false;
        private bool _loadWaiting = false;
        private int _startDescriptionIndex = 0;
        private int _startImageIndex = 0;
        private bool _inWaitingLobby = false;
        #endregion

        #region One Time Triggers
        public void Awake()
        {
            if (fadeInUI == true)
            {
                foreach (GameObject target in fadeObjects)
                {
                    if (target.GetComponent<Image>()) _images.Add(target.GetComponent<Image>());
                    if(target.GetComponent<Text>()) _texts.Add(target.GetComponent<Text>());
                    if (effectChildren == true)
                    {
                        _images.AddRange(target.GetComponentsInChildren<Image>());
                        _texts.AddRange(target.GetComponentsInChildren<Text>());
                    }
                }
                StaticMethods.SetUIOpacity(0, _images, _texts);
            }
            if (fadeInAudio == true)
            {
                FadeAudio(true);
            }
        }
        public void Start()
        {
            NetworkManager.networkManager = (NetworkManager.networkManager == null) ? FindObjectOfType<NetworkManager>() : NetworkManager.networkManager;
            musicSource = (musicSource == null) ? GetComponent<AudioSource>() : musicSource;

            //Loading Page Settings
            _loadImage = new List<Image>() { LoadingPreviewImage };
            _loadPageTexts = new List<Text> { LoadingTitleText, LoadingDescriptionText };
            SceneManager.sceneLoaded += UINewSceneLoaded;
        }
        public void QuitGame()
        {
            Application.Quit();
        }
        public void UINewSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindingFlags bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            bool controllerIsSet = false;
            while (controllerIsSet == false)
            {
                foreach (vThirdPersonController controller in FindObjectsOfType<vThirdPersonController>())
                {
                    if (controller.GetComponent<PhotonView>() || controller.GetComponent<PhotonView>().IsMine)
                    {
                        vGameController.instance.currentPlayer = controller.gameObject;
                        vGameController.instance.GetType().GetField("currentController", bindings).SetValue(vGameController.instance, controller);
                        controllerIsSet = true;
                        break;
                    }
                }
            }
        }
        #endregion

        private void Update()
        {
            #region Host Game Page
            if (HostGamePage.activeInHierarchy == true)
            {
                if (lobbyOnlyRoomCreate == true)
                {
                    createRoomButton.SetActive(PhotonNetwork.InLobby);
                }
            }
            #endregion

            #region Fading UI
            if (fadeInUI == true)
            {
                _uiOpacity += Time.deltaTime * uiFadeSpeed;
                if (_uiOpacity >= 1)
                {
                    fadeInUI = false;
                    _uiOpacity = 1;
                }
                StaticMethods.SetUIOpacity(_uiOpacity, _images, _texts);
            }
            if (prev_status != NetworkManager.networkManager._connectStatus)
            {
                prev_status = NetworkManager.networkManager._connectStatus;
                SetConnectionStatus(prev_status);
            }
            if (_fadeOut == true)
            {
                musicSource.volume -= Time.deltaTime * fadeOutSpeed;
                if (musicSource.volume <= 0)
                {
                    musicSource.volume = 0;
                    _fadeOut = false;
                }
            }
            else if (_fadeIn == true)
            {
                musicSource.volume += Time.deltaTime * fadeOutSpeed;
                if (musicSource.volume >= startVolume)
                {
                    musicSource.volume = startVolume;
                    _fadeIn = false;
                }

            }
            #endregion

            #region Loading Page
            if (LoadingPage.activeInHierarchy == true)
            {
                if (PhotonNetwork.InRoom == true)
                {
                    if (PhotonNetwork.LevelLoadingProgress == 1)
                    {
                        EnableLoadingPage(false);
                    }
                    else
                    {
                        LoadingBar.value = PhotonNetwork.LevelLoadingProgress;
                    }
                }
                if (_cycleLoadingPreviewImages == true)
                {
                    if (_loadFadeIn == true)
                    {
                        _loadImageOpacity += Time.deltaTime * LoadingPreviewImageTransitionSpeed;
                        _loadImageOpacity = (_loadImageOpacity >= 1) ? 1 : _loadImageOpacity;
                        if (_loadWaiting == false && _loadImageOpacity == 1)
                        {
                            StartCoroutine(BeginImageDisplayTime(_loadFadeIn));
                        }
                    }
                    else
                    {
                        _loadImageOpacity -= Time.deltaTime * LoadingPreviewImageTransitionSpeed;
                        _loadImageOpacity = (_loadImageOpacity <= 0) ? 0 : _loadImageOpacity;
                        if (_loadImageOpacity == 0)
                        {
                            _startImageIndex = (_startImageIndex + 1 >= LoadingPreviewImages.Count) ? 0 : _startImageIndex + 1;
                            _startDescriptionIndex = (_startDescriptionIndex + 1 >= _loadingDescriptionText.Count) ? 0 : _startDescriptionIndex + 1;
                            LoadingPreviewImage.sprite = LoadingPreviewImages[_startImageIndex];
                            LoadingDescriptionText.text = _loadingDescriptionText[_startDescriptionIndex];
                            _loadFadeIn = true;
                        }
                    }
                    StaticMethods.SetUIOpacity(_loadImageOpacity, _loadImage, _loadPageTexts);
                }
            }
            #endregion
        }

        #region Loading Events
        void ResetLoadPageOpacity()
        {
            foreach (GameObject target in fadeObjects)
            {
                if (target.GetComponent<Image>()) _images.Add(target.GetComponent<Image>());
                if (target.GetComponent<Text>()) _texts.Add(target.GetComponent<Text>());
                if (effectChildren == true)
                {
                    _images.AddRange(target.GetComponentsInChildren<Image>());
                    _texts.AddRange(target.GetComponentsInChildren<Text>());
                }
            }
        }
        IEnumerator BeginImageDisplayTime(bool fadeIn)
        {
            _loadWaiting = true;
            yield return new WaitForSeconds(loadImageDisplayTime);
            _loadFadeIn = !fadeIn;
            _loadWaiting = false;
        }
        public void SetLoadingImages(List<Sprite> images)
        {
            if (images.Count > 0)
            {
                LoadingPreviewImages.Clear();
                LoadingPreviewImages.AddRange(images);
                _cycleLoadingPreviewImages = (LoadingPreviewImages.Count > 1) ? true : false;
            }
        }
        public void SetLoadingDescriptionText(List<string> loadingDescriptions)
        {
            if (loadingDescriptions.Count > 0)
            {
                _loadingDescriptionText.Clear();
                _loadingDescriptionText.AddRange(loadingDescriptions);
            }
        }
        public void SetLoadingTitleText(string titleText)
        {
            LoadingTitleText.text = titleText;
        }
        public void ResetLoadingBar()
        {
            LoadingBar.value = 0;
        }
        public void EnableLoadingPage(bool enabled)
        {
            ResetLoadingBar();
            _startDescriptionIndex = 0;
            _startImageIndex = 0;
            if (LoadingPreviewImages != null && LoadingPreviewImages.Count > 0)
            {
                LoadingPreviewImage.sprite = LoadingPreviewImages[0];
            }
            if (_loadingDescriptionText != null && _loadingDescriptionText.Count > 0)
            {
                LoadingDescriptionText.text = _loadingDescriptionText[0];
            }
            if (_loadingDescriptionText.Count > 1 || _loadImage.Count > 1)
            {
                _cycleLoadingPreviewImages = true;
            }
            else
            {
                _cycleLoadingPreviewImages = false;
            }
            LoadingPage.SetActive(enabled);
        }
        #endregion

        #region Audio Events
        public void PlayMouseEnter()
        {
            if (soundSource == null || mouseEnter == null) return;
            soundSource.clip = mouseEnter;
            soundSource.volume = mouseEnterVolume;
            soundSource.Play();
        }
        public void PlayMouseExit()
        {
            if (soundSource == null || mouseExit == null) return;
            soundSource.clip = mouseExit;
            soundSource.volume = mouseExitVolume;
            soundSource.Play();
        }
        public void PlayMouseClick()
        {
            if (soundSource == null || mouseClick == null) return;
            soundSource.clip = mouseClick;
            soundSource.volume = mouseClickVolume;
            soundSource.Play();
        }
        public void PlayFinalClick()
        {
            if (soundSource == null || finalClick == null) return;
            soundSource.clip = finalClick;
            soundSource.volume = finalClickVolume;
            soundSource.Play();
        }
        public void StartAudio()
        {
            musicSource.Play();
            _fadeOut = true;
        }
        public void StopAudio()
        {
            musicSource.Stop();
        }
        public void FadeAudio(bool fadeIn=true)
        {
            if (fadeIn == true)
            {
                _fadeOut = false;
                musicSource.volume = 0;
                _fadeIn = true;
            }
            else
            {
                musicSource.volume = startVolume;
                _fadeIn = false;
                _fadeOut = true;
            }
        }
        #endregion

        #region Network Events
        public void DisconnectFromServer()
        {
            NetworkManager.networkManager.Disconnect();
        }
        public void DisplayPlayerCardEnter(Photon.Realtime.Player player)
        {
            _playerDisplayedTime = 0;
            PlayerCardText.text = player.NickName + " Joined";
            StartCoroutine(DisplayPlayerCard());
        }
        public void DisplayPlayerCardLeft(Photon.Realtime.Player player)
        {
            _playerDisplayedTime = 0;
            PlayerCardText.text = player.NickName + " Left";
            StartCoroutine(DisplayPlayerCard());
        }
        IEnumerator DisplayPlayerCard()
        {
            PlayerCardPage.SetActive(true);
            PlayerCardAnimation.Play("SlideIn");
            yield return new WaitUntil(() => PlayerCardAnimation.isPlaying == false);
            yield return new WaitUntil(() => _playerDisplayedTime > playerCardDisplayTime);
            PlayerCardAnimation.Play("SlideOut");
            yield return new WaitUntil(() => PlayerCardAnimation.isPlaying == false);
            PlayerCardPage.SetActive(false);
        }
        public void HostAGame()
        {
            if (string.IsNullOrEmpty(RoomNameInput.text))
            {
                DisplayNetworkErrorMessagePage("Attempted to host a game without a session name, not allowed.");
            }
            else if (RoomNameInput.text.Contains("_"))
            {
                DisplayNetworkErrorMessagePage("Invalid Session Name. Not allowed to contain the \"_\" character");
            }
            else if (PhotonNetwork.InLobby)
            {
                foreach (KeyValuePair<string, RoomInfo> room in NetworkManager.networkManager.cachedRoomList)
                {
                    if (Regex.Replace(room.Key, "_.*","") == RoomNameInput.text)
                    {
                        DisplayNetworkErrorMessagePage("A session with that name already exists, choose another.");
                        return;
                    }
                }
                NetworkManager.networkManager.CreateRoom(RoomNameInput.text);
            }
            else
            {
                NetworkManager.networkManager.CreateRoom(RoomNameInput.text);
            }
        }
        public void LeaveDefaultLobby()
        {
            NetworkManager.networkManager.LeaveLobby();
        }
        void SetConnectionStatus(string status)
        {
            foreach (Text conn in connectionStatus)
            {
                conn.text = status;
            }
        }
        public void DisplayNetworkErrorMessagePage(string message)
        {
            JoinGamePage.SetActive(false);
            HostGamePage.SetActive(false);
            MainPage.SetActive(false);
            NetworkErrorText.text = message;
            titleImage.SetActive(false);
            NetworkErrorPage.SetActive(true);
        }
        public void AttemptSetPlayerName()
        {
            if (string.IsNullOrEmpty(NameInputField.text) || 
                NameInputField.text.Contains(":") ||
                NameInputField.text.Contains("_"))
            {
                PlayerNamePage.SetActive(false);
                NameErrorPage.SetActive(true);
            }
            else
            {
                NetworkManager.networkManager.SetPlayerName(NameInputField.text);
                PlayerNamePage.SetActive(false);
                if (successNameInputTravelTo != null)
                {
                    successNameInputTravelTo.SetActive(true);
                }
                else
                {
                    PlayerPrefabSelectPage.SetActive(true);
                }
            }
        }
        #endregion

        #region Joining Options
        public void SetLobbyOwner(bool isOwner)
        {
            _isLobbyOwner = isOwner;
        }
        public void JoinedGameRoom()
        {
            if (_inWaitingLobby == false)
            {
                _inWaitingLobby = true;
                LobbyPage.SetActive(true);
            }
        }
        public void LeftGameRoom()
        {
            _inWaitingLobby = false;
        }
        public void JoinDefaultLobby()
        {
            NetworkManager.networkManager.JoinLobby();
        }
        public void DisplayRoomOptions(List<LobbyItem> scenes)
        {
            if (roomButton == null) return;
            JoinGamePage.SetActive(false);
            ChooseRoomPage.SetActive(true);
            foreach (Transform child in AvailableScenes.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (LobbyItem scene in scenes)
            {
                Debug.Log(scene.displayName + " - " + scene.sceneName + " - " + scene.isVisible + " - " + scene.isOpen);
                if (string.IsNullOrEmpty(scene.sceneName)) scene.sceneName = "Lobby Area";
                if (scene.isOpen)
                {
                    string sceneName = Regex.Replace(scene.rawRoomName, "[0-9,a-z,A-Z]+_", "");
                    string displayName = sceneName;
                    GameObject room = Instantiate(roomButton) as GameObject;
                    room.transform.SetParent(AvailableScenes.transform);
                    room.transform.localScale = new Vector3(1, 1, 1);
                    room.GetComponent<RoomButton>().AddAvailableScene(new LobbyItem(
                        scene.sceneName,
                        scene.rawRoomName,
                        scene.sceneName,
                        scene.playerCount,
                        (string.IsNullOrEmpty(scene.sceneName) || scene.sceneName.Contains("Lobby Area")) ? true : false,
                        scene.isOpen
                    ));
                }
            }
        }
        public void WaitConnectedRefreshRoomList()
        {
            if (_isWaitingForRefresh == true) return;
            StartCoroutine(WaitToBeConnectedThenRefresh());
        }
        IEnumerator WaitToBeConnectedThenRefresh()
        {
            _isWaitingForRefresh = true;
            yield return new WaitUntil(() => PhotonNetwork.InLobby);
            yield return new WaitUntil(() => NetworkManager.networkManager.cachedRoomList.Count > 0);
            _isWaitingForRefresh = false;
            RefreshRoomList();
        }
        public void RefreshRoomList()
        {
            string original_status = connectionStatus[connectionStatus.Length - 1].text;
            SetConnectionStatus("Refreshing room list, please wait...");
            roomList = NetworkManager.networkManager.cachedRoomList;
            Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
            foreach (Transform child in AvailableRooms.transform)
            {
                Destroy(child.gameObject);
            }
            if (roomList.Count > 0)
            {
                GameObject room = null;
                foreach (RoomInfo roomInfo in roomList.Values)
                {
                    if (roomInfo.IsOpen == false) continue;
                    string displayName = Regex.Replace(roomInfo.Name, "_.*", "");
                    string sceneName = (roomInfo.Name.Contains("_")) ? Regex.Replace(roomInfo.Name, ".+_", "") : "";
                    if (rooms.ContainsKey(displayName))
                    {
                        rooms[displayName].GetComponent<RoomButton>().AddAvailableScene(new LobbyItem(
                            displayName,
                            roomInfo.Name,
                            sceneName,
                            roomInfo.PlayerCount,
                            roomInfo.IsVisible,
                            roomInfo.IsOpen
                        ));
                    }
                    else
                    {
                        room = Instantiate(roomButton) as GameObject;
                        room.transform.SetParent(AvailableRooms.transform);
                        room.transform.localScale = new Vector3(1, 1, 1);
                        rooms.Add(displayName, room);
                        room.GetComponent<RoomButton>().AddAvailableScene(new LobbyItem(
                            displayName,
                            roomInfo.Name,
                            sceneName,
                            roomInfo.PlayerCount,
                            roomInfo.IsVisible,
                            roomInfo.IsOpen
                        ));
                    }
                }
                SetConnectionStatus("Connected to lobby, found: " + rooms.Count + " rooms.");
            }
            else
            {
                SetConnectionStatus("Connected to lobby, No available rooms.");
            }
        }
        public Dictionary<string, RoomInfo> GetRoomList()
        {
            return roomList;
        }
        public void JoinRoom(RoomInfo room)
        {
            NetworkManager.networkManager.JoinRoom(room.Name);
            OnJoinGameRoom.Invoke(room);
        }
        #endregion

        #region Player Options
        public void SetReadyState(bool isReady)
        {

        }
        public void SelectPlayerIndex(int index)
        {
            NetworkManager.networkManager.playerPrefab = players[index];
        }
        #endregion
    }
}