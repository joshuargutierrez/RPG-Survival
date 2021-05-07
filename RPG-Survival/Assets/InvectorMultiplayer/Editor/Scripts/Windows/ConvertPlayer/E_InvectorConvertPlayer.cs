/*
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CBGames.Player;

namespace CBGames.Editors
{
    public partial class E_ConvertPlayer
    {
        #region Context Menus
        [MenuItem("CONTEXT/vHeadTrack/Replace vHeadTrack with MP Version")]
        public static void CB_MENU_vHeadTrack(MenuCommand command)
        {
            vHeadTrack comp = (vHeadTrack)command.context;
            GameObject target = comp.gameObject;
            List<string> log = new List<string>();
            CB_COMP_vHeadTrack(target.gameObject, ref log);
        }
        [MenuItem("CONTEXT/vGenericAction/Add vGenericAction MP Components & Events")]
        public static void CB_MENU_vGenericAction(MenuCommand command)
        {
            vGenericAction comp = (vGenericAction)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vGenericAction(target, ref log);
        }
        #endregion

        #region Conversion Logic
        static partial void CB_COMP_vHeadTrack(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vHeadTrack>()) return;

            if (log != null) log.Add("Replacing vHeadTrack -> MP_HeadTrack");
            if (log != null) log.Add("Adding MP_HeadTrack -> PhotonView");
            E_CompHelper.ReplaceWithComponent(target, typeof(vHeadTrack), typeof(MP_HeadTrack), true);
        }
        static partial void CB_COMP_vGenericAction(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vGenericAction>()) return;
            E_CompHelper.ReplaceWithComponent(target, typeof(vGenericAction), typeof(MP_vGenericAction), false);
        }
        #endregion

        #region Check Logic
        static partial void CB_CHECK_vGenericAction(GameObject target)
        {
            if (target.GetComponent<MP_vGenericAction>())
            {
                CB_hasMPGenericAction = true;
            }
            else
            {
                CB_hasMPGenericAction = false;
            }
        }
        #endregion
    }
}
*/
