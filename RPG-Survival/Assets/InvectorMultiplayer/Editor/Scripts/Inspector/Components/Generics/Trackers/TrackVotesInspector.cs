using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackVotes), true)]
    public class TrackVotesInspector : BaseEditor
    {
        #region Properties
        SerializedProperty indexNumberToTrack;
        SerializedProperty texts;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            indexNumberToTrack = serializedObject.FindProperty("indexNumberToTrack");
            texts = serializedObject.FindProperty("texts");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Votes", 
                "This component requires a \"UICoreLogic\" component somewhere in the scene. \n\n" +
                "This component will track the number of received votes for a particular scene index. Then it will " +
                "modify the targeted texts to display that number.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(indexNumberToTrack);
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackVotes));
            #endregion
        }
    }
}