using UnityEditor;
using UnityEngine;
using CBGames.Core;
using CBGames.Editors;
using CBGames.Player;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(PlayerRespawn), true)]
    public class PlayerRespawnInspector : BaseEditor
    {
        #region Properties
        SerializedProperty respawnDelay;
        SerializedProperty visualCountdown;
        SerializedProperty respawnType;
        SerializedProperty respawnPoint;
        SerializedProperty broadcastDeathMessage;
        SerializedProperty deathMessage;
        SerializedProperty teams;
        SerializedProperty debugging;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            respawnDelay = serializedObject.FindProperty("respawnDelay");
            visualCountdown = serializedObject.FindProperty("visualCountdown");
            respawnType = serializedObject.FindProperty("respawnType");
            respawnPoint = serializedObject.FindProperty("respawnPoint");
            broadcastDeathMessage = serializedObject.FindProperty("broadcastDeathMessage");
            deathMessage = serializedObject.FindProperty("deathMessage");
            teams = serializedObject.FindProperty("teams");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Network Respawning",
                "Call \"Respawn\" function on this component to respawn the player across the network. \n\n" +
                "Best if called from the \"Respawn\" function found in  the \"Sync Player\" component. " +
                "Can have the \"OnDead\" event on the \"vThirdPersonController\" component call the " +
                "\"Respawn\" function of the \"SyncPlayer\" component. \n\n" +
                "Will respawn the player (destroying the old one) based on the settings here. Will respawn " +
                "the player with max health but all the previous settings that were originally on the player, " +
                "including their inventory settings.",
                E_Core.h_respawnPointIcon
            );
            #endregion

            #region Properties
            if (E_Helpers.InspectorTagExists("RespawnPoint") == false)
            {
                EditorGUILayout.HelpBox("There is no tag\"RespawnPoint\" in this project! \n\n" +
                    "To add a new object that is tagged with \"RespawnPoint\" go to CB Games > Network Manager > Respawn > Add Respawn Point", MessageType.Error);
            }
            else if(GameObject.FindGameObjectWithTag("RespawnPoint") == null)
            {
                EditorGUILayout.HelpBox("There is no gameobject with the \"RespawnPoint\" tag in this scene! \n\n" +
                    "To add a new object that is tagged with \"RespawnPoint\" go to CB Games > Network Manager > Respawn > Add Respawn Point", MessageType.Error);
            }
            else
            {
                EditorGUILayout.PropertyField(respawnDelay);
                EditorGUILayout.PropertyField(visualCountdown);
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(respawnType);
                if (respawnType.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(respawnPoint);
                    if (respawnPoint.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("There must always be a transform here otherwise this player will not respawn correctly.", MessageType.Error);
                    }
                }
                if (respawnType.enumValueIndex == 3)
                {
                    EditorGUILayout.PropertyField(teams, true);
                }
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(broadcastDeathMessage);
                if (broadcastDeathMessage.boolValue == true)
                {
                    EditorGUILayout.PropertyField(deathMessage);
                }
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(debugging);
            }
            #endregion

            EndInspectorGUI(typeof(PlayerRespawn));
        }
    }
}