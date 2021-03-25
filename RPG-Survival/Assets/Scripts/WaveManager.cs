using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Invector.vCharacterController;

public class WaveManager : MonoBehaviour
{
    private vThirdPersonController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<vThirdPersonController>();
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
