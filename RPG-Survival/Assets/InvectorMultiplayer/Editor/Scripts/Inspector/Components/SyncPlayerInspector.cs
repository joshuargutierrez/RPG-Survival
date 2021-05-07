using UnityEngine;
using UnityEditor;
using CBGames.Player;
using CBGames.Editors;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(SyncPlayer), true)]
    public class SyncPlayerInspector : BaseEditor
    {
        #region Serialized Properties
        SerializedProperty _syncAnimations;
        SerializedProperty _positionLerpRate;
        SerializedProperty _rotationLerpRate;
        SerializedProperty noneLocalTag;
        SerializedProperty _nonAuthoritativeLayer;
        SerializedProperty teamName;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            _syncAnimations = serializedObject.FindProperty("_syncAnimations");
            _positionLerpRate = serializedObject.FindProperty("_positionLerpRate");
            _rotationLerpRate = serializedObject.FindProperty("_rotationLerpRate");
            noneLocalTag = serializedObject.FindProperty("noneLocalTag");
            _nonAuthoritativeLayer = serializedObject.FindProperty("_nonAuthoritativeLayer");
            teamName = serializedObject.FindProperty("teamName");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            SyncPlayer sp = (SyncPlayer)target;
            DrawTitleBar(
                "Network Sync Player",
                "Component that belongs on each player. Will send events over the network like animations, damage, etc.",
                E_Core.h_playerIcon
            );
            #endregion

            //Properties 
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.FindTexture("animationkeyframe"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Sync Animations Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(_skin.window);
            EditorGUILayout.PropertyField(_syncAnimations, new GUIContent("Sync Animations"));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.FindTexture("d_UnityEditor.AnimationWindow"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Sync Position/Rotation Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(_skin.window);
            GUILayout.BeginVertical();
            EditorGUILayout.Slider(_positionLerpRate, 0, 25, new GUIContent("Position Move Speed"));
            EditorGUILayout.Slider(_rotationLerpRate, 0, 25, new GUIContent("Rotation Move Speed"));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(EditorGUIUtility.FindTexture("TerrainInspector.TerrainToolSettings"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("None Owner Settings", _skin.GetStyle("TextField"));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(_skin.window);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(noneLocalTag, new GUIContent("None Owner Tag"));
            EditorGUILayout.PropertyField(_nonAuthoritativeLayer, new GUIContent("None Owner Layer"));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("d_editcollision_16"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Team Settings", _skin.textField);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(_skin.window);
            GUILayout.BeginVertical();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(teamName);
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            EndInspectorGUI(typeof(SyncPlayer));
        }
    }
}