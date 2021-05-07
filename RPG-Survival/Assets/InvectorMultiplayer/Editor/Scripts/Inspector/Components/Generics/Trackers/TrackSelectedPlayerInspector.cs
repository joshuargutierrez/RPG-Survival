using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackSelectedPlayer), true)]
    public class TrackSelectedPlayerInspector : BaseEditor
    {
        #region Properties
        SerializedProperty texts;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            texts = serializedObject.FindProperty("texts");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Selected Player", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will change the targeted Text to match whatever playerPrefab " +
                "you currently have set in the NetworkManager.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackSelectedPlayer));
            #endregion
        }
    }
}