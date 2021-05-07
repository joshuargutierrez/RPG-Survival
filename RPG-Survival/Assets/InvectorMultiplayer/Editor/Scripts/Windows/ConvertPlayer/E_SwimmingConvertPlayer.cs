/*
using CBGames.Player;
using Invector.vCharacterController.vActions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_ConvertPlayer
    {
        #region Context Menus
        [MenuItem("CONTEXT/vSwimming/Replace vSwimming with MP Version")]
        public static void CB_MENU_vSwimming(MenuCommand command)
        {
            vSwimming comp = (vSwimming)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vSwimming(target, ref log);
        }
        #endregion

        #region Convert Logic
        static partial void CB_COMP_vSwimming(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vSwimming>()) return;
            if (log != null) log.Add("Replace vSwimming -> MP_vSwimming");
            E_CompHelper.ReplaceWithComponent(target, typeof(vSwimming), typeof(MP_vSwimming), false);
        }
        #endregion

        #region Check Logic
        static partial void SWIMMING_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict)
        {
            if (target.GetComponent<vSwimming>())
            {
                dict.Add("* Replace vSwimming with MP_vSwimming component", target.GetComponent<vSwimming>());
            }
        }
        #endregion
    }
}
*/
