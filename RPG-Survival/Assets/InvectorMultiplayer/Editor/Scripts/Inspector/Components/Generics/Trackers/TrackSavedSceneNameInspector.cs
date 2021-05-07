using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackSavedSceneName), true)]
    public class TrackSavedSceneNameInspector : BaseEditor
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
                "Track Saved Scene Name", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will modify your Text components to match the name of the scene that is currently saved " +
                "in the UICoreLogic component.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackSavedSceneName));
            #endregion
        }
    }
}