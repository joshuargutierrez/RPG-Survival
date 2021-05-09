using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_MainMenu : EditorWindow
    {
        #region Editor Variables
        GUISkin _skin = null;
        #endregion

        [MenuItem("CB Games/Main Menu", false, 200)]
        private static void CB_MainMenu()
        {
            EditorWindow window = GetWindow<E_MainMenu>(true);
            window.maxSize = new Vector2(500, 420);
            window.minSize = window.maxSize;
        }
        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);
            
            //Make window title
            this.titleContent = new GUIContent("Main Menu", null, "Steps to setup multiplayer support.");
        }
        private void OnGUI()
        {
            //Apply the gui skin
            GUI.skin = _skin;

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Invector Multiplayer - Main Menu", _skin.label);
            EditorGUILayout.Space();

            //Apply Body Title/Description
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("Select An Action", _skin.label);
            EditorGUILayout.LabelField("All of these options open additional windows with additional " +
                "options, except step 1. This is just a helpful way to know in what the recommend order " +
                "to run things are.", _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Add option buttons
            // --- Convert Invector Scripts
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 145, 270, 30), "1. Convert Invector Scripts", _skin.button))
            {
                E_ModifyInvector.CB_ModifyInvectorFiles();

            }
            EditorGUI.LabelField(new Rect(285, 140, 200, 50), "Converts invector scripts to work correctly with this package.", _skin.textField);
            EditorGUILayout.EndHorizontal();

            // --- Convert Scripts
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 195, 270, 30), "2. Add Core Objects", _skin.button))
            {
                //Open convert invector window
                E_CoreObjects.CB_CoreObjects();
                
            }
            EditorGUI.LabelField(new Rect(285, 190, 200, 50), "Adds essential gameobjects to your scene to make multiplayer work.", _skin.textField);
            EditorGUILayout.EndHorizontal();

            // --- Convert Player
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 235, 270, 30), "3. Convert Player", _skin.button))
            {
                //Open convert player window
                E_ConvertPlayer.CB_ConvertPlayerEditorWindow();
            }
            EditorGUI.LabelField(new Rect(285, 230, 210, 60), "Converts the selected player to be multiplayer compatible.", _skin.textField);
            EditorGUILayout.EndHorizontal();

            // --- Convert Scene Objects
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 275, 270, 30), "4. Convert Scene", _skin.button))
            {
                //Open convert player window
                E_ConvertScene.CB_ConvertScene();
            }
            EditorGUI.LabelField(new Rect(285, 270, 210, 70), "Scan scene for objects and lets you convert them to be multiplayer compatible.", _skin.textField);
            EditorGUILayout.EndHorizontal();

            // --- Convert Prefabs
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 315, 270, 30), "5. Convert Prefabs", _skin.button))
            {
                //Open convert player window
                E_ConvertPrefabs.CB_ConvertPrefabs();
            }
            EditorGUI.LabelField(new Rect(285, 310, 210, 70), "Scans your entire project for all invector prefabs and attempts to convert to be multiplayer compatible.", _skin.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUI.LabelField(new Rect(10, 350, 480, 50), "-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-", _skin.textField);
            // --- Test Scene
            EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(15, 370, 270, 30), "6. Perform Scene Tests", _skin.button))
            {
                //Open convert player window
                E_TestScene.CB_TestScene();
            }
            EditorGUI.LabelField(new Rect(285, 370, 210, 70), "(Optional) Runs a series of automated tests to tell you what is missing/in error.", _skin.textField);
            EditorGUILayout.EndHorizontal();
        }
    }
}