using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Invector.vCharacterController;

public class WaveManager : MonoBehaviour
{
    private vThirdPersonController controller;
    private AssetBundle myLoadedAssetBundle;
    private string[] scenePaths;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<vThirdPersonController>();
        myLoadedAssetBundle = AssetBundle.LoadFromFile("Assets/AssetBundles/scenes");
        scenePaths = myLoadedAssetBundle.GetAllScenePaths();
    }

    // Update is called once per frame
    void Update()
    {
        if(controller.currentHealth <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
