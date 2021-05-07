using CBGames.Core;
using CBGames.Objects;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace CBGames.Editors
{
    public partial class E_ConvertPrefabs
    {
        #region Partial Methods

        #region Shooter
        static partial void CB_COMP_vProjectileControle(GameObject target);
        static partial void CB_COMP_vShooterWeapon(GameObject target);
        static partial void CB_SHOOTER_CheckFromComps(GameObject target);
        static partial void CB_SHOOTER_HasShooterComp(GameObject target, bool checkVal, string prefabPath);
        static partial void CB_COMP_SHOOTER_vItemCollection(vItemCollection ic, GameObject go);
        static partial void CB_PATH_Check_Shooter(GameObject _copiedPrefab, ref string saveLocation);
        #endregion

        #endregion

        static List<string> CB_previewConverts = new List<string>();
        static Dictionary<GameObject, string> CB_prefabs = new Dictionary<GameObject, string>();

        #region CONTEXT Menus
        [MenuItem("CONTEXT/vItemCollection/Add MP Components for vProjectileControle")]
        public static void CB_CONTEXT_vItemCollection(MenuCommand command)
        {
            vItemCollection prefab = (vItemCollection)command.context;
            CB_COMP_vItemCollection(prefab.gameObject);
        }
        [MenuItem("CONTEXT/vBreakableObject/Replace vBreakableObject with MP version")]
        public static void CB_CONTEXT_vBreakableObject(MenuCommand command)
        {
            vBreakableObject prefab = (vBreakableObject)command.context;
            CB_COMP_vBreakableObject(prefab.gameObject);
        }
        [MenuItem("CONTEXT/vHealthController/Replace vHealthController with MP Version")]
        public static void CB_CONTEXT_vHealthController(MenuCommand command)
        {
            vHealthController prefab = (vHealthController)command.context;
            CB_COMP_vHealthController(prefab.gameObject);
        }
        #endregion

        #region Convert Logic
        public static void CB_COMP_vHealthController(GameObject target)
        {
            if (target.GetComponent<vHealthController>())
            {
                if (!target.GetComponent<SyncHealthController>())
                {
                    target.AddComponent<SyncHealthController>();
                }
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                vHealthController hc = target.GetComponent<vHealthController>();
                if (!E_PlayerEvents.HasUnityEvent(hc.onReceiveDamage, "SendDamageOverNetwork", target.GetComponent<SyncHealthController>()))
                {
                    UnityEventTools.AddPersistentListener(hc.onReceiveDamage, target.GetComponent<SyncHealthController>().SendDamageOverNetwork);
                }
            }
            if (target.GetComponentInChildren<vHealthController>())
            {
                foreach (vHealthController hc in target.GetComponentsInChildren<vHealthController>())
                {
                    if (!hc.gameObject.GetComponent<SyncHealthController>())
                    {
                        hc.gameObject.AddComponent<SyncHealthController>();
                    }
                    E_Helpers.SetObjectIcon(hc.gameObject, E_Core.h_genericIcon);
                    if (!E_PlayerEvents.HasUnityEvent(hc.onReceiveDamage, "SendDamageOverNetwork", hc.gameObject.GetComponent<SyncHealthController>()))
                    {
                        UnityEventTools.AddPersistentListener(hc.onReceiveDamage, hc.gameObject.GetComponent<SyncHealthController>().SendDamageOverNetwork);
                    }
                }
            }
        }
        public static void CB_COMP_vBreakableObject(GameObject target)
        {
            if (target.GetComponent<vBreakableObject>())
            {
                if (!target.GetComponent<NetworkBreakObject>())
                {
                    target.AddComponent<NetworkBreakObject>();
                }
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vBreakableObject>().OnBroken, "BreakObject", target.GetComponent<NetworkBreakObject>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vBreakableObject>().OnBroken, target.GetComponent<NetworkBreakObject>().BreakObject);
                }
            }
            if (target.GetComponentInChildren<vBreakableObject>())
            {
                foreach (vBreakableObject bo in target.GetComponentsInChildren<vBreakableObject>())
                {
                    if (!bo.gameObject.GetComponent<NetworkBreakObject>())
                    {
                        bo.gameObject.AddComponent<NetworkBreakObject>();
                    }
                    if (!E_PlayerEvents.HasUnityEvent(bo.OnBroken, "BreakObject", bo.gameObject.GetComponent<NetworkBreakObject>()))
                    {
                        UnityEventTools.AddPersistentListener(bo.OnBroken, bo.gameObject.GetComponent<NetworkBreakObject>().BreakObject);
                    }
                    E_Helpers.SetObjectIcon(bo.gameObject, E_Core.h_genericIcon);
                }
            }
        }
        public static void CB_COMP_vItemCollection(GameObject target)
        {
            //BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            GameObject go = null;
            vItemCollection ic = null;
            if (target.GetComponent<vItemCollection>())
            {
                go = target;
                ic = target.GetComponent<vItemCollection>();
            }
            else if (target.GetComponentInChildren<vItemCollection>())
            {
                go = target.GetComponentInChildren<vItemCollection>().transform.gameObject;
                ic = target.GetComponentInChildren<vItemCollection>();
            }
            if (go)
            {
                if (!go.GetComponent<SyncItemCollection>())
                {
                    go.AddComponent<SyncItemCollection>();
                }
                go.GetComponent<SyncItemCollection>().OnPressActionInput = ic.OnPressActionInput;
                go.GetComponent<SyncItemCollection>().onPressActionInputWithTarget = ic.onPressActionInputWithTarget;
                go.GetComponent<SyncItemCollection>().onPressActionDelay = ic.onPressActionDelay;
                bool sync = !FindObjectOfType<NetworkManager>().syncScenes;
                if (sync == true)
                {
                    go.GetComponent<SyncItemCollection>().OnSceneEnterUpdate = ic.OnPressActionInput;
                    if (ic.transform.parent != null)
                    {
                        go.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), ic.transform.parent);
                    }
                    else
                    {
                        go.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), ic.transform);
                    }
                }
                go.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), sync);
                //bool skipStart = (ic.items.Count > 0);
                go.GetComponent<SyncItemCollection>().GetType().GetField("skipStartCheck", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), false);
                go.GetComponent<SyncItemCollection>().GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), true);
                go.GetComponent<SyncItemCollection>().GetType().GetField("resourcesPrefab", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), ic.transform.root.name);

                ic.OnPressActionInput = new UnityEvent();
                ic.onPressActionInputWithTarget = new OnDoActionWithTarget();
                ic.onPressActionDelay = 0;

                UnityEventTools.AddPersistentListener(ic.onPressActionInputWithTarget, go.GetComponent<SyncItemCollection>().Collect);

                E_Helpers.SetObjectIcon(go, E_Core.h_genericIcon);

                CB_COMP_SHOOTER_vItemCollection(ic, go);
            }
        }

        #endregion

        #region Check Logic
        void CB_CHECK_CORE(GameObject target)
        {
            if (target.GetComponent<vItemCollection>() || target.GetComponentInChildren<vItemCollection>())
            {
                CB_previewConverts.Add("* Add SyncItemCollection component");
                CB_previewConverts.Add("* Copy events -> SyncItemCollection");
                CB_previewConverts.Add("* Add custom event to call SyncItemCollection");
                CB_previewConverts.Add("* Remove copied events from vItemCollection");
            }
            if (target.GetComponent<vBreakableObject>() || target.GetComponentInChildren<vBreakableObject>())
            {
                CB_previewConverts.Add("* Add PhotonView component");
                CB_previewConverts.Add("* Add NetworkBreakObject component");
                CB_previewConverts.Add("* Add BreakObject event -> vBreakableObject");
            }
            if (target.GetComponent<vHealthController>() || target.GetComponentInChildren<vHealthController>())
            {
                CB_previewConverts.Add("* Add PhotonView component");
                CB_previewConverts.Add("* Add SyncHealthController component");
                CB_previewConverts.Add("* Add SendDamageOverNetwork event -> vHealthController");
            }
        }
        void CB_CHECK_CORE_HasComp(GameObject target, bool _cItemCollection, bool _cvBreakableObjects, bool _cvHealthController, string prefabPath)
        {
            if (_cItemCollection == true && (target.GetComponent<vItemCollection>() || target.GetComponentInChildren<vItemCollection>()) &&
                        (!target.GetComponent<v_AIController>() && !target.transform.GetComponentInParent<v_AIController>()) &&
                        (!target.GetComponent<vThirdPersonController>() && !target.transform.GetComponentInParent<vThirdPersonController>()))
            {
                CB_prefabs.Add(target, prefabPath);
            }
            else if (_cvBreakableObjects == true && (target.GetComponent<vBreakableObject>() || target.GetComponentInChildren<vBreakableObject>()) &&
                (!target.GetComponent<v_AIController>() && !target.transform.GetComponentInParent<v_AIController>()))
            {
                CB_prefabs.Add(target, prefabPath);
            }
            else if (_cvHealthController == true && (target.GetComponent<vHealthController>() || target.GetComponentInChildren<vHealthController>()) &&
                (!target.GetComponent<v_AIController>() && !target.transform.GetComponentInParent<v_AIController>()) &&
                (!target.GetComponent<vThirdPersonController>() && !target.transform.GetComponentInParent<vThirdPersonController>()))
            {
                CB_prefabs.Add(target, prefabPath);
            }
        }
        #endregion

        #region Asset Menus
        [MenuItem("Assets/CB Games/Convert Item To Multiplayer")]
        public static void ConvertItemToMultiplayer(MenuCommand command)
        {
            GameObject prefab = (GameObject)Selection.activeObject;
            string saveLocation = string.Format("Assets{0}Resources{0}", Path.DirectorySeparatorChar);
            E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(saveLocation));
            Debug.Log("Converting Prefab: " + prefab.name);
            try
            {
                CB_ConvertPrefabToMultiplayer(prefab.gameObject);
                
                // Change save path if needed
                CB_PATH_Check_Core(prefab.gameObject, ref saveLocation);
                CB_PATH_Check_Shooter(prefab.gameObject, ref saveLocation);

                if (EditorUtility.DisplayDialog("SUCCESS!",
                    "You have successfully converted: " + prefab.name + ". " +
                    "The multiplayer version can be found in the "+ saveLocation.Replace(prefab.name+".prefab","") + " folder.",
                        "Great!"))
                { }
                Debug.Log("Successfully Converted Prefab: " + prefab.name);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                if (EditorUtility.DisplayDialog("FAILURE!",
                    "Failed to convert: " + prefab.name + " with error: \n\n " + ex.ToString(),
                        "Okay"))
                { }
            }
        }
        public static void CB_ConvertPrefabToMultiplayer(GameObject targetPrefab)
        {
            GameObject _copiedPrefab = GameObject.Instantiate(targetPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            _copiedPrefab.name = "MP_" + _copiedPrefab.name.Replace("(Clone)", "");
            string saveLocation = string.Format("Assets{0}Resources{0}{1}.prefab", Path.DirectorySeparatorChar, _copiedPrefab.name);
            CB_COMP_vItemCollection(_copiedPrefab);
            CB_COMP_vBreakableObject(_copiedPrefab);
            CB_COMP_vHealthController(_copiedPrefab);

            #region Shooter Template
            CB_COMP_vProjectileControle(_copiedPrefab);
            CB_COMP_vShooterWeapon(_copiedPrefab);
            #endregion

            // Change save path if needed
            CB_PATH_Check_Core(_copiedPrefab, ref saveLocation);
            CB_PATH_Check_Shooter(_copiedPrefab, ref saveLocation);

            if (E_Helpers.FileExists(saveLocation))
            {
                if (EditorUtility.DisplayDialog("Duplicate Detected!",
                    string.Format("Prefab \"{0}\" already exists, would you like to overwrite it or not copy it?", _copiedPrefab.name),
                    "Overwrite", "Not Copy"))
                {
                    //Overwrite
                    E_Helpers.DeleteFile(saveLocation);
                    PrefabUtility.SaveAsPrefabAsset(_copiedPrefab, saveLocation);
                }
                else
                {
                    Debug.Log("Skipped: " + _copiedPrefab.name);
                }
            }
            else
            {
                //Doesn't exist create it
                saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                PrefabUtility.SaveAsPrefabAsset(_copiedPrefab, saveLocation);
            }
            DestroyImmediate(_copiedPrefab);
        }
        #endregion

        #region Path Change Logic
        public static void CB_PATH_Check_Core(GameObject _copiedPrefab, ref string saveLocation)
        {
            if (_copiedPrefab.GetComponent<vItemCollection>() || _copiedPrefab.GetComponentInChildren<vItemCollection>())
            {
                saveLocation = saveLocation.Replace("Resources" + Path.DirectorySeparatorChar, "Resources/");
                string dirLoc = saveLocation.Replace(_copiedPrefab.name + ".prefab", "");
                E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(dirLoc));
            }
            else if (_copiedPrefab.GetComponent<vBreakableObject>() || _copiedPrefab.GetComponentInChildren<vBreakableObject>() ||
                _copiedPrefab.GetComponent<vHealthController>() || _copiedPrefab.GetComponentInChildren<vHealthController>())
            {
                saveLocation = saveLocation.Replace("Resources" + Path.DirectorySeparatorChar, "MP_Converted/Objects/");
                string dirLoc = saveLocation.Replace(_copiedPrefab.name + ".prefab", "");
                E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(dirLoc));
            }
        }
        #endregion
    }
}