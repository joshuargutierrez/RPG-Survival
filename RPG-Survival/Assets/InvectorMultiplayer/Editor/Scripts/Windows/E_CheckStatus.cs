using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_CheckStatus : EditorWindow
    {
        #region Editor Variables
        GUISkin _skin = null;
        #endregion

        [MenuItem("CB Games/Check File Status'", false, 200)]
        private static void CB_WINDOW_FileStatus()
        {
            EditorWindow window = GetWindow<E_CheckStatus>(true);
            window.maxSize = new Vector2(500, 490);
            window.minSize = window.maxSize;
        }
        private void OnEnable()
        {
            if (!_skin) _skin = E_Helpers.LoadSkin(E_Core.e_guiSkinPath);

            //Make window title
            this.titleContent = new GUIContent("Check File Status'", null, "What files have been modified and what add-ons are enabled.");
        }
        private void OnGUI()
        {
            //Apply the gui skin
            GUI.skin = _skin;
            Color norm = _skin.textField.normal.textColor;
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), E_Colors.e_c_blue_5); 
            EditorGUI.DrawRect(new Rect(5, 5, position.width - 10, 40), E_Colors.e_c_blue_4);
            EditorGUILayout.Space();
            _skin.label.normal.textColor = norm;
            EditorGUILayout.LabelField("Check File Status'", _skin.label);
            EditorGUILayout.Space();

            //Apply Body Title/Description
            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("This displays what files appear to have been modified and and what add-ons are currently enabled.", _skin.GetStyle("TextField"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            #region Title Bar
            EditorGUILayout.BeginHorizontal(_skin.customStyles[0]);
            EditorGUILayout.LabelField("File/Add-on", _skin.textField);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("is Modified or Enabled", _skin.textField);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region MP_vHeadTrack
            bool isEnabled = !E_Helpers.FileContainsText(@"InvectorMultiplayer/Scripts/Player/Basic/MP_HeadTrack.cs", "/*");
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("MP_vHeadTrack.cs File", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region vHeadTrack
            isEnabled = E_Helpers.FileContainsText(
                @"Invector-3rdPersonController/Basic Locomotion/Scripts/HeadTrack/Scripts/vHeadTrack.cs",
                "protected float _currentHeadWeight, _currentbodyWeight;"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("vHeadTrack.cs File", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region vItemManagerUtilities_Shooter
            isEnabled = E_Helpers.FileContainsText(
                @"Invector-3rdPersonController/Shooter/Scripts/Shooter/Editor/vItemManagerUtilities_Shooter.cs",
                "if ((equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) || equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().IsSubclassOf(typeof(vShooterManager))) && equipPointL.onInstantiateEquiment.GetPersistentMethodName(i).Equals(\"SetLeftWeapon\"))"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("vItemManagerUtilities_Shooter.cs File", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region vShooterManager
            isEnabled = !E_Helpers.FileContainsText(
                @"Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterManager.cs",
                "protected bool usingThirdPersonController;"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("vShooterManager.cs File", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region vGenericAction
            isEnabled = !E_Helpers.FileContainsText(
                @"Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/CharacterController/Actions/vGenericAction.cs",
                "protected float animationBehaviourDelay;"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("vGenericAction.cs File", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region Swimming
            isEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/Player/Swimming/MP_vSwimming.cs",
                "/*"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("Swimming Add-On", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region FreeClimb
            isEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/Player/FreeClimb/MP_vFreeClimb.cs",
                "/*"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("FreeClimb Add-On", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region Zipline
            isEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/Player/Zipline/MP_vZipline.cs",
                "/*"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("Zipline Add-On", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region Shooter Template
            isEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_ShooterConvertPlayer.cs",
                "/*"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("Shooter Template Add-On", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            #endregion

            #region FSM AI Template
            isEnabled = !E_Helpers.FileContainsText(
                "InvectorMultiplayer/Scripts/AI/MP_vAIHeadTrack.cs",
                "/*"
            );
            EditorGUILayout.BeginHorizontal(_skin.window);
            _skin.textField.normal.textColor = norm;
            EditorGUILayout.LabelField("FSM AI Template", _skin.textField);
            GUILayout.FlexibleSpace();
            _skin.textField.normal.textColor = (isEnabled) ? Color.green : Color.red;
            EditorGUILayout.LabelField(isEnabled.ToString(), _skin.textField);
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            _skin.textField.normal.textColor = norm;
        }
    }
}