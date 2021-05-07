using UnityEngine;
using UnityEditor;
using CBGames.UI;
using CBGames.Editors;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(ChatBox), true)]
    public class ChatBoxInspector : BaseEditor
    {
        #region CustomEditorVariables
        List<string> availableInputs = new List<string>();
        #endregion

        #region Properties
        #region Sounds Options
        SerializedProperty source;
        SerializedProperty chatNotification;
        SerializedProperty notificationVolume;
        #endregion

        #region Animation Options
        SerializedProperty chatAnim;
        SerializedProperty slideIn;
        SerializedProperty slideOut;
        #endregion

        #region ChatBox GameObjects
        SerializedProperty parentChatObj;
        SerializedProperty msgInput;
        SerializedProperty connectionStatus;
        SerializedProperty messagesObj;
        SerializedProperty scrollRect;
        SerializedProperty newMessageIcon;
        SerializedProperty onlyWhenWindowClose;
        #endregion

        #region Dynamically Generated Objects
        SerializedProperty otherChatMessage;
        SerializedProperty yourChatMessage;
        #endregion

        #region External Object References
        SerializedProperty nm;
        #endregion

        #region Helpful ChatBox Actions
        SerializedProperty autoScroll;
        SerializedProperty startEnabled;
        SerializedProperty enableOnConnect;
        SerializedProperty debugging;
        #endregion

        #region Connection Settings
        SerializedProperty chatChannel;
        SerializedProperty protocol;
        SerializedProperty region;
        SerializedProperty authType;
        #endregion

        #region Input Settings
        SerializedProperty openChatWindowOnPress;
        SerializedProperty closeWindowOnPress;
        SerializedProperty sendChatOnPress;
        #endregion

        #region UnityEvents
        SerializedProperty OnReceiveBroadcastMessage;
        SerializedProperty OnYouSubscribeToAnyChannel;
        SerializedProperty OnYouUnSubscribeFromAnyChannel;
        SerializedProperty ReceivedAnyChatMessage;
        SerializedProperty ReceivedOtherPlayerChatMessage;
        SerializedProperty ChatEnabled;
        SerializedProperty ChatDisabled;
        SerializedProperty OnUserSubscribedToDataChannel;
        SerializedProperty OnUserUnSubscribedToDataChannel;
        SerializedProperty OnYouSubscribeToDataChannel;
        #endregion
        #endregion

        #region DropDown Booleans
        SerializedProperty showHelpFullChatActions;
        SerializedProperty showInputSettings;
        SerializedProperty showConnectionSettings;
        SerializedProperty showExternalObjectRef;
        SerializedProperty showDynamicGenObj;
        SerializedProperty showChatBoxObjs;
        SerializedProperty showAnimSettings;
        SerializedProperty showSoundSettings;
        SerializedProperty eventSettings;
        #endregion

        #region Event Booleans
        SerializedProperty dataEvents;
        SerializedProperty subscribeEvents;
        SerializedProperty messageEvents;
        SerializedProperty enableEvents;
        #endregion

        protected override void OnEnable()
        {
            
            #region Properties
            #region Sounds Options
            source = serializedObject.FindProperty("source");
            chatNotification = serializedObject.FindProperty("chatNotification");
            notificationVolume = serializedObject.FindProperty("notificationVolume");
            #endregion

            #region Animation Options
            chatAnim = serializedObject.FindProperty("chatAnim");
            slideIn = serializedObject.FindProperty("slideIn");
            slideOut = serializedObject.FindProperty("slideOut");
            #endregion

            #region ChatBox GameObjects
            parentChatObj = serializedObject.FindProperty("parentChatObj");
            msgInput = serializedObject.FindProperty("msgInput");
            connectionStatus = serializedObject.FindProperty("connectionStatus");
            messagesObj = serializedObject.FindProperty("messagesObj");
            scrollRect = serializedObject.FindProperty("scrollRect");
            newMessageIcon = serializedObject.FindProperty("newMessageIcon");
            onlyWhenWindowClose = serializedObject.FindProperty("onlyWhenWindowClose");
            #endregion

            #region Dynamically Generate Objects
            otherChatMessage = serializedObject.FindProperty("otherChatMessage");
            yourChatMessage = serializedObject.FindProperty("yourChatMessage");
            #endregion

            #region External Objects References
            nm = serializedObject.FindProperty("nm");
            #endregion

            #region Helpful ChatBox Actions
            autoScroll = serializedObject.FindProperty("autoScroll");
            startEnabled = serializedObject.FindProperty("startEnabled");
            enableOnConnect = serializedObject.FindProperty("enableOnConnect");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            #region Connection Settings
            chatChannel = serializedObject.FindProperty("chatChannel");
            protocol = serializedObject.FindProperty("protocol");
            region = serializedObject.FindProperty("region");
            authType = serializedObject.FindProperty("authType");
            #endregion

            #region Input Settings
            openChatWindowOnPress = serializedObject.FindProperty("openChatWindowOnPress");
            closeWindowOnPress = serializedObject.FindProperty("closeWindowOnPress");
            sendChatOnPress = serializedObject.FindProperty("sendChatOnPress");
            #endregion

            #region UnityEvents
            OnReceiveBroadcastMessage = serializedObject.FindProperty("OnReceiveBroadcastMessage");
            OnYouSubscribeToAnyChannel = serializedObject.FindProperty("OnYouSubscribeToAnyChannel");
            OnYouUnSubscribeFromAnyChannel = serializedObject.FindProperty("OnYouUnSubscribeFromAnyChannel");
            ReceivedAnyChatMessage = serializedObject.FindProperty("ReceivedAnyChatMessage");
            ReceivedOtherPlayerChatMessage = serializedObject.FindProperty("ReceivedOtherPlayerChatMessage");
            ChatEnabled = serializedObject.FindProperty("ChatEnabled");
            ChatDisabled = serializedObject.FindProperty("ChatDisabled");
            OnUserSubscribedToDataChannel = serializedObject.FindProperty("OnUserSubscribedToDataChannel");
            OnUserUnSubscribedToDataChannel = serializedObject.FindProperty("OnUserUnSubscribedToDataChannel");
            OnYouSubscribeToDataChannel = serializedObject.FindProperty("OnYouSubscribeToDataChannel");
            #endregion
            #endregion

            #region Dropdowns
            showHelpFullChatActions = serializedObject.FindProperty("showHelpFullChatActions");
            showInputSettings = serializedObject.FindProperty("showInputSettings");
            showConnectionSettings = serializedObject.FindProperty("showConnectionSettings");
            showExternalObjectRef = serializedObject.FindProperty("showExternalObjectRef");
            showDynamicGenObj = serializedObject.FindProperty("showDynamicGenObj");
            showChatBoxObjs = serializedObject.FindProperty("showChatBoxObjs");
            showAnimSettings = serializedObject.FindProperty("showAnimSettings");
            showSoundSettings = serializedObject.FindProperty("showSoundSettings");
            eventSettings = serializedObject.FindProperty("eventSettings");
            #endregion

            #region Event Booleans
            dataEvents = serializedObject.FindProperty("dataEvents");
            subscribeEvents = serializedObject.FindProperty("subscribeEvents");
            messageEvents = serializedObject.FindProperty("messageEvents");
            enableEvents = serializedObject.FindProperty("enableEvents");
            #endregion

            availableInputs = E_Helpers.GetAllInputAxis();
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            ChatBox sp = (ChatBox)target;

            DrawTitleBar(
                "Text Chat Box", 
                "Component that is used for all text chat logic. Your chatbox should have all needed logic included in this component.",
                E_Core.h_textChatIcon
            );
            #endregion

            #region Properties 
            #region Sound Options
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sound Options", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showSoundSettings.boolValue = !showSoundSettings.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showSoundSettings.boolValue == true)
            {
                EditorGUILayout.PropertyField(source);
                EditorGUILayout.PropertyField(chatNotification);
                EditorGUILayout.PropertyField(notificationVolume);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region Animation Options
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animation Options", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showAnimSettings.boolValue = !showAnimSettings.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showAnimSettings.boolValue == true)
            {
                EditorGUILayout.PropertyField(chatAnim);
                EditorGUILayout.PropertyField(slideIn);
                EditorGUILayout.PropertyField(slideOut);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region ChatBox GameObjects
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ChatBox GameObject Parts", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showChatBoxObjs.boolValue = !showChatBoxObjs.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showChatBoxObjs.boolValue == true)
            {
                EditorGUILayout.PropertyField(parentChatObj);
                EditorGUILayout.PropertyField(msgInput);
                EditorGUILayout.PropertyField(connectionStatus);
                EditorGUILayout.PropertyField(messagesObj);
                EditorGUILayout.PropertyField(scrollRect);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(newMessageIcon);
                EditorGUILayout.PropertyField(onlyWhenWindowClose);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region Generated Objects
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dynamically Generated Objects", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showDynamicGenObj.boolValue = !showDynamicGenObj.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showDynamicGenObj.boolValue == true)
            {
                EditorGUILayout.PropertyField(otherChatMessage);
                EditorGUILayout.PropertyField(yourChatMessage);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region External Objects References
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("External Object References", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showExternalObjectRef.boolValue = !showExternalObjectRef.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showExternalObjectRef.boolValue == true)
            {
                EditorGUILayout.PropertyField(nm);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region Connection Settings
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Connection Settings", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showConnectionSettings.boolValue = !showConnectionSettings.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showConnectionSettings.boolValue)
            {
                EditorGUILayout.PropertyField(protocol);
                EditorGUILayout.PropertyField(region);
                EditorGUILayout.PropertyField(authType);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region Input Settings
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Settings", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showInputSettings.boolValue = !showInputSettings.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showInputSettings.boolValue == true)
            {
                EditorGUILayout.HelpBox("You will only be able to select inputs that are currently" +
                    " available in your input settings. If you wish to add more open Project Settings > Inputs " +
                    "and add your desired input.", MessageType.None);

                #region Open Chat Window On Press
                EditorGUILayout.LabelField("Open Chat Window On Press");
                List<string> open = (List<string>)target.GetType().GetField("openChatWindowOnPress", E_Helpers.allBindings).GetValue(target);
                int[] openInts = new int[open.Count];
                for (int i = 0; i < open.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    openInts[i] = availableInputs.IndexOf(open[i]);
                    if (openInts[i] == -1)
                    {
                        EditorGUILayout.HelpBox("Button: \"" + open[i] + "\" doesn't exist", MessageType.Error);
                    }
                    else
                    {
                        openInts[i] = EditorGUILayout.Popup(openInts[i], availableInputs.ToArray());
                        open[i] = availableInputs[openInts[i]];
                    }
                    if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Minus"), _skin.label, GUILayout.Width(30), GUILayout.Height(15)))
                    {
                        open.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Plus"), _skin.label, GUILayout.Width(30), GUILayout.Height(25)))
                {
                    open.Add(availableInputs[0]);
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Close Chat Window On Press
                EditorGUILayout.LabelField("Close Chat Window On Press");
                List<string> close = (List<string>)target.GetType().GetField("closeWindowOnPress", E_Helpers.allBindings).GetValue(target);
                int[] closeInts = new int[close.Count];
                for (int i = 0; i < close.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    closeInts[i] = availableInputs.IndexOf(close[i]);
                    if (closeInts[i] == -1)
                    {
                        EditorGUILayout.HelpBox("Button: \"" + close[i] + "\" doesn't exist", MessageType.Error);
                    }
                    else
                    {
                        closeInts[i] = EditorGUILayout.Popup(closeInts[i], availableInputs.ToArray());
                        close[i] = availableInputs[closeInts[i]];
                    }
                    if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Minus"), _skin.label, GUILayout.Width(30), GUILayout.Height(15)))
                    {
                        close.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Plus"), _skin.label, GUILayout.Width(30), GUILayout.Height(25)))
                {
                    close.Add(availableInputs[0]);
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Send Chat On Press
                EditorGUILayout.LabelField("Send Chat On Press");
                List<string> send = (List<string>)target.GetType().GetField("sendChatOnPress", E_Helpers.allBindings).GetValue(target);
                int[] sendInts = new int[send.Count];
                for (int i = 0; i < send.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    sendInts[i] = availableInputs.IndexOf(send[i]);
                    if (sendInts[i] == -1)
                    {
                        EditorGUILayout.HelpBox("Button: \"" + send[i] + "\" doesn't exist", MessageType.Error);
                    }
                    else
                    {
                        sendInts[i] = EditorGUILayout.Popup(sendInts[i], availableInputs.ToArray());
                        send[i] = availableInputs[sendInts[i]];
                    }

                    if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Minus"), _skin.label, GUILayout.Width(30), GUILayout.Height(15)))
                    {
                        send.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.FindTexture("d_Toolbar Plus"), _skin.label, GUILayout.Width(30), GUILayout.Height(25)))
                {
                    send.Add(availableInputs[0]);
                }
                EditorGUILayout.EndHorizontal();
                #endregion
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region Helpful ChatBox Actions
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Helpful ChatBox Actions", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                showHelpFullChatActions.boolValue = !showHelpFullChatActions.boolValue;
            }
            GUILayout.EndHorizontal();
            if (showHelpFullChatActions.boolValue == true)
            {
                EditorGUILayout.PropertyField(autoScroll);
                EditorGUILayout.PropertyField(startEnabled);
                EditorGUILayout.PropertyField(enableOnConnect);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(debugging);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region UnityEvents
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Events", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label))
            {
                eventSettings.boolValue = !eventSettings.boolValue;
            }
            GUILayout.EndHorizontal();
            if (eventSettings.boolValue == true)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Data"))
                {
                    dataEvents.boolValue = true;
                    subscribeEvents.boolValue = false;
                    messageEvents.boolValue = false;
                    enableEvents.boolValue = false;
                }
                if (GUILayout.Button("Subscribe"))
                {
                    dataEvents.boolValue = false;
                    subscribeEvents.boolValue = true;
                    messageEvents.boolValue = false;
                    enableEvents.boolValue = false;
                }
                if (GUILayout.Button("Messages"))
                {
                    dataEvents.boolValue = false;
                    subscribeEvents.boolValue = false;
                    messageEvents.boolValue = true;
                    enableEvents.boolValue = false;
                }
                if (GUILayout.Button("Enabled"))
                {
                    dataEvents.boolValue = false;
                    subscribeEvents.boolValue = false;
                    messageEvents.boolValue = false;
                    enableEvents.boolValue = true;
                }
                EditorGUILayout.EndHorizontal();

                if (dataEvents.boolValue == true)
                {
                    EditorGUILayout.HelpBox("When you receive a message that is intended for everyone in the session.", MessageType.None);
                    GUI.skin = _original;
                    EditorGUILayout.PropertyField(OnReceiveBroadcastMessage);
                    EditorGUILayout.HelpBox("When you subscribe to the data channel.", MessageType.None);
                    EditorGUILayout.PropertyField(OnYouSubscribeToDataChannel);
                    GUI.skin = _skin;
                    EditorGUILayout.HelpBox("Called whenever another player joins/leaves the data channel. ", MessageType.None);
                    GUI.skin = _original;
                    EditorGUILayout.PropertyField(OnUserSubscribedToDataChannel);
                    EditorGUILayout.PropertyField(OnUserUnSubscribedToDataChannel);
                }
                if (subscribeEvents.boolValue == true)
                {
                    GUI.skin = _skin;
                    EditorGUILayout.HelpBox("Called whenever you subscribe/unsubscribe from any chat channel.", MessageType.None);
                    GUI.skin = _original;
                    EditorGUILayout.PropertyField(OnYouSubscribeToAnyChannel);
                    EditorGUILayout.PropertyField(OnYouUnSubscribeFromAnyChannel);
                }
                if (messageEvents.boolValue == true)
                {
                    GUI.skin = _skin;
                    EditorGUILayout.HelpBox("Called when you receive chat messages.", MessageType.None);
                    GUI.skin = _original;
                    EditorGUILayout.PropertyField(ReceivedAnyChatMessage);
                    EditorGUILayout.PropertyField(ReceivedOtherPlayerChatMessage);
                }
                if (enableEvents.boolValue == true)
                {
                    GUI.skin = _skin;
                    EditorGUILayout.HelpBox("Called when you enable or disable the chatbox.", MessageType.None);
                    GUI.skin = _original;
                    EditorGUILayout.PropertyField(ChatEnabled);
                    EditorGUILayout.PropertyField(ChatDisabled);
                }
                GUI.skin = _skin;
            }
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion
            #endregion

            EndInspectorGUI(typeof(ChatBox));
        }
    }
}
