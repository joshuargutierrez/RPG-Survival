using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(UICoreLogic), true)]
    public class UICoreLogicInspector : BaseEditor
    {
        #region Properties
        #region Session
        SerializedProperty defaultLevelIndex;
        SerializedProperty selectablePlayers;
        SerializedProperty OnStart;
        SerializedProperty OnSceneLoaded;
        #endregion

        #region Audio
        SerializedProperty musicSource;
        SerializedProperty soundSource;
        SerializedProperty mouseEnter;
        SerializedProperty mouseEnterVolume;
        SerializedProperty mouseExit;
        SerializedProperty mouseExitVolume;
        SerializedProperty mouseClick;
        SerializedProperty mouseClickVolume;
        SerializedProperty finalClick;
        SerializedProperty startVolume;
        SerializedProperty fadeInAudio;
        SerializedProperty fadeInSpeed;
        #endregion

        #region Player Settings
        SerializedProperty OnNameEnterFailed;
        SerializedProperty OnNameEnterSuccess;
        #endregion

        #region Room
        SerializedProperty sceneList;
        SerializedProperty OnCountdownStarted;
        SerializedProperty OnCountdownStopped;
        SerializedProperty OnCreateRoomFailed;
        SerializedProperty OnCreateRoomSuccess;
        SerializedProperty OnWaitToJoinPhotonRoomsLobby;
        SerializedProperty OnStartSession;
        SerializedProperty OnReceiveRoundTime;
        SerializedProperty OnReconnecting;
        #endregion

        #region Generic Network
        SerializedProperty OnNetworkError;
        SerializedProperty debugging;
        #endregion

        #region Loading Page
        SerializedProperty loadingParent;
        SerializedProperty loadingImages;
        SerializedProperty loadingImageDisplayTime;
        SerializedProperty loadingPageFadeSpeed;
        SerializedProperty loadingTitle;
        SerializedProperty loadingDesc;
        SerializedProperty mainLoadingImage;
        SerializedProperty loadingTitleText;
        SerializedProperty loadingDescText;
        SerializedProperty loadingBar;
        SerializedProperty OnStartLoading;
        SerializedProperty OnCompleteLevelLoading;
        #endregion

        #region Countdown

        #endregion

        #region Misc
        SerializedProperty OnResetEverything;
        #endregion
        #endregion

        protected override void OnEnable()
        {   
            #region Properties
            #region Session
            defaultLevelIndex = serializedObject.FindProperty("defaultLevelIndex");
            selectablePlayers = serializedObject.FindProperty("selectablePlayers");
            OnStart = serializedObject.FindProperty("OnStart");
            OnSceneLoaded = serializedObject.FindProperty("OnSceneLoaded");
            #endregion

            #region Audio
            musicSource = serializedObject.FindProperty("musicSource");
            soundSource = serializedObject.FindProperty("soundSource");
            mouseEnter = serializedObject.FindProperty("mouseEnter");
            mouseEnterVolume = serializedObject.FindProperty("mouseEnterVolume");
            mouseExit = serializedObject.FindProperty("mouseExit");
            mouseExitVolume = serializedObject.FindProperty("mouseExitVolume");
            mouseClick = serializedObject.FindProperty("mouseClick");
            mouseClickVolume = serializedObject.FindProperty("mouseClickVolume");
            finalClick = serializedObject.FindProperty("finalClick");
            startVolume = serializedObject.FindProperty("startVolume");
            fadeInAudio = serializedObject.FindProperty("fadeInAudio");
            fadeInSpeed = serializedObject.FindProperty("fadeInSpeed");
            #endregion

            #region Player
            OnNameEnterFailed = serializedObject.FindProperty("OnNameEnterFailed");
            OnNameEnterSuccess = serializedObject.FindProperty("OnNameEnterSuccess");
            #endregion

            #region Room
            OnCountdownStarted = serializedObject.FindProperty("OnCountdownStarted");
            OnCountdownStopped = serializedObject.FindProperty("OnCountdownStopped");
            OnCreateRoomFailed = serializedObject.FindProperty("OnCreateRoomFailed");
            OnCreateRoomSuccess = serializedObject.FindProperty("OnCreateRoomSuccess");
            OnWaitToJoinPhotonRoomsLobby = serializedObject.FindProperty("OnWaitToJoinPhotonRoomsLobby");
            OnStartSession = serializedObject.FindProperty("OnStartSession");
            sceneList = serializedObject.FindProperty("sceneList");
            OnReceiveRoundTime = serializedObject.FindProperty("OnReceiveRoundTime");
            OnReconnecting = serializedObject.FindProperty("OnReconnecting");
            #endregion

            #region Generic Network
            OnNetworkError = serializedObject.FindProperty("OnNetworkError");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            #region Loading Page
            loadingParent = serializedObject.FindProperty("loadingParent");
            loadingImages = serializedObject.FindProperty("loadingImages");
            loadingImageDisplayTime = serializedObject.FindProperty("loadingImageDisplayTime");
            loadingPageFadeSpeed = serializedObject.FindProperty("loadingPageFadeSpeed");
            loadingTitle = serializedObject.FindProperty("loadingTitle");
            loadingDesc = serializedObject.FindProperty("loadingDesc");
            mainLoadingImage = serializedObject.FindProperty("mainLoadingImage");
            loadingTitleText = serializedObject.FindProperty("loadingTitleText");
            loadingDescText = serializedObject.FindProperty("loadingDescText");
            loadingBar = serializedObject.FindProperty("loadingBar");
            OnStartLoading = serializedObject.FindProperty("OnStartLoading");
            OnCompleteLevelLoading = serializedObject.FindProperty("OnCompleteLevelLoading");
            #endregion

            #region Misc
            OnResetEverything = serializedObject.FindProperty("OnResetEverything");
            #endregion
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            UICoreLogic cl = (UICoreLogic)target;
            DrawTitleBar(
                "UI Core Logic", 
                "This component is what almost everything in the UI calls. This is " +
                "also open enough that it exposes enough functions for you to use in your own UIs (hopefully).\n" +
                "Only one of these components should ever be in the scene at a time. If more than one \"UICoreLogic\" component " +
                "is found it could cause errors.",
                E_Core.h_uiIcon
            );
            #endregion

            #region Session/Core Settings
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Core Options", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                cl.e_show_core = !cl.e_show_core;
            }
            EditorGUILayout.EndHorizontal();

            if (cl.e_show_core == true)
            {
                EditorGUILayout.PropertyField(defaultLevelIndex);
                EditorGUILayout.PropertyField(selectablePlayers, true);
                EditorGUILayout.PropertyField(sceneList, true);
                EditorGUILayout.PropertyField(debugging);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            #region Audio
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sound Options", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                cl.e_show_audio = !cl.e_show_audio;
            }
            EditorGUILayout.EndHorizontal();

            if (cl.e_show_audio == true)
            {
                EditorGUILayout.PropertyField(musicSource);
                EditorGUILayout.PropertyField(soundSource);
                EditorGUILayout.PropertyField(mouseEnter);
                EditorGUILayout.PropertyField(mouseEnterVolume);
                EditorGUILayout.PropertyField(mouseExit);
                EditorGUILayout.PropertyField(mouseExitVolume);
                EditorGUILayout.PropertyField(mouseClick);
                EditorGUILayout.PropertyField(mouseClickVolume);
                EditorGUILayout.PropertyField(finalClick);
                EditorGUILayout.PropertyField(startVolume);
                EditorGUILayout.PropertyField(fadeInAudio);
                EditorGUILayout.PropertyField(fadeInSpeed);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            #region Loading Page
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Loading Page Options", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                cl.e_show_loading = !cl.e_show_loading;
            }
            EditorGUILayout.EndHorizontal();

            if (cl.e_show_loading == true)
            {
                EditorGUILayout.PropertyField(loadingParent);
                EditorGUILayout.PropertyField(loadingImages, true);
                EditorGUILayout.PropertyField(loadingImageDisplayTime);
                EditorGUILayout.PropertyField(loadingPageFadeSpeed);
                EditorGUILayout.PropertyField(loadingTitle);
                EditorGUILayout.PropertyField(loadingDesc, true);
                EditorGUILayout.PropertyField(mainLoadingImage);
                EditorGUILayout.PropertyField(loadingTitleText);
                EditorGUILayout.PropertyField(loadingDescText);
                EditorGUILayout.PropertyField(loadingBar);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            #region UnityEvents
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI Events", _skin.textField);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                cl.e_show_events = !cl.e_show_events;
            }
            EditorGUILayout.EndHorizontal();

            if (cl.e_show_events == true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Start"))
                {
                    cl.e_events_oneTime = true;
                    cl.e_events_loading = false;
                    cl.e_events_naming = false;
                    cl.e_events_errors = false;
                    cl.e_events_room = false;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = false;
                }
                if (GUILayout.Button("Loading"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = true;
                    cl.e_events_naming = false;
                    cl.e_events_errors = false;
                    cl.e_events_room = false;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = false;
                }
                if (GUILayout.Button("Naming"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = false;
                    cl.e_events_naming = true;
                    cl.e_events_errors = false;
                    cl.e_events_room = false;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = false;
                }
                if (GUILayout.Button("Errors"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = false;
                    cl.e_events_naming = false;
                    cl.e_events_errors = true;
                    cl.e_events_room = false;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Room"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = false;
                    cl.e_events_naming = false;
                    cl.e_events_errors = false;
                    cl.e_events_room = true;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = false;
                }
                if (GUILayout.Button("Countdown"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = false;
                    cl.e_events_naming = false;
                    cl.e_events_errors = false;
                    cl.e_events_room = false;
                    cl.e_show_countdown = true;
                    cl.e_show_misc = false;
                }
                if (GUILayout.Button("Misc"))
                {
                    cl.e_events_oneTime = false;
                    cl.e_events_loading = false;
                    cl.e_events_naming = false;
                    cl.e_events_errors = false;
                    cl.e_events_room = false;
                    cl.e_show_countdown = false;
                    cl.e_show_misc = true;
                }
                EditorGUILayout.EndHorizontal();

                GUI.skin = _original;
                if (cl.e_events_oneTime == true)
                {
                    EditorGUILayout.HelpBox("This event is triggered only once when this object is first enabled. Called after OnAwake functions.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnStart);
                }
                else if (cl.e_events_loading == true)
                {
                    EditorGUILayout.HelpBox("Called right after a new Unity scene has successfully loaded.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnSceneLoaded);
                    EditorGUILayout.HelpBox("Called right when loading has started for the loading bar ui.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnStartLoading);
                    EditorGUILayout.HelpBox("Called right when the loading bar, in the loading ui, indicates that " +
                        "the loading of the new scene has completed.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnCompleteLevelLoading);
                }
                else if (cl.e_events_naming == true)
                {
                    EditorGUILayout.HelpBox("When the user tries to enter an invalid name this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnNameEnterFailed);
                    EditorGUILayout.HelpBox("When the user enter a valid name this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnNameEnterSuccess);
                }
                else if (cl.e_events_errors == true)
                {
                    EditorGUILayout.HelpBox("Works only if you have enabled 'reconnect' in the NetworkManager. Called" +
                        "whenever you're attempting to reconnect to your last room after having been disconnected.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnReconnecting);
                    EditorGUILayout.HelpBox("When any sort of network error happens this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnNetworkError);
                }
                else if (cl.e_events_room == true)
                {
                    EditorGUILayout.HelpBox("When you attempt to host a room and that room creation fails, this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnCreateRoomFailed);
                    EditorGUILayout.HelpBox("When you attempt to host a room and that room creation succeeds, this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnCreateRoomSuccess);
                    EditorGUILayout.HelpBox("As soon as you attempt to join a PhotonRoom from the UI, this is called.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnWaitToJoinPhotonRoomsLobby);
                }
                else if (cl.e_show_countdown == true)
                {
                    EditorGUILayout.HelpBox("When you recieve a STARTCOUNTDOWN event with a start timer of true, this is called with the countdown number you recieved.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnCountdownStarted);
                    EditorGUILayout.HelpBox("When you recieve a STARTCOUNTDOWN event with a start timer of false, this is called with the countdown number you recieved.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnCountdownStopped);
                }
                else if (cl.e_show_misc == true)
                {
                    EditorGUILayout.HelpBox("When you as the host start the session this is called. (Fired if calling the SendStartSession function).", MessageType.Info);
                    EditorGUILayout.PropertyField(OnStartSession);
                    EditorGUILayout.HelpBox("When you recieve a ROUNDTIME event with a time to set the round to. You can decide what to do with this time.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnReceiveRoundTime);
                    EditorGUILayout.HelpBox("When the \"ResetEverything\" function is called, this is executed.", MessageType.Info);
                    EditorGUILayout.PropertyField(OnResetEverything);
                }
                GUI.skin = _skin;
            }
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            EndInspectorGUI(typeof(UICoreLogic));
        }
    }
}
