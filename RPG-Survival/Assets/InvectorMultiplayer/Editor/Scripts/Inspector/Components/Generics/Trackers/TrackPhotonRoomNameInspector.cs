using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(TrackPhotonRoomName), true)]
    public class TrackPhotonRoomNameInspector : BaseEditor
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
                "Track Photon Room Name", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will change the targeted Text to match whatever the current " +
                "photon room name is.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(texts, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(TrackPhotonRoomName));
            #endregion
        }
    }
}