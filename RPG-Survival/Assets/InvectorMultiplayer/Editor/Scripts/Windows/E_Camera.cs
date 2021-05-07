using CBGames.Core;
using CBGames.Player;
using Invector.vCharacterController;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_Camera : EditorWindow
    {
        [MenuItem("CB Games/Camera/Add Death Camera", false, 0)]
        public static void CB_AddDeathCamera()
        {
            GameObject deathCam = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/DeathCamera.prefab");
            NetworkManager nm = FindObjectOfType<NetworkManager>();
            if (nm == null)
            {
                if (EditorUtility.DisplayDialog("Missing Network Manager", 
                    "This component was added to the scene but there was not NetworkManager found in this scene. " +
                    "It is highly suggested that you make this a child of the NetworkManager or of something " +
                    "that is persistant between scenes.",
                                "Okay"))
                {
                    
                }
            }
            else
            {
                deathCam.transform.SetParent(nm.gameObject.transform);
                Selection.activeGameObject = deathCam;
                if (FindObjectOfType<SyncPlayer>())
                {
                    if (EditorUtility.DisplayDialog("Player Found In Scene",
                        "A convert multiplayer character was found in the scene do you want to add the unity events " +
                        "necessary to enable the death camera system on player death?",
                                    "Yes", "No"))
                    {
                        foreach (SyncPlayer player in FindObjectsOfType<SyncPlayer>())
                        {
                            if (!E_PlayerEvents.HasUnityEvent(player.gameObject.GetComponent<vThirdPersonController>().onDead, "DeadEnableDeathCam", player))
                            {
                                UnityEventTools.AddPersistentListener(player.gameObject.GetComponent<vThirdPersonController>().onDead, player.DeadEnableDeathCam);
                            }
                        }
                        if (EditorUtility.DisplayDialog("Remember To Save!",
                        "Remember to save the changes back to the prefab on these converted players!",
                                    "Okay Thanks!"))
                        {

                        }
                    }
                    else
                    {
                        DisplayEventRequirement();
                    }
                }
                else
                {
                    DisplayEventRequirement();
                }
            }

            void DisplayEventRequirement()
            {
                if (EditorUtility.DisplayDialog("Friendly Reminder For Missing Events",
                    "This will not trigger by itself. You will be required to trigger it when you want to " +
                    "allow the player to switch to targeting other players. A chain of events is available " +
                    "for you to add if you would like:\n\n" +
                    "vThirdPersonController (On Dead Event): Add SyncPlayer.DeadEnableDeathCam event",
                                "Okay Thanks!"))
                {

                }
            }
        }
    }
}