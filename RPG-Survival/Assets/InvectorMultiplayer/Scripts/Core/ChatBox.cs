using UnityEngine;
using CBGames.Core;
using Photon.Pun;
using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.Events;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vItemManager;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using static CBGames.Core.CoreDelegates;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/Core/ChatBox")]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Animator))]
    public partial class ChatBox : MonoBehaviour, IChatClientListener
    {
        #region Delegates
        /// <summary>
        /// Called when you have successfully connected to the chat server.
        /// </summary>
        public BasicDelegate OnConnectedToChat;

        /// <summary>
        /// Called when you show/slide in the chatbox.
        /// </summary>
        public BasicDelegate OnShowChatBox;

        /// <summary>
        /// Called when you hide/slide out the chatbox.
        /// </summary>
        public BasicDelegate OnHideChatBox;

        /// <summary>
        /// Called when you receive a any message, data or chat.
        /// </summary>
        /// <param name="message">string type, SentChatMessage type which consists of the player name and the message.</param>
        public SentChatMessageDelegate OnRecieveMessage;

        /// <summary>
        /// Called when you receive a data message in the chat.
        /// </summary>
        /// <param name="type">System.Type type, The system type of this serialized data.</param>
        /// <param name="incomingData">string type, Serialized dictionary data to be parsed by the data channel.</param>
        public ChatDataMessage OnRecieveData;

        /// <summary>
        /// Called when you unsubscribe from any channel.
        /// </summary>
        /// <param name="channels">string[] type, The chat channels that you want to subscribe to</param>
        /// <param name="results">bool[] type, If subscription was successful or not.</param>
        public ChatSubChannels OnSubscribedToChannles;

        /// <summary>
        /// Called when you subscribe to any channel.
        /// </summary>
        /// <param name="channels">string[] type, the channels that you have unsubscribed from</param>
        public ChatChannels OnUnSubscribedFromChannels;

        /// <summary>
        /// Called when another user subscribes to any channel you're in.
        /// </summary>
        /// <param name="channel">string type, the channel the user has subscribe to</param>
        /// <param name="user">string type, the user that has subscribed</param>
        public ChatUserChannel OnUserSubToChannel;

        /// <summary>
        /// Delegate. Called when another user un-subscribes to any channel you're in.
        /// </summary>
        /// /// <param name="channel">string type, the channel the user has subscribe to</param>
        /// <param name="user">string type, the user that has subscribed</param>
        public ChatUserChannel OnUserUnSubFromChannel;
        #endregion

        #region Inspector Variables
        [HideInInspector] public bool showHelpFullChatActions = false;
        [HideInInspector] public bool showInputSettings = false;
        [HideInInspector] public bool showConnectionSettings = false;
        [HideInInspector] public bool showExternalObjectRef = false;
        [HideInInspector] public bool showDynamicGenObj = false;
        [HideInInspector] public bool showChatBoxObjs = false;
        [HideInInspector] public bool showAnimSettings = false;
        [HideInInspector] public bool showSoundSettings = false;
        [HideInInspector] public bool eventSettings = false;

        [HideInInspector] public bool dataEvents = true;
        [HideInInspector] public bool subscribeEvents = false;
        [HideInInspector] public bool messageEvents = false;
        [HideInInspector] public bool enableEvents = false;
        #endregion

        #region Modifiables
        #region Sound Options
        [Tooltip("Where the sound clip will play from.")]
        [SerializeField] protected AudioSource source = null;
        [Tooltip("The notification sound you want to play")]
        [SerializeField] protected AudioClip chatNotification = null;
        [Range(0,1)]
        [Tooltip("How loud you want to play the notification sound.")]
        [SerializeField] protected float notificationVolume = 0.5f;
        #endregion

        #region Animation Options
        [Tooltip("The animator that controls the slide in/out actions of the chatbox.")]
        [SerializeField] protected Animator chatAnim = null;
        [Tooltip("The name of the animation to play when the chat box is enabled. If you have a chatAnim component supplied.")]
        [SerializeField] protected string slideIn = "Slide_In";
        [Tooltip("The name of the animation to play when the chat box is disabled. If you have a chatAnim component supplied.")]
        [SerializeField] protected string slideOut = "Slide_Out";
        #endregion

        #region Chatbox GameObjects
        [Tooltip("The object that holds everything for your chat UI.")]
        [SerializeField] protected GameObject parentChatObj = null;
        [Tooltip("The input field where you can type a chat message.")]
        [SerializeField] protected InputField msgInput = null;
        [Tooltip("The text element that will display the connection status.")]
        [SerializeField] protected Text connectionStatus = null;
        [Tooltip("The object that will hold all the generate message objects.")]
        [SerializeField] protected GameObject messagesObj = null;
        [Tooltip("The scroll rect that controls the scrolling of the chatbox.")]
        [SerializeField] protected ScrollRect scrollRect = null;
        [Tooltip("The icon that will appear when a new message is recieved.")]
        [SerializeField] protected GameObject newMessageIcon = null;
        [Tooltip("Only display the new message icon when the chat window is closed.")]
        [SerializeField] protected bool onlyWhenWindowClose = true;
        #endregion

        #region External Object References
        [Tooltip("The network manager to reference. This will be things like the game version.")]
        public NetworkManager nm = null;
        #endregion

        #region Dynamically Generate Objects
        [Tooltip("The gameobject holding the \"ChatMessage\" component that will be generated when someone else sends you a message.")]
        [SerializeField] protected GameObject otherChatMessage = null;
        [Tooltip("The gameobject holding the \"ChatMessage\" component that will be generated when you send a message.")]
        [SerializeField] protected GameObject yourChatMessage = null;
        #endregion

        #region Helpful Actions
        [Tooltip("Whether or not you want to automatically scroll to the bottom when a new message comes in.")]
        [SerializeField] protected bool autoScroll = true;
        [Tooltip("Start with the chatbox out and selected.")]
        [SerializeField] protected bool startEnabled = false;
        [Tooltip("When successfully connected display the chatbox.")]
        [SerializeField] protected bool enableOnConnect = true;
        [SerializeField] protected bool debugging = false;
        #endregion

        #region  Connection Settings
        [Tooltip("The default channel to subscribe to if you don't set the channel in other ways.")]
        [SerializeField] protected string chatChannel = "worldChat";
        [Tooltip("The protocol to use for your chat client")]
        [SerializeField] protected ConnectionProtocol protocol = ConnectionProtocol.Udp;
        [Tooltip("The region you want to have this chat client be in.")]
        [SerializeField] protected PhotonChatRegions region = PhotonChatRegions.US;
        [Tooltip("What type of authenication you want to use with this chat.")]
        [SerializeField] protected Photon.Chat.CustomAuthenticationType authType = Photon.Chat.CustomAuthenticationType.None;
        [Space(10)]
        #endregion

        #region Player Input Actions
        [Tooltip("The buttons that are responsible for opening the chat window")]
        [SerializeField] protected List<string> openChatWindowOnPress = new List<string>();
        [Tooltip("The buttons that are responsible for closing the chat window")]
        [SerializeField] protected List<string> closeWindowOnPress = new List<string>();
        [Tooltip("Send your message on this keyboard press.")]
        [SerializeField] protected List<string> sendChatOnPress = new List<string>();
        #endregion

        #region Unity Events
        [Space(10)]
        [Tooltip("UnityEvent. Called whenever ANY chat message is received.")]
        public SentChatUnityEvent ReceivedAnyChatMessage;
        [Tooltip("UnityEvent. Called whenver you receive a chat message from someone else.")]
        public SentChatUnityEvent ReceivedOtherPlayerChatMessage;
        [Tooltip("UnityEvent. Called when you receive a Broadcast data message type.")]
        public BroadCastUnityEvent OnReceiveBroadcastMessage;

        [Tooltip("UnityEvent. Called when you enable the chatbox.")]
        public UnityEvent ChatEnabled;
        [Tooltip("UnityEvent. Called when you disable the chatbox.")]
        public UnityEvent ChatDisabled;

        [Tooltip("UnityEvent. Called when YOU as the owner subscribe to a any new chat channel.")]
        public StringUnityEvent OnYouSubscribeToAnyChannel;
        [Tooltip("UnityEvent. Called when YOU as the owner un-subscribe from any chat channel.")]
        public StringUnityEvent OnYouUnSubscribeFromAnyChannel;

        [Tooltip("UnityEvent. Called when YOU as the owner subscribe to the data chat channel.")]
        public UnityEvent OnYouSubscribeToDataChannel;
        [Tooltip("UnityEvent. Called when YOU as the owner un-subscribe from the data chat channel.")]
        public UnityEvent OnYouUnSubscribeToDataChannel;

        [Tooltip("UnityEvent. Called when another player subscribes to the data chat channel.")]
        public StringUnityEvent OnUserSubscribedToDataChannel;
        [Tooltip("UnityEvent. Called when another player un-subscribes from the data chat channel.")]
        public StringUnityEvent OnUserUnSubscribedToDataChannel;
        #endregion
        #endregion

        #region Interanl Variables
        protected string chatClientUserId = "";
        protected string uniqueHash = "";
        protected Photon.Chat.AuthenticationValues authValues;
        protected string _subToChannel = "";
        protected internal AppSettings chatAppSettings;
        protected ChatClient chatClient;
        protected bool chatActive = false;
        protected List<Text> texts = new List<Text>();
        protected List<Image> images = new List<Image>();
        protected vThirdPersonCamera playerCam = null;
        protected vThirdPersonController player = null;
        protected vMeleeCombatInput meleeInput = null;
        protected vInventory playerInventory = null;
        protected bool orgLockCam = false;
        protected CursorLockMode orgCursorMode;
        protected List<string> subbedChannels = new List<string>();
        #endregion

        #region Inits
        /// <summary>
        /// Used to unlock the mouse and make the mouse cursor visible. Also 
        /// generates your players unique hash for the chat. Finally makes sure 
        /// the chat icon is turned off.
        /// </summary>
        protected virtual void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            DisplayNewChatIcon(false);
            uniqueHash = StaticMethods.RandomHash(10);
        }

        /// <summary>
        /// Used to setup various settings in the chatbox like chat length, chat 
        /// app settings, authentication values, finding player camera, finding 
        /// your player, etc.
        /// </summary>
        protected virtual void Start()
        {
            nm = (nm == null) ? FindObjectOfType<NetworkManager>() : nm;
            FindPlayerCam();
            SetMyPlayer();

            if (chatAnim == null && GetComponent<Animator>())
            {
                chatAnim = GetComponent<Animator>();
            }
            
            Application.runInBackground = true;
            //#if PHOTON_UNITY_NETWORKING
            chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
            //#endif
            if (string.IsNullOrEmpty(chatAppSettings.AppIdChat))
            {
                Debug.LogError("Chat - The chat id is empty in your photon server settings! The chat will not work until this is populated!");
                return;
            }
            authValues = new Photon.Chat.AuthenticationValues();
            images.AddRange(GetComponentsInChildren<Image>());
            texts.AddRange(GetComponentsInChildren<Text>());
            EnableChat(startEnabled);
            if (startEnabled == false)
            {
                EnableVisualBox(false);
            }
            SceneManager.sceneLoaded += SceneLoaded;
        }
        #endregion
        
        /// <summary>
        /// Find the camera that you use to look around with your player.
        /// </summary>
        protected virtual void FindPlayerCam()
        {
            playerCam = FindObjectOfType<vThirdPersonCamera>();
            if (playerCam)
            {
                orgLockCam = playerCam.lockCamera;
            }
        }

        /// <summary>
        /// Callback method. Called when a scene has successfully loaded.
        /// This will call 'FindPlayerCam' function.
        /// </summary>
        /// <param name="scene">Scene type</param>
        /// <param name="mode">LoadSceneMode type</param>
        protected virtual void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindPlayerCam();
        }
        
        /// <summary>
        /// Used to constantly ping the chat master server to keep that 
        /// chatbox alive and capture new messages. Also watches for player 
        /// input to enable/disable the chatbox.
        /// </summary>
        protected virtual void Update()
        {
            if (chatClient != null)
            {
                if (PhotonNetwork.IsConnected == false)
                {
                    chatClient.Disconnect();
                    chatClient = null;
                }
                else
                {
                    chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
                }
                if (!string.IsNullOrEmpty(msgInput.text) && IsPressingButton(sendChatOnPress))
                {
                    SendMessage(chatChannel, msgInput.text);
                    msgInput.text = "";
                }
                if ((chatActive == true && IsPressingButton(closeWindowOnPress)) ||
                    (chatActive == false && IsPressingButton(openChatWindowOnPress)))
                {
                    msgInput.text = "";
                    chatActive = !chatActive;
                    EnableChat(chatActive);
                }
            }
        }

        #region Messages
        /// <summary>
        /// Call this function to send data to a channel name to 
        /// every player subscribed to that channel. Call 'BuildDataObj' 
        /// function to format and build the serialized data string to
        /// send.
        /// </summary>
        /// <param name="channelName">string type, The name of the channel to send data over, must be subscribed to it.</param>
        /// <param name="data">object type, the data to send in the channelName</param>
        public virtual void BroadcastData(string channelName, object data)
        {
            if (chatClient == null) return;
            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            dataObject = BuildDataObj(data);

            if (debugging == true) Debug.Log("Sending data chat...");
            chatClient.PublishMessage(channelName, dataObject);
        }

        /// <summary>
        /// Take an object and return a serialized version of it to send 
        /// in data chatbox channels.
        /// </summary>
        /// <param name="data">object type, the data you want to serialize</param>
        /// <returns>Serialized data that is compatible with the chatbox's data channel</returns>
        public virtual Dictionary<string, string> BuildDataObj(object data)
        {
            Dictionary<string, string> dataObject = new Dictionary<string, string>();
            if (debugging == true) Debug.Log("Preping object data...");
            dataObject.Add(data.GetType().ToString(), JsonUtility.ToJson(data));

            return dataObject;
        }

        /// <summary>
        /// Send a string message to a channel name  that you're already subscribed to.
        /// </summary>
        /// <param name="channelName">string type, the channel name you want to send a message to.</param>
        /// <param name="message">string type, the actual message to send.</param>
        public virtual void SendMessage(string channelName, string message)
        {
            chatClient.PublishMessage(channelName, message);
        }

        /// <summary>
        /// Receive a message that was sent by another player and instantiate a new line 
        /// in the chatbox with this information. Then trigger to display the chat icon.
        /// </summary>
        /// <param name="incoming">SentChatMessage type, consists of a player name and the message</param>
        public virtual void ReceiveNewMessage(SentChatMessage incoming)
        {
            GameObject newMessage = null;
            if (incoming.playerName == chatClient.UserId)
            {
                newMessage = Instantiate(yourChatMessage, messagesObj.transform.position, messagesObj.transform.rotation);
            }
            else
            {
                newMessage = Instantiate(otherChatMessage, messagesObj.transform.position, messagesObj.transform.rotation);
                ReceivedOtherPlayerChatMessage.Invoke(incoming);
            }
            newMessage.GetComponent<ChatMessage>().SetMessage(incoming);
            newMessage.transform.SetParent(messagesObj.transform);
            newMessage.transform.localScale = new Vector3(1, 1, 1);
            if (autoScroll == true)
            {
                AutoScrollToBottom();
            }
            DisplayNewChatIcon(true);
            ReceivedAnyChatMessage.Invoke(incoming);
            if (OnRecieveMessage != null) OnRecieveMessage.Invoke(incoming);
        }
        
        /// <summary>
        /// Use to receive serialized data convert it back to its original type and call
        /// an action based on the type of data that was recieved.
        /// </summary>
        /// <param name="type">System.Type type, The type of data object this is</param>
        /// <param name="incomingData">string type, The serialized data</param>
        public virtual void ReceiveData(Type type, string incomingData)
        {
            if (debugging == true) Debug.Log("Recieved " + type.ToString() + " data!");
            if (type == typeof(ObjectAction))
            {
                if (debugging == true) Debug.Log("Updating Scene database with data: " + JsonUtility.FromJson<ObjectAction>(incomingData));
                NetworkManager.networkManager.UpdateSceneDatabase(JsonUtility.FromJson<ObjectAction>(incomingData));
            }
            else if (type == typeof(BroadCastMessage))
            {
                if (debugging == true) Debug.Log("Sending BroadCastMessage: " + JsonUtility.FromJson<BroadCastMessage>(incomingData));
                OnReceiveBroadcastMessage.Invoke(JsonUtility.FromJson<BroadCastMessage>(incomingData));
            }
            else if (type == typeof(CallPlayerFunction))
            {
                foreach(vThirdPersonController controller in FindObjectsOfType<vThirdPersonController>())
                {
                    CallPlayerFunction functionInfo = JsonUtility.FromJson<CallPlayerFunction>(incomingData);
                    if (string.IsNullOrEmpty(functionInfo.methodArg))
                    {
                        controller.SendMessage(functionInfo.methodName);
                    }
                    else
                    {
                        controller.SendMessage(functionInfo.methodName, functionInfo.methodArg);
                    }
                }
            }
            else if (type == typeof(PlayerListInfo))
            {
                PlayerList playerList = FindObjectOfType<PlayerList>();
                if (playerList != null)
                {
                    FindObjectOfType<PlayerList>().UpdatePlayer(JsonUtility.FromJson<PlayerListInfo>(incomingData));
                }
            }
            if (OnRecieveData != null) OnRecieveData.Invoke(type, incomingData);
        }
        #endregion

        #region Channels
        /// <summary>
        /// The channel name you want to subscribe to.
        /// </summary>
        /// <param name="channelName">string type, the name of the channel</param>
        public virtual void SubscribeToChannel(string channelName)
        {
            if (chatClient == null)
            {
                _subToChannel = channelName;
                Connect();
            }
            else
            {
                if (debugging == true) Debug.Log("Subscribe from public channel " + channelName + "...");
                chatClient.Subscribe(channelName, creationOptions: new ChannelCreationOptions { PublishSubscribers = true });
            }
        }
        
        /// <summary>
        /// The channel name you want to un-subscribe from.
        /// </summary>
        /// <param name="channelName">string type, the name of the channel</param>
        public virtual void UnSubscribeToChannel(string channelName)
        {
            if (chatClient != null)
            {
                if (debugging == true) Debug.Log("Unsubscribe from public channel " + channelName);
                chatClient.Unsubscribe(new string[] { channelName });
            }
        }
        
        /// <summary>
        /// Returns a list of all the channel names that you're subscribed to
        /// </summary>
        /// <returns>List of channel names your subscribed to</returns>
        public virtual List<string> GetSubscribedChannels()
        {
            return subbedChannels;
        }

        /// <summary>
        /// Un-subscribe from the previous 'active' channel and subscribe to this channel, 
        /// making it your currently active one.
        /// </summary>
        /// <param name="newChannel">string type, the channel name.</param>
        public virtual void SetActiveChannel(string newChannel)
        {
            if (chatClient != null && chatClient.State == ChatState.ConnectedToFrontEnd)
            {
                chatClient.Unsubscribe(new string[1] { chatChannel });
            }
            chatChannel = newChannel;
            if (chatClient != null && chatClient.State == ChatState.ConnectedToFrontEnd)
            {
                chatClient.Subscribe(new string[1] { chatChannel });
            }
        }
        
        /// <summary>
        /// Unsubscribe from the previous 'active' channel and subscribe to a channel
        /// name that is the same as the current Photon Room you're in.
        /// </summary>
        public virtual void SetActiveRoomAsChannelName()
        {
            if (chatClient != null && PhotonNetwork.InRoom == true && chatClient.State == ChatState.ConnectedToFrontEnd)
            {
                chatClient.Unsubscribe(new string[1] { chatChannel });
                chatChannel = PhotonNetwork.CurrentRoom.Name;
                chatClient.Subscribe(new string[1] { chatChannel });
            }
            else if (chatClient != null && PhotonNetwork.InRoom == true)
            {
                chatChannel = PhotonNetwork.CurrentRoom.Name;
            }
        }
        #endregion

        #region Server Connect/Disconnect
        /// <summary>
        /// Connect to the chat server.
        /// </summary>
        public virtual void Connect()
        {
            if (chatClient != null) return;
            connectionStatus.text = "Connecting...";

            //Set connection protocol
            chatClient = new ChatClient(this, protocol);
            chatClient.ChatRegion = region.ToString();

            //Set UserId/Nickname
            chatClientUserId = PhotonNetwork.NickName + ":" + uniqueHash;
            authValues.UserId = PhotonNetwork.NickName + ":" + uniqueHash;
            authValues.AuthType = authType;

            //Connect using the above settings
            chatClient.Connect(chatAppSettings.AppIdChat, nm.gameVersion.ToString(), authValues);
        }
        
        /// <summary>
        /// Disconnect from the chat server. This is only used 
        /// in scenarios with things that need a string input.
        /// Simply calls the Disconnect() function.
        /// </summary>
        /// <param name="placeholder">Empty placeholder, does nothing.</param>
        public virtual void Disconnect(string placeholder = "")
        {
            Disconnect();
        }

        /// <summary>
        /// Disconnect from the chat server.
        /// </summary>
        public virtual void Disconnect()
        {
            if (chatClient != null)
            {
                chatClient.Disconnect();
            }
        }
        #endregion

        #region Enable/Disable Chat
        /// <summary>
        /// Enable/Disable the chatbox, Lock/Unlock the mouse, Hide/Show the mouse,
        /// Lock/Unlock camera movement, slide in/slide out the chatbox and selects/
        /// deselects the inputfield.
        /// </summary>
        /// <param name="enabled"></param>
        public virtual void EnableChat(bool enabled)
        {
            if (debugging == true) Debug.Log("Chat enabled: " + enabled);

            chatActive = enabled;
            if (player)
            {
                player.lockMovement = enabled;
                player.lockRotation = enabled;
                if (player && player.GetComponent<vItemManager>() && enabled == true)
                {
                    player.GetComponent<vItemManager>().inventory.CloseInventory();
                }
            }
            if (meleeInput)
            {
                meleeInput.SetLockAllInput(enabled);
            }
            if (playerInventory)
            {
                playerInventory.lockInventoryInput = enabled;
            }
            if (nm.IsInRoom() == true && PhotonNetwork.IsConnected == true)
            {
                Cursor.visible = enabled;
                Cursor.lockState = (enabled == true) ? CursorLockMode.None : orgCursorMode;
            }

            if (enabled == true)
            {
                if (playerCam)
                {
                    playerCam.lockCamera = true;
                }
                else
                {
                    FindPlayerCam();
                    if (playerCam)
                    {
                        playerCam.lockCamera = true;
                    }
                }
                EnableVisualBox(true);
                chatAnim.Play(slideIn, 0);
                msgInput.ActivateInputField();
                msgInput.Select();
                ChatEnabled.Invoke();
                DisplayNewChatIcon(false);
                if (OnShowChatBox != null) OnShowChatBox.Invoke();
            }
            else
            {
                if (playerCam)
                {
                    playerCam.lockCamera = orgLockCam;
                }
                else
                {
                    FindPlayerCam();
                    if (playerCam)
                    {
                        playerCam.lockCamera = orgLockCam;
                    }
                }
                chatAnim.Play(slideOut, 0);
                msgInput.MoveTextEnd(false);
                ChatDisabled.Invoke();
                if (OnHideChatBox != null) OnHideChatBox.Invoke();
            }
        }
        
        /// <summary>
        /// Returns true or false if the chatbox is active
        /// </summary>
        /// <returns>True or False, The chatbox is active?</returns>
        public virtual bool IsEnabled()
        {
            return chatActive;
        }
        
        /// <summary>
        /// Hide/Show the actual chatbox (not sliding in or out), actually
        /// enable of disable the object itself.
        /// </summary>
        /// <param name="enabled"></param>
        public virtual void EnableVisualBox(bool enabled)
        {
            parentChatObj.SetActive(enabled);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns your unique userid for the chatbox
        /// </summary>
        /// <returns>your unique user id</returns>
        public virtual string GetUserId()
        {
            return chatClientUserId;
        }

        /// <summary>
        /// Returns the subscribers of this channel.
        /// </summary>
        /// <param name="channel">string type, the channel name</param>
        /// <returns>list of users currently subscribed to this channel</returns>
        public virtual List<string> GetChannelSubscribers(string channel)
        {
            ChatChannel targetChannel;
            chatClient.TryGetChannel(channel, out targetChannel);
            if (targetChannel != null)
            {
                List<string> subs = new List<string>();
                subs.AddRange(targetChannel.Subscribers);
                return subs;
            }
            else
            {
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Returns true or false if you're currently subscribed to 
        /// the data channel.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConnectedToDataChannel()
        {
            if (string.IsNullOrEmpty(NetworkManager.networkManager.GetChatDataChannel()))
            {
                return false;
            }
            return subbedChannels.Contains(NetworkManager.networkManager.GetChatDataChannel());
        }

        /// <summary>
        /// Internal used function, true or false if the passed in object is 
        /// a dictionary or not. Used as part of the data channel parsing.
        /// </summary>
        /// <param name="o">object type, the object to check</param>
        /// <returns>true if dictionary, false if not</returns>
        public virtual bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        /// <summary>
        /// Make the scroll view of the chatbox scroll to the bottom.
        /// </summary>
        public virtual void AutoScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        /// <summary>
        /// Returns true if pressing one of the passed in button names.
        /// </summary>
        /// <param name="codes">List of strings type, The button codes to check</param>
        /// <returns>true if pressing any one of these passed in button codes</returns>
        private bool IsPressingButton(List<string> codes)
        {
            foreach(string code in codes)
            {
                if (Input.GetButtonDown(code))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds your player and populates internal variables so the chatbox
        /// can enable or disable various aspects of your character when the 
        /// box is open or closed.
        /// </summary>
        protected virtual void SetMyPlayer()
        {
            foreach (vThirdPersonController controller in FindObjectsOfType<vThirdPersonController>())
            {
                if (controller.gameObject.GetComponent<PhotonView>().IsMine == true)
                {
                    player = controller;
                    if (player.GetComponent<vMeleeCombatInput>())
                    {
                        meleeInput = player.GetComponent<vMeleeCombatInput>();
                    }
                    if (player.GetComponent<vInventory>())
                    {
                        playerInventory = player.GetComponent<vInventory>();
                    }
                    if (!playerInventory && player.GetComponent<vItemManager>())
                    {
                        playerInventory = player.GetComponent<vItemManager>().inventory;
                    }
                    break;
                }
            }
            orgCursorMode = Cursor.lockState;
        }
        #endregion

        #region Visuals & Sounds
        /// <summary>
        /// Enable or disable the chat icon.
        /// </summary>
        /// <param name="enabled">bool type, true = on/enabled</param>
        public virtual void DisplayNewChatIcon(bool enabled)
        {
            if (newMessageIcon == null) return;
            if (onlyWhenWindowClose == false || (onlyWhenWindowClose == true && chatActive == false))
            {
                newMessageIcon.SetActive(enabled);
                if (enabled == true)
                {
                    PlayChatNotificationSound();
                }
            }
        }

        /// <summary>
        /// Play the notification sound from the specific sound source in
        /// the parameters.
        /// </summary>
        public virtual void PlayChatNotificationSound()
        {
            if (source == null || chatNotification == null) return;
            source.clip = chatNotification;
            source.volume = notificationVolume;
            source.Play();
        }
        #endregion

        #region Required IChatClientListener Methods
        /// <summary>
        /// For debugging purposes only. Used to debug information
        /// </summary>
        /// <param name="level">DebugLevel type</param>
        /// <param name="message">string type, the message that was recieved</param>
        public virtual void DebugReturn(DebugLevel level, string message)
        {
            if (debugging == true)
            {
                Debug.Log("DebugReturn - DebugLevel: " + level + " message: " + message);
            }
        }
        
        /// <summary>
        /// Callback method. Called when you get disconnected from the chat server
        /// </summary>
        public virtual void OnDisconnected()
        {
            connectionStatus.text = "Disconnected";
            EnableChat(false);
            if (debugging == true)
            {
                Debug.Log("Chat disconnected");
            }
            chatClient = null;
        }
        
        /// <summary>
        /// Callback method. Called when you are connected to the chat server
        /// </summary>
        public virtual void OnConnected()
        {
            if (debugging == true)
            {
                Debug.Log("Chat connected");
            }
            connectionStatus.text = "Connected";
            if (OnConnectedToChat != null) OnConnectedToChat.Invoke();
            chatClient.Subscribe(new string[] { chatChannel });
            if (!player)
            {
                SetMyPlayer();
            }
            if (enableOnConnect == true)
            {
                if (debugging == true) Debug.Log("Enabled On Connect is true!");
                EnableChat(true);
            }
            if (!string.IsNullOrEmpty(_subToChannel))
            {
                SubscribeToChannel(_subToChannel);
                _subToChannel = "";
            }
        }

        /// <summary>
        /// Callback method. Called whenever your connection status changes
        /// </summary>
        /// <param name="state">ChatState type, the current connection state</param>
        public virtual void OnChatStateChange(ChatState state)
        {
            if (debugging == true)
            {
                Debug.Log("Chat state changed: " + state);
            }
        }

        /// <summary>
        /// Callback method. Called whenever you receive a new message on ANY subscribed channel
        /// </summary>
        /// <param name="channelName">string type, The channel name that is receiving the message</param>
        /// <param name="senders">string type, Who is sending the message</param>
        /// <param name="messages">string type, The actual string message</param>
        public virtual void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            if (debugging == true) Debug.Log("Receiving new chat message...");
            int msgCount = messages.Length;
            for (int i = 0; i < msgCount; i++)
            {
                string sender = senders[i];
                if (IsDictionary(messages[i]))
                {
                    if (debugging == true) Debug.Log("Message is data...");
                    foreach (KeyValuePair<string, string> target in messages[i] as Dictionary<string, string>)
                    {
                        ReceiveData(Type.GetType(target.Key), target.Value);
                    }
                }
                else
                {
                    string msg = (string)messages[i];
                    ReceiveNewMessage(new SentChatMessage(sender, msg));
                    if (debugging == true)
                    {
                        Debug.Log(sender + ": " + msg);
                    }
                }
            }
        }

        /// <summary>
        /// Callback method. Called whenever you receive a private message
        /// </summary>
        /// <param name="sender">string type, Who is sending the mesage</param>
        /// <param name="message">string type, The actual message</param>
        /// <param name="channelName">string type, The channel name receiving the message</param>
        public virtual void OnPrivateMessage(string sender, object message, string channelName)
        {
            if (debugging == true)
            {
                Debug.Log("Received private message from " + sender + " on channel name: " + channelName + " - MESSAGE: " + message);
            }
            if (IsDictionary(message))
            {
                if (debugging == true) Debug.Log("Private message is data...");
                foreach (KeyValuePair<string, string> target in message as Dictionary<string, string>)
                {
                    ReceiveData(Type.GetType(target.Key), target.Value);
                }
            }
            else
            {
                ReceiveNewMessage(new SentChatMessage(sender, (string)message));
            }
        }

        /// <summary>
        /// Callback method. Called when you subscribed to a channel.
        /// </summary>
        /// <param name="channels">Array type, All the channels you tried subscribed to</param>
        /// <param name="results">Array type, the success or failure of this subscription</param>
        public virtual void OnSubscribed(string[] channels, bool[] results)
        {
            foreach (string channel in channels)
            {
                if (debugging == true) Debug.Log("You subscribed to channel: " + channel);
                subbedChannels.Add(channel);
                OnYouSubscribeToAnyChannel.Invoke(channel);
                ChatChannel dataChannel;
                chatClient.TryGetChannel(channel, out dataChannel);
                if (channel == NetworkManager.networkManager.GetChatDataChannel())
                {
                    OnYouSubscribeToDataChannel.Invoke();
                    NetworkManager.networkManager.SetInDataChannel(true);
                }
            }
            if (OnSubscribedToChannles != null) OnSubscribedToChannles.Invoke(channels, results);
        }

        /// <summary>
        /// Callback method. Called when a user's status has updated.
        /// </summary>
        /// <param name="user">string type, the user that updated</param>
        /// <param name="status">int type, the users current status</param>
        /// <param name="gotMessage">bool type, recieved the message</param>
        /// <param name="message">object type, the messsage that was sent</param>
        public virtual void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
            if (debugging == true)
            {
                Debug.Log("Status was updated - USER: " + user + " STATUS: " + status + " gotMessage: " + gotMessage + " MESSAGE: " + message);
            }
        }

        /// <summary>
        /// Callback method. Called when you unsubscribe from a channel
        /// </summary>
        /// <param name="channels">Array type, channel that were un-subscribed from</param>
        public virtual void OnUnsubscribed(string[] channels)
        {
            foreach (string channel in channels)
            {
                if (debugging == true) Debug.Log("You Unsubscribed From Channel: " + channel);
                subbedChannels.Remove(channel);
                OnYouUnSubscribeFromAnyChannel.Invoke(channel);
                if (channel == NetworkManager.networkManager.GetChatDataChannel())
                {
                    OnYouUnSubscribeToDataChannel.Invoke();
                    NetworkManager.networkManager.SetInDataChannel(false);
                }
            }
            if (OnUnSubscribedFromChannels != null) OnUnSubscribedFromChannels.Invoke(channels);
        }

        /// <summary>
        /// Callback method. Called when another user subscribes to a channel.
        /// </summary>
        /// <param name="channel">string type, the channel that was subscribed to</param>
        /// <param name="user">string type, the user that subscribed</param>
        public virtual void OnUserSubscribed(string channel, string user)
        {
            if (debugging == true)
            {
                Debug.Log("User: " + user + " subscribed to channel: " + channel);
            }
            if (channel == NetworkManager.networkManager.GetChatDataChannel())
            {
                OnUserSubscribedToDataChannel.Invoke(user);
            }
            if (OnUserSubToChannel != null) OnUserSubToChannel.Invoke(channel, user);
        }

        /// <summary>
        /// Callback method. Called when another user un-subscribes from a channel.
        /// </summary>
        /// <param name="channel">string type, the channel that was unsubscribed from</param>
        /// <param name="user">string type, the user that unsubscribed</param>
        public virtual void OnUserUnsubscribed(string channel, string user)
        {
            if (debugging == true)
            {
                Debug.Log("User: " + user + " unsubscribed from channel: " + channel);
            }
            if (channel == NetworkManager.networkManager.GetChatDataChannel())
            {
                OnUserUnSubscribedToDataChannel.Invoke(user);
            }
            if (OnUserUnSubFromChannel != null) OnUserUnSubFromChannel.Invoke(channel, user);
        }
        #endregion

        #region Extending Built-In Methods
        /// <summary>
        /// To avoid that the Editor becoming unresponsive, disconnect all Photon connections in OnDestroy.
        /// </summary>
        public virtual void OnDestroy()
        {
            if (chatClient != null)
            {
                chatClient.Disconnect();
                EnableChat(false);
            }
        }

        /// <summary>
        /// To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.
        /// </summary>
        public virtual void OnApplicationQuit()
        {
            if (chatClient != null)
            {
                chatClient.Disconnect();
            }
        }
        #endregion
    }
}