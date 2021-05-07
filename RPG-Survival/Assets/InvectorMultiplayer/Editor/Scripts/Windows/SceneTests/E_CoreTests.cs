using CBGames.Objects;
using Invector.vItemManager;
using System.Collections.Generic;
using UnityEngine;
using CBGames.Player;
using UnityEditor;
using CBGames.UI;
using CBGames.Core;
using Invector.vCharacterController;
using Invector.vCamera;
using UnityEngine.SceneManagement;
using System.Reflection;
using Invector.vCharacterController.vActions;
using Invector;
using Invector.vCharacterController.AI;
using System;
using Photon.Pun;
using UnityEditor.Events;
using UnityEngine.UI;
using Photon.Voice.Unity;

namespace CBGames.Editors
{
    public partial class E_TestScene
    {
        #region Partial Methods
        private bool shooterExists = false;
        partial void SHOOTER_CheckAddonEnabled();
        partial void CORE_PerformGenericActionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        #endregion

        void CORE_CheckNestedUI(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vThirdPersonController cont in FindObjectsOfType<vThirdPersonController>())
            {
                if (cont.gameObject.GetComponentInChildren<vHUDController>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vHUDController - This UI is nested into the player object. This will cause this ui to appear based on the joining players. " +
                        "Make sure you don't have the vHUDController as a child of any gameobject, other than a standalone Canvas to render it.",
                        cont.gameObject.GetComponentInChildren<vHUDController>()
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_CheckNestedCamera(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vThirdPersonController cont in FindObjectsOfType<vThirdPersonController>())
            {
                if (cont.gameObject.GetComponentInChildren<vThirdPersonCamera>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vThirdPersonCamera - This camera is nested into the player object. This will cause cameras to switch to the newest player that " +
                        "joins into the room instead of staying with the owning player. Make sure you don't have the vThirdPersonCamera as a child of any " +
                        "gameobject.",
                        cont.gameObject.GetComponentInChildren<vThirdPersonCamera>()
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformNetworkCullingTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            byte prev_group = 0;
            float prev_distance = 0;
            foreach(NetworkCulling nc in FindObjectsOfType<NetworkCulling>())
            {
                prev_group = 0;
                foreach (CullDistance item in (List<CullDistance>)nc.GetType().GetField("cullDistances", E_Helpers.allBindings).GetValue(nc))
                {
                    if (_autoFixTests)
                    {
                        if (item.interest_group <= prev_group)
                        {
                            item.interest_group = (byte)((int)prev_group + 1);
                        }
                        if (item.distance <= prev_distance)
                        {
                            item.distance = prev_distance + 15;
                        }
                        if (item.moveSpeed < 3)
                        {
                            item.moveSpeed = 3;
                        }
                        if (item.rotateSpeed < 3)
                        {
                            item.rotateSpeed = 3;
                        }
                    }
                    if (item.interest_group <= prev_group)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "NetworkCulling - There is a cull group item that has an interest group that is below or equal to the previous one. " +
                            "These interest groups should not match and should follow a sequential ordering as to prevent confusion.",
                            nc
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (item.distance <= prev_distance)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "NetworkCulling - There is a cull group item that has a distance that is the same or less than the previous cull " +
                            "group item. This will lead to network culling in an incorrect fashion. Lower group items should always have a greater " +
                            "distance.",
                            nc
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (item.moveSpeed < 3)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "NetworkCulling - There is a cull group item that has a move speed that is below 3. While this will produce a very " +
                            "smooth movement it also means the players position when they are a part of this group will not fully match across " +
                            "the network accuratly.",
                            nc
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (item.rotateSpeed < 3)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "NetworkCulling - There is a cull group item that has a rotate speed that is below 3. While this will produce a very " +
                            "smooth rotation it also means the players rotation when they are a part of this group will not fully match across " +
                            "the network accuratly.",
                            nc
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    prev_group = item.interest_group;
                    prev_distance = item.distance;
                }
                if (_autoFixTests)
                {
                    if (!nc.transform.GetComponent<SyncPlayer>())
                    {
                        nc.transform.gameObject.AddComponent<SyncPlayer>();
                    }
                    if ((byte)nc.GetType().GetField("last_group", E_Helpers.allBindings).GetValue(nc) < 20)
                    {
                        nc.GetType().GetField("last_group", E_Helpers.allBindings).SetValue(nc, (byte)255);
                    }
                    if (nc.last_moveSpeed < 3)
                    {
                        nc.last_moveSpeed = 3;
                    }
                    if (nc.last_rotateSpeed < 3)
                    {
                        nc.last_rotateSpeed = 3;
                    }
                }
                if (!nc.transform.GetComponent<SyncPlayer>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkCulling - This object is missing the \"SyncPlayer\" component. The \"NetworkCulling\" " +
                        "component requires this component to function properly. It must be at the same level as the " +
                        "\"NetworkCulling\" component. If you don't fix this you will get looping errors at runtime.",
                        nc
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((byte)nc.GetType().GetField("last_group",E_Helpers.allBindings).GetValue(nc) < 20)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkCulling - The last interest group is quite low. There is a chance that you could " +
                        "be using this group for something else. If you're not sure set this to a number above 200.",
                        nc
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nc.last_moveSpeed < 3)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkCulling - The last move speed is low. While the player movement will be very smooth " +
                        "it also means that the player's position will not be very accurate across the network.",
                        nc
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nc.last_rotateSpeed < 3)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkCulling - The last rotate speed is low. While the player rotation will be very smooth " +
                        "it also means that the player's rotation will not be very accurate across the network.",
                        nc
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformItemCollectionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vItemCollection ic in FindObjectsOfType<vItemCollection>())
            {
                try
                {
                    if (_autoFixTests)
                    {
                        if (!ic.gameObject.GetComponent<SyncItemCollection>())
                        {
                            ic.gameObject.AddComponent<SyncItemCollection>();
                        }
                        E_AutoRemediate.CB_REMEDIATE_vItemCollection(ic, false);
                    }
                    if (!ic.gameObject.GetComponent<SyncItemCollection>())
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vItemCollection - This object is missing the \"SyncItemCollection\" component " +
                            "that means things like drops/pickups/etc will not be synced for this item.",
                            ic
                        ));
                        continue;
                    }
                    else
                    {
                        passed += 1;
                    }
                    SyncItemCollection sic = (ic.GetComponent<SyncItemCollection>()) ? ic.GetComponent<SyncItemCollection>() : ic.GetComponentInChildren<SyncItemCollection>();
                    if ((bool)sic.GetType().GetField("skipStartCheck", E_Helpers.allBindings).GetValue(sic) != (ic.items.Count > 0))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SyncItemCollection - The 'SyncItemCollection' component needs to have the " +
                            "'Items In ItemCollection' value set to match true/false based on if you have items " +
                            "already set in the 'vItemCollection' component on this same object.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(ic.onPressActionInputWithTarget, "Collect", ic.gameObject.GetComponent<SyncItemCollection>()) &&
                        !E_PlayerEvents.HasUnityEvent(ic.OnPressActionInput, "Collect", ic.gameObject.GetComponent<SyncItemCollection>()))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vItemCollection - \"vItemCollection\" is missing the \"Collect\" action " +
                            "on the \"OnPressActionInputWithTarget\" or \"OnPressActionInput\" UnityEvent(s). This " +
                            "function comes from \"SyncItemCollection\" component. If this isn't " +
                            "applied this object will not disappear across the network when it is collected.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (ic.onPressActionInputWithTarget.GetPersistentEventCount() > 1 ||
                        ic.OnPressActionInput.GetPersistentEventCount() > 1)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vItemCollectionTest - \"vItemCollection\" has the \"Collect\" action applied " +
                            "on the \"OnPressActionInputWithTarget\" or \"OnPressActionInput\" unityevent. However, it also has more " +
                            "than just the \"Collect\" event. These additional events should be copied to the " +
                            "\"SyncItemCollection\" components and removed from \"vItemCollection\" in order to work across the network.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if (E_PlayerEvents.HasUnityEvent(ic.onPressActionInputWithTarget, "Collect", ic.gameObject.GetComponent<SyncPlayer>()) && ic.onPressActionDelay > 0)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vItemCollectionTest - \"vItemCollection\" has the \"OnPressActionDelay\" value above zero. " +
                            "this value should be copied to the \"SyncItemCollection\" component and the \"vItemCollection\" " +
                            "value should be set to zero.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if ((bool)ic.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(ic.gameObject.GetComponent<SyncItemCollection>()) == true &&
                        (Transform)ic.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("holder", E_Helpers.allBindings).GetValue(ic.gameObject.GetComponent<SyncItemCollection>()) == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vItemCollectionTest - \"syncCrossScenes\" is true but the holder is not set! " +
                            "the holder is required to be able to track the position so the code can know the item's location to update " +
                            "when traveling through the scenes.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if ((bool)ic.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(ic.gameObject.GetComponent<SyncItemCollection>()) == true &&
                        (string)ic.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("resourcesPrefab", E_Helpers.allBindings).GetValue(ic.gameObject.GetComponent<SyncItemCollection>()) == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vItemCollectionTest - \"syncCreateDestroy\" is true but the \"resourcesPrefab\" is not set! " +
                            "If this isn't set then this item will not be spawned in when new players enter the scene correctly.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if ((bool)ic.gameObject.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(ic.gameObject.GetComponent<SyncItemCollection>()) == true &&
                        ic.gameObject.GetComponent<SyncItemCollection>().OnSceneEnterUpdate.GetPersistentEventCount() < 1)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SyncItemCollection - \"syncCrossScenes\" is true but there are no events " +
                            "in \"OnSceneEnterUpdate\". This event is called when this item has already been " +
                            "interacted with by a player and you're first entering this scene. So, for example, if this is " +
                            "a item you probably want to make this item not visible. If this is a treasure " +
                            "chest you might want to make this treasure chest non-interactable and already open. " +
                            "It depends on how you want to show that this has already been interacted with.",
                            ic
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                }
                catch
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vItemCollectionTest - An unknown error occured while testing this object " +
                        "the most common occurance as to why this is happening is because this has " +
                        "some Empty gameobject UnityEvents.",
                        ic
                    ));
                }
            }

            string[] paths = GetResourcePrefabs();
            GameObject target;
            foreach (string path in paths)
            {
                UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(path);
                try
                {
                    target = (GameObject)prefab;
                    if (!target.GetComponent<vItemCollection>() && !target.GetComponentInChildren<vItemCollection>()) continue;
                    if (!target.GetComponent<SyncItemCollection>() && !target.GetComponentInChildren<SyncItemCollection>())
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SyncItemCollection - The 'SyncItemCollection' component is missing from this prefab. " +
                            "This will make it so this item is in no way synced across the network.",
                            AssetDatabase.LoadMainAssetAtPath(path)
                        ));
                    }
                    else
                    {
                        passed += 1;
                        vItemCollection target_ic = (target.GetComponent<vItemCollection>()) ? target.GetComponent<vItemCollection>() : target.GetComponentInChildren<vItemCollection>();
                        SyncItemCollection target_sic = (target.GetComponent<SyncItemCollection>()) ? target.GetComponent<SyncItemCollection>() : target.GetComponentInChildren<SyncItemCollection>();
                        if ((bool)target_sic.GetType().GetField("skipStartCheck", E_Helpers.allBindings).GetValue(target_sic) == true)
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "SyncItemCollection - The 'SyncItemCollection' component needs to have the " +
                                "'Items In Collection' value set to false in the resources folder since these " +
                                "are dynamic objects and their items in the vItemCollection will get populated " +
                                "at runtime when the item is dropped. If you plan on spawning this in a different " +
                                "manner other than dropping it from the inventory then its okay to have this be set " +
                                "to true.",
                                AssetDatabase.LoadMainAssetAtPath(path)
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }

                        if ((bool)target_sic.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(target_sic) == false)
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "SyncItemCollection - The 'SyncItemCollection' component has the 'Sync Cross Scenes' set to false. " +
                                "That means this item will in no way be synced across unity scenes/photon rooms.",
                                AssetDatabase.LoadMainAssetAtPath(path)
                            ));
                        }
                        else
                        {
                            passed += 1;
                            if ((Transform)target_sic.GetType().GetField("holder", E_Helpers.allBindings).GetValue(target_sic) == null)
                            {
                                failed += 1;
                                failures.Add(new DebugFormat(
                                    "SyncItemCollection - The 'SyncItemCollection' component has nothing in 'Track Position' . " +
                                    "Track position is essential get syncing this item correctly across unity scenes.",
                                    AssetDatabase.LoadMainAssetAtPath(path)
                                ));
                            }
                            else
                            {
                                passed += 1;
                            }
                            if (shooterExists == false)
                            {
                                if ((bool)target_sic.GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(target_sic) == false)
                                {
                                    failed += 1;
                                    failures.Add(new DebugFormat(
                                        "SyncItemCollection - The 'SyncItemCollection' component needs to have the 'Is Dynamic Obj' " +
                                        "set to true since items in the 'Resources' folder are only ever dynamically spawned into the world.",
                                        AssetDatabase.LoadMainAssetAtPath(path)
                                    ));
                                }
                                else
                                {
                                    passed += 1;
                                    if (string.IsNullOrEmpty((string)target_sic.GetType().GetField("resourcesPrefab", E_Helpers.allBindings).GetValue(target_sic)))
                                    {
                                        failed += 1;
                                        failures.Add(new DebugFormat(
                                            "SyncItemCollection - The 'SyncItemCollection' component needs to have the " +
                                            "'Resources Prefab' value set. Otherwise it doesn't know what object from the " +
                                            "resources folder to spawn in.",
                                            AssetDatabase.LoadMainAssetAtPath(path)
                                        ));
                                    }
                                    else
                                    {
                                        passed += 1;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
        void CORE_PerformPlayerRespawnTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (FindObjectsOfType<PlayerRespawn>().Length > 1)
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "PlayerRespawn - You have more than 1 \"PlayerRespawn\" component in this scene. This will make respawning impossible.",
                    null
                ));
            }
            else
            {
                passed += 1;
                if (FindObjectsOfType<PlayerRespawn>().Length > 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PlayerRespawn - You have more than 1 \"PlayerRespawn\" component in this scene. This will make respawning impossible.",
                        null
                    ));
                    return;
                }
                else
                {
                    passed += 1;
                    if (!E_Helpers.InspectorTagExists("RespawnPoint") || E_Helpers.InspectorTagExists("RespawnPoint") && GameObject.FindGameObjectsWithTag("RespawnPoint").Length < 1)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "PlayerRespawn - You don't have any respawn points in this scene. That will make respawning in " +
                            "this scene impossible. To add a respawn point go to CB Games > Network Manager > Respawn > Add Respawn Point.",
                            null
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    PlayerRespawn respawn = FindObjectOfType<PlayerRespawn>();
                    if (respawn)
                    {
                        passed += 1;
                        if (respawn.visualCountdown == null)
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "PlayerRespawn - You don't have anything in the \"visualCountdown\" variable " +
                                "this means the player will not see any visual indication that they are waiting to " +
                                "respawn.",
                                respawn
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }
                        if ((bool)respawn.GetType().GetField("broadcastDeathMessage", E_Helpers.allBindings).GetValue(respawn) == true)
                        {
                            if (string.IsNullOrEmpty((string)respawn.GetType().GetField("deathMessage", E_Helpers.allBindings).GetValue(respawn)))
                            {
                                failed += 1;
                                failures.Add(new DebugFormat(
                                    "PlayerRespawn - You have selected to broadcast a message on player death but " +
                                    "there is no message to broadcast in the field value. Either disable \"broadcastDeathMessage\" " +
                                    "or actually put a message to broadcast in here.",
                                    respawn
                                ));
                            }
                            else
                            {
                                passed += 1;
                            }
                        }
                        if (_autoFixTests)
                        {
                            if ((bool)respawn.GetType().GetField("debugging", E_Helpers.allBindings).GetValue(respawn) == true)
                            {
                                respawn.GetType().GetField("debugging", E_Helpers.allBindings).SetValue(respawn, false);
                            }
                        }
                        if ((bool)respawn.GetType().GetField("debugging", E_Helpers.allBindings).GetValue(respawn) == true)
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "PlayerRespawn - You have debug mode enabled on the player respawn. It's a good idea to disable this " +
                                "before the final build.",
                                respawn
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }
                        SpawnTeam[] teams = (SpawnTeam[])respawn.GetType().GetField("teams", E_Helpers.allBindings).GetValue(respawn);
                        if ((RespawnType)respawn.GetType().GetField("respawnType", E_Helpers.allBindings).GetValue(respawn) == RespawnType.TeamStatic &&
                            teams.Length < 1)
                        {
                            failed += 1;
                            failures.Add(new DebugFormat(
                                "PlayerRespawn - You have selected TeamStatic as your respawn method but haven't defined any teams.",
                                respawn
                            ));
                        }
                        else
                        {
                            passed += 1;
                            bool ispassed = true;
                            foreach (SpawnTeam team in teams)
                            {
                                if (string.IsNullOrEmpty(team.tag) || string.IsNullOrEmpty(team.teamName))
                                {
                                    ispassed = false;
                                    failed += 1;
                                    failures.Add(new DebugFormat(
                                        "PlayerRespawn - One of your defined teams is either missing its name or a spawn name defined.",
                                        respawn
                                    ));
                                }
                            }
                            if (ispassed == true)
                            {
                                passed += 1;
                            }
                        }
                    }
                    else
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "PlayerRespawn - There is no player respawn in the scene. This means that players will not be able to respawn.",
                            respawn
                        ));
                    }
                }
            }
        }
        void CORE_PerformUICoreLogicTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            UICoreLogic[] logics = FindObjectsOfType<UICoreLogic>();
            if (logics.Length > 1)
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "UICoreLogic - You have more than 1 UICoreLogic in the scene. Please make sure you only ever have 1.",
                    logics[0]
                ));
            }
            else if (logics.Length == 0)
            {
                failed += 1;
                warnings.Add(new DebugFormat(
                    "UICoreLogic - You don't have a UICoreLogic component in this scene. That probably means you " +
                    "haven't added a UI from the CB Games > UI > Add > Pre-Build UI > ... menu. This is okay if " +
                    "you're using your own UI. If not though, your characters will not spawn automatically without " +
                    "first being spawn via an event. This is pre-built for you in the Pre-Built UIs.",
                    null
                ));
            }
            else
            {
                passed += 1;
            }

            if (logics.Length == 1)
            {
                if (_autoFixTests)
                {
                    if (logics[0].selectablePlayers.Length < 1)
                    {
                        E_UI.CB_CALL_AddPlayersToExampleUI();
                    }
                    if ((bool)logics[0].GetType().GetField("debugging", E_Helpers.allBindings).GetValue(logics[0]) == true)
                    {
                        logics[0].GetType().GetField("debugging", E_Helpers.allBindings).SetValue(logics[0], false);
                    }
                }
                if (logics[0].selectablePlayers.Length < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "UICoreLogic - Dont have any selectable players. That means you won't be allowed to switch players.",
                        logics[0]
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (logics[0].sceneList.Count < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "UICoreLogic - Dont have any selectable scenes. That means you won't be able to implement a voting system in your UI.",
                        logics[0]
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((bool)logics[0].GetType().GetField("debugging", E_Helpers.allBindings).GetValue(logics[0]) == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "UICoreLogic - You have debugging enabled! You should disable this before your production release.",
                        logics[0]
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformvInventoryTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vInventory inv in FindObjectsOfType<vInventory>())
            {
                if (_autoFixTests)
                {
                    E_AutoRemediate.CB_REMEDIATE_vInventory(inv, false);
                }
                if (inv.timeScaleWhileIsOpen != 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vInventory - \"Time Scale While Is Open\" should not be below zero. Having a number other than 1 " +
                        "will result in delayed damage for the owner player and the networked player. This could result in " +
                        "disconnected damage percentages.",
                        inv
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (inv.dontDestroyOnLoad == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vInventory - \"Dont Destory On Load\" should be false. While this doesn't currently break anything " +
                        "this has a high possibility of having adverse effects in the future.",
                        inv
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformPlayerListUITests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            PlayerList pl = FindObjectOfType<PlayerList>();
            if (pl != null)
            {
                ChatBox chatbox = FindObjectOfType<ChatBox>();
                NetworkManager nm = FindObjectOfType<NetworkManager>();

                //Content check
                if (pl.GetType().GetField("content", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PlayerList - You don't have the \"Content\" field populated. That means the player list will not work at all.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Root Obj check
                if (pl.GetType().GetField("rootObj", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PlayerList - You don't have the \"Root Obj\" field populated. That means the player list will not work at all.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Root Obj check
                if (pl.GetType().GetField("playerJoinObject", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PlayerList - You don't have the \"Player Join Object\" field populated. That means nothing will " +
                        "be added to the UI when a player joins.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Key Check
                if ((PlayerList.PressType)pl.GetType().GetField("openWindow", E_Helpers.allBindings).GetValue(pl) == PlayerList.PressType.OnHold ||
                    (PlayerList.PressType)pl.GetType().GetField("openWindow", E_Helpers.allBindings).GetValue(pl) == PlayerList.PressType.OnPress)
                {
                    if (!E_Helpers.GetAllInputAxis().Contains((string)pl.GetType().GetField("keyToPress", E_Helpers.allBindings).GetValue(pl)))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "PlayerList - The \"Key To Press/Hold\" field is set to a key that isn't setup in your project",
                            pl
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }

                //Animation check
                if (pl.GetType().GetField("anim", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Anim\" field is not set. That means no animations will " +
                        "play when you open this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Open Anim check
                if (string.IsNullOrEmpty((string)pl.GetType().GetField("openAnimation", E_Helpers.allBindings).GetValue(pl)))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"openAnimation\" field is not set. That means no open animation will " +
                        "play when you open this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Close Anim check
                if (string.IsNullOrEmpty((string)pl.GetType().GetField("closeAnimation", E_Helpers.allBindings).GetValue(pl)))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"closeAnimation\" field is not set. That means no close animation will " +
                        "play when you close this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Sound Source check
                if (pl.GetType().GetField("soundSource", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Sound Source\" field is not set. That means no sounds of any kind " +
                        "will play for this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Open Sound Check
                if (pl.GetType().GetField("openSound", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Open Sound\" field is not set. That means no open sound " +
                        "will play for this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Close Sound Check
                if (pl.GetType().GetField("closeSound", E_Helpers.allBindings).GetValue(pl) == null)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Close Sound\" field is not set. That means no close sound " +
                        "will play for this UI element.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Volume Checks
                if ((float)pl.GetType().GetField("openSoundVolume", E_Helpers.allBindings).GetValue(pl) < 0.1f)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Open Sound Volume\" is set to be very low. You may not heard your " +
                        "open sound effect at all if this is set too low.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((float)pl.GetType().GetField("closeSoundVolume", E_Helpers.allBindings).GetValue(pl) < 0.1f)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Close Sound Volume\" is set to be very low. You may not heard your " +
                        "close sound effect at all if this is set too low.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Debugging Check
                if ((bool)pl.GetType().GetField("debugging", E_Helpers.allBindings).GetValue(pl) == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PlayerList - The \"Debugging\" field is set to true. You will get a lot of " +
                        "log output from this element. Be sure to disable this before the final build.",
                        pl
                    ));
                }
                else
                {
                    passed += 1;
                }

                //ChatBox Unity Event Checks
                if (!E_PlayerEvents.HasUnityEvent(chatbox.OnYouSubscribeToDataChannel, "SetPlayerList", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ChatBox - You're missing the \"PlayerList.SetPlayerList\" function call on the " +
                        "\"OnYouSubscribeToDataChannel\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        chatbox
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (!E_PlayerEvents.HasUnityEvent(chatbox.OnYouSubscribeToDataChannel, "UpdateLocationToCurrentScene", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ChatBox - You're missing the \"PlayerList.UpdateLocationToCurrentScene\" function call on the " +
                        "\"OnYouSubscribeToDataChannel\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        chatbox
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (!E_PlayerEvents.HasUnityEvent(chatbox.OnUserSubscribedToDataChannel, "AddPlayer", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ChatBox - You're missing the \"PlayerList.AddPlayer\" function call on the " +
                        "\"OnUserSubscribedToDataChannel\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        chatbox
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (!E_PlayerEvents.HasUnityEvent(chatbox.OnUserUnSubscribedToDataChannel, "RemovePlayer", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ChatBox - You're missing the \"PlayerList.RemovePlayer\" function call on the " +
                        "\"OnUserUnSubscribedToDataChannel\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        chatbox
                    ));
                }
                else
                {
                    passed += 1;
                }

                //Network Manager Unity Event Checks
                if (!E_PlayerEvents.HasUnityEvent(nm.roomEvents._onJoinedRoom, "UpdateLocationToGoingToScene", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManager - You're missing the \"PlayerList.UpdateLocationToGoingToScene\" function call on the " +
                        "\"OnJoinedRoom\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (!E_PlayerEvents.HasUnityEvent(nm.otherEvents._onDisconnected, "ClearPlayerList", pl))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManager - You're missing the \"PlayerList.ClearPlayerList\" function call on the " +
                        "\"OnDisconnected\" unity event. This is required to make the \"PlayerList\" " +
                        "component work correctly.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_MissingRequiredComponents(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (_autoFixTests)
            {
                try
                {
                    if (!FindObjectOfType<vThirdPersonCamera>())
                    {
                        E_Helpers.CreatePrefabFromPath("Assets/InvectorMultiplayer/vThirdPersonCamera.prefab");
                    }
                }
                catch { }
            }
            if (!FindObjectOfType<vThirdPersonCamera>())
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "MissingRequiredComponent - There is no \"vThirdPersonCamera\" component found in the scene. You need to add this otherwise the camera will not be auto found when the player spawns in.",
                    null
                ));
            }
            else
            {
                passed += 1;
            }
            if (FindObjectOfType<NetworkManager>())
            {
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                if (nm.playerPrefab && nm.playerPrefab.GetComponent<vLockOn>())
                {
                    if (!FindObjectOfType<vHUDController>())
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "MissingRequiredComponent - There is no \"vHUDController\" component found in the scene. " +
                            "This could be simply because you have all your characters disabled and this is now a part of " +
                            "your character. You need to add this otherwise the vLockOn script that your player prefab is " +
                            "using will throw errors.",
                            null
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        void CORE_SceneInBuildScenes(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            string activeScenePath = SceneManager.GetActiveScene().path;
            bool wasFound = false;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == activeScenePath)
                {
                    wasFound = true;
                    if (scene.enabled == false)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "SceneInBuildScenes - This scene is in your build scenes but isn't enabled.",
                            null
                        ));
                    }
                    else
                    {
                        passed += 1;
                        break;
                    }
                }
            }
            if (wasFound == false)
            {
                failed += 1;
                warnings.Add(new DebugFormat(
                    "SceneInBuildScenes - This scene was not found in your current list of scenes to build.",
                    null
                ));
            }
        }
        void CORE_PerformChatBoxTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (!FindObjectOfType<ChatBox>()) return;
            ChatBox cb = FindObjectOfType<ChatBox>();
            List<string> inputs = E_Helpers.GetAllInputAxis();
            List<string> cbInputs = new List<string>();
            List<string> open = (List<string>)cb.GetType().GetField("openChatWindowOnPress", E_Helpers.allBindings).GetValue(cb);
            List<string> close = (List<string>)cb.GetType().GetField("closeWindowOnPress", E_Helpers.allBindings).GetValue(cb);
            List<string> send = (List<string>)cb.GetType().GetField("sendChatOnPress", E_Helpers.allBindings).GetValue(cb);

            foreach (string code in open)
            {
                cbInputs.Add(code);
            }
            foreach (string code in close)
            {
                cbInputs.Add(code);
            }
            foreach (string code in send)
            {
                cbInputs.Add(code);
            }
            foreach (string input in cbInputs)
            {
                if (!inputs.Contains(input))
                {
                    //missing
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ChaxBox - You're missing the input mapping for \"" + input + "\". Open your project settings > Input and add a definition for this key.",
                        null
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformSceneTransitionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            bool found = false;
            foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootObj.name == "SceneManager")
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (_autoFixTests)
                {
                    try
                    {
                        GameObject go = new GameObject("SceneManager");
                    }
                    catch
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SceneTransition - No \"SceneManager\" was found among the root objects of this scene. " +
                            "This gameobject with this specific name is required.",
                            null
                        ));
                    }
                }
                else
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SceneTransition - No \"SceneManager\" was found among the root objects of this scene. " +
                        "This gameobject with this specific name is required.",
                        null
                    ));
                }
            }
            else
            {
                passed += 1;
            }
            if (_autoFixTests)
            {
                try
                {
                    if (!E_Helpers.InspectorTagExists("LoadPoint")) E_Helpers.AddInspectorTag("LoadPoint");
                }
                catch { }
            }
            if (!E_Helpers.InspectorTagExists("LoadPoint"))
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "SceneTransition - You are missing the \"LoadPoint\" tag in your project. If you ran Invector > Import ProjectSettings " +
                    "after running the CB Games > Scene Manager > Add > Scene Entrance this could be why. Running Import ProjectSettings will " +
                    "overwrite all of your tags and input settings. If you haven't added a Scene Entrance point then this is also why, just add " +
                    "a scene entrance point.",
                    null
                ));
            }
            else
            {
                if (FindObjectsOfType<SceneTransition>().Length > 0 && GameObject.FindGameObjectsWithTag("LoadPoint").Length < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "SceneTransition - There is a SceneTransition defined in this scene but no " +
                        "gameobjects tagged with 'LoadPoint'. You will be able to leave this scene but never " +
                        "able to enter this scene again from another unless you tag at least one gameobject with 'LoadPoint' " +
                        "and update your scene database.",
                        null
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
            foreach (SceneTransition transition in FindObjectsOfType<SceneTransition>())
            {
                if (transition.GetType().GetField("SpawnAtPoint", E_Helpers.allBindings).GetValue(transition) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SceneTransition - There is no valid \"SpawnAtPoint\" available for this selected scene.",
                        transition
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (transition.GetType().GetField("LoadSceneName", E_Helpers.allBindings).GetValue(transition) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SceneTransition - There is no valid \"LoadSceneName\" available. Make sure you have at least one scene in your build settings selected to build.",
                        transition
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
            if (E_Helpers.InspectorTagExists("LoadPoint") == false)
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "SceneTransition - You don't have the \"LoadPoint\" tag made for this project. This is required for the \"Transition Scene Manager\" to find all entry points.",
                    null
                ));
            }
            else
            {
                List<string> pointNames = new List<string>();
                foreach (GameObject point in GameObject.FindGameObjectsWithTag("LoadPoint"))
                {
                    // Check Point Layer
                    if (point.layer != 2)
                    {
                        if (_autoFixTests)
                        {
                            point.layer = 2;
                        }
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SceneTransition - This entry point must have its layer be \"Ignore Raycast\".",
                            point
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    // Check Point Name
                    if (pointNames.Contains(point.name))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "SceneTransition - This point name is named exactly the same thing as another entry point." +
                            " It is highly suggested that you have unquie names for each entry point otherwise you could" +
                            " potentially spawn at a random same named entry point.",
                            point
                        ));
                    }
                    else
                    {
                        pointNames.Add(point.name);
                        passed += 1;
                    }

                    // Check Point Parent Name
                    if (point.transform.parent == null || point.transform.parent.name != "SceneManager")
                    {
                        if (_autoFixTests)
                        {
                            GameObject sm = GameObject.Find("SceneManager");
                            if (sm)
                            {
                                point.transform.SetParent(sm.transform);
                            }
                        }
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SceneTransition - This entry point must be an immediate child of the \"SceneManager\" object. " +
                            "Otherwise when building the scene database it will not be added.",
                            point
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        void CORE_PerformSetLoadingScreenTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (SetLoadingScreen screen in FindObjectsOfType<SetLoadingScreen>())
            {
                List<Sprite> images = (List<Sprite>)screen.GetType().GetField("LoadingImages", E_Helpers.allBindings).GetValue(screen);
                string title = (string)screen.GetType().GetField("LoadingTitle", E_Helpers.allBindings).GetValue(screen);
                List<string> descriptions = (List<string>)screen.GetType().GetField("LoadingDescriptions", E_Helpers.allBindings).GetValue(screen);

                if (images.Count < 1 && string.IsNullOrEmpty(title) && descriptions.Count < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "SetLoadingScreen - Nothing is defined here. You will simply have a blank screen with a loading bar.",
                        screen
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformVoiceChatTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            List<string> allBindings = E_Helpers.GetAllInputAxis();
            bool hasRecorder = false;
            foreach (VoiceChat voiceChat in FindObjectsOfType<VoiceChat>())
            {
                if (voiceChat.gameObject.GetComponent<Recorder>())
                {
                    hasRecorder = true;
                    break;
                }
            }
            if (_autoFixTests && !hasRecorder)
            {
                E_VoiceChat.CB_AddGlobalVoiceChat();
            }
            else if (!hasRecorder)
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "VoiceChat - You have VoiceChat components in the scene but there is no VoiceRecorder in this scene. Add it by going to " +
                    "the CB Games > Chat > Voice > Add Room Voice Chat menu item.",
                    null
                ));
            }
            else
            {
                passed += 1;
            }
            foreach (VoiceChat voiceChat in FindObjectsOfType<VoiceChat>())
            {
                if (_autoFixTests)
                {
                    if ((bool)voiceChat.GetType().GetField("isPlayer", E_Helpers.allBindings).GetValue(voiceChat) == true &&
                        voiceChat.speakerImage == null)
                    {
                        voiceChat.speakerImage = E_Helpers.GetPrefabReference("Assets/InvectorMultiplayer/UI/Voice/VoiceSpeaker.png");
                    }
                    if ((bool)voiceChat.GetType().GetField("debugging", E_Helpers.allBindings).GetValue(voiceChat) == true)
                    {
                        voiceChat.GetType().GetField("debugging", E_Helpers.allBindings).SetValue(voiceChat, false);
                    }
                }
                if ((bool)voiceChat.GetType().GetField("isPlayer", E_Helpers.allBindings).GetValue(voiceChat) == true)
                {
                    if (voiceChat.speakerImage == null)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "VoiceChat - The speaker image is null. When this networked player is speaking there will be no visual indicator.",
                            voiceChat
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
                else
                {
                    if ((bool)voiceChat.GetType().GetField("debugging", E_Helpers.allBindings).GetValue(voiceChat) == true)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "VoiceChat - Debugging mode is enabled. This will produce a lot of logging that shouldn't be enabled for the final build.",
                            voiceChat
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if ((bool)voiceChat.GetType().GetField("pushToTalk", E_Helpers.allBindings).GetValue(voiceChat) == true)
                    {
                        if (!allBindings.Contains((string)voiceChat.GetType().GetField("buttonToPress", E_Helpers.allBindings).GetValue(voiceChat)))
                        {
                            string butttonToPress = (string)voiceChat.GetType().GetField("buttonToPress", E_Helpers.allBindings).GetValue(voiceChat);
                            failed += 1;
                            failures.Add(new DebugFormat(
                                "VoiceChat - You have pushToTalk enabled but the specified \""+ butttonToPress + "\" button does not exist in your Project Settings > Input.",
                                voiceChat
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }
                    }
                }
            }
        }
        void CORE_PerformCallNetworkEventsTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (CallNetworkEvents target in FindObjectsOfType<CallNetworkEvents>())
            {
                if (_autoFixTests)
                {
                    if ((bool)target.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(target) == true &&
                    (Transform)target.GetType().GetField("holder", E_Helpers.allBindings).GetValue(target) == null)
                    {
                        target.GetType().GetField("holder", E_Helpers.allBindings).SetValue(target, target.gameObject);
                    }
                }
                if ((bool)target.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(target) == true &&
                    (Transform)target.GetType().GetField("holder", E_Helpers.allBindings).GetValue(target) == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "CallNetworkEvents - You have \"syncCrossScenes\" enabled but no \"Track Object\" specified. " +
                        "If this object is not specified it will not be found in the scene and will no replay it's actions.",
                        target
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformSyncPlayerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            SyncPlayer[] syncPlayers = FindObjectsOfType<SyncPlayer>();
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            foreach (SyncPlayer syncPlayer in syncPlayers)
            {
                if(_autoFixTests)
                {
                    if ((bool)syncPlayer.GetType().GetField("_syncAnimations", bindFlags).GetValue(syncPlayer) != true)
                    {
                        syncPlayer.GetType().GetField("_syncAnimations", bindFlags).SetValue(syncPlayer, true);
                    }
                    if ((float)syncPlayer.GetType().GetField("_positionLerpRate", bindFlags).GetValue(syncPlayer) < 1 ||
                        (float)syncPlayer.GetType().GetField("_positionLerpRate", bindFlags).GetValue(syncPlayer) > 30)
                    {
                        syncPlayer.GetType().GetField("_positionLerpRate", bindFlags).SetValue(syncPlayer, 6);
                    }
                    if ((float)syncPlayer.GetType().GetField("_rotationLerpRate", bindFlags).GetValue(syncPlayer) < 1 ||
                        (float)syncPlayer.GetType().GetField("_rotationLerpRate", bindFlags).GetValue(syncPlayer) > 30)
                    {
                        syncPlayer.GetType().GetField("_rotationLerpRate", bindFlags).SetValue(syncPlayer, 6);
                    }
                    if (syncPlayer._nonAuthoritativeLayer != 9 && LayerMask.LayerToName(9) != "")
                    {
                        syncPlayer._nonAuthoritativeLayer = 9;
                    }
                }
                #region Animation Testing
                // Animation Testing
                if ((bool)syncPlayer.GetType().GetField("_syncAnimations", bindFlags).GetValue(syncPlayer) != true)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - Sync Animations is not enabled.",
                        syncPlayer.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                #endregion

                #region Position Testing
                // Position Testing
                if ((float)syncPlayer.GetType().GetField("_positionLerpRate", bindFlags).GetValue(syncPlayer) < 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - Position Move Speed is set to zero.",
                        syncPlayer.transform
                    ));
                }
                else if ((float)syncPlayer.GetType().GetField("_positionLerpRate", bindFlags).GetValue(syncPlayer) > 30)
                {
                    passed += 1;
                    warnings.Add(new DebugFormat(
                        "SyncPlayer - Position Move Speed is above 30, this could result in 'snapping' movement, instead of smooth movement.",
                        syncPlayer.transform
                    ));
                }
                else
                {
                    passed += 1;
                }

                if ((float)syncPlayer.GetType().GetField("_rotationLerpRate", bindFlags).GetValue(syncPlayer) < 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - Rotation Move Speed is set to zero.",
                        syncPlayer.transform
                    ));
                }
                if ((float)syncPlayer.GetType().GetField("_rotationLerpRate", bindFlags).GetValue(syncPlayer) > 30)
                {
                    passed += 1;
                    warnings.Add(new DebugFormat(
                        "SyncPlayer - Rotation Move Speed is above 30, this could result in 'snapping' movement, instead of smooth movement.",
                        syncPlayer.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                #endregion

                #region None Owner Settings
                // None Owner Settings Testing
                if (syncPlayer.noneLocalTag == "")
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - None Owner Tag is blank, you seem to be missing important tag/layers. You may need to re-import the 'vProjectSettings' included in the invector package.",
                        syncPlayer.transform
                    ));
                }
                else if (syncPlayer.noneLocalTag == "Untagged")
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - None Owner Tag should not be 'Untagged'.",
                        syncPlayer.transform
                    ));
                }
                else
                {
                    passed += 1;
                }

                if (syncPlayer._nonAuthoritativeLayer == 0 ||
                    syncPlayer._nonAuthoritativeLayer == 1 ||
                    syncPlayer._nonAuthoritativeLayer == 2 ||
                    syncPlayer._nonAuthoritativeLayer == 4 ||
                    syncPlayer._nonAuthoritativeLayer == 5)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - None Owner Layer should not be on layers: 0, 1, 2, 4, or 5.",
                        syncPlayer.transform
                    ));
                }
                else if (syncPlayer._nonAuthoritativeLayer != 9 && LayerMask.LayerToName(9) != "")
                {
                    passed += 1;
                    warnings.Add(new DebugFormat(
                        "SyncPlayer - None Owner Layer should be on layer 9 to correctly send damage across the network to other players.",
                        syncPlayer.transform
                    ));
                }
                else if (LayerMask.LayerToName(9) == "")
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SyncPlayer - The 'Enemy' layer appears to not exist. You're missing important tag/layers. You may need to re-import the 'vProjectSettings' included in the invector package.",
                        syncPlayer.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                #endregion
            }
        }
        void CORE_SpawnPointTesting(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (_autoFixTests)
            {
                if (!E_Helpers.InspectorTagExists("SpawnPoint"))
                {
                    E_Helpers.AddInspectorTag("SpawnPoint");
                }
            }
            if (!E_Helpers.InspectorTagExists("SpawnPoint"))
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "SpawnPointTesting - The tag 'SpawnPoint' doesn't exist in your project it might be that you haven't " +
                    "added a spawn point to your project yet or you ran Invector > Import ProjectSettings " +
                    "after attempting to add these items. Running that will overwrite all of your tags and input settings",
                    null
                ));
            }
            else
            {
                if (GameObject.FindGameObjectsWithTag("SpawnPoint").Length < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "SpawnPointTesting - There are no gameobjects tagged with 'SpawnPoint' in this scene. You will not be " +
                        "able to correctly join this scene in progress. This is fine if you are locking your session after starting.",
                        null
                    ));
                }
                else
                {
                    passed += 1;
                }
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                if (nm == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "SpawnPointTesting - Unable To Find Network Manager to determine the spawn point tag.",
                        null
                    ));
                }
                else
                {
                    GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(nm.spawnPointsTag);
                    if (spawnPoints.Length < 1 && nm.defaultSpawnPoint == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "SpawnPointTesting - No available spawn points or default spawn point.",
                            nm.transform
                        ));
                    }
                    else if (spawnPoints.Length < 1)
                    {
                        passed += 1;
                        warnings.Add(new DebugFormat(
                            "SpawnPointTesting - No other spawn points other than the defined default spawn point.",
                            nm.transform
                        ));
                    }
                    else
                    {
                        for (int i = 0; i < spawnPoints.Length; i++)
                        {
                            if (!Physics.Raycast(spawnPoints[i].transform.position, Vector3.down, 500))
                            {
                                failed += 1;
                                failures.Add(new DebugFormat(
                                    "SpawnPointTesting - Spawn point has no ground beneath it.",
                                    spawnPoints[i].transform
                                ));
                            }
                            else
                            {
                                if (!Physics.Raycast(spawnPoints[i].transform.position, Vector3.down, 3))
                                {
                                    passed += 1;
                                    warnings.Add(new DebugFormat(
                                        "SpawnPointTesting - Spawn point is not close enough to the ground. This might cause damage to the player when they spawn.",
                                        spawnPoints[i].transform
                                    ));
                                }
                            }
                        }
                    }
                }
            }
        }
        void CORE_DisablePlayersTest(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            vThirdPersonController[] controllers = FindObjectsOfType<vThirdPersonController>();
            foreach (vThirdPersonController controller in controllers)
            {
                failed += 1;
                warnings.Add(new DebugFormat(
                    "DisablePlayersTest - This player will need to be disabled before building/testing. All players are dynamically spawned via the resources folders prefabs.",
                    controller.transform
                ));
            }
        }
        void CORE_PerformThirdPersonControllerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            vThirdPersonController[] controllers = FindObjectsOfType<vThirdPersonController>();
            foreach (vThirdPersonController controller in controllers)
            {
                if (_autoFixTests)
                {
                    if (controller.debugActionListener == true)
                    {
                        controller.debugActionListener = false;
                    }
                    if (controller.isImmortal == true)
                    {
                        controller.isImmortal = false;
                    }
                    if (controller.fillHealthOnStart == true)
                    {
                        controller.fillHealthOnStart = false;
                    }
                    if (controller.debugWindow == true)
                    {
                        controller.debugWindow = false;
                    }
                    if (controller.gameObject.GetComponent<SyncPlayer>() &&
                        !E_PlayerEvents.HasUnityEvent(controller.OnJump, "Jump", controller.gameObject.GetComponent<SyncPlayer>()))
                    {
                        UnityEventTools.AddPersistentListener(
                            controller.OnJump,
                            controller.gameObject.GetComponent<SyncPlayer>().Jump
                        );
                    }
                    if (controller.gameObject.GetComponent<SyncPlayer>() && 
                        !E_PlayerEvents.HasUnityEvent(controller.onReceiveDamage, "OnReceiveDamage", controller.gameObject.GetComponent<SyncPlayer>()))
                    {
                        UnityEventTools.AddPersistentListener(
                            controller.onReceiveDamage,
                            controller.gameObject.GetComponent<SyncPlayer>().OnReceiveDamage
                        );
                    }
                }
                if (controller.debugActionListener == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "ThirdPersonController - The debug action listener variable is set to true. Be sure to disable this before the final build.",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (controller.isImmortal == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "ThirdPersonController - This controller is set to be immortal. Did you mean to do that?",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (controller.fillHealthOnStart == true)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ThirdPersonController - The Fill Health On Start variable should be false. Otherwise when doing scene transitions your health will always be 100% no matter what it was in the other scenes.",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (controller.debugWindow == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "ThirdPersonController - The third person controller has the debug window enabled. Be sure to disable this before the final build.",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                // Removed as part of the v2.5.2 invector update
                //if (controller.useInstance != false)
                //{
                //    failed += 1;
                //    failures.Add(new DebugFormat(
                //        "ThirdPersonController - The third person controller must have \"Use Instance\" as false.",
                //        controller.transform
                //    ));
                //}
                //else
                //{
                //    passed += 1;
                //}
                if (!E_PlayerEvents.HasUnityEvent(controller.OnJump, "Jump", controller.gameObject.GetComponent<SyncPlayer>()))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "ThirdPersonController - The \"Jump\" function from the \"SyncPlayer\" " +
                        "component needs to be added to the \"OnJump\" unityevent. If this isn't added the jump will still work " +
                        "across the network but will only be inches above the ground instead of having the newtork player jump at " +
                        "the full height.",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!E_PlayerEvents.HasUnityEvent(controller.onReceiveDamage, "OnReceiveDamage", controller.gameObject.GetComponent<SyncPlayer>()))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "ThirdPersonController - The \"OnReceiveDamage\" function from the \"SyncPlayer\" " +
                        "component needs to be added to the \"OnReceiveDamage\" unityevent. If this isn't added damage will not " +
                        "be synced across the network. While this isn't fully nessacary you would still need this to update health " +
                        "bars if you were going to implement this into your game.",
                        controller.transform
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (controller.transform.GetComponent<vItemManager>())
                {
                    if (E_PlayerEvents.HasUnityEvent(controller.onDead, "DropAllItens", controller.transform.GetComponent<vItemManager>()))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "ThirdPersonController - The \"OnDead\" UnityEvent is calling the \"DropAllItens\" function. This function isn't " +
                            "supported in multiplayer at this time. Including this anyway will cause a whole slew of issues for you. Just " +
                            "don't do it!",
                            controller.transform
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        void CORE_PerformMeleeCombatInputTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (shooterExists) return;
            foreach (vMeleeCombatInput ci in FindObjectsOfType<vMeleeCombatInput>())
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (!ci.GetComponent<MP_vMeleeCombatInput>())
                        {
                            vMeleeCombatInput org = ci.GetComponent<vMeleeCombatInput>();
                            ci.gameObject.AddComponent<MP_vMeleeCombatInput>();
                            E_Helpers.CopyComponentValues(
                                org,
                                ci.GetComponent<MP_vMeleeCombatInput>()
                            );
                            DestroyImmediate(org);
                        }
                    }
                    catch { }
                }
                if (!ci.GetComponent<MP_vMeleeCombatInput>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vMeleeCombatInput - \"vMeleeCombatInput\" needs to be replaced with \"MP_vMeleeCombatInput\" to sync combat actions correctly.",
                        ci
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformLadderActionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vLadderAction la in FindObjectsOfType<vLadderAction>())
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (!E_PlayerEvents.HasUnityEvent(la.OnEnterLadder, "EnterLadder", la.gameObject.GetComponent<SyncPlayer>()))
                        {
                            UnityEventTools.AddPersistentListener(
                                la.OnEnterLadder,
                                la.gameObject.GetComponent<SyncPlayer>().EnterLadder
                            );
                        }
                        if (!E_PlayerEvents.HasUnityEvent(la.OnExitLadder, "ExitLadder", la.gameObject.GetComponent<SyncPlayer>()))
                        {
                            UnityEventTools.AddPersistentListener(
                                la.OnExitLadder,
                                la.gameObject.GetComponent<SyncPlayer>().ExitLadder
                            );
                        }
                    }
                    catch { }
                }
                if (!E_PlayerEvents.HasUnityEvent(la.OnEnterLadder, "EnterLadder", la.gameObject.GetComponent<SyncPlayer>()))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vLadderAction - \"vLadderAction\" needs to have the \"EnterLadder\" action applied" +
                        "on the \"OnEnterLadder\" unityevent. This function comes from \"SyncPlayer\" component. If this isn't " +
                        "applied the animations for entering a ladder will not be synced.",
                        la
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!E_PlayerEvents.HasUnityEvent(la.OnExitLadder, "ExitLadder", la.gameObject.GetComponent<SyncPlayer>()))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vLadderAction - \"vLadderAction\" needs to have the \"ExitLadder\" action applied on " +
                        "the \"OnExitLadder\" unityevent. This function comes from \"SyncPlayer\". If this isn't " +
                        "applied the animations for exiting a ladder will not be synced.",
                        la
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformItemManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vItemManager im in FindObjectsOfType<vItemManager>())
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (!E_PlayerEvents.HasUnityEvent(im.onDropItem, "OnDropItem", im.gameObject.GetComponent<SyncPlayer>()))
                        {
                            UnityEventTools.AddPersistentListener(
                                im.onDropItem,
                                im.gameObject.GetComponent<SyncPlayer>().OnDropItem
                            );
                        }
                    }
                    catch { }
                }
                if (!E_PlayerEvents.HasUnityEvent(im.onDropItem, "OnDropItem", im.gameObject.GetComponent<SyncPlayer>()))
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vItemManager - \"vItemManager\" needs to have the \"OnDropItem\" action applied" +
                        "on the \"OnDropItem\" unityevent. This function comes from \"SyncPlayer\" component. If this isn't " +
                        "applied the dropped item will not appear over the network to other players.",
                        im
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformMeleeManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            //No Longer Needed
        }
        void CORE_PerformHealthControllerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vHealthController hc in FindObjectsOfType<vHealthController>())
            {
                if (_autoFixTests)
                {
                    if (!hc.gameObject.GetComponent<SyncHealthController>() && !hc.gameObject.GetComponent<vThirdPersonController>())
                    {
                        hc.gameObject.AddComponent<SyncHealthController>();
                    }
                    if (hc.gameObject.GetComponent<SyncHealthController>())
                    {
                        if (!E_PlayerEvents.HasUnityEvent(hc.onReceiveDamage, "SendDamageOverNetwork", hc.gameObject.GetComponent<SyncHealthController>()) &&
                            !hc.gameObject.GetComponent<v_AIController>() && !hc.gameObject.GetComponent<vThirdPersonController>())
                        {
                            UnityEventTools.AddPersistentListener(
                                hc.onReceiveDamage,
                                hc.gameObject.GetComponent<SyncHealthController>().SendDamageOverNetwork
                            );
                        }
                    }
                }
                if (!E_PlayerEvents.HasUnityEvent(hc.onReceiveDamage, "SendDamageOverNetwork", hc.gameObject.GetComponent<SyncHealthController>()) &&
                    !hc.gameObject.GetComponent<v_AIController>() && !hc.gameObject.GetComponent<vThirdPersonController>())
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vHealthController - \"vHealthController\" needs to have the \"SendDamageOverNetwork\" action applied" +
                        "on the \"OnReceiveDamage\" unityevent. This function comes from \"SyncHealthController\" component. If this isn't " +
                        "applied damage will not be synced across the network.",
                        hc
                    ));
                }
                else if (!hc.gameObject.GetComponent<SyncHealthController>() && !hc.gameObject.GetComponent<v_AIController>() &&
                    !hc.gameObject.GetComponent<vThirdPersonController>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vHealthController - \"vHealthController\" has the \"SendDamageOverNetwork\" action applied" +
                        "on the \"OnReceiveDamage\" unityevent. However, you're missing the \"SyncHealthController\" component",
                        hc
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformBreakableObjectTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vBreakableObject bo in FindObjectsOfType<vBreakableObject>())
            {
                if (_autoFixTests)
                {
                    if (!bo.gameObject.GetComponent<NetworkBreakObject>())
                    {
                        bo.gameObject.AddComponent<NetworkBreakObject>();
                    }
                    if (!E_PlayerEvents.HasUnityEvent(bo.OnBroken, "BreakObject", bo.gameObject.GetComponent<NetworkBreakObject>()))
                    {
                        UnityEventTools.AddPersistentListener(
                            bo.OnBroken,
                            bo.gameObject.GetComponent<NetworkBreakObject>().BreakObject
                        );
                    }
                    if ((bool)bo.gameObject.GetComponent<NetworkBreakObject>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(bo.gameObject.GetComponent<NetworkBreakObject>()) == true &&
                        (Transform)bo.gameObject.GetComponent<NetworkBreakObject>().GetType().GetField("holder", E_Helpers.allBindings).GetValue(bo.gameObject.GetComponent<NetworkBreakObject>()) == null)
                    {
                        bo.gameObject.GetComponent<NetworkBreakObject>().GetType().GetField("holder", E_Helpers.allBindings).SetValue(bo.gameObject.GetComponent<NetworkBreakObject>(), bo.gameObject);
                    }
                }
                if (!E_PlayerEvents.HasUnityEvent(bo.OnBroken, "BreakObject", bo.gameObject.GetComponent<NetworkBreakObject>()))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vBreakableObject - \"vBreakableObject\" needs to have the \"BreakObject\" action applied" +
                        "on the \"OnBroken\" unityevent. This function comes from \"NetworkBreakObject\" component. If this isn't " +
                        "applied damage this object will not break across the network.",
                        bo
                    ));
                }
                else if (!bo.gameObject.GetComponent<NetworkBreakObject>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vBreakableObject - \"vBreakableObject\" has the \"BreakObject\" action applied" +
                        "on the \"OnBroken\" unityevent. However, you are missing the \"NetworkBreakObject\" component.",
                        bo
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!bo.gameObject.GetComponent<NetworkBreakObject>())
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vBreakableObject - \"NetworkBreakObject\" component is missing from this breakable object. That means " +
                        "this will not be synced across the network in any way.",
                        bo
                    ));
                }
                else
                {
                    passed += 1;
                    if ((bool)bo.gameObject.GetComponent<NetworkBreakObject>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(bo.gameObject.GetComponent<NetworkBreakObject>()) == true &&
                        (Transform)bo.gameObject.GetComponent<NetworkBreakObject>().GetType().GetField("holder", E_Helpers.allBindings).GetValue(bo.gameObject.GetComponent<NetworkBreakObject>()) == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vBreakableObject - \"syncCrossScenes\" is true but the holder is not set! " +
                            "the holder is required to be able to track the position so the code can know the item's location to update " +
                            "when traveling through the scenes.",
                            bo
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        void CORE_PerformPlayerNameBarTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (PlayerNameBar nb in FindObjectsOfType<PlayerNameBar>())
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (nb.playerName == null)
                        {
                            if (nb.transform.Find("PlayerName"))
                            {
                                nb.playerName = nb.transform.Find("PlayerName").GetComponent<Text>();
                            }
                        }
                        if (nb.playerBar == null)
                        {
                            if (nb.transform.Find("PlayerNameBar"))
                            {
                                nb.playerBar = nb.transform.Find("PlayerNameBar").gameObject;
                            }
                        }
                    }
                    catch { }
                }
                if (nb.playerName == null || nb.playerBar == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PlayerNameBar - \"PlayerNameBar\" needs to have both \"playerName\" " +
                        "and \"playerBar\" populated with values!",
                        nb
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformSyncObjectTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            string output = null;
            foreach (vItemManager im in FindObjectsOfType<vItemManager>())
            {
                foreach (EquipPoint ep in im.equipPoints)
                {
                    if (_autoFixTests)
                    {
                        if (ep.handler.defaultHandler)
                        {
                            if (!ep.handler.defaultHandler.gameObject.GetComponent<SyncObject>())
                            {
                                ep.handler.defaultHandler.gameObject.AddComponent<SyncObject>();
                            }
                            E_AutoRemediate.CB_REMEDIATE_SyncObject(ep.handler.defaultHandler.gameObject.GetComponent<SyncObject>());
                        }
                    }
                    output = CORE_SetupSyncCompCorrect(ep.handler.defaultHandler.gameObject, false, ep.equipPointName.ToLower().Contains("left"), false, false, false, true);
                    if (output == "true")
                    {
                        passed += 1;
                    }
                    else
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            output,
                            ep.handler.defaultHandler
                        ));
                    }
                    foreach (Transform customHandler in ep.handler.customHandlers)
                    {
                        if (_autoFixTests)
                        {
                            if (customHandler)
                            {
                                if (!customHandler.gameObject.GetComponent<SyncObject>())
                                {
                                    customHandler.gameObject.AddComponent<SyncObject>();
                                }
                                E_AutoRemediate.CB_REMEDIATE_SyncObject(customHandler.gameObject.GetComponent<SyncObject>());
                            }
                        }
                        bool isLeftHanded = false;
                        if (ep.equipPointName.ToLower().Contains("left") || customHandler.transform.parent.name.ToLower().Contains("left"))
                        {
                            isLeftHanded = true;
                        }
                        output = CORE_SetupSyncCompCorrect(customHandler.gameObject, false, isLeftHanded, false, false, false, true);
                        if (output == "true")
                        {
                            passed += 1;
                        }
                        else
                        {
                            failed += 1;
                            failures.Add(new DebugFormat(
                                output,
                                customHandler
                            ));
                        }
                    }
                }
            }
            foreach (vWeaponHolderManager whm in FindObjectsOfType<vWeaponHolderManager>())
            {
                foreach (vWeaponHolder wh in whm.holders)
                {
                    if (_autoFixTests)
                    {
                        if (wh.holderObject)
                        { 
                            if (!wh.holderObject.GetComponent<SyncObject>())
                            {
                                wh.holderObject.AddComponent<SyncObject>();
                            }
                            E_AutoRemediate.CB_REMEDIATE_SyncObject(wh.holderObject.GetComponent<SyncObject>());
                        }
                    }
                    output = CORE_SetupSyncCompCorrect(wh.holderObject, true, wh.equipPointName.ToLower().Contains("left"), true, true, false, true);
                    if (output == "true")
                    {
                        passed += 1;
                    }
                    else
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            output,
                            wh.holderObject
                        ));
                    }

                    if (_autoFixTests)
                    {
                        if (!wh.weaponObject.GetComponent<SyncObject>())
                        {
                            wh.weaponObject.AddComponent<SyncObject>();
                        }
                        E_AutoRemediate.CB_REMEDIATE_SyncObject(wh.weaponObject.GetComponent<SyncObject>());
                    }
                    output = CORE_SetupSyncCompCorrect(wh.weaponObject, false, false, true, true, false, false);
                    if (output == "true")
                    {
                        passed += 1;
                    }
                    else
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            output,
                            wh.weaponObject
                        ));
                    }
                }
            }
        }
        string CORE_SetupSyncCompCorrect(GameObject target, bool isHolder, bool isLeft, bool syncEnabled, bool syncDisable, bool syncDestroy, bool syncChildren)
        {
            if (!target) return "true";
            if (!target.GetComponent<SyncObject>())
            {
                return "SyncObject - Missing \"SyncObject\" component! In order sync sync " +
                    "holding objects across the network this component must be on this object!";
            }
            else if (target.GetComponent<SyncObject>().syncEnable != syncEnabled)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"syncEnable\" set to " + syncEnabled + ". " +
                    "If this isn't it will result in this object not appearing across the network.";
            }
            else if (target.GetComponent<SyncObject>().syncDisable != syncDisable)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"syncDisable\" set to " + syncDisable + ". " +
                    "If this isn't it will result in this object not disappearing correctly across the network.";
            }
            else if (target.GetComponent<SyncObject>().syncDestroy != syncDestroy)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"syncDestroy\" set to " + syncDestroy + ". " +
                    "If this isn't it will result in this object not disappearing correctly across the network. " +
                    "It will also leave junk instances across the network. Given enough time, this WILL cause game crashes.";
            }

            if (target.GetComponent<SyncObject>().syncImmediateChildren != syncChildren)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"syncImmediateChildren\" set to " + syncChildren + ". " +
                    "If this isn't it will result no new objects that are added appearing across the network.";
            }
            else if (target.GetComponent<SyncObject>().isWeaponHolder != isHolder)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"isWeaponHolder\" set to " + isHolder + ". " +
                    "If this isn't it could result in incorrect positioning across the network";
            }
            else if (target.GetComponent<SyncObject>().isLeftHanded != isLeft && syncChildren == true)
            {
                return "SyncObject - This \"SyncObject\" component needs to have \"isLeftHanded\" set to " + isLeft + ". " +
                    "If this isn't it could result in incorrect positioning across the network";
            }
            return "true";
        }
        void CORE_PerformPreviewCamTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (PreviewCamera cam in FindObjectsOfType<PreviewCamera>())
            {
                for (int i = 0; i < cam.cameraPoints.Count; i++)
                {
                    if (i > 1)
                    {
                        if (cam.cameraPoints[i - 1].GetInstanceID() == cam.cameraPoints[i].GetInstanceID())
                        {
                            failed += 1;
                            failures.Add(new DebugFormat(
                                "PreviewCamera - You have two camera points one after another that are the exact same point. This is undesired logic.",
                                cam
                            ));
                            break;
                        }
                    }
                    if (cam.cameraPoints[0] == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "PreviewCamera - You have a missing point in your list, this will cause errors during runtime!",
                            cam
                        ));
                        break;
                    }
                }
                if (cam.cameraCloseEnough < 0.1f)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "PreviewCamera - The CameraCloseEnough field is set below 0.1f this is dangerous as this could lead to, in certain scenarios, the camera never actually getting close enough to the camera point.",
                        cam
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformNetworkManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            if (!FindObjectOfType<NetworkManager>())
            {
                failed += 1;
                failures.Add(new DebugFormat(
                    "NetworkManager - \"Network Manager\" does not exist in the scene. Did not run the \"CB Games > Add > Core Objects\" script.",
                    null
                ));
            }
            else
            {
                passed += 1;
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                if (_autoFixTests)
                {
                    if (nm.spawnPointsTag == "")
                    {
                        if (E_Helpers.InspectorTagExists("SpawnPoint"))
                        {
                            nm.spawnPointsTag = "SpawnPoint";
                        }
                        if (Convert.ToInt32(nm.GetType().GetField("maxPlayerPerRoom", E_Helpers.allBindings).GetValue(nm)) < 2)
                        {
                            nm.GetType().GetField("maxPlayerPerRoom", E_Helpers.allBindings).SetValue(nm, 4);
                        }
                        if (nm.database == null)
                        {
                            SceneDatabase db = (SceneDatabase)Resources.Load("SceneDatabase/ScenesDatabase.asset");
                            if (db)
                            {
                                nm.database = db;
                            }
                        }
                    }
                }
                if (nm.playerPrefab == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManager - The \"Player Prefab\" on the \"Network Manager\" component is null.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nm.defaultSpawnPoint == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManagerTest - \"Default Spawn Point\" is null.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nm.spawnPointsTag == "")
                {
                    failed += 1;
                    if (nm.defaultSpawnPoint == null)
                    {
                        failures.Add(new DebugFormat(
                            "NetworkManagerTest - No spawn point tag was specified and no default spawn point was either.",
                            nm
                        ));
                    }
                    else
                    {
                        warnings.Add(new DebugFormat(
                            "NetworkManagerTest - No spawn point tag was specified. This is okay since you have a default tag " +
                            "specified but if you want more than just 1 spawn point you need to specify a tag.",
                            nm
                        ));
                    }
                }
                else
                {
                    passed += 1;
                }
                FieldInfo field = nm.GetType().GetField("maxPlayerPerRoom", E_Helpers.allBindings);
                if (Convert.ToInt32(field.GetValue(nm)) < 2)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManagerTest - \"Max Player Per Room\" is set below 2.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nm.database == null)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "NetworkManagerTest - \"Database\" is not set. This will not probably transfer scenes " +
                        "for you until this is made and placed into the NetworkManager. To make this database " +
                        "run 'CB Games > Scene Transition Manager > Update Scene Database.'",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!string.IsNullOrEmpty(nm.teamName))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkManagerTest - You have defined a team name in the network manager. This will force everyone " +
                        "to be on the same team. If this isn't what you want, then make sure the Team Name is empty.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (nm.initalTeamSpawnPointNames.Count < 1)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkManagerTest - You have not defined any initial spawn point names. This is okay if you're not using " +
                        "teams. Otherwise this might have unintended behavior with people randomly spawning around your map.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                    List<GameObject> spawnPoints = new List<GameObject>();
                    spawnPoints.AddRange(GameObject.FindGameObjectsWithTag(nm.spawnPointsTag));
                    foreach (KeyValuePair<string, string> item in nm.initalTeamSpawnPointNames)
                    {
                        if (!spawnPoints.Find(x => x.name == item.Value))
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "NetworkManagerTest - You don't have any spawn point named " + item.Value + " for " +
                                "team name: " + item.Key + " in this scene. This could lead to unwanted behavior " +
                                "with players on this team randomly spawning around the map.",
                                nm
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }
                    }
                }
                if (nm.autoSpawnPlayer == false)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "NetworkManagerTest - You have selected to not auto spawn the player when joining a photon room. " +
                        "This means it will be your job to do this. The easiest solution is to enable this before traveling " +
                        "to your first scene. NOTE: If using a Pre-Built UI, this has logic already completed to do this " +
                        "for you.",
                        nm
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformPhotonViewTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            PhotonView[] views = FindObjectsOfType<PhotonView>();
            foreach (PhotonView view in views)
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (view.Synchronization != ViewSynchronization.Off && 
                            (view.ObservedComponents.ContainsNull() || view.ObservedComponents.Count < 1))
                        {
                            view.Synchronization = ViewSynchronization.Off;
                        }
                    }
                    catch { }
                }
                if (view.transform.parent && view.transform.parent.GetComponent<PhotonView>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PhotonView - There were multiple PhotonView components detected on " +
                        "this gameobject. Only one PhotonView component is recommended since having " +
                        "multiple could negatively effect RPC calls.",
                        view
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (view.Synchronization != ViewSynchronization.Off && view.ObservedComponents.ContainsNull())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PhotonView - The ObservedComponents contains a null component. This needs to be manually cleaned up.",
                        view
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (view.Synchronization != ViewSynchronization.Off && view.ObservedComponents.Count < 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "PhotonView - You have no set ObservedComponents but you have the PhotonView's Observe Option set. " +
                        "The Observe Option must be \"Off\" if you don't have any components in the ObservedComponents section.",
                        view
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        void CORE_PerformItemListDataTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(vItemListData).Name);  //FindAssets uses tags check documentation for more info
            vItemListData[] itemListDatas = new vItemListData[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                itemListDatas[i] = AssetDatabase.LoadAssetAtPath<vItemListData>(path);
            }
            foreach (vItemListData data in itemListDatas)
            {
                foreach (vItem item in data.items)
                {
                    string dropItemPath = null;
                    string spawnItemPath = null;
                    if (item.originalObject != null)
                    {
                        spawnItemPath = AssetDatabase.GetAssetPath(item.originalObject);
                    }
                    if (item.dropObject != null)
                    {
                        dropItemPath = AssetDatabase.GetAssetPath(item.dropObject);
                    }
                    if (_autoFixTests)
                    {
                        if (item.dropObject && !dropItemPath.Contains("Assets/Resources"))
                        {
                            string temp = "Assets/Resources/" + "MP_" + item.dropObject.name + ".prefab";
                            GameObject found = (GameObject)AssetDatabase.LoadAssetAtPath(temp, typeof(GameObject));
                            if (found != null)
                            {
                                item.dropObject = found;
                            }
                        }
                    }
                    if (IsMeleeItemType(item))
                    {
                        if (!item.dropObject || (item.dropObject &&!dropItemPath.Contains("Assets/Resources")))
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "vItemListData(" + data.name + ") - The \"" + item.name + "\" drop object isn't coming from the Assets/Resources folder. " +
                                "If you don't place the object in this folder it can not be properly instantiated " +
                                "across the network. Fix this by finding \"" + item.name + "\" in the ItemListData that you're using " +
                                "and make sure its the MP version coming from the Resource folder.",
                                data
                            ));
                        }
                        else
                        {
                            passed += 1;
                            if (item.dropObject && !item.dropObject.GetComponent<SyncItemCollection>())
                            {
                                failed += 1;
                                failures.Add(new DebugFormat(
                                    "vItemListData(" + data.name + ") - The \"" + item.name + "\" drop object doesn't have the \"SyncItemCollection\" " +
                                    "component. If this component isn't present and setup correctly this object will not properly " +
                                    "appear over the network.",
                                    data
                                ));
                            }
                            else
                            {
                                passed += 1;
                            }
                        }
                    }
                    else if (IsConsumableItemType(item))
                    {
                        if (!item.dropObject || (item.dropObject && !dropItemPath.Contains("Assets/Resources")))
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "vItemListData(" + data.name + ") - The \"" + item.name + "\" drop object isn't coming from the Assets/Resources folder. " +
                                "If you don't place the object in this folder it can not be properly instantiated " +
                                "across the network. Fix this by finding \"" + item.name + "\" in the ItemListData that you're using " +
                                "and make sure its the MP version coming from the Resource folder.",
                                data
                            ));
                        }
                        else
                        {
                            passed += 1;
                            if (!item.dropObject || (item.dropObject &&!item.dropObject.GetComponent<SyncItemCollection>()))
                            {
                                failed += 1;
                                failures.Add(new DebugFormat(
                                    "vItemListData - The \"" + item.name + "\" drop object doesn't have the \"SyncItemCollection\" " +
                                    "component. If this component isn't present and setup correctly this object will not properly " +
                                    "appear over the network.",
                                    item.dropObject
                                ));
                            }
                            else
                            {
                                passed += 1;
                            }
                        }
                    }
                }
            }
        }
        bool IsMeleeItemType(vItem item)
        {
            switch(item.type)
            {
                case vItemType.Defense:
                case vItemType.MeleeWeapon:
                    return true;
            }
            return false;
        }
        bool IsConsumableItemType(vItem item)
        {
            switch (item.type)
            {
                case vItemType.Consumable:
                    return true;
            }
            return false;
        }
    }
}