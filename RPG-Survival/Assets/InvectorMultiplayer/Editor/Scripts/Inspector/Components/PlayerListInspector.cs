using UnityEditor;
using UnityEngine;
using CBGames.Editors;
using CBGames.UI;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(PlayerList), true)]
    public class PlayerListInspector : BaseEditor
    {
        #region Properties
        SerializedProperty content;
        SerializedProperty rootObj;
        SerializedProperty playerJoinObject;
        SerializedProperty openWindow;
        SerializedProperty keyToPress;
        SerializedProperty autoCloseWithChatBox;
        SerializedProperty delayDisable;
        SerializedProperty anim;
        SerializedProperty openAnimation;
        SerializedProperty closeAnimation;
        SerializedProperty soundSource;
        SerializedProperty openSound;
        SerializedProperty closeSound;
        SerializedProperty openSoundVolume;
        SerializedProperty closeSoundVolume;
        SerializedProperty debugging;
        #endregion

        List<string> availableInputs = new List<string>();
        int openInt;

        protected override void OnEnable()
        {
            #region Properties
            content = serializedObject.FindProperty("content");
            rootObj = serializedObject.FindProperty("rootObj");
            playerJoinObject = serializedObject.FindProperty("playerJoinObject");
            openWindow = serializedObject.FindProperty("openWindow");
            keyToPress = serializedObject.FindProperty("keyToPress");
            autoCloseWithChatBox = serializedObject.FindProperty("autoCloseWithChatBox");
            delayDisable = serializedObject.FindProperty("delayDisable");
            anim = serializedObject.FindProperty("anim");
            openAnimation = serializedObject.FindProperty("openAnimation");
            closeAnimation = serializedObject.FindProperty("closeAnimation");
            soundSource = serializedObject.FindProperty("soundSource");
            openSound = serializedObject.FindProperty("openSound");
            openSoundVolume = serializedObject.FindProperty("openSoundVolume");
            closeSound = serializedObject.FindProperty("closeSound");
            closeSoundVolume = serializedObject.FindProperty("closeSoundVolume");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            availableInputs = E_Helpers.GetAllInputAxis();
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar("Player List UI", 
                "This is a visual UI element that is populated with player data " +
                "from the chatbox. \n\n" +
                "Call \"AddPlayer\" function to add a player to the list.\n" +
                "Call \"RemovePlayer\" function to remove a player from the list.",
                E_Core.h_playerlistIcon
            );
            #endregion

            #region Properties
            
            EditorGUILayout.LabelField("UI Parts");
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(content);
            EditorGUILayout.PropertyField(rootObj);
            EditorGUILayout.PropertyField(playerJoinObject);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Key Press");
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(openWindow);
            if (openWindow.intValue != 2)
            {
                string titleText = (openWindow.intValue == 0) ? "Key To Hold" : "Key To Press";
                int index = availableInputs.IndexOf(keyToPress.stringValue);
                if (index == -1)
                {
                    EditorGUILayout.PropertyField(keyToPress, new GUIContent(titleText));
                    EditorGUILayout.HelpBox("This key doesn't exist in your project. Be sure to add it before you final build!", MessageType.Error);
                }
                else
                {
                    openInt = index;
                    openInt = EditorGUILayout.Popup(titleText, openInt, availableInputs.ToArray());
                    keyToPress.stringValue = availableInputs[openInt];
                }
                EditorGUILayout.PropertyField(autoCloseWithChatBox);
            }
            EditorGUILayout.PropertyField(delayDisable);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Animations");
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(anim);
            EditorGUILayout.PropertyField(openAnimation);
            EditorGUILayout.PropertyField(closeAnimation);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Sounds");
            GUILayout.BeginHorizontal(_skin.customStyles[1], GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(soundSource);
            EditorGUILayout.PropertyField(openSound);
            EditorGUILayout.PropertyField(openSoundVolume);
            EditorGUILayout.PropertyField(closeSound);
            EditorGUILayout.PropertyField(closeSoundVolume);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(_skin.box, GUILayout.ExpandHeight(false));
            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            EditorGUILayout.PropertyField(debugging);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            #region Core
            EndInspectorGUI(typeof(PlayerList));
            #endregion

        }
    }
}