using UnityEngine;
using UnityEditor;
using CBGames.UI;
using CBGames.Editors;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(FloatingBar), true)]
    public class FloatingBarInspector : BaseEditor
    {
        #region Properties
        SerializedProperty type;
        SerializedProperty displayType;
        SerializedProperty realTimeTracking;
        SerializedProperty allTexts;
        SerializedProperty allImages;
        SerializedProperty coloredBar;
        SerializedProperty colorBarFillOffset;
        SerializedProperty fillBar;
        SerializedProperty fillDelay;
        SerializedProperty fillSpeed;
        SerializedProperty displayBarNumber;
        SerializedProperty controller;
        SerializedProperty startHidden;
        SerializedProperty displayTime;
        SerializedProperty fadeOut;
        SerializedProperty fadeSpeed;
        SerializedProperty onlyEnableForNoneOwner;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            type = serializedObject.FindProperty("type");
            displayType = serializedObject.FindProperty("displayType");
            realTimeTracking = serializedObject.FindProperty("realTimeTracking");
            allTexts = serializedObject.FindProperty("allTexts");
            allImages = serializedObject.FindProperty("allImages");
            coloredBar = serializedObject.FindProperty("coloredBar");
            colorBarFillOffset = serializedObject.FindProperty("colorBarFillOffset");
            fillBar = serializedObject.FindProperty("fillBar");
            fillDelay = serializedObject.FindProperty("fillDelay");
            fillSpeed = serializedObject.FindProperty("fillSpeed");
            displayBarNumber = serializedObject.FindProperty("displayBarNumber");
            controller = serializedObject.FindProperty("controller");
            startHidden = serializedObject.FindProperty("startHidden");
            displayTime = serializedObject.FindProperty("displayTime");
            fadeOut = serializedObject.FindProperty("fadeOut");
            fadeSpeed = serializedObject.FindProperty("fadeSpeed");
            onlyEnableForNoneOwner = serializedObject.FindProperty("onlyEnableForNoneOwner");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Floating Bar", 
                "Use to visually display the status of your tracked number in text and progress bar format.", 
                E_Core.h_floatingBarIcon
            );
            #endregion

            #region Properties
            #region Others
            EditorGUILayout.LabelField("Core Settings", _skin.textField);
            GUILayout.BeginHorizontal(_skin.box);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(displayType);
            EditorGUILayout.PropertyField(realTimeTracking);
            EditorGUILayout.PropertyField(controller);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            #endregion

            #region Contents
            EditorGUILayout.LabelField("Contents", _skin.textField);
            GUILayout.BeginHorizontal(_skin.box);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(allTexts, true);
            EditorGUILayout.PropertyField(allImages, true);
            EditorGUILayout.PropertyField(coloredBar);
            EditorGUILayout.PropertyField(fillBar);
            EditorGUILayout.PropertyField(displayBarNumber);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            #endregion

            #region Bar Settings
            EditorGUILayout.LabelField("Bar Fill Settings", _skin.textField);
            GUILayout.BeginHorizontal(_skin.box);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(colorBarFillOffset);
            EditorGUILayout.PropertyField(fillDelay);
            EditorGUILayout.PropertyField(fillSpeed);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            #endregion

            #region Display Options
            EditorGUILayout.LabelField("Display Options", _skin.textField);
            GUILayout.BeginHorizontal(_skin.box);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(startHidden);
            EditorGUILayout.PropertyField(displayTime);
            EditorGUILayout.PropertyField(fadeOut);
            EditorGUILayout.PropertyField(fadeSpeed);
            EditorGUILayout.PropertyField(onlyEnableForNoneOwner);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            #endregion
            #endregion

            #region Core
            EndInspectorGUI(typeof(FloatingBar));
            #endregion
        }
    }
}
