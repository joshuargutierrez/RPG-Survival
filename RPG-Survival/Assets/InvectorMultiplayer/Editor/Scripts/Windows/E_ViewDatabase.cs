using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CBGames.Core;

namespace CBGames.Editors
{
    public class E_ViewDatabase : EditorWindow
    {
        #region View Database
        GUISkin _skin = null;
        Vector2 _scrollPos = Vector2.zero;
        List<DatabaseScene> databaseScenes = new List<DatabaseScene>();

        [MenuItem("CB Games/Scene Transition Manager/View Scenes Database", false, 0)]
        public static void CB_Scene_DisplayDatabase()
        {
            EditorWindow window = GetWindow<E_ViewDatabase>(true);
            window.maxSize = new Vector2(500, 550);
            window.minSize = window.maxSize;
        }

        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Make window title
            this.titleContent = new GUIContent("Scenes Database", null, "List Of All Transition Scenes/Points.");

            SceneDatabase database = (SceneDatabase)AssetDatabase.LoadAssetAtPath("Assets/Resources/ScenesDatabase/ScenesDatabase.asset", typeof(SceneDatabase));
            databaseScenes.Clear();
            if (database)
            {
                databaseScenes = database.storedScenesData;
            }
        }
        private void OnGUI()
        {
            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 40), E_Colors.e_c_blue_4);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenes Database", _skin.label);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("Here is a list of currently available scenes to travel to " +
                "and all of the points within those scenes that can be used as transition spawn points.", _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("This will show you everything that is currently in the database. If something is missing run the \"CB Games/Scene Manager/Update Scene Database\" menu item to update the database.", _skin.GetStyle("TextField"));
            EditorGUILayout.Space();

            //Scroll View Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene Name", _skin.textField, GUILayout.MaxWidth(150), GUILayout.MinWidth(150));
            EditorGUILayout.LabelField("|", _skin.textField);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Entrance Points", _skin.textField);
            EditorGUILayout.EndHorizontal();

            //Scroll View
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (DatabaseScene scene in databaseScenes)
            {
                EditorGUILayout.BeginHorizontal(_skin.window);
                EditorGUILayout.LabelField(scene.index.ToString(), _skin.textField, GUILayout.MaxWidth(22), GUILayout.MinWidth(22));
                EditorGUILayout.LabelField(scene.sceneName, _skin.textField, GUILayout.MaxWidth(150), GUILayout.MinWidth(150));
                EditorGUILayout.LabelField("|", _skin.textField);
                GUILayout.FlexibleSpace();
                if (scene.entrancePoints != null && scene.entrancePoints.Count > 0)
                {
                    EditorGUILayout.BeginVertical();
                    foreach (string point in scene.entrancePoints)
                    {
                        EditorGUILayout.LabelField(point, _skin.textArea);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}