using UnityEngine;
using UnityEditor;

namespace CBGames.Editors
{
    public class CopyComp : Editor
    {
        public static SerializedObject CB_COMP_COPY = null;

        [MenuItem("CONTEXT/Component/CB - Copy Component Values", false, 1)]
        public static void CB_CONTEXT_COPY(MenuCommand command)
        {
            CB_COMP_COPY = new SerializedObject(command.context);
        }
        [MenuItem("CONTEXT/Component/CB - Paste Component Values", false, 2)]
        public static void CB_CONTEXT_PASTE(MenuCommand command)
        {
            if (CB_COMP_COPY == null)
            {
                if (EditorUtility.DisplayDialog("No Source Component",
                        "You don't have a source component targeted. First select \"Copy Component Values\" on the " +
                        "component that you would like to copy.",
                        "Okay"))
                {
                    return;
                }
            }
            else
            {
                SerializedObject target = new SerializedObject(command.context);
                if (EditorUtility.DisplayDialog("Paste Copied Component?",
                        "Would you like to paste \"" + CB_COMP_COPY.targetObject + "\" component's values to: \""+ target.targetObject + "\"?",
                        "Yes", "No"))
                {
                    E_Helpers.CopyComponentValues((Component)CB_COMP_COPY.targetObject, (Component)target.targetObject, true);
                }
            }
        }
    }
}