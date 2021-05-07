using CBGames.Objects;
using Invector;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_AutoRemediate : MonoBehaviour
    {
        #region CONTEXT Menus
        [MenuItem("CONTEXT/vItemCollection/Attempt To Auto Fix Failed Tests")]
        public static void CB_CONTEXT_Remediate_vItemCollection(MenuCommand command)
        {
            vItemCollection prefab = (vItemCollection)command.context;
            CB_REMEDIATE_vItemCollection(prefab, true);
        }
        [MenuItem("CONTEXT/SyncObject/Attempt To Auto Fix Failed Tests")]
        public static void CB_CONTEXT_Remediate_SyncObject(MenuCommand command)
        {
            SyncObject prefab = (SyncObject)command.context;
            CB_REMEDIATE_SyncObject(prefab, true);
        }
        [MenuItem("CONTEXT/SyncObject/Attempt To Auto Fix Failed Tests")]
        public static void CB_CONTEXT_Remediate_vInventory(MenuCommand command)
        {
            vInventory prefab = (vInventory)command.context;
            CB_REMEDIATE_vInventory(prefab, true);
        }
        #endregion

        #region Remediate Logic
        public static bool CB_REMEDIATE_vItemCollection(vItemCollection prefab, bool debug = false)
        {
            bool successful = true;
            if (prefab.GetComponent<SyncItemCollection>())
            {
                SyncItemCollection sic = (prefab.GetComponent<SyncItemCollection>()) ? prefab.GetComponent<SyncItemCollection>() : prefab.GetComponentInChildren<SyncItemCollection>();
                if ((bool)sic.GetType().GetField("skipStartCheck", E_Helpers.allBindings).GetValue(sic) != (prefab.items.Count > 0))
                {
                    if (debug) Debug.Log("Enabling skipStartCheck on SyncItemCollection");
                    sic.GetType().GetField("skipStartCheck", E_Helpers.allBindings).SetValue(sic, true);
                }
                if ((bool)prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(prefab.gameObject.GetComponent<SyncItemCollection>()) == true &&
                        (Transform)prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).GetValue(prefab.gameObject.GetComponent<SyncItemCollection>()) == null)
                {
                    if (debug) Debug.Log("Setting holder to same gameobject");
                    prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(prefab.gameObject.GetComponent<SyncItemCollection>(), prefab.transform);
                }
                if ((bool)prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(prefab.gameObject.GetComponent<SyncItemCollection>()) == true &&
                        (string)prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("resourcesPrefab", E_Helpers.allBindings).GetValue(prefab.gameObject.GetComponent<SyncItemCollection>()) == null)
                {
                    if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab.gameObject))
                    {
                        if (debug) Debug.Log("Adding resourcesPrefab from found reference");
                        GameObject reference = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab.gameObject);
                        prefab.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("resourcesPrefab", E_Helpers.allBindings).SetValue(prefab.gameObject.GetComponent<SyncItemCollection>(), reference);
                    }
                    else if (debug)
                    {
                        Debug.Log("Unable to find reference prefab for resourcesPrefab");
                        successful = false;
                    }
                }
                if (prefab.GetComponent<SyncItemCollection>().OnSceneEnterUpdate.GetPersistentEventCount() < 1)
                {
                    try
                    {
                        if (debug) Debug.Log("Attempting to add missing OnSceneEnterUpdate unityevents...");
                        UnityEventTools.AddBoolPersistentListener(prefab.GetComponent<SyncItemCollection>().OnSceneEnterUpdate, prefab.gameObject.SetActive, false);
                        if (debug) Debug.Log("SUCCESS!");
                    }
                    catch
                    {
                        if (debug) Debug.Log("FAILED TO AUTO REMEDIATE.");
                        successful = false;
                    }
                }
                if (prefab.GetComponent<SyncItemCollection>().OnPressActionInput.GetPersistentEventCount() != prefab.OnPressActionInput.GetPersistentEventCount())
                {
                    List<E_UnityEvents> ignore = new List<E_UnityEvents>();
                    ignore.Add(new E_UnityEvents(prefab.GetComponent<SyncItemCollection>(), "Collect"));
                    E_Helpers.CopyUnityEvents(prefab, "OnPressActionInput", prefab.GetComponent<SyncItemCollection>(), "OnPressActionInput", true, ignore);
                }
                if (prefab.GetComponent<SyncItemCollection>().onPressActionInputWithTarget.GetPersistentEventCount() != prefab.onPressActionInputWithTarget.GetPersistentEventCount())
                {
                    try
                    {
                        if (debug) Debug.Log("Attempting to copy OnPressActionInput events...");
                        List<E_UnityEvents> ignore = new List<E_UnityEvents>();
                        ignore.Add(new E_UnityEvents(prefab.GetComponent<SyncItemCollection>(), "Collect"));
                        E_Helpers.CopyUnityEvents(prefab, "onPressActionInputWithTarget", prefab.GetComponent<SyncItemCollection>(), "onPressActionInputWithTarget", true, ignore);
                        if (debug) Debug.Log("SUCCESS!");
                    }
                    catch
                    {
                        if (debug) Debug.Log("FAILED TO AUTO REMEDIATE.");
                        successful = false;
                    }
                }
                if (prefab.OnPressActionInput.GetPersistentEventCount() > 1)
                {
                    prefab.OnPressActionInput = null;
                }
                if (prefab.onPressActionInputWithTarget.GetPersistentEventCount() > 1)
                {
                    prefab.OnPressActionInput = null;
                }
                if (!E_Helpers.ContainsUnityEvent(prefab, "OnPressActionInput", prefab.GetComponent<SyncItemCollection>(), "Collect"))
                {
                    try
                    {
                        if (debug) Debug.Log("Attempting to add SyncItemCollection unity event to OnPressInput...");
                        UnityEventTools.AddPersistentListener(prefab.OnPressActionInput, prefab.GetComponent<SyncItemCollection>().Collect);
                        prefab.OnPressActionInput.AddListener(prefab.GetComponent<SyncItemCollection>().Collect);
                        if (debug) Debug.Log("SUCCESS!");
                    }
                    catch
                    {
                        if (debug) Debug.Log("FAILED TO AUTO REMEDIATE");
                        successful = false;
                    }
                }
                if (debug) Debug.Log("Completed Auto Remediation.");
            }
            return successful;
        }
        public static bool CB_REMEDIATE_SyncObject(SyncObject prefab, bool debug = false)
        {
            try
            {
                vItemManager im = prefab.GetComponentInParent<vItemManager>();
                vWeaponHolderManager whm = prefab.GetComponentInParent<vWeaponHolderManager>();
                if (im.equipPoints.Find(p => p.handler.defaultHandler.gameObject.Equals(prefab.gameObject)) != null)
                {
                    EquipPoint ep = im.equipPoints.Find(p => p.handler.defaultHandler.gameObject.Equals(prefab.gameObject));
                    if (prefab.syncEnable != false)
                    {
                        if (debug) Debug.Log("Disabling \"syncEnable\"");
                        prefab.syncEnable = false;
                    }
                    if (prefab.syncDisable != false)
                    {
                        if (debug) Debug.Log("Disabling \"syncDisable\"");
                        prefab.syncDisable = false;
                    }
                    if (prefab.syncDestroy != false)
                    {
                        if (debug) Debug.Log("Disabling \"syncDestroy\"");
                        prefab.syncDestroy = false;
                    }
                    if (prefab.syncImmediateChildren != true)
                    {
                        if (debug) Debug.Log("Enabling \"syncImmediateChildren\"");
                        prefab.syncImmediateChildren = true;
                    }
                    if (prefab.isWeaponHolder != false)
                    {
                        if (debug) Debug.Log("Disabling \"isWeaponHolder\"");
                        prefab.isWeaponHolder = false;
                    }

                    bool leftHanded = ep.equipPointName.ToLower().Contains("left");
                    if (prefab.isLeftHanded != leftHanded && prefab.syncImmediateChildren == true)
                    {
                        if (debug) Debug.Log("Setting \"isLeftHanded\" to " + leftHanded);
                        prefab.isLeftHanded = leftHanded;
                    }
                }
                else
                {
                    bool found = false;
                    foreach (EquipPoint ep in im.equipPoints)
                    {
                        foreach (Transform handler in ep.handler.customHandlers)
                        {
                            if (handler.Equals(prefab.transform))
                            {
                                bool leftHanded = ep.equipPointName.ToLower().Contains("left") || handler.transform.parent.name.ToLower().Contains("left");
                                if (prefab.syncEnable != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncEnable\"");
                                    prefab.syncEnable = false;
                                }
                                if (prefab.syncDisable != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncDisable\"");
                                    prefab.syncDisable = false;
                                }
                                if (prefab.syncDestroy != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncDestroy\"");
                                    prefab.syncDestroy = false;
                                }
                                if (prefab.syncImmediateChildren != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncImmediateChildren\"");
                                    prefab.syncImmediateChildren = true;
                                }
                                if (prefab.isWeaponHolder != false)
                                {
                                    if (debug) Debug.Log("Disabling \"isWeaponHolder\"");
                                    prefab.isWeaponHolder = false;
                                }
                                if (prefab.isLeftHanded != leftHanded && prefab.syncImmediateChildren == true)
                                {
                                    if (debug) Debug.Log("Setting \"isLeftHanded\" to " + leftHanded);
                                    prefab.isLeftHanded = leftHanded;
                                }
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        foreach (vWeaponHolder wh in whm.holders)
                        {
                            if (wh.holderObject.Equals(prefab.gameObject))
                            {
                                bool leftHanded = wh.equipPointName.ToLower().Contains("left");
                                if (prefab.syncEnable != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncEnable\"");
                                    prefab.syncEnable = true;
                                }
                                if (prefab.syncDisable != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncDisable\"");
                                    prefab.syncDisable = true;
                                }
                                if (prefab.syncDestroy != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncDestroy\"");
                                    prefab.syncDestroy = false;
                                }
                                if (prefab.syncImmediateChildren != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncImmediateChildren\"");
                                    prefab.syncImmediateChildren = true;
                                }
                                if (prefab.isWeaponHolder != true)
                                {
                                    if (debug) Debug.Log("Enabling \"isWeaponHolder\"");
                                    prefab.isWeaponHolder = true;
                                }
                                if (prefab.isLeftHanded != leftHanded && prefab.syncImmediateChildren == true)
                                {
                                    if (debug) Debug.Log("Setting isLeftHanded to " + leftHanded);
                                    prefab.isLeftHanded = leftHanded;
                                }
                            }
                            if (wh.weaponObject.Equals(prefab.gameObject))
                            {
                                if (prefab.syncEnable != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncEnable\"");
                                    prefab.syncEnable = true;
                                }
                                if (prefab.syncDisable != true)
                                {
                                    if (debug) Debug.Log("Enabling \"syncDisable\"");
                                    prefab.syncDisable = true;
                                }
                                if (prefab.syncDestroy != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncDestory\"");
                                    prefab.syncDestroy = false;
                                }
                                if (prefab.syncImmediateChildren != false)
                                {
                                    if (debug) Debug.Log("Disabling \"syncImmediateChildren\"");
                                    prefab.syncImmediateChildren = false;
                                }
                                if (prefab.isWeaponHolder != false)
                                {
                                    if (debug) Debug.Log("Disabling \"isWeaponHolder\"");
                                    prefab.isWeaponHolder = false;
                                }
                                if (prefab.isLeftHanded != false)
                                {
                                    if (debug) Debug.Log("Disabling \"isLeftHanded\"");
                                    prefab.isLeftHanded = false;
                                }
                            }
                        }
                    }
                }
                if (debug) Debug.Log("Completed Auto Remediation.");
                return true;
            }
            catch
            {
                if (debug) Debug.Log("Failed remediation");
                return false;
            }
        }
        public static bool CB_REMEDIATE_vInventory(vInventory prefab, bool debug = false)
        {
            try
            {
                if (prefab.timeScaleWhileIsOpen != 1)
                {
                    if (debug) Debug.Log("Setting time scale while is open to 1");
                    prefab.timeScaleWhileIsOpen = 1;
                }
                if (prefab.dontDestroyOnLoad == true)
                {
                    if (debug) Debug.Log("Setting dontDestroyOnLoad to false");
                    prefab.dontDestroyOnLoad = false;
                }
                if (debug) Debug.Log("Completed Auto Remediation.");
                return true;
            }
            catch
            {
                Debug.Log("Failed to Auto Remediate");
                return false;
            }
        }
        #endregion
    }
}