using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using marijnz.EditorCoroutines;
using System;
using CBGames.Core;
using System.IO;


namespace CBGames.Editors
{
    public partial class E_ConvertPlayer : EditorWindow
    {
        #region Editor Variables
        List<string> convertLog = new List<string>();
        List<GameObject> _players = new List<GameObject>();
        Dictionary<string, Component> foundComps = new Dictionary<string, Component>();
        List<string> options = new List<string>();
        Vector2 _scrollPos;
        Vector2 _logScrollPos;
        GUISkin _skin = null;
        Color _titleColor;
        Color _titleBoxColor;
        Color _convertColor;
        Color _convertSuccessColor;
        Color _convertErrorColor;
        GameObject playerObj = null;
        GameObject _previousplayerObj = null;
        Editor _prefabPreview;
        int _autoSelectNumber;
        float _scrollHeight = 0.0f;
        bool _manualPlayerSelect = false;
        bool _playerChanged = false;
        bool _converting = false;
        bool _running = false;
        bool _done = false;
        bool _errorsOccured = false;
        static bool CB_addNameBar = true;
        static bool CB_addVoiceChat = true;
        #endregion

        [MenuItem("CB Games/Convert/Players", false, 0)]
        public static void CB_ConvertPlayerEditorWindow()
        {
            EditorWindow window = GetWindow<E_ConvertPlayer>(true);
            window.maxSize = new Vector2(500, 510);
            window.minSize = window.maxSize;
        }

        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Set title bar colors
            _titleColor = new Color32(1, 9, 28, 255); //dark blue
            _titleBoxColor = new Color32(1, 16, 51, 255); //Lighter Blue
            _convertColor = new Color32(173, 127, 0, 255); //Darker Yellow
            _convertSuccessColor = new Color32(8, 156, 0, 255); //Light Green
            _convertErrorColor = new Color32(222, 11, 0, 255); //Red

            //Make window title
            this.titleContent = new GUIContent("Player Conversion", null, "Convert a player to support multiplayer.");
            FindAllPlayers();
        }

