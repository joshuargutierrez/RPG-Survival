using UnityEditor;
using UnityEngine;
using CBGames.Core;
using CBGames.Editors;
using CBGames.Objects;
using System.Collections.Generic;

namespace CBGames.Inspector
{
    [CustomEditor(typeof(SceneTransition), true)]
    public class SceneTransitionInspector : BaseEditor
    {
        #region CustomEditorVariables
        SceneDatabase database;
        int selectedDatabaseIndex = 0;
        int selectedEntrancePointIndex = 0;
        List<string> sceneNames = new List<string>();
        int previous_selectedDatabaseIndex = 0;
        GUIStyle error;
        List<string> availableInputs = new List<string>();
        int selectedButton = 0;
        #endregion

        #region Properties
        SerializedProperty autoTravel;
        SerializedProperty buttonToTravel;
        SerializedProperty activeOnEnter;
        SerializedProperty LoadSceneName;
        SerializedProperty SpawnAtPoint;
        SerializedProperty sendAllTogether;
        SerializedProperty inspectorDatabase;
        SerializedProperty OnOwnerEnterTrigger;
        SerializedProperty OnOwnerExitTrigger;
        SerializedProperty OnAnyPlayerEnterTrigger;
        SerializedProperty OnAnyPlayerExitTrigger;
        SerializedProperty BeforeTravel;
        #endregion

