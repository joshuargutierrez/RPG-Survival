using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class BaseEditor : Editor
    {
        protected GUISkin _skin = null;
        protected GUISkin _original = null;
        protected Color _titleColor;
        protected CBColorHolder _originalHolder;
        protected CBColorHolder _originalFoldout;
        protected CBColorHolder _skinHolder;
        protected Rect rect;
        protected virtual void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);
            _titleColor = E_Colors.e_c_blue_5;
            _skinHolder = new CBColorHolder(_skin.label);
        }
        public override void OnInspectorGUI()
        {
            if (_originalHolder == null) _originalHolder = new CBColorHolder(EditorStyles.label);
            if (_originalFoldout == null) _originalFoldout = new CBColorHolder(EditorStyles.foldout);
            _original = GUI.skin;
            GUI.skin = _skin;
            serializedObject.Update();
            rect = GUILayoutUtility.GetRect(1, 1);

            GUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
        }

        protected virtual void EndInspectorGUI(System.Type serializedType)
        {
            DrawPropertiesExcluding(serializedObject, E_Helpers.EditorGetVariables(serializedType));
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            serializedObject.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        protected virtual void DrawTitleBar(string title, string helpbox, string texturePath=null)
        {
            EditorGUI.DrawRect(new Rect(rect.x + 5, rect.y + 10, rect.width - 10, 40), _titleColor);
            if (!string.IsNullOrEmpty(texturePath))
            {
                GUI.DrawTexture(new Rect(rect.x + 10, rect.y + 15, 30, 30), E_Helpers.LoadTexture(texturePath, new Vector2(256, 256)));
            }
            GUILayout.Space(5);
            GUILayout.Label(title, _skin.label);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(helpbox, MessageType.Info);
        }

    }
}