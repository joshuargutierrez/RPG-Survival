using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(SyncItemCollection), true)]
    public class SyncItemCollectionInspector : BaseEditor
    {
        #region Properties
        SerializedProperty syncCrossScenes;
        SerializedProperty syncCreateDestroy;
        SerializedProperty holder;
        SerializedProperty resourcesPrefab;
        SerializedProperty skipStartCheck;
        SerializedProperty onPressActionDelay;
        SerializedProperty OnPressActionInput;
        SerializedProperty onPressActionInputWithTarget;
        SerializedProperty OnSceneEnterUpdate;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            syncCrossScenes = serializedObject.FindProperty("syncCrossScenes");
            syncCreateDestroy = serializedObject.FindProperty("syncCreateDestroy");
            holder = serializedObject.FindProperty("holder");
            resourcesPrefab = serializedObject.FindProperty("resourcesPrefab");
            skipStartCheck = serializedObject.FindProperty("skipStartCheck");
            onPressActionDelay = serializedObject.FindProperty("onPressActionDelay");
            OnPressActionInput = serializedObject.FindProperty("OnPressActionInput");
            onPressActionInputWithTarget = serializedObject.FindProperty("onPressActionInputWithTarget");
            OnSceneEnterUpdate = serializedObject.FindProperty("OnSceneEnterUpdate");
            #endregion

            base.OnEnable();
        }
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            SyncItemCollection sic = (SyncItemCollection)target;
            DrawTitleBar(
                "Sync Item Collection",
                "Used to sync events from collecting this item across the network. \n\n" +
                "Required Setup Actions:\n" +
                "1. Copy the settings on the following fields from \"vItemCollection\" to this component:\n" +
                " * \"OnPressActionDelay\"\n" +
                " * \"OnPressActionInput\"\n" +
                " * \"OnPressActionInputWithTarget\"\n\n" +
                "2. On \"vItemCollection\" do the following:\n" +
                " * Set \"OnPressActionDelay\" to zero\n" +
                " * Remove all events from \"OnPressActionInput\"\n" +
                " * Remove all events from \"OnPressActionInputWithTarget\"\n",
                E_Core.h_genericIcon
            );
            #endregion

            #region Properties
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(syncCrossScenes);
            if (syncCrossScenes.boolValue == true)
            {
                EditorGUILayout.PropertyField(holder, new GUIContent("Track Position"));
                EditorGUILayout.PropertyField(syncCreateDestroy, new GUIContent("Is Dynamic Obj"));
                if (syncCreateDestroy.boolValue == true)
                {
                    EditorGUILayout.PropertyField(resourcesPrefab);
                }
            }
            EditorGUILayout.PropertyField(skipStartCheck, new GUIContent("Items In ItemCollection"));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(onPressActionDelay);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            EditorGUILayout.HelpBox("These should be copied exactly from the vItemColletion component.", MessageType.None);
            EditorGUILayout.PropertyField(OnPressActionInput);
            EditorGUILayout.PropertyField(onPressActionInputWithTarget);

            EditorGUILayout.HelpBox("This is called when this object was actived by another player " +
                "when previously in another scene. When you enter this scene this is called.", MessageType.None);
            EditorGUILayout.PropertyField(OnSceneEnterUpdate);

            GUI.skin = _skin;
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            EndInspectorGUI(typeof(SyncItemCollection));
        }
    }
}