        protected override void OnEnable()
        {

            #region Properties
            autoTravel = serializedObject.FindProperty("autoTravel");
            buttonToTravel = serializedObject.FindProperty("buttonToTravel");
            activeOnEnter = serializedObject.FindProperty("activeOnEnter");
            LoadSceneName = serializedObject.FindProperty("LoadSceneName");
            SpawnAtPoint = serializedObject.FindProperty("SpawnAtPoint");
            inspectorDatabase = serializedObject.FindProperty("database");
            sendAllTogether = serializedObject.FindProperty("sendAllTogether");
            OnOwnerEnterTrigger = serializedObject.FindProperty("OnOwnerEnterTrigger");
            OnOwnerExitTrigger = serializedObject.FindProperty("OnOwnerExitTrigger");
            OnAnyPlayerEnterTrigger = serializedObject.FindProperty("OnAnyPlayerEnterTrigger");
            OnAnyPlayerExitTrigger = serializedObject.FindProperty("OnAnyPlayerExitTrigger");
            BeforeTravel = serializedObject.FindProperty("BeforeTravel");
            #endregion

            sceneNames.Clear();
            database = (SceneDatabase)inspectorDatabase.objectReferenceValue;
            if (!database)
            {
                selectedDatabaseIndex = -1;
            }
            else
            {
                for (int i = 0; i < database.storedScenesData.Count; i++)
                {
                    if (database.storedScenesData[i].sceneName == LoadSceneName.stringValue)
                    {
                        selectedDatabaseIndex = i;
                        previous_selectedDatabaseIndex = i;
                    }
                    sceneNames.Add(database.storedScenesData[i].sceneName);
                }
                if (selectedDatabaseIndex != -1)
                {
                    for (int i = 0; i < database.storedScenesData[selectedDatabaseIndex].entrancePoints.Count; i++)
                    {
                        if (database.storedScenesData[selectedDatabaseIndex].entrancePoints[i] == SpawnAtPoint.stringValue)
                        {
                            selectedEntrancePointIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    selectedEntrancePointIndex = 0;
                    selectedEntrancePointIndex = -1;
                }
            }
            error = new GUIStyle();
            error.normal.textColor = Color.red;

            //Get Available Buttons
            availableInputs.Clear();
            availableInputs = E_Helpers.GetAllInputAxis();

            base.OnEnable();
        }
        public override void OnInspectorGUI()
        {
            #region Core
            base.OnInspectorGUI();
            SceneTransition sic = (SceneTransition)target;
            DrawTitleBar(
                "Scene Exit", 
                "This is used to travel to another scene. It will load the " +
                "\"LoadSceneName\" and drop your player at the \"SpawnAtPoint\" within that scene." +
                "This data is all stored within the SceneDatabase. Run \"Scene Transition Manager " +
                "> Update Scene Database\" to build this, then place that built database into the " +
                "\"Database\" variable slot.", 
                E_Core.h_sceneExitIcon
            );
            #endregion

            #region Properties
            EditorGUILayout.PropertyField(inspectorDatabase);
            if (!(SceneDatabase)inspectorDatabase.objectReferenceValue)
            {
                EditorGUILayout.HelpBox("No \"Database\" is present. Please add a \"Database\" before continuing.", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                if ((SceneDatabase)inspectorDatabase.objectReferenceValue)
                {
                    OnEnable();
                }
            }
            else if (!database)
            {
                OnEnable();
            }
            else
            {
                EditorGUILayout.PropertyField(autoTravel);
                if (autoTravel.boolValue == false)
                {
                    selectedButton = (availableInputs.Contains(buttonToTravel.stringValue)) ? availableInputs.IndexOf(buttonToTravel.stringValue) : 0;
                    selectedButton = EditorGUILayout.Popup("Button To Travel", selectedButton, availableInputs.ToArray());
                    buttonToTravel.stringValue = availableInputs[selectedButton];
                    EditorGUILayout.PropertyField(activeOnEnter, true);
                }
                selectedDatabaseIndex = EditorGUILayout.Popup("Load Scene Name", selectedDatabaseIndex, sceneNames.ToArray());
                if (selectedDatabaseIndex == -1)
                {
                    selectedDatabaseIndex = 0;
                    OnEnable();
                }
                LoadSceneName.stringValue = sceneNames[selectedDatabaseIndex];
                if (previous_selectedDatabaseIndex != selectedDatabaseIndex)
                {
                    previous_selectedDatabaseIndex = selectedDatabaseIndex;
                    selectedEntrancePointIndex = 0;
                }

                EditorGUILayout.BeginHorizontal();
                if (database.storedScenesData[selectedDatabaseIndex].entrancePoints.Count > 0)
                {
                    selectedEntrancePointIndex = EditorGUILayout.Popup("Spawn At Point", selectedEntrancePointIndex, database.storedScenesData[selectedDatabaseIndex].entrancePoints.ToArray());
                    if (selectedEntrancePointIndex == -1)
                    {
                        SpawnAtPoint.stringValue = null;
                        EditorGUILayout.LabelField("** Select A Spawn Point");
                    }
                    else
                    {
                        SpawnAtPoint.stringValue = database.storedScenesData[selectedDatabaseIndex].entrancePoints[selectedEntrancePointIndex];
                    }
                }
                else
                {
                    SpawnAtPoint.stringValue = null;
                    EditorGUILayout.HelpBox("There are no available entry points in this scene. Either " +
                        "they haven't been created or the \"Database\" is out of date. You can update the" +
                        " database by running \"CB Games > Scene Transition Manager > Update Scenes Database\".", MessageType.Error);
                    EditorGUILayout.LabelField("Spawn At Point", GUILayout.MinWidth(0));
                    EditorGUILayout.LabelField("No Points Found In Database.");
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(sendAllTogether);
                GUI.skin = _original;
                CBEditor.SetColorToEditorStyle(_originalHolder, _originalFoldout);
                EditorGUILayout.HelpBox("This event is called right before traveling to the new scene. It will be invoked on each client if the 'Send All Together' is true.", MessageType.None);
                EditorGUILayout.PropertyField(BeforeTravel, true);
                EditorGUILayout.HelpBox("These events are called for ANY player that enters this trigger.", MessageType.None);
                EditorGUILayout.PropertyField(OnAnyPlayerEnterTrigger, true);
                EditorGUILayout.PropertyField(OnAnyPlayerExitTrigger, true);
                EditorGUILayout.HelpBox("These events are only called for the network player that you are controlling. This isn't called for other network players entering/exiting this trigger.", MessageType.None);
                EditorGUILayout.PropertyField(OnOwnerEnterTrigger, true);
                EditorGUILayout.PropertyField(OnOwnerExitTrigger, true);
                GUI.skin = _skin;
                CBEditor.SetColorToEditorStyle(_skinHolder, _skinHolder);
            }
            #endregion

            #region End Core
            EndInspectorGUI(typeof(SceneTransition));
            #endregion
        }
    }
}