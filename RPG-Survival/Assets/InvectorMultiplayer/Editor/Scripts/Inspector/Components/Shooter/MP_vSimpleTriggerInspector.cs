/*
using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(MP_vSimpleTrigger), true)]
    public class MP_vSimpleTriggerInspector : BaseEditor
    {
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Network Simple Trigger",
                "This will watch when a target tagged/layered collider enters/exits the trigger. " +
                "If on your vSimpleTrigger component you have actions in either of these it will trigger " +
                "it over the network ONLY if the collider that entered the trigger has a PhotonView component " +
                "attached to it. If no PhotonView is found on the object that entered/exited the trigger then " +
                "it will NOT Invoke these UnityEvents over the network.",
                E_Core.h_genericIcon
            );
            #endregion

            #region End Core
            EndInspectorGUI(typeof(MP_vSimpleTrigger));
            #endregion
        }
    }
}
*/
