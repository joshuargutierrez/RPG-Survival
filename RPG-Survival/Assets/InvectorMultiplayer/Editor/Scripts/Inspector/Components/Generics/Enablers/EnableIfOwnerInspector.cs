using UnityEngine;
using UnityEditor;
using CBGames.Editors;
using CBGames.UI;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(EnableIfOwner), true)]
    public class EnableIfOwnerInspector : BaseEditor
    {
        #region Properties
        SerializedProperty isOwner;
        #pragma warning disable CS0108
        SerializedProperty targets;
        #pragma warning restore CS0108
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            isOwner = serializedObject.FindProperty("isOwner");
            targets = serializedObject.FindProperty("targets");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            DrawTitleBar("Enable If Owner", 
                "This component requires to have a \"UICoreLogic\" somewhere in your scene.\n\n" +
                "This component will enable target gameobjects if you are the Photon Rooms " +
                "MasterClient or not.", 
                E_Core.h_uiIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(isOwner);
            EditorGUILayout.PropertyField(targets, true);
            #endregion

            #region Core
            EndInspectorGUI(typeof(EnableIfOwner));
            #endregion
        }
    }
}