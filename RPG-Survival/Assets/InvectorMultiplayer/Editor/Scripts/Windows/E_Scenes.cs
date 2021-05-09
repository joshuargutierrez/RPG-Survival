using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CBGames.Objects;
using CBGames.Editors;
using Invector.vCamera;
using CBGames.Core;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.Events;
using UnityEditor.Events;
using CBGames.UI;

namespace CBGames.Editors
{
    public class E_Scenes : EditorWindow
    {
        public static float cb_scenes_progress = 0.0f;
        public static string cb_scenes_action = "";
        public static bool cb_scenes_displayProgressBar = false;

        #region Add/Remove Transition/Point
        [MenuItem("CB Games/Scene Transition Manager/Add/Scene Exit", false, 0)]
        public static void CB_Scene_AddExit()
        {
            NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();
            if (!nm)
            {
                if (EditorUtility.DisplayDialog("No Found Network Manager", "The \"NetworkManager\" component doesn't exist in this scene. Please add it before continuing.",
                            "Okay"))
                {
                }
                return;
            }
            GameObject sceneManager = GameObject.Find("SceneManager");
            if (!sceneManager)
            {
                sceneManager = new GameObject("SceneManager");
            }

            GameObject exitPoint = new GameObject("Exit Point");
            E_Helpers.SetObjectIcon(exitPoint, E_Core.h_sceneExitIcon);

            exitPoint.AddComponent<SceneTransition>();
            exitPoint.AddComponent<SetLoadingScreen>();

            SceneDatabase database = (SceneDatabase)AssetDatabase.LoadAssetAtPath(string.Format("Assets{0}Resources{0}ScenesDatabase{0}ScenesDatabase.asset", Path.DirectorySeparatorChar), typeof(SceneDatabase));
            exitPoint.GetComponent<SceneTransition>().SetDatabase(database);
            exitPoint.GetComponent<SceneTransition>().sendAllTogether = nm.syncScenes;

            UnityEvent beforeTravel = (UnityEvent)exitPoint.GetComponent<SceneTransition>().GetType().GetField("BeforeTravel", E_Helpers.allBindings).GetValue(exitPoint.GetComponent<SceneTransition>());
            if (beforeTravel == null)
            {
                exitPoint.GetComponent<SceneTransition>().GetType().GetField("BeforeTravel", E_Helpers.allBindings).SetValue(exitPoint.GetComponent<SceneTransition>(), new UnityEvent());
                beforeTravel = (UnityEvent)exitPoint.GetComponent<SceneTransition>().GetType().GetField("BeforeTravel", E_Helpers.allBindings).GetValue(exitPoint.GetComponent<SceneTransition>());
            }

            UnityEventTools.AddPersistentListener(beforeTravel, exitPoint.GetComponent<SetLoadingScreen>().EnableLoadingScreen);

            exitPoint.GetComponent<BoxCollider>().isTrigger = true;

            exitPoint.layer = 2;
            exitPoint.transform.SetParent(sceneManager.transform);
            exitPoint.transform.position = E_Helpers.PositionInFrontOfEditorCamera();
            Selection.activeGameObject = exitPoint;

            Debug.Log("Successfully created scene exit point!");
        }

        [MenuItem("CB Games/Scene Transition Manager/Add/Scene Entrance", false, 0)]
        public static void CB_Scene_AddEntrance()
        {
            GameObject sceneManager = GameObject.Find("SceneManager");
            if (!sceneManager)
            {
                sceneManager = new GameObject("SceneManager");
            }

            if (!E_Helpers.InspectorTagExists("LoadPoint"))
            {
                E_Helpers.AddInspectorTag("LoadPoint");
            }
            GameObject entrancePoint = new GameObject("Scene Entrance Point");
            E_Helpers.SetObjectIcon(entrancePoint, E_Core.h_sceneEntranceIcon);
            entrancePoint.layer = 2;
            entrancePoint.tag = "LoadPoint";
            entrancePoint.transform.SetParent(sceneManager.transform);
            entrancePoint.transform.position = E_Helpers.PositionInFrontOfEditorCamera();
            Selection.activeGameObject = entrancePoint;
            Debug.Log("Successfully created scene entrance point!");
        }
        #endregion

