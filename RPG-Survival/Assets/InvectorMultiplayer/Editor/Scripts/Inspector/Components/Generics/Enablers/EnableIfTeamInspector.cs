using CBGames.Editors;
using UnityEditor;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(EnableIfTeam), true)]
    public class EnableIfTeamInspector : BaseEditor
    {
        #region Properties
        SerializedProperty checkType;
        SerializedProperty teamName;
        SerializedProperty items;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            checkType = serializedObject.FindProperty("checkType");
            teamName = serializedObject.FindProperty("teamName");
            items = serializedObject.FindProperty("items");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Enable If Team", 
                "Doesn't require a UICoreLogic component. Will look at the team set " +
                "in the NetworkManager component as see if it matches. If it does will " +
                "enable all the targeted items otherwise it will disable them.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(checkType);
            EditorGUILayout.PropertyField(teamName);
            EditorGUILayout.PropertyField(items, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(EnableIfTeam));
            #endregion
        }
    }
}