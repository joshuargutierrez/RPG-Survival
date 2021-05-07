using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using marijnz.EditorCoroutines;
using Invector.vShooter;
using Invector.vItemManager;
using Invector;

namespace CBGames.Editors
{
    public partial class E_ConvertPrefabs : EditorWindow
    {
        #region Editor Variables
        List<string> foundPaths = new List<string>();
        Vector2 _scrollPos = Vector2.zero;
        GUISkin _skin = null;
        GameObject _target;
        Editor _objectPreview;
        float _scrollHeight = 0.0f;
        bool _scanning = false;
        bool _scanned = false;
        bool _converting = false;
        bool _converted = false;
        int _count = 0;

        bool _cItemCollection = true;
        bool _cvBreakableObjects = true;
        bool _cvHealthController = true;
        bool _cvShooterWeapons = true;
        #endregion

        [MenuItem("CB Games/Convert/Prefabs", false, 0)]
        public static void CB_ConvertPrefabs()
        {
            EditorWindow window = GetWindow<E_ConvertPrefabs>(true);
            window.maxSize = new Vector2(500, 550);
            window.minSize = window.maxSize;
        }

        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Make window title
            this.titleContent = new GUIContent("Convert Project Prefabs", null, "Converts all the prefabs in your project to support multiplayer.");
        }

