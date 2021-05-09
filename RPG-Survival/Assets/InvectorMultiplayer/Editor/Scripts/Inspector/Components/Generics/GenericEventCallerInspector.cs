using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(GenericEventCaller), true)]
    public class GenericEventCallerInspector : BaseEditor
    {
        #region Properties
        SerializedProperty onAwake;
        SerializedProperty onStart;
        SerializedProperty onEnable;
        SerializedProperty onDisable;
        SerializedProperty EventsToCall;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            onAwake = serializedObject.FindProperty("onAwake");
            onStart = serializedObject.FindProperty("onStart");
            onEnable = serializedObject.FindProperty("onEnable");
            onDisable = serializedObject.FindProperty("onDisable");
            EventsToCall = serializedObject.FindProperty("EventsToCall");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Generic Event Caller",
                "This component will simply execute this UnityEvent on the given events.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(onAwake);
            EditorGUILayout.PropertyField(onStart);
            EditorGUILayout.PropertyField(onEnable);
            EditorGUILayout.PropertyField(onDisable);
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            EditorGUILayout.PropertyField(EventsToCall);
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            GUI.skin = _skin;
            #endregion

            #region Core
            EndInspectorGUI(typeof(GenericEventCaller));
            #endregion
        }
    }
}