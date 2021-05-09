using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(VisualizeRooms), true)]
    public class VisualizeRoomsInspector : BaseEditor
    {
        #region Properties
        SerializedProperty parentObj;
        SerializedProperty roomButton;
        SerializedProperty autoUpate;
        SerializedProperty canDisplaySessionRooms;
        SerializedProperty onlyDisplaySessionRooms;
        SerializedProperty filterRooms;
        SerializedProperty debugging;
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            parentObj = serializedObject.FindProperty("parentObj");
            roomButton = serializedObject.FindProperty("roomButton");
            autoUpate = serializedObject.FindProperty("autoUpate");
            canDisplaySessionRooms = serializedObject.FindProperty("canDisplaySessionRooms");
            onlyDisplaySessionRooms = serializedObject.FindProperty("onlyDisplaySessionRooms");
            filterRooms = serializedObject.FindProperty("filterRooms");
            debugging = serializedObject.FindProperty("debugging");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            VisualizeRooms vr = (VisualizeRooms)target;
            DrawTitleBar(
                "Visualize Photon Rooms", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will spawn child objects under your selected parentObj " +
                "for every photon room that it finds. If that child object holds a component called \"RoomButton\" " +
                "it will send the data about that photon room to that component.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(parentObj);
            EditorGUILayout.PropertyField(roomButton);
            EditorGUILayout.PropertyField(autoUpate);
            EditorGUILayout.PropertyField(canDisplaySessionRooms);
            EditorGUILayout.PropertyField(onlyDisplaySessionRooms);
            EditorGUILayout.PropertyField(filterRooms);
            EditorGUILayout.PropertyField(debugging);
            #endregion

            #region Core
            EndInspectorGUI(typeof(VisualizeRooms));
            #endregion
        }
    }
}