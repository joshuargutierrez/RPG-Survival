using UnityEditor;
using UnityEngine;
using CBGames.Core;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(SyncHealthController), true)]
    public class SyncHealthControllerInspector : BaseEditor
    {
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            SyncHealthController hc = (SyncHealthController)target;
            DrawTitleBar(
                "Sync Health Controller", 
                "Component that offers the \"SendDamageOverNetwork\" RPC. On the \"vHealthController\"'s " +
                "\"OnReceiveDamage\" event add this function call to sync the damage over the network.", 
                E_Core.h_genericIcon
            );
            #endregion

            EndInspectorGUI(typeof(SyncHealthController));
        }
    }
}