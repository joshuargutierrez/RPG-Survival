using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackSelectedTeam), true)]
    public class TrackSelectedTeamInspector : BaseEditor
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
                "Track Selected Team", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will change the targeted Text to match whatever team you currently" +
                " have set for yourself.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackSelectedTeam));
            #endregion
        }
    }
}