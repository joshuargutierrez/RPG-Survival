using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    [InitializeOnLoad]
    public static class HierarchyWindowGameObjectIcon
    {
        const string IgnoreIcons = "GameObject Icon, Prefab Icon";

        static HierarchyWindowGameObjectIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var content = EditorGUIUtility.ObjectContent(EditorUtility.InstanceIDToObject(instanceID), null);

            if (content.image != null && !IgnoreIcons.Contains(content.image.name))
                GUI.DrawTexture(new Rect(selectionRect.xMax - 34, selectionRect.yMin, 16, 16), content.image);

        }
    }
}
