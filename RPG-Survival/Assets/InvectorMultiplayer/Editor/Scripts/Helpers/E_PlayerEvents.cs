#if UNITY_EDITOR
using CBGames.Core;
using Invector;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using Invector.vMelee;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#region Shooter FSMAI Shared
using Invector.vShooter;
using System.Reflection;
using UnityEditor.Events;
using System.Linq;
using Photon.Voice.IOS;
using UnityEditor;
using Photon.Pun;
#endregion

namespace CBGames.Editors
{
    public class E_PlayerEvents
    {
        #region UnityEvents

        #region Shooter Template
        public static bool HasUnityEvent(vShooterWeaponBase.OnInstantiateProjectile targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(vProjectileControl.ProjectileCastColliderEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Shooter FSMAI Shared
        public static bool HasUnityEvent(vShooterWeapon.OnChangePowerCharger targetEvent, string HasMethodName, Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Copy UnityEvents
        
        public static bool HasUnityEvent(vTriggerGenericAction.OnUpdateValue targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(PlayerListEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(UnityEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(UnityEvent<GameObject> targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(UnityEvent<vHitInfo> targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(UnityEvent<vItem, int> targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(UnityEvent<vEquipArea, vItem> targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasUnityEvent(InputField.SubmitEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasUnityEvent(StringUnityEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasUnityEvent(UnityEvent<vDamage> targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasUnityEvent(PlayerEvent targetEvent, string HasMethodName, UnityEngine.Object hasTarget)
        {
            int count = targetEvent.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (targetEvent.GetPersistentMethodName(i) == HasMethodName && targetEvent.GetPersistentTarget(i) == hasTarget)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}
#endif
