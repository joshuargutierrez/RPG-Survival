using UnityEngine;
using UnityEditor;
using CBGames.Core;
using UnityEngine.Events;
using UnityEditor.Events;
using System.Reflection;
using System;
using CBGames.UI;
using UnityEngine.UI;
using CBGames.Player;

namespace CBGames.Editors
{
    public class E_CoreObjects : EditorWindow
    {
        #region Editor Variables
        GUISkin _skin = null;
        #endregion

        [MenuItem("CB Games/Network Manager/Open Core Objects Window", false, 0)]
        public static void CB_CoreObjects()
        {
            EditorWindow window = GetWindow<E_CoreObjects>(true);
            window.maxSize = new Vector2(500, 280);
            window.minSize = window.maxSize;
        }

        [MenuItem("CB Games/Network Manager/Respawn/Add Player Respawn Component", false, 0)]
        public static void CB_CorePlayerRespawn()
        {
            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("Network Manager Missing", 
                    "Unable to locate a \"NetworkManager\" component in the scene. Add this first before attempting " +
                    "to add this component.\r\n",
                        "Okay"))
                {}
            }
            else
            {
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                if (nm.GetComponent<PlayerRespawn>())
                {
                    if (EditorUtility.DisplayDialog("PlayerRespawn Component Already Exists",
                    "The \"PlayerRespawn\" component already exists on this found NetworkManager. " +
                    "Do you want to continue and potentially overwrite settings on this component?",
                        "Continue", "Cancel"))
                    {
                        nm.GetComponent<PlayerRespawn>().visualCountdown = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InvectorMultiplayer/UI/RespawnCounter.prefab");
                    }
                }
                else
                {
                    nm.gameObject.AddComponent<PlayerRespawn>();
                    nm.GetComponent<PlayerRespawn>().visualCountdown = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InvectorMultiplayer/UI/RespawnCounter.prefab");
                }
            }
        }

        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Make window title
            this.titleContent = new GUIContent("Add Core Objects", null, "Adds the \"Core\" objects to the scene.");
        }
        private void OnGUI()
        {
            //Apply the gui skin
            GUI.skin = _skin;

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            EditorGUILayout.Space();
            EditorGUI.DrawRect(new Rect(10, 10, position.width - 20, 40), E_Colors.e_c_blue_5);
            EditorGUILayout.LabelField("Add Core Multiplayer Objects To Scene", _skin.label);
            EditorGUILayout.Space();

            //Draw Helpful Text
            EditorGUILayout.BeginHorizontal(_skin.window, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("This will add a new gameobject called \"Network Manager\". " +
                "It will setup this object with the ability to control/keep track of" +
                " things like the player count, lobby system, player disconnects, etc. This object is " +
                "absolutley essential to make multiplayer work. The ChatBox is also absolutely essential. " +
                "If you don't want a chatbox in your game thats fine, just don't make it visible to players. " +
                "However, essential scene/player data is transfered via this component with the NetworkManager " +
                "so make sure you have it in your scene.", _skin.GetStyle("TextField"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(_skin.window, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("IMPORTANT NOTE: If you don't add the pre-build UI, you will need to make one yourself that" +
                " makes use of the exposed functions in the \"NetworkManager.cs\" file.", _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Add Buttons
            if (GUI.Button(new Rect(20, position.height-50, 220, 30), "Add Network Manager", _skin.button))
            {
                AddNetworkManagerToScene();
            }
            if (GUI.Button(new Rect(260, position.height - 50, 220, 30), "Add ChatBox", _skin.GetStyle("Button")))
            {
                E_AddChatBox.CB_AddGlobalChatBox();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void AddNetworkManagerToScene()
        {
            // Will add this new tag to the inspector if it doesn't already exist
            E_Helpers.AddInspectorTag("SpawnPoint");

            GameObject _target = null;
            if (FindObjectOfType<NetworkManager>())
            {
                _target = FindObjectOfType<NetworkManager>().gameObject;
            }
            if (!_target)
            {
                _target = new GameObject("Network Manager");
            }
            if (!_target.GetComponent<NetworkManager>())
            {
                _target.AddComponent<NetworkManager>();
            }
            if (_target.GetComponent<NetworkManager>().defaultSpawnPoint == null)
            {
                GameObject spawnPoints = (GameObject.Find("Spawn Points")) ? GameObject.Find("Spawn Points") : new GameObject("Spawn Points");
                GameObject spawnPoint = (GameObject.Find("Player Spawn Point")) ? GameObject.Find("Player Spawn Point") : new GameObject("Player Spawn Point");
                E_Helpers.SetObjectIcon(spawnPoint, E_Core.h_spawnPointIcon);
                _target.GetComponent<NetworkManager>().defaultSpawnPoint = spawnPoint.transform;
                spawnPoints.layer = 2;
                spawnPoint.layer = 2;
                spawnPoint.tag = "SpawnPoint";
                spawnPoint.transform.SetParent(spawnPoints.transform);
            }
            if (_target.GetComponent<NetworkManager>().database == null)
            {
                if (System.IO.File.Exists("Assets/Resources/ScenesDatabase/ScenesDatabase.asset"))
                {
                    SceneDatabase database = AssetDatabase.LoadAssetAtPath<SceneDatabase>("Assets/Resources/ScenesDatabase/ScenesDatabase.asset");
                    if (database)
                    {
                        _target.GetComponent<NetworkManager>().database = database;
                    }
                }
                else
                {
                    Debug.LogWarning("Was unable to automatically add a \"SceneDatabase\" to the database entry on the NetworkManager. Remember to add one before making your final build!");
                }
            }
            E_Helpers.SetObjectIcon(_target, E_Core.h_networkIcon);
            Selection.activeGameObject = _target;
            _target.layer = 2; // Ignore Raycast Layer
            if (!_target.GetComponent<PlayerRespawn>())
            {
                _target.AddComponent<PlayerRespawn>();
            }
            _target.GetComponent<PlayerRespawn>().visualCountdown = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InvectorMultiplayer/UI/RespawnCounter.prefab");
            if (EditorUtility.DisplayDialog("Friendly Reminder","Remember to adjust the spawn point to where you want it to be. \r\n" +
                "\r\n",
                        "Okay"))
            {
                Debug.Log("Successfully added the \"Network Manager\" gameobject and assigned a Spawn Point.");
            }
        }
    }
}
