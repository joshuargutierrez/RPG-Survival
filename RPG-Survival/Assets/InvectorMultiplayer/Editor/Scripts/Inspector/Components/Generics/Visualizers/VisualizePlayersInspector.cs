using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(VisualizePlayers), true)]
    public class VisualizePlayersInspector : BaseEditor
    {
        #region Properties
        SerializedProperty parentObj;
        SerializedProperty ownerPlayer;
        SerializedProperty otherPlayer;
        SerializedProperty teamName;
        SerializedProperty autoSetTeamIfNotSet;
        SerializedProperty allocateViewIds;
        SerializedProperty debugging;
        SerializedProperty joinedSound;
        SerializedProperty leftSound;
        SerializedProperty soundSource;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            parentObj = serializedObject.FindProperty("parentObj");
            ownerPlayer = serializedObject.FindProperty("ownerPlayer");
            otherPlayer = serializedObject.FindProperty("otherPlayer");
            teamName = serializedObject.FindProperty("teamName");
            autoSetTeamIfNotSet = serializedObject.FindProperty("autoSetTeamIfNotSet");
            allocateViewIds = serializedObject.FindProperty("allocateViewIds");
            debugging = serializedObject.FindProperty("debugging");
            joinedSound = serializedObject.FindProperty("joinedSound");
            leftSound = serializedObject.FindProperty("leftSound");
            soundSource = serializedObject.FindProperty("soundSource");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Visualize Players",
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will spawn child objects under your selected parentObj for every " +
                "player that connects to this Photon Room.", 
            E_Core.h_uiIcon);
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(debugging);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(parentObj);
            EditorGUILayout.PropertyField(ownerPlayer);
            EditorGUILayout.PropertyField(otherPlayer);
            EditorGUILayout.PropertyField(teamName);
            EditorGUILayout.PropertyField(joinedSound, true);
            EditorGUILayout.PropertyField(leftSound, true);
            EditorGUILayout.PropertyField(soundSource);
            EditorGUILayout.PropertyField(allocateViewIds);
            EditorGUILayout.HelpBox("Read the tooltip before setting this value.", MessageType.Warning);
            EditorGUILayout.PropertyField(autoSetTeamIfNotSet);
            #endregion

            #region Core
            EndInspectorGUI(typeof(VisualizePlayers));
            #endregion
        }
    }
}