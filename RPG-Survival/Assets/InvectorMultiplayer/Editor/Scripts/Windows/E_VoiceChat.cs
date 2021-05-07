using CBGames.Core;
using CBGames.Player;
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_VoiceChat : EditorWindow
    {
        [MenuItem("CB Games/Chat/Voice/Add Room Voice Chat", false, 0)]
        public static void CB_AddGlobalVoiceChat()
        {
            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("Network Manager Not Found", "No NetworkManager was found in the scene. Please add a NetworkManager to the scene and try again.",
                            "Okay"))
                {
                    return;
                }
            }
            else
            {
                GameObject vc = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/Voice/VoiceRecorder.prefab");
                vc.transform.SetParent(FindObjectOfType<NetworkManager>().transform);
                E_Helpers.SetObjectIcon(vc, E_Core.h_voiceIcon);
                if (EditorUtility.DisplayDialog("Added Voice Chat To Scene", "The \"VoiceRecorder\" has been successfully added to the scene. " +
                    "Make sure that each spawnable player that you want to be able to transmit voice over the network also has the \"VoiceChat\" " +
                    "component on them. This can be done when first setting up your player using the Convert Player menu. Read the help window " +
                    "on this component for other needed items if doing this manually.",
                            "Okay"))
                {
                    Selection.activeGameObject = vc;
                }
            }
        }
    }
}
