using UnityEditor;
using UnityEngine;
using CBGames.Core;
using CBGames.Editors;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(NetworkManager), true)]
    public class NetworkManagerInspector : BaseEditor
    {
        #region Properties
        SerializedProperty voiceRecorder;
        SerializedProperty spawnAtSaved;
        SerializedProperty connect_attempts;
        SerializedProperty reconnect;
        SerializedProperty replayScenes;
        SerializedProperty gameVersion;
        SerializedProperty maxPlayerPerRoom;
        SerializedProperty playerPrefab;
        SerializedProperty defaultSpawnPoint;
        SerializedProperty spawnPointsTag;
        SerializedProperty connectStatus;
        SerializedProperty syncScenes;
        SerializedProperty database;
        SerializedProperty debugging;
        SerializedProperty displayDebugWindow;
        SerializedProperty teamName;
        SerializedProperty allowTeamDamaging;
        SerializedProperty initalTeamSpawnPointNames;
        SerializedProperty autoSpawnPlayer;

        SerializedProperty _lobbyEvents;
        SerializedProperty onJoinedLobby;
        SerializedProperty onLeftLobby;

        SerializedProperty _roomEvents;
        SerializedProperty onJoinedRoom;
        SerializedProperty onLeftRoom;
        SerializedProperty onCreatedRoom;
        SerializedProperty onCreateRoomFailed;
        SerializedProperty onJoinRoomFailed;
        SerializedProperty onReconnecting;

        SerializedProperty _playerEvents;
        SerializedProperty onPlayerEnteredRoom;
        SerializedProperty onPlayerLeftRoom;

        SerializedProperty _miscEvents;
        SerializedProperty onMasterClientSwitched;
        SerializedProperty onDisconnected;
        SerializedProperty onConnectedToMaster;
        SerializedProperty onFailedToConnectToPhoton;
        SerializedProperty onConnectionFail;

        SerializedProperty cameraPoints;
        SerializedProperty moveCameraPriorToJoin;
        SerializedProperty cameraMoveSpeed;
        SerializedProperty cameraCloseEnough;
        #endregion

        #region CustomEditorVariables
        bool _displayLobbyEvents = true;
        bool _displayRoomEvents = false;
        bool _displayPlayerEvents = false;
        bool _displayMiscEvents = false;
        bool _addNewTeam = false;
        bool _displayTeams = false;
        string _newTeamName = "";
        string _newTeamSpawn = "";
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            voiceRecorder = serializedObject.FindProperty("voiceRecorder");
            spawnAtSaved = serializedObject.FindProperty("spawnAtSaved");
            connect_attempts = serializedObject.FindProperty("connect_attempts");
            reconnect = serializedObject.FindProperty("reconnect");
            replayScenes = serializedObject.FindProperty("replayScenes");
            gameVersion = serializedObject.FindProperty("gameVersion");
            maxPlayerPerRoom = serializedObject.FindProperty("maxPlayerPerRoom");
            playerPrefab = serializedObject.FindProperty("playerPrefab");
            defaultSpawnPoint = serializedObject.FindProperty("defaultSpawnPoint");
            spawnPointsTag = serializedObject.FindProperty("spawnPointsTag");
            connectStatus = serializedObject.FindProperty("_connectStatus");
            syncScenes = serializedObject.FindProperty("syncScenes");
            database = serializedObject.FindProperty("database");
            debugging = serializedObject.FindProperty("debugging");
            displayDebugWindow = serializedObject.FindProperty("displayDebugWindow");
            teamName = serializedObject.FindProperty("teamName");
            allowTeamDamaging = serializedObject.FindProperty("allowTeamDamaging");
            initalTeamSpawnPointNames = serializedObject.FindProperty("initalTeamSpawnPointNames");
            autoSpawnPlayer = serializedObject.FindProperty("autoSpawnPlayer");

            // Unity Events
            // // Lobby Events
            _lobbyEvents = serializedObject.FindProperty("lobbyEvents");
            onJoinedLobby = _lobbyEvents.FindPropertyRelative("_onJoinedLobby");
            onLeftLobby = _lobbyEvents.FindPropertyRelative("_onLeftLobby");

            // // Room Events
            _roomEvents = serializedObject.FindProperty("roomEvents");
            onJoinedRoom = _roomEvents.FindPropertyRelative("_onJoinedRoom");
            onLeftRoom = _roomEvents.FindPropertyRelative("_onLeftRoom");
            onCreatedRoom = _roomEvents.FindPropertyRelative("_OnCreatedRoom");
            onCreateRoomFailed = _roomEvents.FindPropertyRelative("_onCreateRoomFailed");
            onJoinRoomFailed = _roomEvents.FindPropertyRelative("_onJoinRoomFailed");
            onReconnecting = _roomEvents.FindPropertyRelative("_onReconnect");

            // // Player Events
            _playerEvents = serializedObject.FindProperty("playerEvents");
            onPlayerEnteredRoom = _playerEvents.FindPropertyRelative("_onPlayerEnteredRoom");
            onPlayerLeftRoom = _playerEvents.FindPropertyRelative("_onPlayerLeftRoom");

            // // Misc Events
            _miscEvents = serializedObject.FindProperty("otherEvents");
            onMasterClientSwitched = _miscEvents.FindPropertyRelative("_onMasterClientSwitched");
            onDisconnected = _miscEvents.FindPropertyRelative("_onDisconnected");
            onConnectedToMaster = _miscEvents.FindPropertyRelative("_onConnectedToMaster");
            onFailedToConnectToPhoton = _miscEvents.FindPropertyRelative("_onFailedToConnectToPhoton");
            onConnectionFail = _miscEvents.FindPropertyRelative("_onConnectionFail");

            //Camera
            cameraPoints = serializedObject.FindProperty("cameraPoints");
            moveCameraPriorToJoin = serializedObject.FindProperty("moveCameraPriorToJoin");
            cameraMoveSpeed = serializedObject.FindProperty("cameraMoveSpeed");
            cameraCloseEnough = serializedObject.FindProperty("cameraCloseEnough");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            NetworkManager nm = (NetworkManager)target;
            DrawTitleBar(
                "Network Manager",
                "Persistant component that is used to control network events like players joining, disconnects, lobby management, etc.",
                E_Core.h_networkIcon
            );
            #endregion

            #region Universal Settings
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("BuildSettings.Web.Small"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Universal Settings", _skin.textField);
            GUILayout.EndVertical();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nm.e_show_unv = !nm.e_show_unv;
            }
            GUILayout.EndHorizontal();
            if (nm.e_show_unv)
            {
                GUILayout.BeginHorizontal(_skin.customStyles[1]);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(gameVersion, new GUIContent("Game Version"));
                GUILayout.BeginHorizontal();
                if (maxPlayerPerRoom.intValue < 2)
                {
                    GUILayout.Label(EditorGUIUtility.FindTexture("CollabError"), GUILayout.ExpandWidth(false), GUILayout.Height(15));
                }
                EditorGUILayout.IntSlider(maxPlayerPerRoom, 0, 255, new GUIContent("Players Per Room"));
                GUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(syncScenes);
                EditorGUILayout.PropertyField(replayScenes);
                EditorGUILayout.PropertyField(database);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            #endregion

            #region Player Settings
            EditorGUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("SoftlockInline"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label("Player Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nm.e_show_player = !nm.e_show_player;
            }
            EditorGUILayout.EndHorizontal();

            if (nm.e_show_player)
            {
                EditorGUILayout.BeginHorizontal(_skin.customStyles[1]);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(playerPrefab);
                EditorGUILayout.PropertyField(voiceRecorder);
                EditorGUILayout.PropertyField(allowTeamDamaging);
                EditorGUILayout.HelpBox("Read the tooltip before setting the team name.", MessageType.Info);
                EditorGUILayout.PropertyField(teamName);

                GUI.skin = _original;
                GUIStyle foldoutStyle = CBEditor.GetEditorStyle(_skin.textArea, EditorStyles.foldout);
                _displayTeams = EditorGUILayout.Foldout(_displayTeams, "Initial Team Spawn Point Names", foldoutStyle);
                GUI.skin = _skin;
                if (_displayTeams == true)
                {
                    EditorGUILayout.BeginHorizontal(_skin.box);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.Space();
                    //EditorGUILayout.Space();
                    //EditorGUILayout.Space();
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = _skin.textArea.normal.textColor;
                    EditorGUILayout.LabelField("Team Name", GUI.skin.label, GUILayout.Width(73));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Starting Spawn Name", GUI.skin.label, GUILayout.Width(130));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("+")))
                    {
                        _addNewTeam = true;
                    }
                    GUI.skin = _skin;
                    _newTeamName = EditorGUILayout.TextField(_newTeamName);
                    _newTeamSpawn = EditorGUILayout.TextField(_newTeamSpawn);
                    EditorGUILayout.EndHorizontal();

                    if (_addNewTeam == true)
                    {
                        _addNewTeam = false;

                        nm.initalTeamSpawnPointNames.Add(_newTeamName, _newTeamSpawn);
                        _newTeamName = "";
                        _newTeamSpawn = "";
                    }

                    EditorGUILayout.BeginHorizontal(_skin.box);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Initial Team Spawn Names", _skin.label);
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = _skin.textArea.normal.textColor;
                    foreach (KeyValuePair<string, string> item in nm.initalTeamSpawnPointNames)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("-"), GUILayout.Width(20)))
                        {
                            nm.initalTeamSpawnPointNames.Remove(item.Key);
                            break;
                        }
                        EditorGUILayout.LabelField(item.Key, GUI.skin.label, GUILayout.MinWidth(73));
                        EditorGUILayout.LabelField(item.Value, GUI.skin.label, GUILayout.MinWidth(110));
                        EditorGUILayout.EndHorizontal();
                    }
                    GUI.skin = _skin;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            #endregion

            #region Spawn Settings
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("AvatarPivot"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label("Spawning Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nm.e_show_spawn = !nm.e_show_spawn;
            }
            GUILayout.EndHorizontal();

            if (nm.e_show_spawn)
            {
                GUILayout.BeginHorizontal(_skin.customStyles[1]);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(autoSpawnPlayer);
                GUILayout.BeginHorizontal();
                if (nm.defaultSpawnPoint == null && nm.spawnPointsTag == "")
                {
                    GUILayout.Label(EditorGUIUtility.FindTexture("CollabError"), GUILayout.ExpandWidth(false), GUILayout.Height(15));
                }
                else if (nm.defaultSpawnPoint == null && nm.spawnPointsTag != "")
                {
                    GUILayout.Label(EditorGUIUtility.FindTexture("d_console.warnicon.sml"), GUILayout.ExpandWidth(false), GUILayout.Height(19));
                }
                EditorGUILayout.PropertyField(defaultSpawnPoint, new GUIContent("Default Spawn Point"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (nm.spawnPointsTag == "" && nm.defaultSpawnPoint != null)
                {
                    GUILayout.Label(EditorGUIUtility.FindTexture("d_console.warnicon.sml"), GUILayout.ExpandWidth(false), GUILayout.Height(19));
                }
                else if (nm.spawnPointsTag == "" && nm.defaultSpawnPoint == null)
                {
                    GUILayout.Label(EditorGUIUtility.FindTexture("CollabError"), GUILayout.ExpandWidth(false), GUILayout.Height(15));
                }
                EditorGUILayout.PropertyField(spawnPointsTag, new GUIContent("Available Spawn Tag"));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            #endregion

            #region Debug Settings
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("d_Settings"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Debug Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nm.e_show_debug = !nm.e_show_debug;
            }
            GUILayout.EndHorizontal();
            if (nm.e_show_debug)
            {
                GUILayout.BeginHorizontal(_skin.customStyles[1]);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(debugging, new GUIContent("Verbose Console Logging"));
                EditorGUILayout.PropertyField(connectStatus, new GUIContent("Connection Status"));
                EditorGUILayout.PropertyField(displayDebugWindow);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();
            #endregion

            #region Network Settings
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("vcs_branch"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Network Events", _skin.textField);
            GUILayout.EndVertical();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nm.e_show_network = !nm.e_show_network;
            }
            GUILayout.EndHorizontal();
            if (nm.e_show_network)
            {
                GUILayout.BeginHorizontal(_skin.customStyles[1]);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(reconnect);
                EditorGUILayout.PropertyField(connect_attempts);
                EditorGUILayout.PropertyField(spawnAtSaved);

                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Lobby", _skin.button))
                {
                    _displayLobbyEvents = true;
                    _displayMiscEvents = false;
                    _displayPlayerEvents = false;
                    _displayRoomEvents = false;
                }
                if (GUILayout.Button("Room", _skin.button))
                {
                    _displayLobbyEvents = false;
                    _displayMiscEvents = false;
                    _displayPlayerEvents = false;
                    _displayRoomEvents = true;
                }
                if (GUILayout.Button("Player", _skin.button))
                {
                    _displayLobbyEvents = false;
                    _displayMiscEvents = false;
                    _displayPlayerEvents = true;
                    _displayRoomEvents = false;
                }
                if (GUILayout.Button("Misc", _skin.button))
                {
                    _displayLobbyEvents = false;
                    _displayMiscEvents = true;
                    _displayPlayerEvents = false;
                    _displayRoomEvents = false;
                }
                GUILayout.EndHorizontal();
                if (_displayLobbyEvents == true)
                {
                    GUILayout.BeginVertical("window", GUILayout.ExpandHeight(false));
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = Color.black;
                    EditorGUILayout.PropertyField(onJoinedLobby);
                    EditorGUILayout.PropertyField(onLeftLobby);
                    GUI.skin = _skin;
                    GUILayout.EndHorizontal();
                }
                if (_displayRoomEvents == true)
                {
                    GUILayout.BeginVertical("window");
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = Color.black;
                    EditorGUILayout.PropertyField(onJoinedRoom);
                    EditorGUILayout.PropertyField(onLeftRoom);
                    EditorGUILayout.PropertyField(onCreatedRoom);
                    EditorGUILayout.PropertyField(onCreateRoomFailed);
                    EditorGUILayout.PropertyField(onJoinRoomFailed);
                    EditorGUILayout.PropertyField(onReconnecting);
                    GUI.skin = _skin;
                    GUILayout.EndVertical();
                }
                if (_displayPlayerEvents == true)
                {
                    GUILayout.BeginVertical("window");
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = Color.black;
                    EditorGUILayout.PropertyField(onPlayerEnteredRoom);
                    EditorGUILayout.PropertyField(onPlayerLeftRoom);
                    GUI.skin = _skin;
                    GUILayout.EndVertical();
                }
                if (_displayMiscEvents == true)
                {
                    GUILayout.BeginVertical("window");
                    GUI.skin = _original;
                    GUI.skin.label.normal.textColor = Color.black;
                    EditorGUILayout.PropertyField(onMasterClientSwitched);
                    EditorGUILayout.PropertyField(onDisconnected);
                    EditorGUILayout.PropertyField(onConnectedToMaster);
                    EditorGUILayout.PropertyField(onFailedToConnectToPhoton);
                    EditorGUILayout.PropertyField(onConnectionFail);
                    GUI.skin = _skin;
                    GUILayout.EndVertical();
                }
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            #endregion

            EndInspectorGUI(typeof(NetworkManager));
        }
    }
}
