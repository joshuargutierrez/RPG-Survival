/*
using Invector.vCharacterController;
using Invector.vShooter;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_ConvertPlayer
    {
        #region Context Menus
        [MenuItem("CONTEXT/vShooterMeleeInput/Replace vShooterMeleeInput with MP Version")]
        public static void CB_MENU_vShooterMeleeInput(MenuCommand command)
        {
            vShooterMeleeInput comp = (vShooterMeleeInput)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vShooterMeleeInput(target, ref log);
        }
        [MenuItem("CONTEXT/vShooterManager/Replace vShooterManager with MP Version")]
        public static void CB_MENU_vShooterManager(MenuCommand command)
        {
            vShooterManager comp = (vShooterManager)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vShooterManager(target, ref log);
        }

        #endregion

        #region Conversion Logic
        static partial void CB_COMP_vRagdoll(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vRagdoll>()) return;
            if (log != null) log.Add("Replacing vRagdoll -> MP_vRagdoll");
            E_CompHelper.ReplaceWithComponent(target, typeof(vRagdoll), typeof(MP_vRagdoll), true);
        }
        static partial void CB_COMP_vShooterMeleeInput(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vShooterMeleeInput>()) return;
            if (log != null) log.Add("Replacing vShooterMeleeInput -> MP_vShooterMeleeInput");
            E_CompHelper.ReplaceWithComponent(target, typeof(vShooterMeleeInput), typeof(MP_vShooterMeleeInput), true);
        }
        static partial void CB_COMP_vShooterManager(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vShooterManager>()) return;
            if (log != null) log.Add("Replacing vShooterManager -> MP_vShooterManager");
            E_CompHelper.ReplaceWithComponent(target, typeof(vShooterManager), typeof(MP_vShooterManager), false);
        }
        #endregion

        #region Check Logic
        static partial void SHOOTER_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict)
        {
            if (target.GetComponent<vShooterMeleeInput>())
            {
                dict.Add("* Replace vShooterMeleeInput -> MP_vShooterMeleeInput", target.GetComponentInChildren<vShooterMeleeInput>());
            }
            if (target.GetComponent<vShooterManager>())
            {
                dict.Add("* Replace vShooterManager -> MP_vShooterManager", target.GetComponentInChildren<vShooterManager>());
            }
        }
        static partial void SHOOTER_HasvShooterManagerComp(GameObject target)
        {
            if (target.GetComponent<vShooterManager>())
            {
                CB_shooterEnabled = true;
            }
        }
        #endregion
    }
}
*/
