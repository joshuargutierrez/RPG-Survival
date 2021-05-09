using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.Player;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(VoiceChat), true)]
    public class VoiceChatInspector : BaseEditor
    {
        #region CustomEditorVariables
        List<string> availableInputs = new List<string>();
        int selectedButton = 0;
        #endregion

        #region Properties
        SerializedProperty isPlayer;
        SerializedProperty ifOnTeam;

        SerializedProperty speakerImage;
        SerializedProperty recordingImage;
        SerializedProperty pushToTalk;
        SerializedProperty buttonToPress;

        SerializedProperty OnConnectedToServer;
        SerializedProperty OnDisconnect;
        SerializedProperty OnJoinRoom;
        SerializedProperty OnStart;

        SerializedProperty debugging;
        #endregion
        protected override void OnEnable()
        {
            #region Fields
            isPlayer = serializedObject.FindProperty("isPlayer");
            ifOnTeam = serializedObject.FindProperty("ifOnTeam");
            speakerImage = serializedObject.FindProperty("speakerImage");
            recordingImage = serializedObject.FindProperty("recordingImage");
            debugging = serializedObject.FindProperty("debugging");
            pushToTalk = serializedObject.FindProperty("pushToTalk");
            buttonToPress = serializedObject.FindProperty("buttonToPress");

            OnConnectedToServer = serializedObject.FindProperty("OnConnectedToServer");
            OnDisconnect = serializedObject.FindProperty("OnDisconnect");
            OnJoinRoom = serializedObject.FindProperty("OnJoinRoom");
            OnStart = serializedObject.FindProperty("OnStart");
            #endregion

            availableInputs = E_Helpers.GetAllInputAxis();
            base.OnEnable();
        }
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            VoiceChat vc = (VoiceChat)target;
            DrawTitleBar(
                "Voice Chat", 
                "If this component is... " +
                "\n" +
                "...not for a player...\n" +
                "This is for a peristant object. This should be made a child object of the \"NetworkManager\" gameobject." +
                " This needs to have a \"Recorder\" & \"PhotonVoiceNetwork\" components attached. This is designed to " +
                "record the owner and send that voice recording over the network. This will display special icons and " +
                "invoke other things based on the state of the voice connection.\n" +
                "\n" +
                "...for a player...\n" +
                "This requires a \"Speaker\" & \"PhotonVoiceView\" components attached. This will display an icon when " +
                "voice from another player is playing through the \"Speaker\" component. Adjust the generated \"AudioSource\" " +
                "component to 2D if you want global room chat or 3D for proximity based chat. The \"Recorder In Use\" field " +
                "on the \"PhotonVoiceView\" will be auto populate by finding the \"Recorder\" component in the scene.", 
                E_Core.h_voiceIcon
            );
            #endregion

            EditorGUILayout.BeginHorizontal(_skin.box);
            EditorGUILayout.PropertyField(isPlayer);
            EditorGUILayout.EndHorizontal();
            if (isPlayer.boolValue == true)
            {
                EditorGUILayout.PropertyField(ifOnTeam);
                EditorGUILayout.PropertyField(speakerImage);
            }
            else
            {
                EditorGUILayout.PropertyField(debugging);
                EditorGUILayout.PropertyField(recordingImage);
                EditorGUILayout.PropertyField(pushToTalk);
                if (!availableInputs.Contains(buttonToPress.stringValue))
                {
                    EditorGUILayout.PropertyField(buttonToPress);
                    EditorGUILayout.HelpBox("The Button To Press value: \""+ buttonToPress.stringValue + "\" doesn't exist. " +
                        "To fix this go to your Project Settings > Input and add this key name.", MessageType.Error);
                }
                else
                {
                    selectedButton = availableInputs.IndexOf(buttonToPress.stringValue);
                    selectedButton = EditorGUILayout.Popup("Button To Hold", selectedButton, availableInputs.ToArray());
                    buttonToPress.stringValue = availableInputs[selectedButton];
                }

                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.HelpBox("Called when successfully connected/disconnected from the master voice server.", MessageType.None);
                EditorGUILayout.PropertyField(OnConnectedToServer, true);
                EditorGUILayout.PropertyField(OnDisconnect, true);
                EditorGUILayout.HelpBox("Called when successfully joined ANY chat room.", MessageType.None);
                EditorGUILayout.PropertyField(OnJoinRoom, true);
                EditorGUILayout.HelpBox("Called once when this component is first loaded in a scene.", MessageType.None);
                EditorGUILayout.PropertyField(OnStart, true);
                GUI.skin = _skin;
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
                EditorGUILayout.Space();
            }

            EndInspectorGUI(typeof(VoiceChat));
        }
    }
}
