using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(NetworkBreakObject), true)]
    public class NetworkBreakObjectInspector : BaseEditor
    {
        #region Properties
        SerializedProperty syncCrossScenes;
        SerializedProperty holder;
        SerializedProperty dropPrefab;
        SerializedProperty prefabName;
        SerializedProperty dropPoint;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            syncCrossScenes = serializedObject.FindProperty("syncCrossScenes");
            holder = serializedObject.FindProperty("holder");

            dropPrefab = serializedObject.FindProperty("dropPrefab");
            prefabName = serializedObject.FindProperty("prefabName");
            dropPoint = serializedObject.FindProperty("dropPoint");
            #endregion
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            NetworkBreakObject sp = (NetworkBreakObject)target;
            DrawTitleBar(
                "Network Break Object", 
                "Component that offers an RPC call \"BreakObject\" that will sync " +
                "the break object action across the network using the attached " +
                "PhotonView. On your \"vBreakableObject\" event, add the \"NetworkBreakObject.BreakObject\" " +
                "to trigger breaking this object across the network.",
                E_Core.h_genericIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(syncCrossScenes);
            if (syncCrossScenes.boolValue == true)
            {
                EditorGUILayout.PropertyField(holder);
            }
            EditorGUILayout.PropertyField(dropPrefab);
            if (dropPrefab.boolValue == true)
            {
                EditorGUILayout.PropertyField(prefabName);
                EditorGUILayout.PropertyField(dropPoint);
            }
            #endregion

            EndInspectorGUI(typeof(NetworkBreakObject));
        }
    }
}