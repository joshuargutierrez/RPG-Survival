using UnityEditor;
using UnityEngine;
using CBGames.Core;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(SyncObject), true)]
    public class SyncObjectInspector : BaseEditor
    {
        #region Properties
        SerializedProperty view;
        SerializedProperty syncEnable;
        SerializedProperty syncDisable;
        SerializedProperty syncDestroy;
        SerializedProperty syncImmediateChildren;
        SerializedProperty isLeftHanded;
        SerializedProperty isWeaponHolder;
        SerializedProperty debugging;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            view = serializedObject.FindProperty("view");
            syncEnable = serializedObject.FindProperty("syncEnable");
            syncDisable = serializedObject.FindProperty("syncDisable");
            syncDestroy = serializedObject.FindProperty("syncDestroy");
            syncImmediateChildren = serializedObject.FindProperty("syncImmediateChildren");
            isLeftHanded = serializedObject.FindProperty("isLeftHanded");
            isWeaponHolder = serializedObject.FindProperty("isWeaponHolder");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            base.OnEnable();
        }
        public override void OnInspectorGUI()
        {
            #region Core
            // Core Requirements
            base.OnInspectorGUI();
            SyncObject nm = (SyncObject)target;
            DrawTitleBar(
                "Network Sync Object",
                "Component used to sync actions that happen to this object across the network. " +
                "Objects must be instantiated with this component, cannot be added a runtime. " +
                "This is generally used for player equipment.", 
                E_Core.h_genericIcon
            );
            #endregion

            #region Properties
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.Label("PhotonView For RPC Calls (Optional)", _skin.customStyles[0]);
            EditorGUILayout.PropertyField(view);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUILayout.Label("Sync Options", _skin.customStyles[0]);
            EditorGUILayout.PropertyField(syncEnable);
            EditorGUILayout.PropertyField(syncDisable);
            EditorGUILayout.PropertyField(syncDestroy);
            EditorGUILayout.PropertyField(syncImmediateChildren);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(5);

            if (syncImmediateChildren.boolValue == true)
            {
                GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
                GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                GUILayout.Label("Instantiation Options", _skin.customStyles[0]);
                EditorGUILayout.PropertyField(isLeftHanded);
                EditorGUILayout.PropertyField(isWeaponHolder);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal(_skin.window, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(debugging);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            EndInspectorGUI(typeof(SyncObject));
        }
    }
}