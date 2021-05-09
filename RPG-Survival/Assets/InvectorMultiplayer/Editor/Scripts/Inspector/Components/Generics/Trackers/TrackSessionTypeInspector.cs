using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackSessionType), true)]
    public class TrackSessionTypeInspector : BaseEditor
    {
        #region Properties
        SerializedProperty texts;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            texts = serializedObject.FindProperty("texts");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Session Type", 
                "This component requires a \"UICoreLogic\" component somewhere in the scene. \n\n" +
                "This component will track if this room is a private or public room and modify the " +
                "targeted texts to display that information.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackSessionType));
            #endregion
        }
    }
}