        #region Update Scene Database
        [MenuItem("CB Games/Scene Transition Manager/Update Scenes Database", false, 0)]
        public static void CB_Scene_UpdateDatabase()
        {
            cb_scenes_displayProgressBar = true;
            cb_scenes_action = "Part 1 of 3: Getting scenes database...";
            cb_scenes_progress = 1 / 2;
            if (!E_Helpers.DirExists(string.Format("Assets{0}Resources{0}ScenesDatabase{0}", Path.DirectorySeparatorChar)))
            {
                E_Helpers.CreateDirectory(string.Format("Assets{0}Resources{0}ScenesDatabase{0}", Path.DirectorySeparatorChar));
            }
            SceneDatabase database;
            if (E_Helpers.FileExists(string.Format("Assets{0}Resources{0}ScenesDatabase{0}ScenesDatabase.asset", Path.DirectorySeparatorChar)))
            {
                database = (SceneDatabase)AssetDatabase.LoadAssetAtPath(string.Format("Assets{0}Resources{0}ScenesDatabase{0}ScenesDatabase.asset", Path.DirectorySeparatorChar), typeof(SceneDatabase));
            }
            else
            {
                database = ScriptableObject.CreateInstance<SceneDatabase>();
                AssetDatabase.CreateAsset(database, string.Format("Assets{0}Resources{0}ScenesDatabase{0}ScenesDatabase.asset", Path.DirectorySeparatorChar));
            }
            cb_scenes_action = "Part 1 of 3: Gathering Scenes List...";
            cb_scenes_progress = 2 / 2;
            List<DatabaseScene> scenes = (database.storedScenesData != null) ? database.storedScenesData : new List<DatabaseScene>();
            scenes = scenes.Where(item => item != null).ToList();
            EditorBuildSettingsScene[] editorScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < editorScenes.Length; i++)
            {
                string sceneName = editorScenes[i].path.Split('/')[editorScenes[i].path.Split('/').Length - 1];
                sceneName = sceneName.Replace(".unity", "");
                cb_scenes_action = "Part 2 of 3: Adding Scene: "+sceneName;
                cb_scenes_progress = i / editorScenes.Length;
                int foundSceneIndex = scenes.FindIndex(target => target.name == sceneName);
                if (foundSceneIndex > -1)
                {
                    scenes[foundSceneIndex].path = editorScenes[i].path;
                    scenes[foundSceneIndex].index = i;
                    scenes[foundSceneIndex].enabled = editorScenes[i].enabled;
                    EditorUtility.SetDirty(scenes[foundSceneIndex]);
                }
                else
                {
                    DatabaseScene newScene = ScriptableObject.CreateInstance<DatabaseScene>();
                    newScene.name = sceneName;
                    newScene.path = editorScenes[i].path;
                    newScene.sceneName = sceneName;
                    newScene.index = i;
                    newScene.enabled = editorScenes[i].enabled;
                    AssetDatabase.CreateAsset(newScene, string.Format("Assets{0}Resources{0}ScenesDatabase{0}{1}.asset", Path.DirectorySeparatorChar, sceneName));
                    Debug.Log("Create " + sceneName + " scene asset.");
                    scenes.Add(newScene);
                    Debug.Log("Added " + sceneName + " scene asset to scene database.");
                }
            }
            SetAllLoadPointsInAllScenes(ref scenes);
            cb_scenes_action = "Part 3 of 3: Saving built database.. ";
            cb_scenes_progress = 1 / 1;
            database.storedScenesData = scenes;
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            cb_scenes_displayProgressBar = false;

            GameObject roomButton = E_Helpers.GetPrefabReference("Assets/InvectorMultiplayer/UI/RoomButton.prefab");
            roomButton.GetComponent<RoomButton>().database = database;
            E_Helpers.SaveAsPrefab(roomButton, "Assets/InvectorMultiplayer/UI/RoomButton.prefab");
            if (EditorUtility.DisplayDialog("Success!", "The scene database has successfully been updated!",
                            "Okay"))
            {
            }
            
            Debug.Log("Successfully updated the ScenesDatabase!");
        }

        public static void SetAllLoadPointsInAllScenes(ref List<DatabaseScene> scenes)
        {
            List<string> points = new List<string>();
            Debug.Log("Saving currently open scene.");
            EditorSceneManager.SaveOpenScenes();
            for (int i = 0; i < scenes.Count; i++)
            {
                points.Clear();
                Debug.Log("Updating scene entrance points for: " + scenes[i].sceneName);
                cb_scenes_action = "Part 3 of 3: Locating entrance points for scene: " + scenes[i].sceneName;
                cb_scenes_progress = i / scenes.Count;
                EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Additive);
                foreach (GameObject go in EditorSceneManager.GetSceneByName(scenes[i].sceneName).GetRootGameObjects())
                {
                    if (go.name == "SceneManager")
                    {
                        foreach (Transform child in go.transform)
                        {
                            if (child.tag == "LoadPoint")
                            {
                                points.Add(child.name);
                            }
                        }
                    }
                }
                if (scenes[i].entrancePoints != null)
                {
                    scenes[i].entrancePoints.Clear();
                }
                else
                {
                    scenes[i].entrancePoints = new List<string>();
                }
                scenes[i].entrancePoints.AddRange(points);
                EditorUtility.SetDirty(scenes[i]);
                if (EditorSceneManager.GetActiveScene().name != scenes[i].sceneName)
                {
                    EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(scenes[i].path), true);
                }
            }
        }
        #endregion
    }
}