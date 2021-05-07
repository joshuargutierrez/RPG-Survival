using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.Objects;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(CallNetworkEvents), true)]
    public class CallNetworkEventsInspector : BaseEditor
    {
        #region Properties
        SerializedProperty syncCrossScenes;
        SerializedProperty holder;
        SerializedProperty NetworkInvoke1;
        SerializedProperty NetworkInvoke2;
        SerializedProperty NetworkInvoke3;
        SerializedProperty NetworkInvoke4;
        SerializedProperty NetworkInvoke5;
        SerializedProperty showNetworkInvoke1;
        SerializedProperty showNetworkInvoke2;
        SerializedProperty showNetworkInvoke3;
        SerializedProperty showNetworkInvoke4;
        SerializedProperty showNetworkInvoke5;

        SerializedProperty NetworkGameObjectInvoke1;
        SerializedProperty NetworkGameObjectInvoke2;
        SerializedProperty NetworkGameObjectInvoke3;
        SerializedProperty NetworkGameObjectInvoke4;
        SerializedProperty showGameObjectInvoke1;
        SerializedProperty showGameObjectInvoke2;
        SerializedProperty showGameObjectInvoke3;
        SerializedProperty showGameObjectInvoke4;

        SerializedProperty NetworkSingleInvoke1;
        SerializedProperty showSingleInvoke1;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            syncCrossScenes = serializedObject.FindProperty("syncCrossScenes");
            holder = serializedObject.FindProperty("holder");
            NetworkInvoke1 = serializedObject.FindProperty("NetworkInvoke1");
            NetworkInvoke2 = serializedObject.FindProperty("NetworkInvoke2");
            NetworkInvoke3 = serializedObject.FindProperty("NetworkInvoke3");
            NetworkInvoke4 = serializedObject.FindProperty("NetworkInvoke4");
            NetworkInvoke5 = serializedObject.FindProperty("NetworkInvoke5");
            showNetworkInvoke1 = serializedObject.FindProperty("showNetworkInvoke1");
            showNetworkInvoke2 = serializedObject.FindProperty("showNetworkInvoke2");
            showNetworkInvoke3 = serializedObject.FindProperty("showNetworkInvoke3");
            showNetworkInvoke4 = serializedObject.FindProperty("showNetworkInvoke4");
            showNetworkInvoke5 = serializedObject.FindProperty("showNetworkInvoke5");

            NetworkGameObjectInvoke1 = serializedObject.FindProperty("NetworkGameObjectInvoke1");
            NetworkGameObjectInvoke2 = serializedObject.FindProperty("NetworkGameObjectInvoke2");
            NetworkGameObjectInvoke3 = serializedObject.FindProperty("NetworkGameObjectInvoke3");
            NetworkGameObjectInvoke4 = serializedObject.FindProperty("NetworkGameObjectInvoke4");
            showGameObjectInvoke1 = serializedObject.FindProperty("showGameObjectInvoke1");
            showGameObjectInvoke2 = serializedObject.FindProperty("showGameObjectInvoke2");
            showGameObjectInvoke3 = serializedObject.FindProperty("showGameObjectInvoke3");
            showGameObjectInvoke4 = serializedObject.FindProperty("showGameObjectInvoke4");

            NetworkSingleInvoke1 = serializedObject.FindProperty("NetworkSingleInvoke1");
            showSingleInvoke1 = serializedObject.FindProperty("showSingleInvoke1");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            CallNetworkEvents nevents = (CallNetworkEvents)target;
            DrawTitleBar(
                "Call Network Events",
                "Anything inside the NetworkInvoke events will be invoked across the network. " +
                "From another component call the \"CallNetworkInvoke(#)\" function to " +
                "trigger that set of events across the network.", 
                E_Core.h_genericIcon
            );
            #endregion

            #region Properties
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(syncCrossScenes);
            if (syncCrossScenes.boolValue == true)
            {
                EditorGUILayout.PropertyField(holder, new GUIContent("Track Position"));
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            #region No Input Invokes
            EditorGUILayout.HelpBox("Events that don't take any inputs. Can be called by calling the function CallNetworkInvoke(#).", MessageType.None);
            showNetworkInvoke1.boolValue = EditorGUILayout.Foldout(showNetworkInvoke1.boolValue, "Network Invoke 1");
            if (showNetworkInvoke1.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkInvoke1);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showNetworkInvoke2.boolValue = EditorGUILayout.Foldout(showNetworkInvoke2.boolValue, "Network Invoke 2");
            if (showNetworkInvoke2.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkInvoke2);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showNetworkInvoke3.boolValue = EditorGUILayout.Foldout(showNetworkInvoke3.boolValue, "Network Invoke 3");
            if (showNetworkInvoke3.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkInvoke3);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showNetworkInvoke4.boolValue = EditorGUILayout.Foldout(showNetworkInvoke4.boolValue, "Network Invoke 4");
            if (showNetworkInvoke4.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkInvoke4);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showNetworkInvoke5.boolValue = EditorGUILayout.Foldout(showNetworkInvoke5.boolValue, "Network Invoke 5");
            if (showNetworkInvoke5.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkInvoke5);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            #endregion

            #region GameObject Invokes
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("The gameobject that is past into these events is " +
                "derived from finding the photon id from the player that originally trigger these events.", MessageType.None);
            showGameObjectInvoke1.boolValue = EditorGUILayout.Foldout(showGameObjectInvoke1.boolValue, "Network GameObject Invoke 1");
            if (showGameObjectInvoke1.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkGameObjectInvoke1);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showGameObjectInvoke2.boolValue = EditorGUILayout.Foldout(showGameObjectInvoke2.boolValue, "Network GameObject Invoke 2");
            if (showGameObjectInvoke2.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkGameObjectInvoke2);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showGameObjectInvoke3.boolValue = EditorGUILayout.Foldout(showGameObjectInvoke3.boolValue, "Network GameObject Invoke 3");
            if (showGameObjectInvoke3.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkGameObjectInvoke3);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            showGameObjectInvoke4.boolValue = EditorGUILayout.Foldout(showGameObjectInvoke4.boolValue, "Network GameObject Invoke 4");
            if (showGameObjectInvoke4.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkGameObjectInvoke4);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            #endregion

            #region Single Invokes
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Unity events that take a float as an input value. " +
                "Can be invoked over the network by calling CallFloatInvoke(#).", MessageType.None);
            showSingleInvoke1.boolValue = EditorGUILayout.Foldout(showSingleInvoke1.boolValue, "Network Float Invoke 1");
            if (showSingleInvoke1.boolValue == true)
            {
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.PropertyField(NetworkSingleInvoke1);
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                GUI.skin = _skin;
            }
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            #endregion

            #region End Core
            EndInspectorGUI(typeof(CallNetworkEvents));
            #endregion
        }
    }
}