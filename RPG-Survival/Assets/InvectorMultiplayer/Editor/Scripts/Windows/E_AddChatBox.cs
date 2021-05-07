using UnityEngine;
using UnityEditor;
using CBGames.Core;
using System.IO;
using CBGames.UI;
using UnityEditor.Events;
using System.Collections.Generic;

namespace CBGames.Editors
{
    public class E_AddChatBox : EditorWindow
    {
        [MenuItem("CB Games/Chat/Text/Add Global Text Chat", false, 0)]
        public static void CB_AddGlobalChatBox()
        {
            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("Missing Network Manager", "No NetworkManager object was found in this scene. In order for this component to work properly there must be a network manager in the scene. Please add a \"NetworkManager\" component to the scene.",
                            "Okay"))
                { }
            }
            else if (FindObjectOfType<ChatBox>())
            {
                Selection.activeGameObject = FindObjectOfType<ChatBox>().gameObject;
                if (EditorUtility.DisplayDialog("Scene Already Has ChatBox", "This scene already contains a \"ChatBox\" component. You should never add more than one at a time to a scene.",
                            "Okay"))
                { }
            }
            else
            {
                string chatBoxPrefabPath = string.Format("Assets{0}InvectorMultiplayer{0}UI{0}ChatBox.prefab", Path.DirectorySeparatorChar);
                GameObject ChatBox = E_Helpers.CreatePrefabFromPath(chatBoxPrefabPath);
                ChatBox.transform.SetParent(FindObjectOfType<NetworkManager>().transform);
                ChatBox.GetComponent<ChatBox>().nm = FindObjectOfType<NetworkManager>();
                ChatBox.GetComponent<ChatBox>().GetType().GetField("openChatWindowOnPress", E_Helpers.allBindings).SetValue(ChatBox.GetComponent<ChatBox>(), new List<string> { "T" });
                ChatBox.GetComponent<ChatBox>().GetType().GetField("closeWindowOnPress", E_Helpers.allBindings).SetValue(ChatBox.GetComponent<ChatBox>(), new List<string> { "Escape" });
                ChatBox.GetComponent<ChatBox>().GetType().GetField("sendChatOnPress", E_Helpers.allBindings).SetValue(ChatBox.GetComponent<ChatBox>(), new List<string> { "KeypadEnter", "Return" });
                E_Helpers.SetObjectIcon(ChatBox, E_Core.h_textChatIcon);

                //Join Room Events
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, "SetActiveRoomAsChannelName", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, ChatBox.GetComponent<ChatBox>().SetActiveRoomAsChannelName);
                }
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, "Connect", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, ChatBox.GetComponent<ChatBox>().Connect);
                }
                if (E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, "EnableVisualBox", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddBoolPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, ChatBox.GetComponent<ChatBox>().EnableVisualBox, true);
                }

                //Left Room Events
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().roomEvents._onLeftRoom, "EnableVisualBox", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddBoolPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onLeftRoom, ChatBox.GetComponent<ChatBox>().EnableVisualBox, false);
                }

                //Joined Room Failed Events
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().roomEvents._onJoinRoomFailed, "SetActiveChannel", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddStringPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinRoomFailed, ChatBox.GetComponent<ChatBox>().SetActiveChannel, "lobbyChat");
                }

                // Misc - OnDisconnect Events
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().otherEvents._onDisconnected, "Disconnect", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddStringPersistentListener(FindObjectOfType<NetworkManager>().otherEvents._onDisconnected, ChatBox.GetComponent<ChatBox>().Disconnect, "");
                }
                if (!E_PlayerEvents.HasUnityEvent(FindObjectOfType<NetworkManager>().otherEvents._onDisconnected, "EnableChat", ChatBox.GetComponent<ChatBox>()))
                {
                    UnityEventTools.AddBoolPersistentListener(FindObjectOfType<NetworkManager>().otherEvents._onDisconnected, ChatBox.GetComponent<ChatBox>().EnableChat, false);
                }
            }
        }
    }
}
