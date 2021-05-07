using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.Objects;
using UnityEditorInternal;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(PreviewCamera), true, isFallback = true)]
    public class PreviewCameraInspector : BaseEditor
    {
        #region Properties
        SerializedProperty cameraPoints;
        SerializedProperty moveImmediatly;
        SerializedProperty cameraMoveSpeed;
        SerializedProperty cameraCloseEnough;
        SerializedProperty stopOnJoinRoom;
        SerializedProperty targetCam;
        SerializedProperty networkManager;
        #endregion

        #region CustomEditorVariables
        PreviewCamera _target;
        private ReorderableList _cameraPointsList;
        #endregion

        protected override void OnEnable()
        {
            _target = (PreviewCamera)target;
            _cameraPointsList = new ReorderableList(serializedObject, serializedObject.FindProperty("cameraPoints"), true, true, true, true);
            _cameraPointsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Camera Transition Points", EditorStyles.boldLabel);
            };
            _cameraPointsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _cameraPointsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };

            #region Properties
            //Get Properties
            cameraPoints = serializedObject.FindProperty("cameraPoints");
            moveImmediatly = serializedObject.FindProperty("moveImmediatly");
            cameraMoveSpeed = serializedObject.FindProperty("cameraMoveSpeed");
            cameraCloseEnough = serializedObject.FindProperty("cameraCloseEnough");
            stopOnJoinRoom = serializedObject.FindProperty("stopOnJoinRoom");
            targetCam = serializedObject.FindProperty("targetCam");
            networkManager = serializedObject.FindProperty("networkManager");
            #endregion

            base.OnEnable();
        }
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            PreviewCamera pc = (PreviewCamera)target;
            DrawTitleBar(
                "Preview Camera", 
                "Component thats used to make a target camera follow a series of points. " +
                "This is designed specifically for a lobby preview camera however this does " +
                "contain a start/stop function for outside components to call.", 
                E_Core.h_cameraPath
            );
            #endregion

            #region Properties
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(moveImmediatly);
            EditorGUILayout.PropertyField(stopOnJoinRoom);
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            _cameraPointsList.DoLayoutList();
            GUI.skin = _skin;
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.PropertyField(cameraMoveSpeed);
            EditorGUILayout.PropertyField(cameraCloseEnough);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional Fields", _skin.textArea);
            EditorGUILayout.PropertyField(targetCam);
            EditorGUILayout.PropertyField(networkManager);
            #endregion

            #region Core
            EndInspectorGUI(typeof(PreviewCamera));
            #endregion
        }
    }
}