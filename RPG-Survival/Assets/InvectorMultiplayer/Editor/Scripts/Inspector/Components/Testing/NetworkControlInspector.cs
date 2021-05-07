using UnityEditor;
using CBGames.Testing;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace CBGames.Editors
{
    [CustomEditor(typeof(NetworkControl), true)]
    public class NetworkControlInspector : BaseEditor
    {
        #region Properties
        SerializedProperty simulate;
        SerializedProperty lagAmount;
        SerializedProperty jitterAmount;
        SerializedProperty lossPercent;
        #endregion

        GUIStyle greenLabelText;
        GUIStyle redLabelText;
        protected override void OnEnable()
        {
            #region Properties
            simulate = serializedObject.FindProperty("simulate");
            lagAmount = serializedObject.FindProperty("lagAmount");
            jitterAmount = serializedObject.FindProperty("jitterAmount");
            lossPercent = serializedObject.FindProperty("lossPercent");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Text Colors
            greenLabelText = new GUIStyle(EditorStyles.label);
            greenLabelText.normal.textColor = Color.green;

            redLabelText = new GUIStyle(EditorStyles.label);
            redLabelText.normal.textColor = Color.red;
            #endregion

            #region Core
            base.OnInspectorGUI();
            NetworkControl nc = (NetworkControl)target;
            DrawTitleBar(
                "Network Testing Controller",
                "THIS IS FOR DEBUGGING ONLY!\n\n" +
                "This allows you to test certain network scenarios that you might come across at runtime. " +
                "This will add a helpful window at runtime to see live network traffic.",
                E_Core.h_networkIcon
            );
            #endregion

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (!PhotonNetwork.IsConnected)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    float orgWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 100f;
                    EditorGUILayout.LabelField("NOT CONNECTED TO PHOTON", redLabelText, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = orgWidth;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else if (PhotonNetwork.CurrentRoom == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    float orgWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 100f;
                    EditorGUILayout.LabelField("NOT IN ROOM", redLabelText, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = orgWidth;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    float orgWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 200f;
                    EditorGUILayout.LabelField("CONNECTED TO ROOM: " + PhotonNetwork.CurrentRoom.Name, greenLabelText, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = orgWidth;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Disconnect"))
                {
                    nc.DisconnectFromPhoton();
                }
                if (GUILayout.Button("Leave Room"))
                {
                    nc.LeaveRoom();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Enable Reconnect"))
                {
                    nc.SetReconnect(true);
                }
                if (GUILayout.Button("Disable Reconnect"))
                {
                    nc.SetReconnect(false);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-", _skin.textField);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // Enable or Disables Simulation
            EditorGUILayout.HelpBox("Enables and disables the simulation. A sudden, big change of network conditions might result in disconnects.", MessageType.Info);
            simulate.boolValue = GUILayout.Toggle(simulate.boolValue, "Simulate");
            EditorGUILayout.Space();

            // Lag Simulator
            EditorGUILayout.HelpBox("Adds a fixed delay to all outgoing and incoming messages. In milliseconds.", MessageType.Info);
            GUILayout.Label("Lag Amount: " + lagAmount.floatValue, _skin.textArea);
            lagAmount.floatValue = GUILayout.HorizontalSlider(lagAmount.floatValue, 0, 500);
            EditorGUILayout.Space();

            // Jitter Simulator
            EditorGUILayout.HelpBox("Adds a random delay of \"up to X milliseconds\" per message.", MessageType.Info);
            GUILayout.Label("Jitter Amount: " + jitterAmount.floatValue, _skin.textArea);
            jitterAmount.floatValue = GUILayout.HorizontalSlider(jitterAmount.floatValue, 0, 100);
            EditorGUILayout.Space();

            // Loss Simulator
            EditorGUILayout.HelpBox("Drops the set percentage of messages. You can expect less than 2% drop in the internet today.", MessageType.Info);
            GUILayout.Label("Loss Amount: " + lossPercent.floatValue, _skin.textArea);
            lossPercent.floatValue = GUILayout.HorizontalSlider(lossPercent.floatValue, 0, 20);
            EditorGUILayout.Space();
            EndInspectorGUI(typeof(NetworkControl));
        }
    }
}