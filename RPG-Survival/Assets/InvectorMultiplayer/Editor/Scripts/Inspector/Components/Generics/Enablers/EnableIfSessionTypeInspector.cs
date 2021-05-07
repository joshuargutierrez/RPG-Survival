using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(EnableIfSessionType), true)]
    public class EnableIfSessionTypeInspector : BaseEditor
    {
        #region Properties
        SerializedProperty isOwner;
        #pragma warning disable CS0108
        SerializedProperty targets;
        #pragma warning restore CS0108
        SerializedProperty type;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            isOwner = serializedObject.FindProperty("isOwner");
            targets = serializedObject.FindProperty("targets");
            type = serializedObject.FindProperty("type");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Enable If Session Type", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will enable target gameobjects if your selected session type " +
                "matches and you are/are not the room owner.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(isOwner);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(targets, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(EnableIfSessionType));
            #endregion
        }
    }
}