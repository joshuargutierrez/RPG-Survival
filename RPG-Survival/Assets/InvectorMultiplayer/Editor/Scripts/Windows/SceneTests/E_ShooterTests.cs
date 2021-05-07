/*
using System.Collections.Generic;
using UnityEngine;
using Invector.vShooter;
using Invector.vCharacterController;
using CBGames.Objects;
using UnityEditor;
using Invector;
using Invector.vItemManager;
using Photon.Pun;
using UnityEditor.Events;

namespace CBGames.Editors
{
    public partial class E_TestScene
    {
        partial void SHOOTER_CheckInventoryTimeScale(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vInventory inv in FindObjectsOfType<vInventory>())
            {
                if (inv.timeScaleWhileIsOpen < 1)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vInventory - The time scale is set to 0. This will cause weird and delayed behavior with networking. The time scale when " +
                        "open should always be 1.",
                        inv
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        partial void SHOOTER_CheckNestedAimCanvas(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vThirdPersonController cont in FindObjectsOfType<vThirdPersonController>())
            {
                if (cont.gameObject.GetComponentInChildren<vControlAimCanvas>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vControlAimCanvas - This aim canvas is nested into the player object. This will cause aiming issues with every new joining player " +
                        "Make sure you don't have the vControlAimCanvas as a child of any gameobject.",
                        cont.gameObject.GetComponentInChildren<vControlAimCanvas>()
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        partial void SHOOTER_PerformvShooterManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vShooterManager manager in FindObjectsOfType<vShooterManager>())
            {
                if (_autoFixTests)
                {
                    if (!manager.damageLayer.ContainsLayer(LayerMask.NameToLayer("BodyPart")))
                    {
                        manager.damageLayer |= (1 << LayerMask.NameToLayer("BodyPart"));
                    }
                }
                if (!manager.damageLayer.ContainsLayer(LayerMask.NameToLayer("BodyPart")))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vShooterManager - You don't have 'BodyPart' as a Damage Layer on your " +
                        "vShooterManager component. If you don't have this you won't be able to " +
                        "damage players in multiplayer.",
                        manager
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        partial void SHOOTER_PerformvThrowCollectable(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vThrowCollectable collectable in FindObjectsOfType<vThrowCollectable>())
            {
                if (_autoFixTests)
                {
                    if (!collectable.gameObject.GetComponent<CallNetworkEvents>())
                    {
                        collectable.gameObject.AddComponent<CallNetworkEvents>();
                    }
                    if (!E_Helpers.ContainsUnityEvent(collectable, "onCollectObject", collectable.gameObject.GetComponent<CallNetworkEvents>(), "CallNetworkInvoke1"))
                    {
                        UnityEventTools.AddPersistentListener(collectable.onCollectObject, collectable.gameObject.GetComponent<CallNetworkEvents>().CallNetworkInvoke1);
                    }
                }
                if (!collectable.gameObject.GetComponent<CallNetworkEvents>())
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vThrowCollectable - Missing the 'CallNetworkEvents' component. " +
                        "This component is needed to send events over the network when this object is collected.",
                        collectable
                    ));
                }
                else
                {
                    passed += 1;
                    if (!E_PlayerEvents.HasUnityEvent(collectable.onCollectObject, "CallNetworkInvoke1", collectable.gameObject.GetComponent<CallNetworkEvents>()))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vThrowCollectable - Missing the 'CallNetworkInvoke1' UnityEvent on the 'OnCollectObject' event. " +
                            "You need to have this network event called to disable this object across the network when collected.",
                            collectable
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void SHOOTER_PerformvLockOnShooterTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vLockOnShooter lockOn in FindObjectsOfType<vLockOnShooter>())
            {

            }
        }
        partial void SHOOTER_PerformvShooterWeaponPrefabTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            string[] prefabPaths = GetResourcePrefabs();
            GameObject target;

            foreach (string prefabPath in prefabPaths)
            {
                UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath);
                try
                {
                    target = (GameObject)prefab;
                    if (!target.GetComponent<vShooterWeapon>()) continue;
                    if (_autoFixTests)
                    {
                        try
                        {
                            if (!target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<MP_Projectile>())
                            {
                                target.GetComponent<vShooterWeapon>().projectile.gameObject.AddComponent<MP_Projectile>();
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<vProjectileControl>(),
                                "onPassDamage", target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<MP_Projectile>(),
                                "TeamDamageCheck"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<vProjectileControl>().onPassDamage,
                                    target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<MP_Projectile>().TeamDamageCheck);
                            }
                            if (target.GetComponent<vShooterWeapon>().ignoreTags.Contains("Player"))
                            {
                                target.GetComponent<vShooterWeapon>().ignoreTags.Remove("Player");
                            }
                            if (!target.GetComponent<vShooterWeapon>().hitLayer.ContainsLayer(LayerMask.NameToLayer("Default")))
                            {
                                target.GetComponent<vShooterWeapon>().hitLayer |= (1 << LayerMask.NameToLayer("Default"));
                            }
                            if (!target.GetComponent<vShooterWeapon>().hitLayer.ContainsLayer(LayerMask.NameToLayer("BodyPart")))
                            {
                                target.GetComponent<vShooterWeapon>().hitLayer |= (1 << LayerMask.NameToLayer("BodyPart"));
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onReload", target.GetComponent<MP_ShooterWeapon>(), "SendNetworkReload"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onReload,
                                    target.GetComponent<MP_ShooterWeapon>().SendNetworkReload
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onFinishReload", target.GetComponent<MP_ShooterWeapon>(), "SendNetworkOnFinishReload"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onFinishReload,
                                    target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFinishReload
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onFinishAmmo", target.GetComponent<MP_ShooterWeapon>(), "SendNetworkOnFinishAmmo"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onFinishAmmo,
                                    target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFinishAmmo
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onEnableAim", target.GetComponent<MP_ShooterWeapon>(), "SendOnEnableAim"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onEnableAim,
                                    target.GetComponent<MP_ShooterWeapon>().SendOnEnableAim
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onDisableAim", target.GetComponent<MP_ShooterWeapon>(), "SendOnDisableAim"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onDisableAim,
                                    target.GetComponent<MP_ShooterWeapon>().SendOnDisableAim
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onFullPower", target.GetComponent<MP_ShooterWeapon>(), "SendNetworkOnFullPower"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onFullPower,
                                    target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFullPower
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onChangerPowerCharger", target.GetComponent<MP_ShooterWeapon>(), "SendOnChangerPowerCharger"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onPowerChargerChanged,
                                    target.GetComponent<MP_ShooterWeapon>().SendOnChangerPowerCharger
                                );
                            }
                            if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onChangerPowerCharger", target.GetComponent<MP_ShooterWeapon>(), "SendOnChangerPowerCharger"))
                            {
                                UnityEventTools.AddPersistentListener(
                                    target.GetComponent<vShooterWeapon>().onPowerChargerChanged,
                                    target.GetComponent<MP_ShooterWeapon>().SendOnChangerPowerCharger
                                );
                            }
                            if (target.GetComponent<vBowControl>())
                            {
                                if (!E_Helpers.ContainsUnityEvent(target.GetComponent<vShooterWeapon>(), "onInstantiateProjectile", target.GetComponent<MP_ShooterWeapon>(), "SetArrowView"))
                                {
                                    UnityEventTools.AddPersistentListener(
                                        target.GetComponent<vShooterWeapon>().onInstantiateProjectile,
                                        target.GetComponent<MP_ShooterWeapon>().SetArrowView
                                    );
                                }
                            }
                        }
                        catch { }
                    }

                    if (!target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<MP_Projectile>())
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Your projectile doesn't have the \"MP_Projectile\" component on it. You're most " +
                            "likely not using the mutliplayer projectile. This will not account for teams and will damage everyone.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<vProjectileControl>().onPassDamage,
                        "TeamDamageCheck", target.GetComponent<vShooterWeapon>().projectile.gameObject.GetComponent<MP_Projectile>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Your projectile's \"vProjectileControl\" component doesn't have the \"TeamDamageCheck\" UnityEvent on the " +
                            "\"onPassDamage\" event. This function comes from the \"MP_Projectile\" component on the projectile. If this isn't applied that " +
                            "means teams will be ignored and damage will always be sent to everyone.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if (target.GetComponent<vShooterWeapon>().ignoreTags.Contains("Player"))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Weapon is ignoring the player tag. This means networked players will not be " +
                            "able to damage the player when using this weapon.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if (!target.GetComponent<vShooterWeapon>().hitLayer.ContainsLayer(LayerMask.NameToLayer("Default")))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Weapon doesn't have the default layer as a layer this can hit. " +
                            "That mean weapon hit's on walls wont show.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!target.GetComponent<vShooterWeapon>().hitLayer.ContainsLayer(LayerMask.NameToLayer("BodyPart")))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Weapon doesn't have the 'BodyPart' layer as a layer " +
                            "it can hit. That means you wont be able to damage any AI or players as by " +
                            "default they are setup to use the 'BodyPart' layer.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }

                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onReload, "SendNetworkReload", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendNetworkReload' UnityEvent on the 'onReload' event. " +
                            "This means when this weapon triggers a reload event it will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishReload, "SendNetworkOnFinishReload", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendNetworkOnFinishReload' UnityEvent on the 'onFinishReload' event. " +
                            "This means when this weapon triggers a event when finished reloading it will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishAmmo, "SendNetworkOnFinishAmmo", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendNetworkOnFinishAmmo' UnityEvent on the 'onFinishAmmo' event. " +
                            "This means when this weapon triggers a event when finished its ammo it will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onEnableAim, "SendOnEnableAim", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendOnEnableAim' UnityEvent on the 'onEnableAim' event. " +
                            "This means when this weapon aims and fires it's custom events, they will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onDisableAim, "SendOnDisableAim", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendOnDisableAim' UnityEvent on the 'onDisableAim' event. " +
                            "This means when this weapon stops aiming and fires it's custom events, they will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFullPower, "SendNetworkOnFullPower", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendNetworkOnFullPower' UnityEvent on the 'onFullPower' event. " +
                            "This means when this weapon reaches full power and fires it's custom events, they will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onPowerChargerChanged, "SendOnChangerPowerCharger", target.GetComponent<MP_ShooterWeapon>()))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterWeapon - Missing the 'SendOnChangerPowerCharger' UnityEvent on the 'onChangerPowerCharger' event. " +
                            "This means when this weapon changes its power and fires it's custom events, they will not be triggered over the network.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (target.GetComponent<vBowControl>())
                    {
                        if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onInstantiateProjectile, "SetArrowView", target.GetComponent<MP_ShooterWeapon>()))
                        {
                            failed += 1;
                            failures.Add(new DebugFormat(
                                "vShooterWeapon - Missing the 'SetArrowView' UnityEvent on the 'onInstantiateProjectile' event. " +
                                "This means a unique identifier will not be applied to fired arrows. This will cause arrows to not be properly cleaned " +
                                "up across the network when picked up.",
                                AssetDatabase.LoadMainAssetAtPath(prefabPath)
                            ));
                        }
                        else
                        {
                            passed += 1;
                        }
                    }

                    if (target.GetComponent<vShooterWeapon>().projectile == null)
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vShooterWeapon - Weapon doesn't have a projectile assigned to it. " +
                            "That means nothing will happen when you fire this weapon.",
                            AssetDatabase.LoadMainAssetAtPath(prefabPath)
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
                        "vShooterWeapon - Failed to parse prefab at path: " + prefabPath,
                        AssetDatabase.LoadMainAssetAtPath(prefabPath)
                    ));
                }
            }
        }
        partial void SHOOTER_PerformItemCollectionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vItemCollection ic in FindObjectsOfType<vItemCollection>())
            {
                try
                {
                    string[] paths = GetResourcePrefabs();
                    GameObject target;
                    foreach (string path in paths)
                    {
                        UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(path);
                        target = (GameObject)prefab;
                        if (target.GetComponent<SyncItemCollection>() || target.GetComponentInChildren<SyncItemCollection>())
                        {
                            vItemCollection target_ic = (target.GetComponent<vItemCollection>()) ? target.GetComponent<vItemCollection>() : target.GetComponentInChildren<vItemCollection>();
                            SyncItemCollection target_sic = (target.GetComponent<SyncItemCollection>()) ? target.GetComponent<SyncItemCollection>() : target.GetComponentInChildren<SyncItemCollection>();
                            if (_autoFixTests)
                            {
                                try
                                {
                                    if (target.GetComponent<vArrow>() || target.GetComponentInChildren<vArrow>())
                                    {
                                        if (!E_Helpers.ContainsUnityEvent(target_ic, "OnPressActionInput", target_sic, "NetworkArrowViewDestroy"))
                                        {
                                            UnityEventTools.AddPersistentListener(
                                                target_ic.OnPressActionInput,
                                                target_sic.NetworkArrowViewDestroy
                                            );
                                        }
                                        if ((bool)target_sic.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(target_sic) == false)
                                        {
                                            target_sic.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).SetValue(target_sic, true);
                                        }
                                        if ((Transform)target_sic.GetType().GetField("holder", E_Helpers.allBindings).GetValue(target_sic) != target_sic.transform)
                                        {
                                            target_sic.GetType().GetField("holder", E_Helpers.allBindings).SetValue(target_sic, target_sic.transform);
                                        }
                                    }
                                    else
                                    {
                                        if ((bool)target_sic.GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(target_sic) == true)
                                        {
                                            target_sic.GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).SetValue(target_sic, true);
                                        }
                                        if (string.IsNullOrEmpty((string)target_sic.GetType().GetField("resourcesPrefab", E_Helpers.allBindings).GetValue(target_sic)))
                                        {
                                            target_sic.GetType().GetField("resourcesPrefab", E_Helpers.allBindings).SetValue(target_sic, PrefabUtility.GetCorrespondingObjectFromSource(target_sic.gameObject));
                                        }
                                    }
                                }
                                catch { }
                            }
                            if (target.GetComponent<vArrow>() || target.GetComponentInChildren<vArrow>())
                            {
                                if (!E_PlayerEvents.HasUnityEvent(target_ic.OnPressActionInput, "NetworkArrowViewDestroy", target_sic))
                                {
                                    failed += 1;
                                    failures.Add(new DebugFormat(
                                        "vItemCollection - Missing the 'NetworkArrowViewDestroy' UnityEvent on the 'OnPressActionInput' event. " +
                                        "If this isn't fired it will not destroy this arrow when picked up across the network.",
                                        target
                                    ));
                                }
                                else
                                {
                                    passed += 1;
                                }

                                if ((bool)target_sic.GetType().GetField("syncCrossScenes", E_Helpers.allBindings).GetValue(target_sic) == false)
                                {
                                    failed += 1;
                                    failures.Add(new DebugFormat(
                                        "SyncItemCollection - \"syncCrossScenes\" is false. While arrows are not technically synced across scenes this is still " +
                                        "required to get the network destroy functionality on pickups working.",
                                        target
                                    ));
                                }
                                else
                                {
                                    passed += 1;
                                    if ((Transform)target_sic.GetType().GetField("holder", E_Helpers.allBindings).GetValue(target_sic) != target_sic.transform)
                                    {
                                        failed += 1;
                                        failures.Add(new DebugFormat(
                                            "SyncItemCollection - \"Track Position\" transform needs to be the same as the \"SyncItemCollection\" transform.",
                                            target
                                        ));
                                    }
                                    else
                                    {
                                        passed += 1;
                                    }

                                    if ((bool)target_sic.GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(target_sic) == true)
                                    {
                                        failed += 1;
                                        failures.Add(new DebugFormat(
                                            "SyncItemCollection - \"Is Dynamic Object\" is not supported for arrows. This needs to be false.",
                                            target
                                        ));
                                    }
                                    else
                                    {
                                        passed += 1;
                                    }
                                }

                                if (!target.GetComponent<ArrowView>() && !target.GetComponentInChildren<ArrowView>())
                                {
                                    failed += 1;
                                    failures.Add(new DebugFormat(
                                        "SyncItemCollection - \"ArrowView\" component is missing. This component is required for " +
                                        "all arrow objects. If not applied it means a uniquie identifier cannot be applied which " +
                                        "will make this arrow not properly cleaned up over the network when picked up.",
                                        target
                                    ));
                                }
                                else
                                {
                                    passed += 1;
                                    vProjectileControl control = (target.GetComponent<vProjectileControl>()) ? target.GetComponent<vProjectileControl>() : target.GetComponentInChildren<vProjectileControl>();
                                    ArrowView arrowView = (target.GetComponent<ArrowView>()) ? target.GetComponent<ArrowView>() : target.GetComponentInChildren<ArrowView>();
                                    if (!E_PlayerEvents.HasUnityEvent(control.onDestroyProjectile, "NetworkSetPosRot", arrowView))
                                    {
                                        failed += 1;
                                        failures.Add(new DebugFormat(
                                            "vProjectileControl - Missing the 'NetworkSetPosRot' UnityEvent on the 'onDestroyProjectile' event. " +
                                            "If this isn't fired it will result in different positions/rotations of the arrows across the network.",
                                            target
                                        ));
                                    }
                                    else
                                    {
                                        passed += 1;
                                    }
                                }

                                if ((bool)target_sic.GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).GetValue(target_sic) == false)
                                {
                                    if (!target.GetComponent<vArrow>() || !target.GetComponentInChildren<vArrow>())
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
                                    }
                                }
                            }
                            else
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
        partial void SHOOTER_PerformItemListDataTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(vItemListData).Name);  //FindAssets uses tags check documentation for more info
            vItemListData[] itemListDatas = new vItemListData[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                itemListDatas[i] = AssetDatabase.LoadAssetAtPath<vItemListData>(path);
            }
            foreach(vItemListData data in itemListDatas)
            {
                foreach(vItem item in data.items)
                {
                    if (item.type == vItemType.ShooterWeapon)
                    {
                        string dropItemPath = AssetDatabase.GetAssetPath(item.dropObject);
                        string spawnItemPath = AssetDatabase.GetAssetPath(item.originalObject);
                        if (_autoFixTests)
                        {
                            if (dropItemPath.Contains("Assets/Resources") && 
                                item.dropObject && !item.dropObject.GetComponent<SyncItemCollection>())
                            {
                                item.dropObject.AddComponent<SyncItemCollection>();
                            }
                            else if (item.dropObject && !dropItemPath.Contains("Assets/Resources"))
                            {
                                string temp = "Assets/Resources/"+"MP_" + item.dropObject.name + ".prefab";
                                GameObject found = (GameObject)AssetDatabase.LoadAssetAtPath(temp, typeof(GameObject));
                                if (found != null)
                                {
                                    item.dropObject = found;
                                }
                            }
                            if (item.originalObject)
                            {
                                if (!item.originalObject.GetComponent<MP_ShooterWeapon>())
                                {
                                    string temp = "Assets/MP_Converted/Weapons/MP_" + item.originalObject.name + ".prefab";
                                    GameObject found = (GameObject)AssetDatabase.LoadAssetAtPath(temp, typeof(GameObject));
                                    if (found != null)
                                    {
                                        item.originalObject = found;
                                    }
                                }
                            }
                        }
                        if (!dropItemPath.Contains("Assets/Resources"))
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
                            if (!item.dropObject.GetComponent<SyncItemCollection>())
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
                        if (item.originalObject)
                        {
                            passed += 1;
                            if (!item.originalObject.GetComponent<MP_ShooterWeapon>())
                            {
                                failed += 1;
                                warnings.Add(new DebugFormat(
                                    "vItemListData(" + data.name + ") - The \"" + item.name + "\" spawn object is probably not the multiplayer " +
                                    "version because it doesn't have the \"MP_ShooterWeapon\" component attached. That means " +
                                    "no actions taken with this weapon will be synced across the network.",
                                    data
                                ));
                            }
                            else
                            {
                                passed += 1;
                            }
                        }
                        else
                        {
                            failed += 1;
                            warnings.Add(new DebugFormat(
                                "vItemListData(" + data.name + ") - The \"" + item.name + "\" spawn object is blank. That means if this item " +
                                "is meant to be spawned nothing will be spawned and it could potentially throw and error at runtime. If its not " +
                                "meant to be spawned then this can be safetly ignored.",
                                data
                            ));
                        }
                    }
                }
            }
        }
        partial void SHOOTER_PerformvShooterMeleeInputTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vShooterMeleeInput input in FindObjectsOfType<vShooterMeleeInput>())
            {
                if (_autoFixTests)
                {
                    try
                    {
                        if (!input.gameObject.GetComponent<PhotonView>())
                        {
                            input.gameObject.AddComponent<PhotonView>();
                        }
                        if (!input.GetComponent<PhotonView>().ObservedComponents.Contains(input))
                        {
                            input.GetComponent<PhotonView>().ObservedComponents.Add(input);
                        }
                    }
                    catch { }
                }
                if (!input.gameObject.GetComponent<PhotonView>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vShooterMeleeInput - The gameobject that holds this component is missing a PhotonView component. " +
                        "This will cause errors at runtime if this component is missing from this gameobject.",
                        input
                    ));
                }
                else
                {
                    passed += 1;
                    if (input.GetComponent<PhotonView>() && !input.GetComponent<PhotonView>().ObservedComponents.Contains(input))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "vShooterMeleeInput - The PhotonView component is missing the ShooterMeleeInput component from its " +
                            "\"ObservedComponents\" variable. While this won't cause errors the reload animations will not properly " +
                            "play over the network until this component is added to the \"Observed Components\" list in the \"PhotonView\" " +
                            "component.",
                            input
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void SHOOTER_CheckAddonEnabled()
        {
            shooterExists = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/Objects/Shooter/MP_ShooterWeapon.cs",
                "/*"
            );
        }
        
    }
}
*/
