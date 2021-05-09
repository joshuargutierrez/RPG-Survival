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
        [MenuItem("CONTEXT/vZipline/Replace vZipline with MP Version")]
        public static void CB_MENU_vZipline(MenuCommand command)
        {
            vZipLine comp = (vZipLine)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vZipline(target, ref log);
        }
        #endregion

        #region Convert Logic
        static partial void CB_COMP_vZipline(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vZipLine>()) return;
            if (log != null) log.Add("Replace vZipline -> MP_vZipline");
            E_CompHelper.ReplaceWithComponent(target, typeof(vZipLine), typeof(MP_vZipline), false);
        }
        #endregion

        #region Check Logic
        static partial void ZIPLINE_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict)
        {
            if (target.GetComponent<vZipLine>())
            {
                dict.Add("* Replace vZipline with MP_vZipline component", target.GetComponent<vSwimming>());
            }
        }
        #endregion
    }
}
*/
