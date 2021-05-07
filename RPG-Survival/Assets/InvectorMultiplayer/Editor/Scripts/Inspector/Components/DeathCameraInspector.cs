using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.UI;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(DeathCamera), true)]
    public class DeathCameraInspector : BaseEditor
    {
        #region Core Variables
        GUIContent eventContent;
        #endregion

        #region Properties
        SerializedProperty keyToSwitchPrevious;
        SerializedProperty keyToSwitchNext;
        SerializedProperty deathVisual;
        SerializedProperty OnEnableSwitching;
        SerializedProperty OnDisableSwitching;
        #endregion

        List<string> availableInputs = new List<string>();
        int prevInt;
        int nextInt;

        protected override void OnEnable()
        {
            #region Properties
            keyToSwitchPrevious = serializedObject.FindProperty("keyToSwitchPrevious");
            keyToSwitchNext = serializedObject.FindProperty("keyToSwitchNext");
            deathVisual = serializedObject.FindProperty("deathVisual");
            OnEnableSwitching = serializedObject.FindProperty("OnEnableSwitching");
            OnDisableSwitching = serializedObject.FindProperty("OnDisableSwitching");
            #endregion

            availableInputs = E_Helpers.GetAllInputAxis();
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Death Camera", 
                "Call \"EnableSwitching\" function to turn on/off " +
                "the functionality to allow the player to switch between other players. " +
                "A helper function \"DeadEnableDeathCamera\" function is availabe in the \"SyncPlayer\" " +
                "component that can be tied to the \"vThirdPersonController\" component.\n\n" +
                "vThirdPersonController (OnDead) -> SyncPlayer(DeadEnableDeathCamera) -> DeathCamera(EnableSwitching)", 
                E_Core.h_deathCameraIcon
            );
            #endregion

            #region Properties
            int pre_index = availableInputs.IndexOf(keyToSwitchPrevious.stringValue);
            if (pre_index == -1)
            {
                EditorGUILayout.PropertyField(keyToSwitchPrevious, new GUIContent("Previous Player Key"));
                EditorGUILayout.HelpBox("This key doesn't exist in your project. Be sure to add it before you final build!", MessageType.Error);
            }
            else
            {
                prevInt = pre_index;
                prevInt = EditorGUILayout.Popup("Previous Player Key", prevInt, availableInputs.ToArray());
                keyToSwitchPrevious.stringValue = availableInputs[prevInt];
            }

            int next_index = availableInputs.IndexOf(keyToSwitchNext.stringValue);
            if (next_index == -1)
            {
                EditorGUILayout.PropertyField(keyToSwitchNext, new GUIContent("Next Player Key"));
                EditorGUILayout.HelpBox("This key doesn't exist in your project. Be sure to add it before you final build!", MessageType.Error);
            }
            else
            {
                nextInt = next_index;
                nextInt = EditorGUILayout.Popup("Next Player Key", nextInt, availableInputs.ToArray());
                keyToSwitchNext.stringValue = availableInputs[nextInt];
            }
            EditorGUILayout.PropertyField(deathVisual);
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            EditorGUILayout.PropertyField(OnEnableSwitching);
            EditorGUILayout.PropertyField(OnDisableSwitching);
            GUI.skin = _skin;
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            #endregion

            #region Core
            EndInspectorGUI(typeof(DeathCamera));
            #endregion
        }
    }
}