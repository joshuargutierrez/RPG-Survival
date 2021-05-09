using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace CBGames.Inspector
{
    [CustomPropertyDrawer(typeof(LayerSelector))]
    public class LayerSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);
            int index = prop.intValue;
            if (index > 31)
            {
                Debug.Log("CustomPropertyDrawer, layer index is to high '" + index + "', is set to 31.");
                index = 31;
            }
            else if (index < 0)
            {
                Debug.Log("CustomPropertyDrawer, layer index is to low '" + index + "', is set to 0");
                index = 0;
            }

            //Restore Tooltip
            string tooltip;
            var attributes = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                tooltip = ((TooltipAttribute)attributes[0]).tooltip;
            }
            else
            {
                tooltip = null;
            }
            label.tooltip = tooltip;

            prop.intValue = EditorGUI.LayerField(pos, label, index);
            EditorGUI.EndProperty();
        }
    }
}

