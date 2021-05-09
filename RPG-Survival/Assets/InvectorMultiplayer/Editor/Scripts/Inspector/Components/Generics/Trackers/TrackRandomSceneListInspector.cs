using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackRandomSceneList), true)]
    public class TrackRandomSceneListInspector : BaseEditor
    {
        #region Properties
        SerializedProperty indexNumberToTrack;
        SerializedProperty texts;
        SerializedProperty images;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            indexNumberToTrack = serializedObject.FindProperty("indexNumberToTrack");
            texts = serializedObject.FindProperty("texts");
            images = serializedObject.FindProperty("images");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar(
                "Track Random Scene List", 
                "This component requires a \"UICoreLogic\" component somewhere in the scene. \n\n" +
                "This component will track the randomly generated scene list (from the original " +
                "specified scenelist in the UICoreLogic component) and display the image/scene " +
                "name at the specified index. I will overwrite the images sprite and texts text " +
                "to be that is at the specified index of the random version.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(indexNumberToTrack);
            EditorGUILayout.PropertyField(images, true);
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackRandomSceneList));
            #endregion
        }
    }
}