        private void OnGUI()
        {
            CBColorHolder _org = new CBColorHolder(EditorStyles.label);
            CBColorHolder _orgFoldout = new CBColorHolder(EditorStyles.foldout);
            CBColorHolder _skinHolder = new CBColorHolder(_skin.label);

            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 40), E_Colors.e_c_blue_4);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Convert Prefabs To Multiplayer", _skin.label);
            EditorGUILayout.Space();

            //Draw Body
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            //Apply Body Title/Description
            EditorGUILayout.LabelField("This will find all prefabs in your project. " +
                "Then it will scan each one and give you a list of prefabs that could be converted " +
                "to support multiplayer. You can click the 'X' button to the left and it will remove the prefab " +
                "from the convert list. You can also click on the name of each prefab and get a preview of " +
                "the object and what will be converted on it. Converted items are saved in \"Assets/Resources\"." +
                "So they can be PhotonInstantiated across the network when needed.\n\n" +
                "WARNING NOTE: If your project is large this could take considerable time. It is suggested to right " +
                "click on each prefab you want to convert and selecting the appropriate action from the CB Games menu " +
                "item.", _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_converted == true)
            {
                EditorGUILayout.LabelField(string.Format("Successfully copied and converted all selected prefabs.\n" +
                    "View converted prefabs at one of the following locations: \n" +
                    "\n\n" + 
                    "\"Assets{0}Resources\"\n" +
                    "\"Assets{0}MP_Converted\"\n" +
                    "\n\n" +
                    "NOTE: The original prefabs remain unmodified.", Path.DirectorySeparatorChar), _skin.GetStyle("Label"));
                return;
            }
            if (_scanned == false && _scanning == false)
            {
                EditorGUILayout.BeginVertical(_skin.box, GUILayout.Width(100));
                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                _cvBreakableObjects = EditorGUILayout.ToggleLeft("Show vBreakableObjects", _cvBreakableObjects, GUILayout.Width(150));
                _cvShooterWeapons = EditorGUILayout.ToggleLeft("Show vShooterWeapons", _cvShooterWeapons, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                _cItemCollection = EditorGUILayout.ToggleLeft("Show vItemCollections", _cItemCollection, GUILayout.Width(150));
                _cvHealthController = EditorGUILayout.ToggleLeft("Show vHealthControllers", _cvHealthController, GUILayout.Width(163));
                CBEditor.SetColorToEditorStyle(_org, _orgFoldout);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Scan Project", _skin.button, GUILayout.Height(40), GUILayout.Width(300)) && _scanning == false)
                {
                    this.StartCoroutine(ScanProject());
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            if (_scanning == true)
            {
                EditorGUI.DrawRect(new Rect(10, 160, position.width - 20, 40), E_Colors.e_c_blue_2);
                EditorGUI.LabelField(new Rect(100, 160, 300, 50), "Scanning, please wait...\nImportant Note: You can click into this window to refresh it.", _skin.textField);
            }
            if (_scanned == true && CB_prefabs.Count > 0)
            {
                _scrollPos = GUI.BeginScrollView(new Rect(15, 210, 300, position.height - 260), _scrollPos, new Rect(0, 0, 250, _scrollHeight), false, true);
                _count = 0;
                foreach (KeyValuePair<GameObject, string> prefab in CB_prefabs)
                {
                    if (GUI.Button(new Rect(5, _count * 39, 30, 35), "X", _skin.button))
                    {
                        CB_prefabs.Remove(prefab.Key);
                        break;
                    }
                    if (GUI.Button(new Rect(40, _count * 39, 220, 35), prefab.Key.name, _skin.button))
                    {
                        ViewSelectedObject(prefab.Key);
                        SetConvertables(prefab.Key);
                    }
                    _count += 1;
                }
                GUI.EndScrollView();
                if (GUI.Button(new Rect(10, position.height-40, 235, 30), "Convert Found Prefabs", _skin.button) && _scanning == false)
                {
                    ConvertPrefabsToMultiplayer();
                }
                if (GUI.Button(new Rect(255, position.height - 40, 235, 30), "Rescan Project", _skin.button) && _scanning == false)
                {
                    _scanning = false;
                    _scanned = false;
                    CB_prefabs.Clear();
                    _target = null;
                }
            }

            if (_target)
            {
                _objectPreview.OnInteractivePreviewGUI(new Rect(position.width - 170, 207, 150, 150), "window");
                EditorGUI.DrawRect(new Rect(position.width - 170, 350, 150, 150), E_Colors.e_c_blue_4);
                for (int i = 0; i < CB_previewConverts.Count; i++)
                {
                    EditorGUI.LabelField(new Rect(position.width - 170, (350+(i * 30)), 150, 60), CB_previewConverts[i], _skin.textField);
                }
            }
            if (_converting == true)
            {
                EditorGUI.DrawRect(new Rect(0, 50, position.width, position.height - 30), E_Colors.e_c_blue_1);
                EditorGUI.DrawRect(new Rect(0, position.height / 2 - 50, position.width, 100), E_Colors.e_c_blue_3);
                EditorGUI.LabelField(new Rect(0, position.height / 2 - 50, position.width, 100), "Converting Prefabs, please wait...", _skin.label);
                EditorGUI.LabelField(new Rect(30, position.height / 2 + 10, position.width, 100), "If this window is just staying here longer than 2 seconds, then an internal error occured.", _skin.GetStyle("TextField"));
            }
        }

        void SetConvertables(GameObject target)
        {
            CB_previewConverts.Clear();
            CB_CHECK_CORE(target);
            CB_SHOOTER_CheckFromComps(target);
        }

        IEnumerator ScanProject()
        {
            _scanned = false;
            _scanning = true;
            foundPaths.Clear();
            CB_prefabs.Clear();
            string[] prefabPaths = E_Helpers.GetAllPrefabs(true, false);
            GameObject target;

            foreach (string prefabPath in prefabPaths)
            {
                Object prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath);
                try
                {
                    target = (GameObject)prefab;
                    CB_CHECK_CORE_HasComp(target, _cItemCollection, _cvBreakableObjects, _cvHealthController, prefabPath);
                    CB_SHOOTER_HasShooterComp(target, _cvShooterWeapons, prefabPath);
                }
                catch
                {
                    Debug.LogWarning("Unable to cast: " + prefabPath + " to GameObject, skipping");
                }
            }
            _scrollHeight = CB_prefabs.Count * 39;
            _scanning = false;
            _scanned = true;
            yield return null;
        }

        private void ViewSelectedObject(GameObject obj)
        {
            Selection.activeObject = obj;
            _target = obj;
            _objectPreview = Editor.CreateEditor(_target);
        }

        void ConvertPrefabsToMultiplayer()
        {
            _converted = false;
            _converting = true;
            string saveLocation = string.Format("Assets{0}Resources{0}", Path.DirectorySeparatorChar);
            E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(saveLocation));
            GameObject _copiedPrefab = null;
            foreach (KeyValuePair<GameObject, string> prefab in CB_prefabs)
            {
                _copiedPrefab = GameObject.Instantiate(prefab.Key, Vector3.zero, Quaternion.identity) as GameObject;
                _copiedPrefab.name = "MP_"+_copiedPrefab.name.Replace("(Clone)", "");
                saveLocation = string.Format("Assets{0}Resources{0}{1}.prefab", Path.DirectorySeparatorChar, _copiedPrefab.name);
                CB_COMP_vItemCollection(_copiedPrefab);
                CB_COMP_vBreakableObject(_copiedPrefab);
                CB_COMP_vHealthController(_copiedPrefab);

                CB_COMP_vProjectileControle(_copiedPrefab);
                CB_COMP_vShooterWeapon(_copiedPrefab);

                // Change path if needed
                CB_PATH_Check_Core(_copiedPrefab, ref saveLocation);
                CB_PATH_Check_Shooter(_copiedPrefab, ref saveLocation);
                
                if (E_Helpers.FileExists(saveLocation))
                {
                    if (EditorUtility.DisplayDialog("Duplicate Detected!",
                        string.Format("Prefab \"{0}\" already exists, would you like to overwrite it or not copy it?", _copiedPrefab.name),
                        "Overwrite", "Not Copy"))
                    {
                        //Overwrite
                        E_Helpers.DeleteFile(saveLocation);
                        PrefabUtility.SaveAsPrefabAsset(_copiedPrefab, saveLocation);
                    }
                    else
                    {
                        Debug.Log("Skipped: " + _copiedPrefab.name);
                    }
                }
                else
                {
                    //Doesn't exist create it
                    saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                    PrefabUtility.SaveAsPrefabAsset(_copiedPrefab, saveLocation);
                }
                DestroyImmediate(_copiedPrefab);
            }
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            _converting = false;
            _converted = true;
            Debug.Log("Finished converting CB_prefabs");
        }

    }
}
