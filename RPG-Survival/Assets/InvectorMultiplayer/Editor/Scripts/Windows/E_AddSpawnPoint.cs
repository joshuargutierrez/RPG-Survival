using UnityEngine;
using UnityEditor;
using CBGames.Core;

namespace CBGames.Editors
{
    public class E_AddSpawnPoint : EditorWindow
    {
        [MenuItem("CB Games/Network Manager/Add Spawn Point", false, 0)]
        public static void CB_AddSpawnPoint()
        {
            // Will add this new tag to the inspector if it doesn't already exist
            E_Helpers.AddInspectorTag("SpawnPoint");

            GameObject spawnPoint = new GameObject("Player Spawn Point");
            spawnPoint.layer = 2;
            spawnPoint.tag = "SpawnPoint";

            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("Missing Network Manager", "No NetworkManager object was found in this scene. Be sure to add one since that is what these spawn points are for. :) \r\n" +
                    "\r\n" +
                    "",
                            "Okay"))
                {}
            }
            GameObject spawnPointParentGO = null;
            if (!GameObject.Find("Spawn Points"))
            {
                spawnPointParentGO = new GameObject("Spawn Points");
                spawnPointParentGO.layer = 2;
                spawnPointParentGO.tag = "SpawnPoints";
                Debug.Log("Generated \"Spawn Points\" holder gameObject.");
            }
            else
            {
                spawnPointParentGO = GameObject.Find("Spawn Points");
            }
            spawnPoint.transform.SetParent(spawnPointParentGO.transform);
            spawnPoint.transform.position = E_Helpers.PositionInFrontOfEditorCamera();
            E_Helpers.SetObjectIcon(spawnPoint, E_Core.h_spawnPointIcon);
            Selection.activeGameObject = spawnPoint;
            Debug.Log("Added a new \"Player Spawn Point\" gameobject to the scene.");
        }

        [MenuItem("CB Games/Network Manager/Respawn/Add Respawn Point", false, 0)]
        public static void CB_AddRespawnPoint()
        {
            // Will add this new tag to the inspector if it doesn't already exist
            E_Helpers.AddInspectorTag("RespawnPoint");

            GameObject respawnPoint = new GameObject("Respawn Point");
            respawnPoint.layer = 2;
            respawnPoint.tag = "RespawnPoint";

            GameObject respawnPointParentGO = null;
            if (!GameObject.Find("Spawn Points"))
            {
                respawnPointParentGO = new GameObject("Spawn Points");
                respawnPointParentGO.layer = 2;
                Debug.Log("Generated \"Spawn Points\" holder gameObject.");
            }
            else
            {
                respawnPointParentGO = GameObject.Find("Spawn Points");
            }
            respawnPoint.transform.SetParent(respawnPointParentGO.transform);
            respawnPoint.transform.position = E_Helpers.PositionInFrontOfEditorCamera();
            E_Helpers.SetObjectIcon(respawnPoint, E_Core.h_respawnPointIcon);
            Selection.activeGameObject = respawnPoint;
            Debug.Log("Added a new \"Respawn Point\" gameobject to the scene.");
        }
    }
}
