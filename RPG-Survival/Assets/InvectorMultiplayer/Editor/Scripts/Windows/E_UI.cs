using CBGames.Core;
using CBGames.UI;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CBGames.Editors
{
    public class E_UI : EditorWindow
    {
        #region Floating Health Bar
        [MenuItem("CB Games/UI/Floating Bar/Add Health Bar", false, 0)]
        public static void CB_AddHealthBar()
        {
            GameObject target = Selection.activeGameObject;
            if (target == null)
            {
                if (EditorUtility.DisplayDialog("Select Valid Player",
                    "You don't seem to have anything selected. Please select a valid player " +
                    "before attempting to select this menu item.",
                    "Okay"))
                {
                }
            }
            else if (!target.GetComponent<vThirdPersonController>())
            {
                if (EditorUtility.DisplayDialog("Select Valid Player",
                    "The current selected object doesn't appear to be a player. Please select a player " +
                    "then select this menu item again to add the health bar to the selected player.",
                    "Okay"))
                {
                }
            }
            else
            {
                GameObject healthBar = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/FloatingHealthBar.prefab");
                healthBar.transform.SetParent(target.transform);
                healthBar.GetComponent<FloatingBar>().controller = target.GetComponent<vThirdPersonController>();
                if (target.GetComponent<Animator>() && target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head))
                {
                    healthBar.transform.position = target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position + Vector3.up / 2;
                    if (EditorUtility.DisplayDialog("Success!",
                    "Successfully added the health bar to your player.",
                    "Great!"))
                    {
                    }
                }
                else
                {
                    healthBar.transform.localPosition = Vector3.zero;
                    if (EditorUtility.DisplayDialog("Partial Success",
                    "Successfully added the health bar to your player but was unable to determine where the head of your " +
                    "player was. You will need to manually position the floating bar where you want it to be.",
                    "Great!"))
                    {
                    }
                }
                Selection.activeGameObject = healthBar;
            }
        }

        [MenuItem("CB Games/UI/Floating Bar/Add Stamina Bar", false, 0)]
        public static void CB_AddStaminaBar()
        {
            GameObject target = Selection.activeGameObject;
            if (target == null)
            {
                if (EditorUtility.DisplayDialog("Select Valid Player",
                    "You don't seem to have anything selected. Please select a valid player " +
                    "before attempting to select this menu item.",
                    "Okay"))
                {
                }
            }
            else if (!target.GetComponent<vThirdPersonController>())
            {
                if (EditorUtility.DisplayDialog("Select Valid Player",
                    "The current selected object doesn't appear to be a player. Please select a player " +
                    "then select this menu item again to add the stamina bar to the selected player.",
                    "Okay"))
                {
                }
            }
            else
            {
                GameObject staminaBar = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/FloatingStaminaBar.prefab");
                staminaBar.transform.SetParent(target.transform);
                staminaBar.GetComponent<FloatingBar>().controller = target.GetComponent<vThirdPersonController>();
                if (target.GetComponent<Animator>() && target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head))
                {
                    staminaBar.transform.position = target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position + Vector3.up / 2;
                    if (EditorUtility.DisplayDialog("Success!",
                    "Successfully added the health bar to your player.",
                    "Great!"))
                    {
                    }
                }
                else
                {
                    staminaBar.transform.localPosition = Vector3.zero;
                    if (EditorUtility.DisplayDialog("Partial Success",
                    "Successfully added the health bar to your player but was unable to determine where the head of your " +
                    "player was. You will need to manually position the floating bar where you want it to be.",
                    "Great!"))
                    {
                    }
                }
                Selection.activeGameObject = staminaBar;
            }
        }
        #endregion

        #region Example UI
        [MenuItem("CB Games/UI/Add/All Player Prefabs To UICoreLogic", false, 0)]
        public static void CB_MENU_AddPlayersToExampleUI()
        {
            CB_AddPlayersToExampleUI(false);
        }
        public static void CB_CALL_AddPlayersToExampleUI()
        {
            CB_AddPlayersToExampleUI(true);
        }
        public static void CB_AddPlayersToExampleUI(bool alwaysYes = false)
        {
            if (alwaysYes || !alwaysYes && EditorUtility.DisplayDialog("About To Add Players",
                        "This will scan all the prefabs in your root Resources folder and attempt to find all " +
                        "of the player prefabs. Then it will assign these found prefabs to the \"players\" gameobject " +
                        "array in the \"ExampleUI\" component. This will overwrite any settings currently there.\n\n" +
                        "Do you want to continue?",
                                    "Continue","Cancel"))
            {
                UICoreLogic ui = FindObjectOfType<UICoreLogic>();
                if (ui == null)
                {
                    if (alwaysYes || !alwaysYes && EditorUtility.DisplayDialog("Missing ExampleUI",
                        "You are missing an example UI from the scene. Please add it before trying this again.",
                                    "Okay"))
                    {

                    }
                }
                else
                {
                    string[] prefabPaths = E_Helpers.GetAllPrefabs(false, true);
                    GameObject target;
                    List<GameObject> foundPlayers = new List<GameObject>();
                    foreach (string prefabPath in prefabPaths)
                    {
                        try
                        {
                            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath);
                            target = (GameObject)prefab;
                            if (target.GetComponent<vThirdPersonController>())
                            {
                                foundPlayers.Add(target);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    ui.selectablePlayers = foundPlayers.ToArray();
                    if (alwaysYes || !alwaysYes && EditorUtility.DisplayDialog("Completed!",
                        "The found player prefabs of the resources folder have been added to the ExampleUI. \n\n" +
                        "Remember to save this to the prefab otherwise it will be removed when you click play.",
                                    "Great!"))
                    {
                        Selection.activeGameObject = ui.gameObject;
                    }
                }
            }
        }

        [MenuItem("CB Games/UI/Add Player List UI Element", false, 0)]
        public static void CB_CorePlayerList()
        {
            if (!FindObjectOfType<NetworkManager>())
            {
                if (EditorUtility.DisplayDialog("ChatBox Missing",
                    "Unable to locate a \"NetworkManager\" component in the scene. Add this first before attempting " +
                    "to add this UI Element as this UI element depends on the \"NetworkManager\".\r\n",
                        "Okay"))
                { }
            }
            else if (!FindObjectOfType<ChatBox>())
            {
                if (EditorUtility.DisplayDialog("ChatBox Missing",
                    "Unable to locate a \"ChatBox\" component in the scene. Add this first before attempting " +
                    "to add this UI Element as this UI element depends on the ChatBox.\r\n",
                        "Okay"))
                { }
            }
            else
            {
                if (FindObjectOfType<PlayerList>())
                {
                    if (EditorUtility.DisplayDialog("PlayerList Found In Scene",
                    "The \"PlayerList\" component was already found in the scene. Do you want to overwrite " +
                    "all of your settings on this component back to it's original values?",
                        "Yes", "No"))
                    {
                        CB_CreatePlayerList();
                    }
                    else
                    {

                    }
                }
                else
                {
                    CB_CreatePlayerList();
                }
            }
        }
        public static void CB_CreatePlayerList()
        {
            PlayerList listComp = FindObjectOfType<PlayerList>();
            if (!listComp)
            {
                GameObject playerList = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PlayerList.prefab");
                playerList.transform.SetParent(FindObjectOfType<NetworkManager>().transform);
                listComp = playerList.GetComponent<PlayerList>();
            }

            //ChatBox Events
            ChatBox chatbox = FindObjectOfType<ChatBox>();
            if (!E_PlayerEvents.HasUnityEvent(chatbox.OnYouSubscribeToDataChannel, "SetPlayerList", listComp))
            {
                UnityEventTools.AddPersistentListener(chatbox.OnYouSubscribeToDataChannel, listComp.SetPlayerList);
            }
            if (!E_PlayerEvents.HasUnityEvent(chatbox.OnYouSubscribeToDataChannel, "UpdateLocationToCurrentScene", listComp))
            {
                UnityEventTools.AddPersistentListener(chatbox.OnYouSubscribeToDataChannel, listComp.UpdateLocationToCurrentScene);
            }
            if (!E_PlayerEvents.HasUnityEvent(chatbox.OnUserSubscribedToDataChannel, "AddPlayer", listComp))
            {
                UnityEventTools.AddPersistentListener(chatbox.OnUserSubscribedToDataChannel, listComp.AddPlayer);
            }
            if (!E_PlayerEvents.HasUnityEvent(chatbox.OnUserUnSubscribedToDataChannel, "RemovePlayer", listComp))
            {
                UnityEventTools.AddPersistentListener(chatbox.OnUserUnSubscribedToDataChannel, listComp.RemovePlayer);
            }

            //Network Manager Events
            NetworkManager nm = FindObjectOfType<NetworkManager>();
            if (!E_PlayerEvents.HasUnityEvent(nm.roomEvents._onJoinedRoom, "UpdateLocationToGoingToScene", listComp))
            {
                UnityEventTools.AddPersistentListener(nm.roomEvents._onJoinedRoom, listComp.UpdateLocationToGoingToScene);
            }
            if (!E_PlayerEvents.HasUnityEvent(nm.roomEvents._onJoinedRoom, "ClearPlayerList", listComp))
            {
                UnityEventTools.AddPersistentListener(nm.otherEvents._onDisconnected, listComp.ClearPlayerList);
            }

            E_Helpers.SetObjectIcon(listComp.gameObject, E_Core.h_playerlistIcon);
        }

        //[MenuItem("CB Games/UI/Add/Pre-Built UI/Free-For-All/Add Dark UI", false, 30)]
        //public static void CB_AddDarkUIToScene()
        //{
        //    GameObject ui = (GameObject.Find("DarkUI") != null) ? GameObject.Find("DarkUI") : E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PreBuilt/DarkUI.prefab");
        //    ui.transform.SetParent(FindObjectOfType<NetworkManager>().transform);
        //    NetworkManager nm = FindObjectOfType<NetworkManager>();

        //    //Capture Event Groups
        //    UnityEvent joinRoomEvents = nm.roomEvents._onJoinedRoom;
        //    UnityEvent onLeftLobby = nm.lobbyEvents._onLeftLobby;
        //    PlayerEvent onPlayerEnter = nm.playerEvents._onPlayerEnteredRoom;
        //    PlayerEvent onPlayerLeft = nm.playerEvents._onPlayerLeftRoom;
        //    StringUnityEvent joinRoomFailedEvents = nm.roomEvents._onJoinRoomFailed;
        //    StringUnityEvent createRoomFailedEvents = nm.roomEvents._onCreateRoomFailed;
        //    StringUnityEvent onDisconnected = nm.otherEvents._onDisconnected;
        //    StringUnityEvent onConnectionFailedEvents = nm.otherEvents._onConnectionFail;
        //    StringUnityEvent photonFailedEvents = nm.otherEvents._onFailedToConnectToPhoton;
        //    InputField uiNameInput = ui.GetComponent<ExampleUI>().NameInputField;

        //    //Lobby Events
        //    if (!E_PlayerEvents.HasUnityEvent(onLeftLobby, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityAction<string> action = ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage;
        //        UnityEventTools.AddStringPersistentListener(FindObjectOfType<NetworkManager>().lobbyEvents._onLeftLobby, action, "You left the lobby");
        //    }

        //    //Room Events
        //    if (!E_PlayerEvents.HasUnityEvent(joinRoomEvents, "SetActive", ui.GetComponent<ExampleUI>().PanelPage))
        //    {
        //        UnityAction<bool> action = ui.GetComponent<ExampleUI>().PanelPage.SetActive;
        //        UnityEventTools.AddBoolPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, action, false);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(joinRoomEvents, "FadeAudio", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddBoolPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinedRoom, ui.GetComponent<ExampleUI>().FadeAudio, false);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(createRoomFailedEvents, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onCreateRoomFailed, ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(joinRoomFailedEvents, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().roomEvents._onJoinRoomFailed, ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage);
        //    }

        //    //Player Events
        //    if (!E_PlayerEvents.HasUnityEvent(onPlayerEnter, "DisplayPlayerCardEnter", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().playerEvents._onPlayerEnteredRoom, ui.GetComponent<ExampleUI>().DisplayPlayerCardEnter);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(onPlayerLeft, "DisplayPlayerCardLeft", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().playerEvents._onPlayerLeftRoom, ui.GetComponent<ExampleUI>().DisplayPlayerCardLeft);
        //    }

        //    //Misc Events
        //    if (!E_PlayerEvents.HasUnityEvent(photonFailedEvents, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().otherEvents._onFailedToConnectToPhoton, ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(onConnectionFailedEvents, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().otherEvents._onConnectionFail, ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage);
        //    }
        //    if (!E_PlayerEvents.HasUnityEvent(onDisconnected, "DisplayNetworkErrorMessagePage", ui.GetComponent<ExampleUI>()))
        //    {
        //        UnityEventTools.AddPersistentListener(FindObjectOfType<NetworkManager>().otherEvents._onDisconnected, ui.GetComponent<ExampleUI>().DisplayNetworkErrorMessagePage);
        //    }

        //    // UI Events
        //    if (!E_PlayerEvents.HasUnityEvent(uiNameInput.onEndEdit, "SetPlayerName", nm))
        //    {
        //        UnityEventTools.AddPersistentListener(uiNameInput.onEndEdit, nm.SetPlayerName);
        //    }

        //    //Add nm gameobject to UI
        //    //ui.GetComponent<ExampleUI>().GetType().GetField("nm", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ui.GetComponent<ExampleUI>(), nm);
        //    E_Helpers.SetObjectIcon(ui, E_Core.h_genericIcon);
        //    Debug.Log("Successfully added \"DarkUI\" and setup the UnityEvents in the \"Network Manager\" and the \"DarkUI\".");
        //    if (EditorUtility.DisplayDialog("Successfully Added DarkUI!",
        //                "The ExampleUI has a \"players\" gameobject array that must be populated with your converted players that " +
        //                "are stored in the \"Resources\" folder. A helper menu option is available for you to automatically find " +
        //                "all of your converted players in the \"Resources\" folder and add it to this components array.\n\n" +
        //                "You can find this helper method under CB Games > UI > Example UI > Add All Player Prefabs.",
        //                            "Thanks For The Tip!"))
        //    {
        //        Selection.activeGameObject = ui.gameObject;
        //    }
        //}

        [MenuItem("CB Games/UI/Add/Pre-Built UI/Team-Based/Add Future UI", false, 30)]
        public static void CB_AddTeamBasedFutureUI()
        {
            GameObject ui = (GameObject.Find("FutureUI") != null) ? GameObject.Find("FutureUI") : E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PreBuilt/FutureUI.prefab");
            NetworkManager nm = FindObjectOfType<NetworkManager>();
            ui.transform.SetParent(nm.transform);
            
            //Setup team settings
            nm.allowTeamDamaging = false;
            nm.teamName = "";
            if (!nm.initalTeamSpawnPointNames.ContainsKey("RedTeam"))
            {
                nm.initalTeamSpawnPointNames.Add("RedTeam", "RedTeamSpawn");
            }
            if (!nm.initalTeamSpawnPointNames.ContainsKey("BlueTeam"))
            {
                nm.initalTeamSpawnPointNames.Add("BlueTeam", "BlueTeamSpawn");
            }
            nm.autoSpawnPlayer = false;

            //Capture Event Groups
            UnityEvent joinRoomEvents = nm.roomEvents._onJoinedRoom;
            UnityEvent onLeftLobby = nm.lobbyEvents._onLeftLobby;
            PlayerEvent onPlayerEnter = nm.playerEvents._onPlayerEnteredRoom;
            PlayerEvent onPlayerLeft = nm.playerEvents._onPlayerLeftRoom;
            StringUnityEvent joinRoomFailedEvents = nm.roomEvents._onJoinRoomFailed;
            StringUnityEvent createRoomFailedEvents = nm.roomEvents._onCreateRoomFailed;
            StringUnityEvent onDisconnected = nm.otherEvents._onDisconnected;
            StringUnityEvent onConnectionFailedEvents = nm.otherEvents._onConnectionFail;
            StringUnityEvent photonFailedEvents = nm.otherEvents._onFailedToConnectToPhoton;

            if (!E_PlayerEvents.HasUnityEvent(createRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
            {
                UnityEventTools.AddPersistentListener(createRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
            }
            if (!E_PlayerEvents.HasUnityEvent(joinRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
            {
                UnityEventTools.AddPersistentListener(joinRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
            }
            if (!E_PlayerEvents.HasUnityEvent(onDisconnected, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
            {
                UnityEventTools.AddPersistentListener(onDisconnected, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
            }
            if (!E_PlayerEvents.HasUnityEvent(photonFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
            {
                UnityEventTools.AddPersistentListener(photonFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
            }
            if (!E_PlayerEvents.HasUnityEvent(onConnectionFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
            {
                UnityEventTools.AddPersistentListener(onConnectionFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
            }

            E_Helpers.SetObjectIcon(ui, E_Core.h_uiIcon);
            Debug.Log("Successfully added \"FutureUI\" and setup the UnityEvents in the \"Network Manager\" and the \"FutureUI\".");
            if (EditorUtility.DisplayDialog("Successfully Added FutureUI!",
                        "The UICoreLogic has a \"selectablePlayers\" gameobject array that must be populated with your converted players that " +
                        "are stored in the \"Resources\" folder. A helper menu option is available for you to automatically find " +
                        "all of your converted players in the \"Resources\" folder and add it to this components array.\n\n" +
                        "You can find this helper method under CB Games > UI > Add > All Player Prefabs To UICoreLogic.",
                                    "Thanks For The Tip!"))
            {
                Selection.activeGameObject = ui.gameObject;
            }
        }

        [MenuItem("CB Games/UI/Add/Pre-Built UI/Co-Op/Add Dark Fantasy UI", false, 30)]
        public static void CB_AddCoOpBasedDarkFantasyUI()
        {
            if (EditorUtility.DisplayDialog("Are you sure?",
                    "This will overwrite the settings for your teamName, initialTeamSpawnPointNames, " +
                    "autoSpawnPlayer, and allowTeamDamaging options on your NetworkManager component. " +
                    "Do you want to continue?",
                        "Yes, Add DarkFantasyUI", "No"))
            {
                GameObject ui = (GameObject.Find("DarkFantasyUI") != null) ? GameObject.Find("DarkFantasyUI") : E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PreBuilt/DarkFantasyUI.prefab");
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                ui.transform.SetParent(nm.transform);

                //Setup team settings
                nm.allowTeamDamaging = false;
                nm.teamName = "CoOp";
                nm.initalTeamSpawnPointNames.Clear();
                nm.initalTeamSpawnPointNames.Add("CoOp", "CoOpSpawn");
                nm.autoSpawnPlayer = false;

                //Capture Event Groups
                UnityEvent joinRoomEvents = nm.roomEvents._onJoinedRoom;
                UnityEvent onLeftLobby = nm.lobbyEvents._onLeftLobby;
                PlayerEvent onPlayerEnter = nm.playerEvents._onPlayerEnteredRoom;
                PlayerEvent onPlayerLeft = nm.playerEvents._onPlayerLeftRoom;
                StringUnityEvent joinRoomFailedEvents = nm.roomEvents._onJoinRoomFailed;
                StringUnityEvent createRoomFailedEvents = nm.roomEvents._onCreateRoomFailed;
                StringUnityEvent onDisconnected = nm.otherEvents._onDisconnected;
                StringUnityEvent onConnectionFailedEvents = nm.otherEvents._onConnectionFail;
                StringUnityEvent photonFailedEvents = nm.otherEvents._onFailedToConnectToPhoton;

                if (!E_PlayerEvents.HasUnityEvent(createRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(createRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(joinRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(joinRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(onDisconnected, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(onDisconnected, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(photonFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(photonFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(onConnectionFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(onConnectionFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }

                E_Helpers.SetObjectIcon(ui, E_Core.h_uiIcon);
                Debug.Log("Successfully added \"DarkFantasyUI\" and setup the UnityEvents in the \"Network Manager\" and the \"DarkFantasyUI\".");
                if (EditorUtility.DisplayDialog("Successfully Added DarkFantasyUI!",
                            "The UICoreLogic has a \"selectablePlayers\" gameobject array that must be populated with your converted players that " +
                            "are stored in the \"Resources\" folder. A helper menu option is available for you to automatically find " +
                            "all of your converted players in the \"Resources\" folder and add it to this components array.\n\n" +
                            "You can find this helper method under CB Games > UI > Add > All Player Prefabs To UICoreLogic.",
                                        "Thanks For The Tip!"))
                {
                    Selection.activeGameObject = ui.gameObject;
                }
            }
        }

        [MenuItem("CB Games/UI/Add/Pre-Built UI/Free-For-All/Add Modern UI", false, 30)]
        public static void CB_AddFreeForAllModernUI()
        {
            if (EditorUtility.DisplayDialog("Are you sure?",
                    "This will overwrite the settings for your teamName, initialTeamSpawnPointNames, " +
                    "autoSpawnPlayer, and allowTeamDamaging options on your NetworkManager component. " +
                    "Do you want to continue?",
                        "Yes, Add ModernUI", "No"))
            {
                GameObject ui = (GameObject.Find("ModernUI") != null) ? GameObject.Find("ModernUI") : E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PreBuilt/ModernUI.prefab");
                NetworkManager nm = FindObjectOfType<NetworkManager>();
                ui.transform.SetParent(nm.transform);

                //Setup team settings
                nm.allowTeamDamaging = true;
                nm.teamName = "";
                nm.initalTeamSpawnPointNames.Clear();
                nm.autoSpawnPlayer = false;

                //Capture Event Groups
                UnityEvent joinRoomEvents = nm.roomEvents._onJoinedRoom;
                UnityEvent onLeftLobby = nm.lobbyEvents._onLeftLobby;
                PlayerEvent onPlayerEnter = nm.playerEvents._onPlayerEnteredRoom;
                PlayerEvent onPlayerLeft = nm.playerEvents._onPlayerLeftRoom;
                StringUnityEvent joinRoomFailedEvents = nm.roomEvents._onJoinRoomFailed;
                StringUnityEvent createRoomFailedEvents = nm.roomEvents._onCreateRoomFailed;
                StringUnityEvent onDisconnected = nm.otherEvents._onDisconnected;
                StringUnityEvent onConnectionFailedEvents = nm.otherEvents._onConnectionFail;
                StringUnityEvent photonFailedEvents = nm.otherEvents._onFailedToConnectToPhoton;

                if (!E_PlayerEvents.HasUnityEvent(createRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(createRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(joinRoomFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(joinRoomFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(onDisconnected, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(onDisconnected, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(photonFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(photonFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }
                if (!E_PlayerEvents.HasUnityEvent(onConnectionFailedEvents, "NetworkErrorOccured", ui.GetComponent<UICoreLogic>()))
                {
                    UnityEventTools.AddPersistentListener(onConnectionFailedEvents, ui.GetComponent<UICoreLogic>().NetworkErrorOccured);
                }

                E_Helpers.SetObjectIcon(ui, E_Core.h_uiIcon);
                Debug.Log("Successfully added \"ModernUI\" and setup the UnityEvents in the \"Network Manager\" and the \"ModernUI\".");
                if (EditorUtility.DisplayDialog("Successfully Added ModernUI!",
                            "The UICoreLogic has a \"selectablePlayers\" gameobject array that must be populated with your converted players that " +
                            "are stored in the \"Resources\" folder. A helper menu option is available for you to automatically find " +
                            "all of your converted players in the \"Resources\" folder and add it to this components array.\n\n" +
                            "You can find this helper method under CB Games > UI > Add > All Player Prefabs To UICoreLogic.",
                                        "Thanks For The Tip!"))
                {
                    Selection.activeGameObject = ui.gameObject;
                }
            }
        }

        #endregion
    }
}