using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(EnableIfChatConnected), true)]
    public class EnableIfChatConnectedInspector : BaseEditor
    {
        #region Properties
        SerializedProperty buttons;
        SerializedProperty gameObjects;
        SerializedProperty invertActions;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            buttons = serializedObject.FindProperty("buttons");
            gameObjects = serializedObject.FindProperty("gameObjects");
            invertActions = serializedObject.FindProperty("invertActions");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Enable If Chat Connected", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will enable/disable target objects based on if the ChatBox is " +
                "connected yet or not.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(buttons, true);
            EditorGUILayout.PropertyField(gameObjects, true);
            EditorGUILayout.PropertyField(invertActions);
            #endregion

            #region Core
            EndInspectorGUI(typeof(EnableIfChatConnected));
            #endregion
        }
    }
}