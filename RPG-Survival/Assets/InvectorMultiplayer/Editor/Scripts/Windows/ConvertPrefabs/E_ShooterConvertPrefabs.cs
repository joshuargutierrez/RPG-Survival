using CBGames.Editors;
using CBGames.Objects;
using Invector.vItemManager;
using Invector.vShooter;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_ConvertPrefabs
    {
        #region Context Menus
        [MenuItem("CONTEXT/vShooterWeapon/Add MP Components for vShooterWeapon")]
        public static void CB_CONTEXT_vShooterWeapon(MenuCommand command)
        {
            vShooterWeapon prefab = (vShooterWeapon)command.context;
            CB_COMP_vShooterWeapon(prefab.gameObject);
        }
        [MenuItem("CONTEXT/vProjectileControl/Add MP Components for vProjectileControle")]
        public static void CB_CONTEXT_vProjectileControl(MenuCommand command)
        {
            vProjectileControl prefab = (vProjectileControl)command.context;
            CB_COMP_vProjectileControle(prefab.gameObject);
        }
        #endregion

        #region Convert Logic
        static partial void CB_COMP_vProjectileControle(GameObject target)
        {
            if (target.GetComponent<vProjectileControl>())
            {
                if (!target.GetComponent<MP_Projectile>())
                {
                    target.AddComponent<MP_Projectile>();
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vProjectileControl>().onPassDamage, "TeamDamageCheck", target.GetComponent<MP_Projectile>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vProjectileControl>().onPassDamage, target.GetComponent<MP_Projectile>().TeamDamageCheck);
                }
            }
            else if (target.GetComponentInChildren<vProjectileControl>())
            {
                foreach (vProjectileControl control in target.GetComponentsInChildren<vProjectileControl>())
                {
                    if (!control.gameObject.GetComponent<MP_Projectile>())
                    {
                        control.gameObject.AddComponent<MP_Projectile>();
                    }
                    if (!E_PlayerEvents.HasUnityEvent(control.onPassDamage, "TeamDamageCheck", control.gameObject.GetComponent<MP_Projectile>()))
                    {
                        UnityEventTools.AddPersistentListener(control.onPassDamage, control.gameObject.GetComponent<MP_Projectile>().TeamDamageCheck);
                    }
                }
            }
        }
        static partial void CB_COMP_vShooterWeapon(GameObject target)
        {
            if (target.GetComponent<vShooterWeapon>())
            {
                if (!target.GetComponent<MP_ShooterWeapon>())
                {
                    target.AddComponent<MP_ShooterWeapon>();
                }
                E_Helpers.SetObjectIcon(target, E_Core.h_genericIcon);
                //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onShot, "SendNetworkShot", target.GetComponent<MP_ShooterWeapon>()))
                //{
                //    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onShot, target.GetComponent<MP_ShooterWeapon>().SendNetworkShot);
                //}
                //if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onEmptyClip, "SendNetworkEmptyClip", target.GetComponent<MP_ShooterWeapon>()))
                //{
                //    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onEmptyClip, target.GetComponent<MP_ShooterWeapon>().SendNetworkEmptyClip);
                //}
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onReload, "SendNetworkReload", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onReload, target.GetComponent<MP_ShooterWeapon>().SendNetworkReload);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishReload, "SendNetworkOnFinishReload", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFinishReload, target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFinishReload);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFullPower, "SendNetworkOnFullPower", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFullPower, target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFullPower);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onFinishAmmo, "SendNetworkOnFinishAmmo", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onFinishAmmo, target.GetComponent<MP_ShooterWeapon>().SendNetworkOnFinishAmmo);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onEnableAim, "SendOnEnableAim", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onEnableAim, target.GetComponent<MP_ShooterWeapon>().SendOnEnableAim);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onDisableAim, "SendOnDisableAim", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onDisableAim, target.GetComponent<MP_ShooterWeapon>().SendOnDisableAim);
                }
                if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vShooterWeapon>().onPowerChargerChanged, "SendOnChangerPowerCharger", target.GetComponent<MP_ShooterWeapon>()))
                {
                    UnityEventTools.AddPersistentListener(target.GetComponent<vShooterWeapon>().onPowerChargerChanged, target.GetComponent<MP_ShooterWeapon>().SendOnChangerPowerCharger);
                }
            }
            if (target.GetComponentInChildren<vShooterWeapon>())
            {
                foreach (vShooterWeapon weapon in target.GetComponentsInChildren<vShooterWeapon>())
                {
                    if (!weapon.gameObject.GetComponent<MP_ShooterWeapon>())
                    {
                        weapon.gameObject.AddComponent<MP_ShooterWeapon>();
                    }
                    E_Helpers.SetObjectIcon(weapon.gameObject, E_Core.h_genericIcon);
                    //if (!E_PlayerEvents.HasUnityEvent(weapon.onShot, "SendNetworkShot", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    //{
                    //    UnityEventTools.AddPersistentListener(weapon.onShot, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendNetworkShot);
                    //}
                    //if (!E_PlayerEvents.HasUnityEvent(weapon.onEmptyClip, "SendNetworkEmptyClip", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    //{
                    //    UnityEventTools.AddPersistentListener(weapon.onEmptyClip, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendNetworkEmptyClip);
                    //}
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onReload, "SendNetworkReload", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onReload, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendNetworkReload);
                    }
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onFinishReload, "SendNetworkOnFinishReload", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onFinishReload, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendNetworkOnFinishReload);
                    }
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onFullPower, "SendNetworkOnFullPower", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onFullPower, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendNetworkOnFullPower);
                    }
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onEnableAim, "SendOnEnableAim", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onEnableAim, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendOnEnableAim);
                    }
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onDisableAim, "SendOnDisableAim", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onDisableAim, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendOnDisableAim);
                    }
                    if (!E_PlayerEvents.HasUnityEvent(weapon.onPowerChargerChanged, "SendOnChangerPowerCharger", weapon.gameObject.GetComponent<MP_ShooterWeapon>()))
                    {
                        UnityEventTools.AddPersistentListener(weapon.onPowerChargerChanged, weapon.gameObject.GetComponent<MP_ShooterWeapon>().SendOnChangerPowerCharger);
                    }
                }
            }
        }
        static partial void CB_COMP_SHOOTER_vItemCollection(vItemCollection ic, GameObject go)
        {
            if (ic.transform.GetComponentInParent<vProjectileControl>() && ic.transform.GetComponentInParent<vArrow>())
            {
                go.GetComponent<SyncItemCollection>().GetType().GetField("syncCrossScenes", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), false);
                go.GetComponent<SyncItemCollection>().GetType().GetField("syncCreateDestroy", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), false);
                go.GetComponent<SyncItemCollection>().GetType().GetField("skipStartCheck", E_Helpers.allBindings).SetValue(go.GetComponent<SyncItemCollection>(), false);
                if (!ic.transform.gameObject.GetComponent<ArrowView>())
                {
                    ic.transform.gameObject.AddComponent<ArrowView>();
                }
                if (!E_PlayerEvents.HasUnityEvent(ic.OnPressActionInput, "NetworkArrowViewDestroy", ic.transform.gameObject.GetComponent<SyncItemCollection>()))
                {
                    UnityEventTools.AddPersistentListener(ic.OnPressActionInput, ic.transform.gameObject.GetComponent<SyncItemCollection>().NetworkArrowViewDestroy);
                }
                if (!E_PlayerEvents.HasUnityEvent(ic.transform.GetComponentInParent<vProjectileControl>().onDestroyProjectile, "NetworkSetPosRot", ic.transform.gameObject.GetComponent<ArrowView>()))
                {
                    UnityEventTools.AddPersistentListener(ic.transform.GetComponentInParent<vProjectileControl>().onDestroyProjectile, ic.transform.gameObject.GetComponent<ArrowView>().NetworkSetPosRot);
                }
            }
        }
        #endregion

        #region Check Logic
        static partial void CB_SHOOTER_CheckFromComps(GameObject target)
        {
            if (target.GetComponent<vShooterWeapon>())
            {
                CB_previewConverts.Add("* Add PhotonView component");
                CB_previewConverts.Add("* Add MP_ShooterWeapon component");
                CB_previewConverts.Add("* Add MP_ShooterWeapon events -> vShooterWeapon");
            }
        }
        static partial void CB_SHOOTER_HasShooterComp(GameObject target, bool checkVal, string prefabPath)
        {
            if (checkVal == true && !CB_prefabs.ContainsValue(prefabPath) &&
                (target.GetComponent<vShooterWeapon>() || target.GetComponent<vProjectileControl>()))
            {
                CB_prefabs.Add(target, prefabPath);
            }
        }
        #endregion

        #region Path Change Logic
        static partial void CB_PATH_Check_Shooter(GameObject _copiedPrefab, ref string saveLocation)
        {
            if (_copiedPrefab.GetComponent<vShooterWeapon>() || _copiedPrefab.GetComponentInChildren<vShooterWeapon>() ||
                    _copiedPrefab.GetComponent<vProjectileControl>() || _copiedPrefab.GetComponentInChildren<vProjectileControl>())
            {
                saveLocation = saveLocation.Replace("Resources" + Path.DirectorySeparatorChar, "MP_Converted/Weapons/");
                string dirLoc = saveLocation.Replace(_copiedPrefab.name + ".prefab", "");
                E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(dirLoc));
            }
        }
        #endregion
    }
}
