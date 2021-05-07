using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackPlayerCount), true)]
    public class TrackPlayerCountInspector : BaseEditor
    {
        #region Properties
        SerializedProperty useRoomOwnerShip;
        SerializedProperty isOwner;
        SerializedProperty texts;
        SerializedProperty teamName;
        SerializedProperty executeEventAtPlayerCount;
        SerializedProperty reachPlayerCount;
        SerializedProperty fallBelowCount;
        SerializedProperty ReachedPlayerCount;
        SerializedProperty ReachedFallPlayerCount;
        SerializedProperty OnCountChanged;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            useRoomOwnerShip = serializedObject.FindProperty("useRoomOwnerShip");
            isOwner = serializedObject.FindProperty("isOwner");
            texts = serializedObject.FindProperty("texts");
            teamName = serializedObject.FindProperty("teamName");
            executeEventAtPlayerCount = serializedObject.FindProperty("executeEventAtPlayerCount");
            reachPlayerCount = serializedObject.FindProperty("reachPlayerCount");
            fallBelowCount = serializedObject.FindProperty("fallBelowCount");
            ReachedPlayerCount = serializedObject.FindProperty("ReachedPlayerCount");
            ReachedFallPlayerCount = serializedObject.FindProperty("ReachedFallPlayerCount");
            OnCountChanged = serializedObject.FindProperty("OnCountChanged");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Player Count", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will modify the targets Text components to display what the " +
                "current player count is.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            EditorGUILayout.PropertyField(teamName);
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            EditorGUILayout.PropertyField(OnCountChanged);
            GUI.skin = _skin;
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            EditorGUILayout.PropertyField(executeEventAtPlayerCount);
            if (executeEventAtPlayerCount.boolValue == true)
            {
                EditorGUILayout.PropertyField(useRoomOwnerShip);
                if (useRoomOwnerShip.boolValue == true)
                {
                    EditorGUILayout.PropertyField(isOwner);
                }
                EditorGUILayout.PropertyField(reachPlayerCount);
                EditorGUILayout.PropertyField(fallBelowCount);
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(ReachedPlayerCount);
                EditorGUILayout.PropertyField(ReachedFallPlayerCount);
                GUI.skin = _skin;
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            }
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackPlayerCount));
            #endregion
        }
    }
}