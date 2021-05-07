using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class CBColorHolder
    {
        public Color n_c = new Color();
        public Color h_c = new Color();
        public Color a_c = new Color();
        public Color f_c = new Color();
        public Color on_c = new Color();
        public Color oh_c = new Color();
        public Color oa_c = new Color();
        public Color of_c = new Color();

        public CBColorHolder(GUIStyle inputStyle)
        {
            n_c = inputStyle.normal.textColor;
            h_c = inputStyle.hover.textColor;
            a_c = inputStyle.active.textColor;
            f_c = inputStyle.focused.textColor;
            on_c = inputStyle.onNormal.textColor;
            oh_c = inputStyle.onHover.textColor;
            oa_c = inputStyle.onActive.textColor;
            of_c = inputStyle.onFocused.textColor;
        }
    }
    public class CBEditor : Editor
    {
        public static void PropertyField(SerializedProperty prop, GUIStyle style, GUIContent content=null, bool includeChildren = false, params GUILayoutOption[] options)
        {
            Color org = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = style.normal.textColor;
            EditorGUILayout.PropertyField(prop, content, includeChildren, options);
            EditorStyles.label.normal.textColor = org;
        }

        public static void IntSlider(SerializedProperty value, int leftValue, int rightValue, GUIStyle style, GUIContent content = null, params GUILayoutOption[] options)
        {
            Color org = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = style.normal.textColor;
            EditorGUILayout.IntSlider(value, leftValue, rightValue, content, options);
            EditorStyles.label.normal.textColor = org;
        }

        public static void SetColorToEditorStyle(CBColorHolder label, CBColorHolder foldout)
        {
            EditorStyles.label.normal.textColor = label.n_c;
            EditorStyles.label.hover.textColor = label.h_c;
            EditorStyles.label.active.textColor = label.a_c;
            EditorStyles.label.focused.textColor = label.f_c;
            EditorStyles.label.onNormal.textColor = label.on_c;
            EditorStyles.label.onHover.textColor = label.oh_c;
            EditorStyles.label.onActive.textColor = label.oa_c;
            EditorStyles.label.onFocused.textColor = label.of_c;

            EditorStyles.foldout.normal.textColor = foldout.n_c;
            EditorStyles.foldout.hover.textColor = foldout.h_c;
            EditorStyles.foldout.active.textColor = foldout.a_c;
            EditorStyles.foldout.focused.textColor = foldout.f_c;
            EditorStyles.foldout.onNormal.textColor = foldout.on_c;
            EditorStyles.foldout.onHover.textColor = foldout.oh_c;
            EditorStyles.foldout.onActive.textColor = foldout.oa_c;
            EditorStyles.foldout.onFocused.textColor = foldout.of_c;
        }

        public static GUIStyle GetEditorStyle(GUIStyle _skin, GUIStyle editorStyle)
        {
            GUIStyle foldoutStyle = new GUIStyle(editorStyle);
            foldoutStyle.normal.textColor = _skin.normal.textColor;
            foldoutStyle.onNormal.textColor = _skin.normal.textColor;
            foldoutStyle.hover.textColor = _skin.normal.textColor;
            foldoutStyle.onHover.textColor = _skin.normal.textColor;
            foldoutStyle.focused.textColor = _skin.normal.textColor;
            foldoutStyle.onFocused.textColor = _skin.normal.textColor;
            foldoutStyle.active.textColor = _skin.normal.textColor;
            foldoutStyle.onActive.textColor = _skin.normal.textColor;

            return foldoutStyle;
        }
    }
}