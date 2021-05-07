using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vShooter
{
    [vClassHeader("Set Weapon IK Settings", openClose = false)]
    public class vSetWeaponIKSettings : vMonoBehaviour
    {
        public List<IKSettings> settings;
        [System.Serializable]
        public class IKSettings
        {
            [Tooltip("IK will help the right hand to align where you actually is aiming")]
            public bool alignRightHandToAim = true;
            [Tooltip("IK will help the right hand to align where you actually is aiming")]
            public bool alignRightUpperArmToAim = true;
            public bool raycastAimTarget = true;
            [Tooltip("Left IK on Idle")]
            public bool useIkOnIdle = true;
            [Tooltip("Left IK on free locomotion")]
            public bool useIkOnFree = true;
            [Tooltip("Left IK on strafe locomotion")]
            public bool useIkOnStrafe = true;
            [Tooltip("Left IK while attacking")]
            public bool useIkAttacking = false;
            [Tooltip("Left IK while Shot")]
            public bool disableIkOnShot = false;
            [Tooltip("Left IK while Aming")]
            public bool useIKOnAiming = true;
        }

        [vHelpBox("It's recommended to attach this component in a Handler")]

        [Tooltip("Auto get shooter weapon when set settings")]
        public bool getWeaponOnSet = true;
        [vHideInInspector("getWeaponOnSet", invertValue = true)]
        public vShooterWeapon weapon;

        public bool setOnStart;
        [vHideInInspector("setOnStart")]
        public int indexOfSetting;

        private void Start()
        {
            if (setOnStart)
            {
                SetSettings(indexOfSetting);
            }
        }

        public void SetSettings(int index)
        {
            if (getWeaponOnSet)
            {
                weapon = GetComponentInChildren<vShooterWeapon>();
            }

            if (!weapon)
            {
                return;
            }

            if (settings.Count > 0 && index >= 0 && index < settings.Count)
            {
                IKSettings setting = settings[index];
                weapon.alignRightHandToAim = setting.alignRightHandToAim;
                weapon.alignRightUpperArmToAim = setting.alignRightUpperArmToAim;
                weapon.raycastAimTarget = setting.raycastAimTarget;
                weapon.useIkAttacking = setting.useIkAttacking;
                weapon.useIKOnAiming = setting.useIKOnAiming;
                weapon.useIkOnFree = setting.useIkOnFree;
                weapon.useIkOnIdle = setting.useIkOnIdle;
                weapon.useIkOnStrafe = setting.useIkOnStrafe;
            }
        }
    }
}