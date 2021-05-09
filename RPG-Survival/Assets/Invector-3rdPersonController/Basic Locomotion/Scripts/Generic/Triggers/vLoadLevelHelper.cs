using Invector.vCharacterController;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Invector.Utils
{
    public static class LoadLevelHelper
    {
        public static vThirdPersonInput targetCharacter;
        public static string spawnPointName;
        public static string sceneName;

        public static void LoadScene(string _sceneName, string _spawnPointName, vThirdPersonInput tpInput)
        {
            if (!tpInput) return;
            targetCharacter = tpInput;
            spawnPointName = _spawnPointName;
            sceneName = _sceneName;
            targetCharacter.StartCoroutine(LoadAsyncScene());

        }

        static IEnumerator LoadAsyncScene()
        {
            // Set the current Scene to be able to unload it later
            Scene currentScene = SceneManager.GetActiveScene();
            if (!currentScene.name.Equals(sceneName))
            {
                SceneManager.sceneUnloaded += OnSceneLoaded;
                // The Application loads the Scene in the background at the same time as the current Scene.
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                // Wait until the last operation fully loads to return anything
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
                // Move the GameObject (you attach this in the Inspector) to the newly loaded Scene
                SceneManager.MoveGameObjectToScene(targetCharacter.gameObject, SceneManager.GetSceneByName(sceneName));
                // Unload the previous Scene
                SceneManager.UnloadSceneAsync(currentScene);
            }
            else MoveCharaterToSpawnPoint();
        }

        static void OnSceneLoaded(Scene arg0)
        {
            MoveCharaterToSpawnPoint();
            SceneManager.sceneUnloaded -= OnSceneLoaded;
        }

        static void MoveCharaterToSpawnPoint()
        {
            var spawnPoint = GameObject.Find(spawnPointName);
            //Set character position to target spawnPoint
            if (spawnPoint && targetCharacter)
            {
                targetCharacter.transform.position = spawnPoint.transform.position;
                targetCharacter.transform.rotation = spawnPoint.transform.rotation;
                //reset character camera
                //targetCharacter.StartCoroutine(ResetCamera());
            }
        }

        //static IEnumerator ResetCamera()
        //{
        //    yield return new WaitForEndOfFrame();
        //    if (targetCharacter.tpCamera)
        //        targetCharacter.tpCamera.ResetCamera();
        //}
    }
}