        private void OnGUI()
        {
            CBColorHolder _original = new CBColorHolder(EditorStyles.label);
            CBColorHolder _originalFold = new CBColorHolder(EditorStyles.foldout);
            CBColorHolder _skinHolder = new CBColorHolder(_skin.label);
            //Apply the gui skin
            GUI.skin = _skin;

            //Draw title bar
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Convert Players For Multiplayer Support", _skin.GetStyle("Label"));

            if (_converting == false)
            {
                EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                //Apply Body Title/Description
                EditorGUILayout.LabelField("Converts a player to support multiplayer logic", _skin.GetStyle("Label"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                //Divider
                EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                EditorGUILayout.LabelField(
                    "This will do all of the following on your selected player object:\n\r" +
                    "* Copies the original player and only does the following modifications on the copied player\r\n" +
                    "* Replace all components that have an equivalent multiplayer version in this package\r\n" +
                    "* Copy the original values to the newly created multiplayer versions\r\n" +
                    "* Attempt to re-create the custom UnityEvents into the new multiplayer versions\r\n" +
                    "* Notify you of any failures that might have occured\r\n" +
                    "\r\n" +
                    "This entire process will only take a few seconds for your selected character.",
                _skin.GetStyle("TextField"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                //Help text
                EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                EditorGUILayout.LabelField("First select the player object you want to convert. You can either do this " +
                    "manually or choose automatically from the dropdown list.", _skin.textField);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                //Toggle for adding player name bar or not
                CB_addNameBar = EditorGUI.ToggleLeft(new Rect(320, 273, 250, 20), "Add Player Name Bar", CB_addNameBar, _skin.textField);
                CB_addVoiceChat = EditorGUI.ToggleLeft(new Rect(320, 293, 250, 20), "Add Voice Chat Support", CB_addVoiceChat, _skin.textField);

                //Player Object Selection / Player Preview Window
                if (GUI.Button(new Rect(5, 275, 250, 30), "Switch Player Selection Method", _skin.button))
                {
                    _manualPlayerSelect = !_manualPlayerSelect;
                }
                if (_manualPlayerSelect == false && options.Count > 0)
                {
                    CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                    _autoSelectNumber = EditorGUI.Popup(new Rect(5, 310, 305, 16), "Available Player Objects:", _autoSelectNumber, options.ToArray());
                    CBEditor.SetColorToEditorStyle(_original, _originalFold);
                    playerObj = _players[_autoSelectNumber];
                }
                else
                {
                    CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                    playerObj = EditorGUI.ObjectField(new Rect(5, 310, 310, 16), "Player Object:", playerObj, typeof(GameObject), true) as GameObject;
                    CBEditor.SetColorToEditorStyle(_original, _originalFold);
                }
                if (playerObj != null)
                {
                    if ((_previousplayerObj == null && playerObj != null) || (_previousplayerObj.GetInstanceID() != playerObj.GetInstanceID()))
                    {
                        _playerChanged = true;
                        _previousplayerObj = playerObj;
                    }
                    Selection.activeObject = playerObj;
                    EditorGUI.DrawRect(new Rect(322, 312, 160, 160), E_Colors.e_c_blue_1);
                    if (_prefabPreview == null || _playerChanged == true)
                    {
                        _prefabPreview = null;
                        _prefabPreview = Editor.CreateEditor(playerObj);
                    }
                    if (_prefabPreview != null)
                    {
                        _prefabPreview.OnInteractivePreviewGUI(new Rect(327, 317, 150, 150), "window");
                    }
                }
                else
                {
                    _prefabPreview = null;
                }
                for (int i = 0; i < 9; i++)
                {
                    EditorGUILayout.Space();
                }
                EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false), GUILayout.Height(120), GUILayout.Width(305));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                //Draw Toggles Based on Player Components
                EditorGUILayout.LabelField("List of found components that will attempt to be converted:", _skin.textField);

                if (foundComps.Count == 0 && _prefabPreview != null || _playerChanged == true)
                {
                    _playerChanged = false;
                    foundComps.Clear();
                    foundComps = GetConvertableComps();
                }

                EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(100));
                foreach (KeyValuePair<string, Component> item in foundComps)
                {
                    EditorGUILayout.LabelField(item.Key, _skin.textField);
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //Draw Convert Now Button
                if (playerObj != null)
                {
                    if (GUI.Button(new Rect(30, 477, 430, 30), "Convert Now!", _skin.GetStyle("Button")))
                    {
                        try
                        {
                            this.StartCoroutine(ConvertPlayer(playerObj));
                        }
                        catch (Exception ex)
                        {
                            _errorsOccured = true;
                            if (convertLog != null) convertLog.Add("\n");
                            if (convertLog != null) convertLog.Add("ERRORS OCCURED!");
                            if (convertLog != null) convertLog.Add("Please review the log window for the full error.");
                            if (convertLog != null) convertLog.Add("\n");
                            if (convertLog != null) convertLog.Add("--------------------------------------");
                            if (convertLog != null) convertLog.Add("ERROR: " + ex.Message);
                            if (convertLog != null) convertLog.Add("--------------------------------------");
                            Debug.LogError(ex);
                        }
                        finally
                        {
                            _running = false;
                            _done = true;
                        }
                    }
                }
            }
            //Currently converting a player window
            else
            {
                //Apply Body Title/Description
                if (_done == false)
                {
                    EditorGUI.LabelField(new Rect(10, 40, 480, 50), "Converting, please wait...", _skin.label);
                }
                else
                {
                    EditorGUI.LabelField(new Rect(10, 40, 480, 50), "Completed! You can close this window anytime.", _skin.label);
                }

                EditorGUI.DrawRect(new Rect(10, 80, position.width - 20, position.height - 160), E_Colors.e_c_blue_4);
                _prefabPreview.OnInteractivePreviewGUI(new Rect(position.width - 170, 90, 150, 150), "window");
                if (_done == false)
                {
                    EditorGUI.DrawRect(new Rect(position.width - 170, 215, 150, 25), _convertColor);
                    EditorGUI.LabelField(new Rect(position.width - 125, 215, 150, 25), "Converting...", _skin.textField);
                }
                else
                {
                    if (_errorsOccured == false)
                    {
                        EditorGUI.DrawRect(new Rect(position.width - 170, 215, 150, 25), _convertSuccessColor);
                        EditorGUI.LabelField(new Rect(position.width - 115, 215, 150, 25), "Success!", _skin.textField);
                    }
                    else
                    {
                        EditorGUI.DrawRect(new Rect(position.width - 170, 215, 150, 25), _convertErrorColor);
                        EditorGUI.LabelField(new Rect(position.width - 130, 215, 150, 25), "Error Occured!", _skin.textField);
                    }
                }

                if (_running == false && _done == false)
                {
                    _running = true;
                    ConvertPlayer(playerObj);
                }
                else
                {
                    EditorGUI.LabelField(new Rect(330, 250, 160, 90), "IMPORTANT NOTE: You need to click into this window to refresh it manually.", _skin.GetStyle("TextField"));
                    _scrollHeight = convertLog.Count * 10.5f;
                    _logScrollPos = GUI.BeginScrollView(new Rect(10, 80, 310, position.height - 210), _logScrollPos, new Rect(0, 0, 430, _scrollHeight), false, true);
                    for (int i = 0; i < convertLog.Count; i++)
                    {
                        EditorGUI.LabelField(new Rect(10, i * 10, 490, 90), convertLog[i], _skin.GetStyle("TextField"));
                    }
                    GUI.EndScrollView();
                    //GUI.EndGroup();
                    if (_done == true)
                    {
                        EditorGUI.LabelField(new Rect(10, 390, 490, 90), "Completed! Review this log to make sure there is nothing manually that you need to do or errors that need to be addressed. If you would like to convert another player press the respective button below.", _skin.GetStyle("TextField"));
                        if (GUI.Button(new Rect(50, 460, 370, 30), "Convert Another Player", _skin.GetStyle("Button")))
                        {
                            _converting = false;
                            _done = false;
                            _running = false;
                            OnEnable();
                        }
                    }
                }
            }
        }

        void FindAllPlayers()
        {
            Invector.vCharacterController.vThirdPersonController[] _foundComponents = FindObjectsOfType(typeof(Invector.vCharacterController.vThirdPersonController)) as Invector.vCharacterController.vThirdPersonController[];
            foreach (Invector.vCharacterController.vThirdPersonController foundComponent in _foundComponents)
            {
                options.Add(foundComponent.gameObject.name);
                _players.Add(foundComponent.gameObject);
            }
        }

        Dictionary<string, Component> GetConvertableComps()
        {
            if (playerObj == null) return null;
            Dictionary<string, Component> convertActions = new Dictionary<string, Component>();

            CORE_GetConvertableComponents(playerObj, ref convertActions);
            SHOOTER_GetConvertableComponents(playerObj, ref convertActions);
            FREECLIMB_GetConvertableComponents(playerObj, ref convertActions);
            ZIPLINE_GetConvertableComponents(playerObj, ref convertActions);
            SWIMMING_GetConvertableComponents(playerObj, ref convertActions);

            return convertActions;
        }
        public void CONTEXT_ConvertPlayer(GameObject target)
        {
            this.StartCoroutine(ConvertPlayer(target));
        }
        IEnumerator ConvertPlayer(GameObject targetObj)
        {
            GameObject _builtPrefab = null;
            convertLog.Clear();
            _converting = true;
            _errorsOccured = false;
            if (convertLog != null) convertLog.Add("Beginning to convert: " + targetObj.name);
            if (convertLog != null) convertLog.Add("");
            if (convertLog != null) convertLog.Add("Copying " + targetObj.name + " -> MP_" + targetObj.name);

            // Copy Player GameObject
            GameObject _copiedPlayer = GameObject.Instantiate(targetObj, targetObj.transform.position + Vector3.left, Quaternion.identity) as GameObject;
            _copiedPlayer.name = _copiedPlayer.name.Replace("(Clone)", "");
            _copiedPlayer.name = "MP_" + _copiedPlayer.name;

            //Add Components
            if (CB_addVoiceChat == true)
            {
                CB_COMP_PlayerVoiceChat(_copiedPlayer, ref convertLog);
            }
            if (CB_addNameBar == true)
            {
                CB_COMP_PlayerNameBar(_copiedPlayer, ref convertLog);
            }

            CB_COMP_vRagdoll(_copiedPlayer, ref convertLog);
            CB_COMP_vMeleeCombatInput(_copiedPlayer, ref convertLog);
            CB_COMP_vWeaponHolderManager(_copiedPlayer, ref convertLog);
            CB_COMP_vGenericAction(_copiedPlayer, ref convertLog);
            CB_COMP_vMeleeManager(_copiedPlayer, ref convertLog);

            CB_COMP_vHeadTrack(_copiedPlayer, ref convertLog);
            CB_COMP_vShooterMeleeInput(_copiedPlayer, ref convertLog);
            CB_COMP_vShooterManager(_copiedPlayer, ref convertLog);
            CB_COMP_vFreeClimb(_copiedPlayer, ref convertLog);
            CB_COMP_vSwimming(_copiedPlayer, ref convertLog);
            CB_COMP_vZipline(_copiedPlayer, ref convertLog);

            CB_COMP_SyncPlayer(_copiedPlayer, ref convertLog);
            CB_COMP_vThirdPersonController(_copiedPlayer, ref convertLog);
            CB_COMP_vItemManager(_copiedPlayer, ref convertLog);
            CB_COMP_vLadderAction(_copiedPlayer, ref convertLog);
            CB_COMP_NetworkCulling(_copiedPlayer, ref convertLog);

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            string saveLocation = string.Format("Assets{0}Resources{0}{1}.prefab", Path.DirectorySeparatorChar, _copiedPlayer.name);
            if (E_Helpers.CreateDirectory(E_Helpers.GetDirectoryPath(saveLocation)))
            {
                if (convertLog != null) convertLog.Add("Created Assets/Resources directory");
            }

            E_Helpers.SetObjectIcon(_copiedPlayer, E_Core.h_playerIcon);

            GameObject createdPrefab = null;
            if (E_Helpers.FileExists(saveLocation))
            {
                if (EditorUtility.DisplayDialog("Duplicate Detected!",
                    "A prefab with the same name already exists, would you like to overwrite it or create a new one, keeping both?",
                    "Overwrite", "Keep Both"))
                {
                    //Overwrite
                    E_Helpers.DeleteFile(saveLocation);
                    if (convertLog != null) convertLog.Add("Generating prefab at: " + saveLocation);
                    createdPrefab = PrefabUtility.SaveAsPrefabAsset(_copiedPlayer, saveLocation);
                }
                else
                {
                    //Keep Both
                    saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                    if (convertLog != null) convertLog.Add("Generating prefab at: " + saveLocation);
                    createdPrefab = PrefabUtility.SaveAsPrefabAsset(_copiedPlayer, saveLocation);
                }
            }
            else
            {
                saveLocation = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
                if (convertLog != null) convertLog.Add("Generating prefab at: " + saveLocation);
                createdPrefab = PrefabUtility.SaveAsPrefabAsset(_copiedPlayer, saveLocation);
            }

            if (FindObjectOfType<NetworkManager>())
            {
                if (convertLog != null) convertLog.Add("Adding generated prefab to the \"playerPrefab\" field in the \"NetworkManager\"");
                _builtPrefab = E_Helpers.GetPrefabReference(saveLocation);
                int timeout = 4;
                int count = 0;
                while (_builtPrefab == null)
                {
                    if (count == timeout)
                    {
                        Debug.LogWarning("Unable to locate the player prefab to place into the NetworkManager.");
                        break;
                    }
                    yield return new WaitForSeconds(0.5f);
                    count += 1;
                    _builtPrefab = E_Helpers.GetPrefabReference(saveLocation);
                }
                FindObjectOfType<NetworkManager>().playerPrefab = _builtPrefab;
            }
            else
            {
                if (convertLog != null) convertLog.Add("WARNING: Skipped adding to \"NetworkManager\" component.");
                if (EditorUtility.DisplayDialog("Unable To Find NetworkManager",
                            "No NetworkManager was found in the scene so this converted player was not added to the \"playerPrefab\"!" +
                            " When the NetworkManager is added to the scene make sure to add the generated prefab in the \"Resources\" folder to that field.",
                            "Okay"))
                {
                    Debug.LogWarning("No NetworkManager component was found in the scene so this prefab was not added a spawn object. When the NetworkManager component is added to the scene please add a prefab to it.");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            DestroyImmediate(_copiedPlayer);
            AssetDatabase.Refresh();
            Selection.activeObject = createdPrefab;
            GameObject _createdPrefab = PrefabUtility.InstantiatePrefab(createdPrefab) as GameObject;
            _createdPrefab.transform.position = targetObj.transform.position + Vector3.left;
            _createdPrefab.transform.rotation = Quaternion.identity;
            Selection.activeObject = _createdPrefab;


            foreach (string log in convertLog)
            {
                Debug.Log(log);
            }
            yield return null;
        }
    }
}
