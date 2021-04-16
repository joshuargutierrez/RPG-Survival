using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLoad : MonoBehaviour
{
    private GameObject character;

    // Start is called before the first frame update
    void Start()
    {
        string characterName = StaticClass.CrossSceneInformation;

        Debug.Log("Loading Prefab " + characterName + ".");

        character = Resources.Load("Prefabs/" + characterName) as GameObject;

        Instantiate(character, new Vector3(0, 0, 0), Quaternion.identity);

        Debug.Log("Prefab " + characterName + "loaded.");
    }

    
}
