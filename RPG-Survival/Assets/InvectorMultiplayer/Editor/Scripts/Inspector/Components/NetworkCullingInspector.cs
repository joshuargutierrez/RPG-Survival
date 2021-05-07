using UnityEditor;
using CBGames.Player;
using UnityEngine;
using System.Collections.Generic;

namespace CBGames.Editors
{
    [CustomEditor(typeof(NetworkCulling), true)]
    public class NetworkCullingInspector : BaseEditor
    {
        #region Properties
        SerializedProperty cullDistances;
        SerializedProperty last_group;
        SerializedProperty last_moveSpeed;
        SerializedProperty last_rotateSpeed;
        SerializedProperty drawGizmos;
        Color[] colorArray = new Color[]
        {
            new Color32(0, 255, 81, 255),
            new Color32(0, 69, 116, 255),
            new Color32(58, 100, 100, 100),
            new Color32(37, 100, 100, 100),
            new Color32(14, 100, 100, 100)
        };
        Color largerColor = new Color(191, 169, 0, 255);
        #endregion

        protected override void OnEnable()
        {
            #region Properties
            cullDistances = serializedObject.FindProperty("cullDistances");
            last_group = serializedObject.FindProperty("last_group");
            last_moveSpeed = serializedObject.FindProperty("last_moveSpeed");
            last_rotateSpeed = serializedObject.FindProperty("last_rotateSpeed");
            drawGizmos = serializedObject.FindProperty("drawGizmos");
            #endregion

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            NetworkCulling nc = (NetworkCulling)target;
            DrawTitleBar(
                "Network Culling",
                "This component is used to reduce the number of network calls that are received by all players. " +
                "It will make a very far player only receive a few network packets while the very close player " +
                "will receive all network packets.",
                E_Core.h_playerIcon
            );
            #endregion

            #region Properties

            EditorGUILayout.PropertyField(drawGizmos);
            #region Culling
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("Collab.NoInternet"), GUILayout.ExpandWidth(false), GUILayout.Height(30), GUILayout.Width(15));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Culling Distance Settings", _skin.textField);
            GUILayout.EndVertical();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nc.show_culling = !nc.show_culling;
            }
            GUILayout.EndHorizontal();
            if (nc.show_culling == true)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIContent content = new GUIContent("Add New Cull Group", EditorGUIUtility.FindTexture("d_Toolbar Plus More"));
                // Add Cull group button
                if (GUILayout.Button(content))
                {
                    byte prev_group = 0;
                    float prev_distance = 0;
                    float prev_moveSpeed = 6;
                    float prev_rotateSpeed = 6;
                    if (cullDistances.arraySize > 0)
                    {
                        SerializedProperty prev_reference = cullDistances.GetArrayElementAtIndex(cullDistances.arraySize - 1);
                        SerializedProperty s_prev_group = prev_reference.FindPropertyRelative("interest_group");
                        SerializedProperty s_prev_distance = prev_reference.FindPropertyRelative("distance");
                        SerializedProperty s_prev_moveSpeed = prev_reference.FindPropertyRelative("moveSpeed");
                        SerializedProperty s_prev_rotateSpeed = prev_reference.FindPropertyRelative("rotateSpeed");

                        prev_group = (byte)s_prev_group.intValue;
                        prev_distance = s_prev_distance.floatValue;
                        prev_moveSpeed = s_prev_moveSpeed.floatValue;
                        prev_rotateSpeed = s_prev_rotateSpeed.floatValue;
                    }
                    cullDistances.arraySize += 1;
                    SerializedProperty cur_reference = cullDistances.GetArrayElementAtIndex(cullDistances.arraySize-1);
                    SerializedProperty group = cur_reference.FindPropertyRelative("interest_group");
                    SerializedProperty distance = cur_reference.FindPropertyRelative("distance");
                    SerializedProperty circleColor = cur_reference.FindPropertyRelative("circleColor");
                    SerializedProperty moveSpeed = cur_reference.FindPropertyRelative("moveSpeed");
                    SerializedProperty rotateSpeed = cur_reference.FindPropertyRelative("rotateSpeed");

                    group.intValue = prev_group + 1;
                    distance.floatValue = prev_distance + 10;
                    circleColor.colorValue = (cullDistances.arraySize <= colorArray.Length) ? colorArray[cullDistances.arraySize - 1] : largerColor;
                    moveSpeed.floatValue = prev_moveSpeed;
                    rotateSpeed.floatValue = prev_rotateSpeed;
                }
                GUILayout.EndHorizontal();
                CullDistance prev = null;
                for (int i = 0; i < cullDistances.arraySize; i++)
                {
                    SerializedProperty reference = cullDistances.GetArrayElementAtIndex(i);
                    SerializedProperty group = reference.FindPropertyRelative("interest_group");
                    SerializedProperty distance = reference.FindPropertyRelative("distance");
                    SerializedProperty circleColor = reference.FindPropertyRelative("circleColor");
                    SerializedProperty moveSpeed = reference.FindPropertyRelative("moveSpeed");
                    SerializedProperty rotateSpeed = reference.FindPropertyRelative("rotateSpeed");

                    EditorGUILayout.BeginHorizontal(_skin.window);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    content = new GUIContent("Delete Group", EditorGUIUtility.FindTexture("d_Toolbar Minus"));
                    if (cullDistances.arraySize > 1)
                    {
                        if (GUILayout.Button(content, _skin.box))
                        {
                            cullDistances.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    if (prev != null && group.intValue <= prev.interest_group && prev.interest_group < 255)
                    {
                        group.intValue = prev.interest_group + 1;
                    }
                    else if (prev != null && prev.interest_group >= 255)
                    {
                        EditorGUILayout.HelpBox("You're not allowed to add more interest groups above 255. Lower your " +
                            "previous culling groups interest groups to add more.", MessageType.Error);
                        break;
                    }
                    EditorGUILayout.PropertyField(group);
                    if (prev != null && distance.floatValue < prev.distance)
                    {
                        distance.floatValue = prev.distance + 1;
                    }
                    EditorGUILayout.PropertyField(distance);
                    EditorGUILayout.Space();
                    if (prev != null && moveSpeed.floatValue > prev.moveSpeed)
                    {
                        moveSpeed.floatValue = prev.moveSpeed;
                    }
                    EditorGUILayout.PropertyField(moveSpeed);
                    if (prev != null && rotateSpeed.floatValue > prev.moveSpeed)
                    {
                        rotateSpeed.floatValue = prev.rotateSpeed;
                    }
                    EditorGUILayout.PropertyField(rotateSpeed);
                    EditorGUILayout.Space();
                    if (prev != null && circleColor.colorValue == prev.circleColor)
                    {
                        Color temp = circleColor.colorValue;
                        temp.b += 1;
                        circleColor.colorValue = temp;
                    }
                    EditorGUILayout.PropertyField(circleColor);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);

                    if (prev == null) prev = new CullDistance(0, 0, Color.red, 5, 5);
                    prev.interest_group = (byte)group.intValue;
                    prev.distance = distance.floatValue;
                    prev.moveSpeed = moveSpeed.floatValue;
                    prev.rotateSpeed = rotateSpeed.floatValue;
                    prev.circleColor = circleColor.colorValue;
                }
            }
            #endregion
            
            GUILayout.Space(5);

            #region Last Group
            GUILayout.BeginHorizontal(_skin.customStyles[1]);
            GUILayout.Label(EditorGUIUtility.FindTexture("BuildSettings.Web.Small"), GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Space(9);
            GUILayout.Label("Last Cull Group Settings", _skin.textField);
            GUILayout.EndVertical();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu"), _skin.label, GUILayout.Width(50)))
            {
                nc.show_lastGroup = !nc.show_lastGroup;
            }
            GUILayout.EndHorizontal();
            if (nc.show_lastGroup == true)
            {
                if (last_group.intValue < 10)
                {
                    EditorGUILayout.HelpBox("You're setting the interest group low. Make sure you're not overwriting " +
                        "another one of your previously used photon interest groups.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(last_group, new GUIContent("Last Interest Group"));
                EditorGUILayout.PropertyField(last_moveSpeed, new GUIContent("Last Move Speed"));
                EditorGUILayout.PropertyField(last_rotateSpeed, new GUIContent("Last Rotation Speed"));
            }
            #endregion

            #endregion

            EndInspectorGUI(typeof(NetworkCulling));
        }
    }
}