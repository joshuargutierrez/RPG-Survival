using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackConnectionStatus), true)]
    public class TrackConnectionStatusInspector : BaseEditor
    {
        #region Properties
        SerializedProperty texts;
        SerializedProperty OnConnectedToLobby;
        SerializedProperty OnConnectedToRoom;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            texts = serializedObject.FindProperty("texts");
            OnConnectedToLobby = serializedObject.FindProperty("OnConnectedToLobby");
            OnConnectedToRoom = serializedObject.FindProperty("OnConnectedToRoom");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Connection Status", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will change the targeted Text to match whatever the connection " +
                "status is, from the NetworkManager.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            GUI.skin = _original;
            CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
            EditorGUILayout.HelpBox("Call when you successfully connect to the Photon Lobby.", MessageType.Info);
            EditorGUILayout.PropertyField(OnConnectedToLobby);
            EditorGUILayout.HelpBox("Call when you successfully connect to a Photon Room.", MessageType.Info);
            EditorGUILayout.PropertyField(OnConnectedToRoom);
            GUI.skin = _skin;
            CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackConnectionStatus));
            #endregion
        }
    }
}