/*
using CBGames.AI;
using CBGames.Objects;
using Invector;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;
using Invector.vShooter;
using marijnz.EditorCoroutines;
using Photon.Pun;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Networking;

namespace CBGames.Editors
{
    public class E_ConvertAI : EditorWindow
    {
        #region Convert Window
        #region Properties
        GameObject aiObject = null;
        GUISkin _skin = null;
        #endregion

        [MenuItem("CB Games/Convert/(Beta) AI Characters", false, 0)]
        public static void CB_ConvertPlayerEditorWindow()
        {
            EditorWindow window = GetWindow<E_ConvertAI>(true);
            window.maxSize = new Vector2(470, 120);
            window.minSize = window.maxSize;
        }
        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);
            this.titleContent = new GUIContent("(Beta) AI Character Conversion", null, "(Beta) Convert an AI character to support multiplayer.");
        }

        private void OnGUI()
        {
            CBColorHolder _org = new CBColorHolder(EditorStyles.label);
            CBColorHolder _orgFoldout = new CBColorHolder(EditorStyles.foldout);
            CBColorHolder _skinHolder = new CBColorHolder(_skin.label);

            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 90), E_Colors.e_c_blue_5);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int i = 0; i < 29; i++)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.LabelField("(Beta) Convert AI Character", _skin.GetStyle("Label"));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorStyles.label.normal.textColor = Color.white;
            aiObject = EditorGUI.ObjectField(new Rect(155, 50, 300, 16), "             AI Character:", aiObject, typeof(GameObject), true) as GameObject;

            GUI.DrawTexture(new Rect(-15, -20, 250, 150), E_Helpers.LoadImage(new Vector2(1024, 512), E_Core.e_invectorMPTitle));
            for (int i = 0; i < 7; i++)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginHorizontal(_skin.box);
            if (aiObject != null)
            {
                if (aiObject.GetComponent<vFSMBehaviourController>())
                {
                    if (GUILayout.Button("CONVERT NOW"))
                    {
                        ConvertAICharacter(aiObject);
                        if (EditorUtility.DisplayDialog("Finished Converting",
                            "This has finished converting the prefab. Review the debug console for the logs.",
                            "Okay"))
                        { }
                    }
                }
                else
                {
                    //CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                    GUIStyle redLabl = new GUIStyle();
                    redLabl = EditorStyles.label;
                    redLabl.normal.textColor = Color.red;
                    EditorGUILayout.LabelField("ERROR: Character Missing vFSMBehaviourController component", redLabl);
                    CBEditor.SetColorToEditorStyle(_org, _orgFoldout);
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region UI Convert
        void ConvertAICharacter(GameObject character)
        {
            // Create A Copy First
            GameObject _copiedAI = GameObject.Instantiate(character, character.transform.position + Vector3.left, Quaternion.identity) as GameObject;
            _copiedAI.name = _copiedAI.name.Replace("(Clone)", "");
            _copiedAI.name = "MP_" + _copiedAI.name;

            // Convert weapons and components on this character
            CB_COMP_MPAI(_copiedAI);
            CB_COMP_vAIHeadTrack(_copiedAI);
            CB_COMP_vControlAIShooter(_copiedAI);
            CB_COMP_vAIShooterManager(_copiedAI);
            CB_COMP_AIGenericSync(_copiedAI);
            foreach (vShooterWeapon weapon in _copiedAI.GetComponentsInChildren<vShooterWeapon>(true))
            {
                CB_COMP_AIvShooterWeapon(weapon.gameObject);
            }

            _copiedAI.SetActive(true);
            // Save Converted Copy to Resources Folder
            string saveLocation = string.Format("Assets{0}Resources{0}{1}.prefab", Path.DirectorySeparatorChar, _copiedAI.name);
            if (E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(saveLocation)))
            {
                Debug.Log("Created Assets/Resources directory");
            }

            // Add Icon To AI
            E_Helpers.SetObjectIcon(_copiedAI, E_Core.h_playerIcon);

            // Allow user to overwrite or not if already existing
            if (E_Helpers.FileExists(saveLocation))
            {
                if (EditorUtility.DisplayDialog("Duplicate Detected!",
                    "A prefab with the same name already exists, would you like to overwrite it or create a new one, keeping both?",
                    "Overwrite", "Keep Both"))
                {
                    //Overwrite
                    E_Helpers.DeleteFile(saveLocation);
                    Debug.Log("Generating prefab at: " + saveLocation);
                    PrefabUtility.SaveAsPrefabAsset(_copiedAI, saveLocation);
                }
                else
                {
                    //Keep Both
                    saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                    Debug.Log("Generating prefab at: " + saveLocation);
                    PrefabUtility.SaveAsPrefabAsset(_copiedAI, saveLocation);
                }
            }
            else
            {
                saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                Debug.Log("Generating prefab at: " + saveLocation);
                PrefabUtility.SaveAsPrefabAsset(_copiedAI, saveLocation);
            }

            Selection.activeGameObject = _copiedAI;
        }
        #endregion

        #region vAIHeadTrack
        [MenuItem("CONTEXT/vAIHeadtrack/Replace vAIHeadtrack With MP_vAIHeadtrack")]
        public static void CB_CONTEXT_vAIHeadTrack(MenuCommand command)
        {
            vAIHeadtrack headTrack = (vAIHeadtrack)command.context;
            CB_COMP_vAIHeadTrack(headTrack.gameObject);
        }

        public static void CB_COMP_vAIHeadTrack(GameObject target)
        {
            if (!target.GetComponent<vAIHeadtrack>() || target.GetComponent<MP_vAIHeadTrack>()) return;
            Debug.Log("Replacing vAIHeadTrack -> MP_vAIHeadTrack.");
            Debug.Log("Adding MP_vAIHeadTrack -> PhotonView Observed Components.");
            E_CompHelper.ReplaceWithComponent(target, typeof(vAIHeadtrack), typeof(MP_vAIHeadTrack), true);
        }
        #endregion

        #region vControlAIShooter
        [MenuItem("CONTEXT/vControlAIShooter/Replace vControlAIShooter With MP_vControlAIShooter")]
        public static void CB_CONTEXT_vControlAIShooter(MenuCommand command)
        {
            vControlAIShooter headTrack = (vControlAIShooter)command.context;
            CB_COMP_vControlAIShooter(headTrack.gameObject);
        }

        public static void CB_COMP_vControlAIShooter(GameObject target)
        {
            if (!target.GetComponent<vControlAIShooter>() || target.GetComponent<MP_vControlAIShooter>()) return;
            Debug.Log("Replacing vControlAIShooter -> MP_vControlAIShooter.");
            Debug.Log("Adding MP_vControlAIShooter -> PhotonView Observed Components.");
            E_CompHelper.ReplaceWithComponent(target, typeof(vControlAIShooter), typeof(MP_vControlAIShooter), true);
        }
        #endregion

        #region vAIShooterManager
        [MenuItem("CONTEXT/vAIShooterManager/Replace vAIShooterManager With MP_vAIShooterManager")]
        public static void CB_CONTEXT_vAIShooterManager(MenuCommand command)
        {
            vAIShooterManager headTrack = (vAIShooterManager)command.context;
            CB_COMP_vAIShooterManager(headTrack.gameObject);
        }

        public static void CB_COMP_vAIShooterManager(GameObject target)
        {
            if (!target.GetComponent<vControlAIShooter>() || target.GetComponent<MP_vAIShooterManager>()) return;
            Debug.Log("Replacing vAIShooterManager -> MP_vAIShooterManager.");
            Debug.Log("Adding MP_vAIShooterManager -> PhotonView Observed Components.");
            E_CompHelper.ReplaceWithComponent(target, typeof(vAIShooterManager), typeof(MP_vAIShooterManager), false);
        }
        #endregion

        #region vShooterWeapon
        [MenuItem("CONTEXT/MP_ShooterWeapon/Replace MP_ShooterWeapon -> AI_MP_ShooterWeapon")]
        public static void CB_CONTEXT_MPvShooterWeapon(MenuCommand command)
        {
            MP_BaseShooterWeapon shooterWeapon = (MP_BaseShooterWeapon)command.context;
            CB_COMP_AIvShooterWeapon(shooterWeapon.gameObject);
        }
        [MenuItem("CONTEXT/vShooterWeapon/Add vShooterWeapon AI MP Component")]
        public static void CB_CONTEXT_AIvShooterWeapon(MenuCommand command)
        {
            vShooterWeapon shooterWeapon = (vShooterWeapon)command.context;
            CB_COMP_AIvShooterWeapon(shooterWeapon.gameObject);
        }
        public static void CB_COMP_AIvShooterWeapon(GameObject target)
        {
            if (target.GetComponent<vShooterWeapon>())
            {
                if (target.GetComponent<MP_BaseShooterWeapon>())
                {
                    DestroyImmediate(target.GetComponent<MP_BaseShooterWeapon>());
                }
                if (!target.GetComponent<AI_MP_ShooterWeapon>())
                {
                    target.AddComponent<AI_MP_ShooterWeapon>();
                }
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onShot, "SendNetworkShot", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onShot, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkShot);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onEmptyClip, "SendNetworkEmptyClip", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onEmptyClip, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkEmptyClip);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onReload, "SendNetworkReload", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onReload, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkReload);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishReload, "SendNetworkOnFinishReload", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFinishReload, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkOnFinishReload);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFullPower, "SendNetworkOnFullPower", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFullPower, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkOnFullPower);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishAmmo, "SendNetworkOnFinishAmmo", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFinishAmmo, target.GetComponent<AI_MP_ShooterWeapon>().SendNetworkOnFinishAmmo);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onEnableAim, "SendOnEnableAim", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onEnableAim, target.GetComponent<AI_MP_ShooterWeapon>().SendOnEnableAim);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onDisableAim, "SendOnDisableAim", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onDisableAim, target.GetComponent<AI_MP_ShooterWeapon>().SendOnDisableAim);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onPowerChargerChanged, "SendOnChangerPowerCharger", target.GetComponent<AI_MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onPowerChargerChanged, target.GetComponent<AI_MP_ShooterWeapon>().SendOnChangerPowerCharger);
                }
            }
        }
        #endregion

        #region AI_MP
        [MenuItem("CONTEXT/Transform/Add MP_AI component")]
        public static void CB_CONTEXT_MPAI(MenuCommand command)
        {
            Transform trans = (Transform)command.context;
            CB_COMP_vAIShooterManager(trans.gameObject);
        }
        public static void CB_COMP_MPAI(GameObject target)
        {
            if (!target.GetComponent<vFSMBehaviourController>()) return;
            if (!target.GetComponent<MP_AI>())
            {
                Debug.Log("Adding MP_AI component.");
                target.AddComponent<MP_AI>();
            }
            bool containsFSM = false;
            foreach (Behaviour comp in target.GetComponent<MP_AI>().components)
            {
                if (comp == target.GetComponent<vFSMBehaviourController>())
                {
                    containsFSM = true;
                }
            }
            if (containsFSM == false)
            {
                Debug.Log("Adding vFSMBehaviourController component to \"components\" value in MP_AI component.");
                target.GetComponent<MP_AI>().components.Add(target.GetComponent<vFSMBehaviourController>());
            }
        }
        #endregion

        #region Generic Sync
        [MenuItem("CONTEXT/Transform/Add Generic Sync component")]
        public static void CB_CONTEXT_AIGenericSync(MenuCommand command)
        {
            Transform trans = (Transform)command.context;
            CB_COMP_AIGenericSync(trans.gameObject);
        }
        public static void CB_COMP_AIGenericSync(GameObject target)
        {
            if (!target.GetComponent<GenericSync>())
            {
                Debug.Log("Adding GenericSync component.");
                target.AddComponent<GenericSync>();
            }
            if (target.GetComponent<GenericSync>().syncTriggers)
            {
                Debug.Log("Setting GenericSync's syncTriggers == false.");
                target.GetComponent<GenericSync>().syncTriggers = false;
            }
            if (target.GetComponent<PhotonView>())
            {
                Debug.Log("Adding \"GenericSync\" to \"PhotonView\"'s ObserveredComponents.");
                target.GetComponent<PhotonView>().ObservedComponents.Add(target.GetComponent<GenericSync>());
                if (target.GetComponent<PhotonView>().Synchronization != ViewSynchronization.UnreliableOnChange)
                {
                    Debug.Log("Setting PhotonView's Synchronization to Unreliable On Change.");
                    target.GetComponent<PhotonView>().Synchronization = ViewSynchronization.UnreliableOnChange;
                }
            }
        }
        #endregion
    }
}
*/
