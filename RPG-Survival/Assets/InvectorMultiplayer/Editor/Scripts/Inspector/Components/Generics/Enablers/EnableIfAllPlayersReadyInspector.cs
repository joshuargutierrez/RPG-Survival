using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(EnableIfAllPlayersReady), true)]
    public class EnableIfAllPlayersReadyInspector : BaseEditor
    {
        #region Properties
        SerializedProperty mustBeOnwer;
        #pragma warning disable CS0108
        SerializedProperty targets;
        #pragma warning restore CS0108
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            mustBeOnwer = serializedObject.FindProperty("mustBeOnwer");
            targets = serializedObject.FindProperty("targets");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Enable If All Player Ready", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will enable/disable target objects based on if every single " +
                "player has indicated they are \"Ready\".", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(mustBeOnwer);
            EditorGUILayout.PropertyField(targets, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(EnableIfAllPlayersReady));
            #endregion
        }
    }
}