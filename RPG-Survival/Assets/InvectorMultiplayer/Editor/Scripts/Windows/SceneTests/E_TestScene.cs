using UnityEngine;
using UnityEditor;
using marijnz.EditorCoroutines;
using System.Collections;
using System.Collections.Generic;

namespace CBGames.Editors
{
    public class DebugFormat
    {
        public string message;
        public UnityEngine.Object context;
        public DebugFormat(string Message, UnityEngine.Object Context)
        {
            message = Message;
            context = Context;
        }
    }

    public partial class E_TestScene : EditorWindow
    {
        
        #region Editor Variables
        GUISkin _skin = null;
        bool _runningTests = false;
        Color _lockColor;
        Color _convertBar;
        bool _ranTests = false;
        bool _autoFixTests = false;
        #endregion

        #region Partial Methods

        #region Shooter
        partial void SHOOTER_CheckInventoryTimeScale(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_CheckNestedAimCanvas(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformvShooterManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformItemCollectionTests(ref int passing, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformvShooterWeaponPrefabTests(ref int passing, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformvThrowCollectable(ref int passing, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformvLockOnShooterTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformItemListDataTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void SHOOTER_PerformvShooterMeleeInputTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        #endregion

        #region AI
        partial void AI_GenericSyncTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void AI_ShooterManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void AI_ControlAIShooterTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void AI_MPAITests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        partial void AI_AIHeadTrackTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        #endregion

        #region Invector Modified Files
        partial void CORE_PerformMPHeadTrackTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings);
        #endregion
        #endregion

        #region Visual Window
        [MenuItem("CB Games/Testing/Perform Scene Tests", false, 0)]
        public static void CB_TestScene()
        {
            EditorWindow window = GetWindow<E_TestScene>(true);
            window.maxSize = new Vector2(500, 340);
            window.minSize = window.maxSize;
        }
        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);
                        
            //Make window title
            this.titleContent = new GUIContent("Perform Scene Tests", null, "Perform a series of automated tests on this scene to make sure its ready for multiplayer.");
        }
        private void OnGUI()
        {
            _lockColor = new Color32(158, 158, 158, 200);
            _convertBar = new Color32(139, 0, 0, 255);// new Color32(95, 165, 245, 255);

            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 40), E_Colors.e_c_blue_4);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Perform Scene Tests", _skin.label);
            EditorGUILayout.Space();

            //Draw Body
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            //Draw Helpful Text
            EditorGUILayout.LabelField("This will scan your scene and find converted components and check them. " +
                "Then it will check converted prefabs in your \"Assets/Resources\" and \"MP_Converted\" folders. After performing QA tests on " +
                "all of these components it will output and show you how many tests were run and out of those " +
                "how many are warnings and errors. Errors should be fixed immediately, and warnings could be optional. " +
                "Each warning & error will give a description of how to fix the target object/component. Select the " +
                "warning/error and it will highlight the target object with the issue (in your folder or scene). This " +
                "does not make any changes to any found objects unless you select to attempt to auto fix any failed errors. " +
                "Auto fixing isn't perfect and it may fix things in a way that you don't want. You can enable this feature " +
                "if you would like or not.", _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (_ranTests == true)
            {
                EditorGUILayout.BeginHorizontal(_skin.window, GUILayout.Height(40));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Finished running all tests. Look in the console for the test results.", _skin.textField);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            GUIStyle tmpSyle = new GUIStyle(_skin.label);
            tmpSyle.normal.textColor = (_autoFixTests) ? Color.green : Color.red;
            EditorGUI.LabelField(new Rect(330, 260, 80, 30), (_autoFixTests) ? "TRUE" : "FALSE", tmpSyle);
            if (GUI.Button(new Rect(80, 260, 230, 30), "Attempt To Auto Fix Failed Tests", _skin.GetStyle("Button")))
            {
                _autoFixTests = !_autoFixTests;
            }
            //Draw test button
            if (GUI.Button(new Rect(30, 300, 430, 30), "Perform Tests Now!", _skin.GetStyle("Button")))
            {
                if (_runningTests == false)
                {
                    this.StartCoroutine(PerformTests());
                }
            }
            if (_runningTests == true)
            {
                EditorGUI.DrawRect(new Rect(0, 50, position.width, position.height - 30), _lockColor);
                EditorGUI.DrawRect(new Rect(0, position.height / 2 - 50, position.width, 100), _convertBar);
                EditorGUI.LabelField(new Rect(0, position.height / 2 - 80, position.width, 100), "Potential Error Occured", _skin.GetStyle("Label"));
                EditorGUI.LabelField(new Rect(10, position.height / 2 - 10, position.width, 100), 
                    "If this window stays like this longer than 2 seconds that means there was an error during the testing process. " +
                    "This is because of a coding issue. Please send the error in the console to the \"submit-bugs\" \n" +
                    "channel on discord to have the package creater fix this.", _skin.GetStyle("TextField"));
            }
        }
        #endregion

        string[] GetResourcePrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (s.Contains(".prefab"))
                {
                    if (s.Contains("Assets/Resources") || s.Contains("Assets/MP_Converted"))
                    {
                        result.Add(s);
                    }
                }
            }
            return result.ToArray();
        }

