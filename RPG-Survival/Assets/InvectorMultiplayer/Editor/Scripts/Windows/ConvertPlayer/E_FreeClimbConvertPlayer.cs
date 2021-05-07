/*
using CBGames.Player;
using Invector.vCharacterController.vActions;
using Photon.Pun;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_ConvertPlayer
    {
        #region Context Menus
        [MenuItem("CONTEXT/vFreeClimb/Replace vFreeClimb with MP Version")]
        public static void CB_MENU_vFreeClimb(MenuCommand command)
        {
            vFreeClimb comp = (vFreeClimb)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vFreeClimb(target, ref log);
        }
        #endregion

        #region Conversion Logic
        static partial void CB_COMP_vFreeClimb(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vFreeClimb>()) return;
            if (log != null) log.Add("Replacing vFreeClimb -> MP_vFreeClimb");
            E_CompHelper.ReplaceWithComponent(target, typeof(vFreeClimb), typeof(MP_vFreeClimb), true);
            if (!target.GetComponent<PhotonRigidbodyView>())
            {
                if (log != null) log.Add("Adding PhotonRigidbodyView component.");
                target.AddComponent<PhotonRigidbodyView>();
            }
            if (log != null) log.Add("Adding PhotonRigidbodyView to PhotonView.");
            E_CompHelper.AddCompToPhotonView(target, target.GetComponent<PhotonRigidbodyView>());
        }
        #endregion

        #region Check Logic
        static partial void FREECLIMB_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict)
        {
            if (target.GetComponentInChildren<vFreeClimb>())
            {
                dict.Add("* Replace vFreeClimb with MP_vFreeClimb component", target.GetComponentInChildren<vFreeClimb>());
            }
        }
        #endregion
    }
}
*/
