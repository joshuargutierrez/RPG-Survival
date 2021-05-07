using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Invector
{
    [CustomPropertyDrawer(typeof(vDamageModifier))]
    public class vDamageModifierDrawer : PropertyDrawer
    {
        public GUISkin skin;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldSkin = GUI.skin;
            if (skin == null) skin =(GUISkin) Resources.Load("vSkin");
            GUI.skin = skin;
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty damageTypes = property.FindPropertyRelative("damageTypes");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty percentage = property.FindPropertyRelative("percentage");
            position.height = EditorGUIUtility.singleLineHeight;
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if(property.isExpanded)
            {
                // GUI.Box(position, "");        
                using (new EditorGUI.IndentLevelScope())
                {
                   
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, name);
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, value);
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, percentage);
                    position.y += EditorGUIUtility.singleLineHeight;                  
                    Rect box = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * (damageTypes.arraySize + 1) + 5));
                    GUI.Box(box, "", "LightBox");
                    GUI.Label(EditorGUI.IndentedRect(new Rect(position.x+5, position.y, position.width-5, EditorGUIUtility.singleLineHeight)), "Damage Types");
                    Rect btnClear = new Rect(position.x + position.width - position.height-1, position.y+1, position.height, position.height);
                    if (GUI.Button(btnClear, new GUIContent("x", "Clear Types"), "LightBox"))
                    {
                        damageTypes.arraySize = 0;
                    }
                    position.y += EditorGUIUtility.singleLineHeight + 2f;
                    position.width -= 8;
                    position.x += 5;
                    for (int i = 0; i < damageTypes.arraySize; i++)
                    {
                        var p = damageTypes.GetArrayElementAtIndex(i);
                        var _label = EditorGUI.BeginProperty(position, new GUIContent($"Type {i.ToString("0")}"), p);
                        EditorGUI.PropertyField(position, p, _label);
                        EditorGUI.EndProperty();
                        position.y += EditorGUIUtility.singleLineHeight;
                    }
                    position.y += 1f;
                    position.width += 3;
                    Rect btnA = new Rect(position.x + position.width - position.height * 2f+1, position.y, position.height, position.height);
                    Rect btnB = new Rect(position.x + position.width - position.height, position.y, position.height, position.height);
                    if (GUI.Button(btnA, "-", "LightBox"))
                    {
                        if (damageTypes.arraySize > 0) damageTypes.arraySize--;
                    }
                    if (GUI.Button(btnB, "+", "LightBox"))
                    {
                        damageTypes.arraySize++;
                    }
                }
            }
         
            EditorGUI.EndProperty();
            GUI.skin = oldSkin;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded? EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.singleLineHeight *(property.FindPropertyRelative("damageTypes").arraySize + 1) : EditorGUIUtility.singleLineHeight;
        }
    }
}