        #region Running Order / Output Logic
        IEnumerator PerformTests()
        {
            _runningTests = true;

            int failed = 0;
            int passing = 0;
            List<DebugFormat> failures = new List<DebugFormat>();
            List<DebugFormat> warnings = new List<DebugFormat>();

            //Check for enabled packages
            SHOOTER_CheckAddonEnabled();

            //Run Tests
            CORE_PerformPreviewCamTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformNetworkManagerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformSyncPlayerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_SpawnPointTesting(ref passing, ref failed, ref failures, ref warnings);
            CORE_DisablePlayersTest(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformThirdPersonControllerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformMeleeCombatInputTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformGenericActionTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformLadderActionTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformItemManagerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformMeleeManagerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformHealthControllerTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformBreakableObjectTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformItemCollectionTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformPlayerNameBarTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformSyncObjectTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_MissingRequiredComponents(ref passing, ref failed, ref failures, ref warnings);
            CORE_SceneInBuildScenes(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformChatBoxTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformSceneTransitionTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformVoiceChatTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformCallNetworkEventsTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformPlayerRespawnTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformPlayerListUITests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformUICoreLogicTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformvInventoryTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformSetLoadingScreenTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformPhotonViewTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformItemListDataTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformMPHeadTrackTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_PerformNetworkCullingTests(ref passing, ref failed, ref failures, ref warnings);
            CORE_CheckNestedCamera(ref passing, ref failed, ref failures, ref warnings);
            CORE_CheckNestedUI(ref passing, ref failed, ref failures, ref warnings);

            SHOOTER_PerformItemCollectionTests(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_PerformvShooterWeaponPrefabTests(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_PerformvThrowCollectable(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_PerformItemListDataTests(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_PerformvShooterManagerTests(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_PerformvShooterMeleeInputTests(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_CheckNestedAimCanvas(ref passing, ref failed, ref failures, ref warnings);
            SHOOTER_CheckInventoryTimeScale(ref passing, ref failed, ref failures, ref warnings);

            AI_GenericSyncTests(ref passing, ref failed, ref failures, ref warnings);
            AI_ShooterManagerTests(ref passing, ref failed, ref failures, ref warnings);
            AI_ControlAIShooterTests(ref passing, ref failed, ref failures, ref warnings);
            AI_MPAITests(ref passing, ref failed, ref failures, ref warnings);
            AI_AIHeadTrackTests(ref passing, ref failed, ref failures, ref warnings);

            //Display Results
            for (int i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning(warnings[i].message, warnings[i].context);
            }
            for(int i = 0; i < failures.Count; i++)
            {
                Debug.LogError(failures[i].message, failures[i].context);
            }
            Debug.Log("PASSED: " + passing + " FAILED: " + failed);

            _runningTests = false;
            _ranTests = true;
            yield return null;
        }
        #endregion
    }
}
