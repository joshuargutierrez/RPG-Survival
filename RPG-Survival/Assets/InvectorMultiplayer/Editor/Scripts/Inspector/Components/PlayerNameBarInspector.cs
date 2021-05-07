using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.Player;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(PlayerNameBar),true)]
    public class PlayerNameBarInspector : BaseEditor
    {
        #region Properties
        SerializedProperty playerName;
        SerializedProperty playerBar;
        #endregion

        protected override void OnEnable()
        {

            #region Properties
            playerName = serializedObject.FindProperty("playerName");
            playerBar = serializedObject.FindProperty("playerBar");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            PlayerNameBar nb = (PlayerNameBar)target;
            DrawTitleBar(
                "Network Player Name Bar", 
                "Component that belongs on each player. Will set the PlayerName " +
                "text to be what the network nickname is set to. The nickname can " +
                "be set via the NetworkManager component.", 
            E_Core.h_playerIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(playerName);
            EditorGUILayout.PropertyField(playerBar);
            #endregion

            EndInspectorGUI(typeof(PlayerNameBar));
        }
    }
}
