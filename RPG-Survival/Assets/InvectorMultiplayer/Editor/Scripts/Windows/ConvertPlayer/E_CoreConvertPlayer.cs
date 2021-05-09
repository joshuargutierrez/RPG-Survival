using CBGames.Objects;
using CBGames.Player;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vItemManager;
using Invector.vMelee;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.Editors
{
    public partial class E_ConvertPlayer
    {
        #region Partial Methods
        
        #region Shooter Methods
        static bool CB_shooterEnabled = false;
        static partial void SHOOTER_HasvShooterManagerComp(GameObject target);
        static partial void SHOOTER_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict);
        static partial void CB_COMP_vShooterMeleeInput(GameObject target, ref List<string> log);
        static partial void CB_COMP_vShooterManager(GameObject target, ref List<string> log);
        static partial void CB_COMP_vRagdoll(GameObject target, ref List<string> log);
        #endregion

        #region FreeClimb Methods
        static partial void FREECLIMB_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict);
        static partial void CB_COMP_vFreeClimb(GameObject target, ref List<string> log);
        #endregion

        #region Invector Files
        static partial void CB_COMP_vHeadTrack(GameObject target, ref List<string> log);
        static partial void CB_COMP_vGenericAction(GameObject target, ref List<string> log);
        static bool CB_hasMPGenericAction = false;
        static partial void CB_CHECK_vGenericAction(GameObject target);
        #endregion

        #region Swimming
        static partial void CB_COMP_vSwimming(GameObject target, ref List<string> log);
        static partial void SWIMMING_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict);
        #endregion

        #region Zipline
        static partial void CB_COMP_vZipline(GameObject target, ref List<string> log);
        static partial void ZIPLINE_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict);
        #endregion

        #endregion

        #region Context Menus
        [MenuItem("Assets/CB Games/Convert Player To Multiplayer")]
        public static void CB_MENU_ConvertPlayer(MenuCommand command)
        {
            GameObject prefab = (GameObject)Selection.activeObject;
            E_ConvertPlayer context = new E_ConvertPlayer();
            context.CONTEXT_ConvertPlayer(prefab);
        }
        [MenuItem("CONTEXT/Transform/Add PlayerNameBar component")]
        public static void CB_MENU_PlayerNameBar(MenuCommand command)
        {
            Transform comp = (Transform)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_PlayerNameBar(target, ref log);
        }
        [MenuItem("CONTEXT/Transform/Add VoiceChat component")]
        public static void CB_MENU_VoiceChat(MenuCommand command)
        {
            Transform comp = (Transform)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_PlayerVoiceChat(target, ref log);
        }
        [MenuItem("CONTEXT/Transform/Add SyncPlayer component")]
        public static void CB_MENU_SyncPlayer(MenuCommand command)
        {
            Transform comp = (Transform)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_SyncPlayer(target.gameObject, ref log);
        }
        [MenuItem("CONTEXT/Transform/Add GenericSync component")]
        public static void CB_MENU_GenericSync(MenuCommand command)
        {
            Transform comp = (Transform)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_GenericSync(target, ref log);
        }
        [MenuItem("CONTEXT/vThirdPersonController/Add vThirdPersonController MP Events & Comps")]
        public static void CB_MENU_vThirdPersonController(MenuCommand command)
        {
            vThirdPersonController comp = (vThirdPersonController)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vThirdPersonController(target, ref log);
        }
        [MenuItem("CONTEXT/vMeleeManager/Replace vMeleeManager With MP Version")]
        public static void CB_MENU_vMeleeManager(MenuCommand command)
        {
            vMeleeManager comp = (vMeleeManager)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vMeleeManager(target, ref log);
        }
        [MenuItem("CONTEXT/vMeleeCombatInput/Replace vMeleeCombatInput With MP Version")]
        public static void CB_MENU_vMeleeCombatInput(MenuCommand command)
        {
            vMeleeCombatInput comp = (vMeleeCombatInput)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vMeleeCombatInput(target, ref log);
        }
        [MenuItem("CONTEXT/vItemManager/Add vItemManager MP Components")]
        public static void CB_MENU_vItemManager(MenuCommand command)
        {
            vItemManager comp = (vItemManager)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vItemManager(target, ref log);
        }
        [MenuItem("CONTEXT/vWeaponHolderManager/Add vWeaponHolderManager MP Components")]
        public static void CB_MENU_vWeaponHolderManager(MenuCommand command)
        {
            vWeaponHolderManager comp = (vWeaponHolderManager)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vWeaponHolderManager(target, ref log);
        }
        [MenuItem("CONTEXT/vLadderAction/Add vLadderAction MP Components & Events")]
        public static void CB_MENU_vLadderAction(MenuCommand command)
        {
            vLadderAction comp = (vLadderAction)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_vLadderAction(target, ref log);
        }
        [MenuItem("CONTEXT/Transform/Add Network Culling Component")]
        public static void CB_MENU_NetworkCulling(MenuCommand command)
        {
            Transform comp = (Transform)command.context;
            List<string> log = new List<string>();
            GameObject target = comp.gameObject;
            CB_COMP_NetworkCulling(target, ref log);
        }
        #endregion

        #region Conversion Logic
        public static void CB_COMP_NetworkCulling(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<NetworkCulling>())
            {
                if (log != null) log.Add("Adding NetworkCulling component.");
                target.AddComponent<NetworkCulling>();
            }
        }
        public static void CB_COMP_SyncPlayer(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<SyncPlayer>())
            {
                E_Helpers.AddInspectorTag("OtherPlayer");
                E_Helpers.AddInspectorLayer("OtherPlayer");
                if (log != null) log.Add("Adding SyncPlayer Component to copied player");
                target.AddComponent<SyncPlayer>();
            }

            E_CompHelper.AddCompToPhotonView(target, target.GetComponent<SyncPlayer>());
        }
        public static void CB_COMP_vThirdPersonController(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vThirdPersonController>()) return;
            if (log != null) log.Add("Setting vThirdPersonController 'useInstance' to false");
            //target.GetComponent<vThirdPersonController>().useInstance = false;
            if (log != null) log.Add("Setting vThirdPersonController 'fillHealthOnStart' to false");
            target.GetComponent<vThirdPersonController>().fillHealthOnStart = false;
            if (log != null) log.Add("Setting vThirdPersonController health to max");
            target.GetComponent<vThirdPersonController>().ResetHealth();
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vThirdPersonController>().onReceiveDamage, "OnReceiveDamage", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Setting vThirdPersonController onReceiveDamage event to use SyncPlayer");
                UnityEventTools.AddPersistentListener(target.GetComponent<vThirdPersonController>().onReceiveDamage, target.GetComponent<SyncPlayer>().OnReceiveDamage);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vThirdPersonController>().onActiveRagdoll, "NetworkActiveRagdoll", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Setting vThirdPersonController onActiveRagdoll event to use SyncPlayer");
                UnityEventTools.AddPersistentListener(target.GetComponent<vThirdPersonController>().onActiveRagdoll, target.GetComponent<SyncPlayer>().NetworkActiveRagdoll);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vThirdPersonController>().OnJump, "Jump", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Setting vThirdPersonController OnJump event to use SyncPlayer");
                UnityEventTools.AddPersistentListener(target.GetComponent<vThirdPersonController>().OnJump, target.GetComponent<SyncPlayer>().Jump);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vThirdPersonController>().onDead, "Respawn", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Setting vThirdPersonController OnDead event to use SyncPlayer");
                UnityEventTools.AddPersistentListener(target.GetComponent<vThirdPersonController>().onDead, target.GetComponent<SyncPlayer>().Respawn);
            }
        }
        public static void CB_COMP_vMeleeManager(GameObject target, ref List<string> log)
        {
            SHOOTER_HasvShooterManagerComp(target);
            //if (CB_shooterEnabled) return;

            if (!target.GetComponent<vMeleeManager>()) return;
            if (log != null) log.Add("Replacing vMeleeManager -> MP_vMeleeManager");
            if (log != null) log.Add("Adding MP_vMeleeManager -> PhotonView");
            E_CompHelper.ReplaceWithComponent(target, typeof(vMeleeManager), typeof(MP_vMeleeManager), false);
        }
        public static void CB_COMP_vMeleeCombatInput(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vMeleeCombatInput>()) return;
            SHOOTER_HasvShooterManagerComp(target);
            if (CB_shooterEnabled) return;

            if (log != null) log.Add("Replacing vMeleeCombatInput -> MP_vMeleeCombatInput");
            if (log != null) log.Add("Adding MP_vMeleeCombatInput -> PhotonView");
            E_CompHelper.ReplaceWithComponent(target, typeof(vMeleeCombatInput), typeof(MP_vMeleeCombatInput), true);
        }
        public static void CB_COMP_AddSyncObjComp(GameObject target, bool isHolder, bool isLeft, bool syncEnable, bool syncDisable, bool syncDestroy, bool syncChildren, ref List<string> log)
        {
            if (!target.GetComponent<SyncObject>())
            {
                if (log != null) log.Add("Adding SyncObject component.");
                target.AddComponent<SyncObject>();
            }
            if (log != null) log.Add("Modifying SyncObject component.");
            target.GetComponent<SyncObject>().view = target.transform.GetComponentInParent<PhotonView>();
            target.GetComponent<SyncObject>().syncDestroy = syncDestroy;
            target.GetComponent<SyncObject>().syncEnable = syncEnable;
            target.GetComponent<SyncObject>().syncDisable = syncDisable;

            target.GetComponent<SyncObject>().syncImmediateChildren = syncChildren;
            target.GetComponent<SyncObject>().isLeftHanded = (syncChildren == true) ? isLeft : false;
            target.GetComponent<SyncObject>().isWeaponHolder = (syncChildren == true) ? isHolder : false;
        }
        public static void CB_COMP_vItemManager(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vItemManager>()) return;
            if (!target.GetComponent<SyncPlayer>())
            {
                CB_COMP_SyncPlayer(target, ref log);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vItemManager>().onDropItem, "OnDropItem", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add(" * Adding OnDropItem Event to vItemManager");
                UnityEventTools.AddPersistentListener(target.GetComponent<vItemManager>().onDropItem, target.GetComponent<SyncPlayer>().OnDropItem);
            }
            foreach (EquipPoint ep in target.GetComponent<vItemManager>().equipPoints)
            {
                CB_COMP_AddSyncObjComp(ep.handler.defaultHandler.gameObject, false, ep.equipPointName.ToLower().Contains("left"), false, false, false, true, ref log);
                foreach (Transform customHandler in ep.handler.customHandlers)
                {
                    CB_COMP_AddSyncObjComp(customHandler.gameObject, false, ep.equipPointName.ToLower().Contains("left"), false, false, false, true, ref log);
                }
            }
        }
        public static void CB_COMP_vWeaponHolderManager(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vWeaponHolderManager>()) return;
            foreach (vWeaponHolder wh in target.GetComponent<vWeaponHolderManager>().holders)
            {
                if (wh.holderObject)
                {
                    CB_COMP_AddSyncObjComp(wh.holderObject, true, wh.equipPointName.ToLower().Contains("left"), true, true, false, true, ref log);
                }
                if (wh.weaponObject)
                {
                    CB_COMP_AddSyncObjComp(wh.weaponObject, true, wh.equipPointName.ToLower().Contains("left"), true, true, false, false, ref log);
                }
            }
        }
        public static void CB_COMP_vLadderAction(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<vLadderAction>()) return;
            if (!target.GetComponent<SyncPlayer>())
            {
                CB_COMP_SyncPlayer(target, ref log);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vLadderAction>().OnEnterLadder, "EnterLadder", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Adding 'EnterLadder' event to vLadderAction's 'OnEnterLadder' unityEvent.");
                UnityEventTools.AddPersistentListener(target.GetComponent<vLadderAction>().OnEnterLadder, target.GetComponent<SyncPlayer>().EnterLadder);
            }
            if (!E_PlayerEvents.HasUnityEvent(target.GetComponent<vLadderAction>().OnExitLadder, "ExitLadder", target.GetComponent<SyncPlayer>()))
            {
                if (log != null) log.Add("Adding 'ExitLadder' event to vLadderAction's 'OnExitLadder' unityEvent.");
                UnityEventTools.AddPersistentListener(target.GetComponent<vLadderAction>().OnExitLadder, target.GetComponent<SyncPlayer>().ExitLadder);
            }
        }
        
        public static void CB_COMP_PlayerNameBar(GameObject target, ref List<string> log)
        {
            if (log != null) log.Add("Adding Player Name Bar");
            GameObject playerNameBar = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/PlayerNameBar.prefab");
            if (log != null) log.Add(" * Positioning name bar above player head...");
            Vector3 barPosition = target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position + (Vector3.up / 2);
            playerNameBar.transform.position = barPosition;
            playerNameBar.transform.SetParent(target.transform);

            if (!target.GetComponent<PlayerNameBar>())
            {
                if (log != null) log.Add("Adding PlayerNameBar component.");
                target.AddComponent<PlayerNameBar>();
            }
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            Text playerName = playerNameBar.transform.Find("Background").Find("PlayerName").GetComponent<Text>();

            if (log != null) log.Add("Adding name bar and player name to PlayerNameBar component.");
            target.GetComponent<PlayerNameBar>().GetType().GetField("playerName", flags).SetValue(target.GetComponent<PlayerNameBar>(), playerName);
            target.GetComponent<PlayerNameBar>().GetType().GetField("playerBar", flags).SetValue(target.GetComponent<PlayerNameBar>(), playerNameBar);
        }
        public static void CB_COMP_PlayerVoiceChat(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<Speaker>())
            {
                if (log != null) log.Add("Adding Speaker component.");
                target.AddComponent<Speaker>();
            }
            if (!target.GetComponent<PhotonVoiceView>())
            {
                if (log != null) log.Add("Adding PhotonVoiceView component.");
                target.AddComponent<PhotonVoiceView>();
            }
            if (!target.GetComponent<VoiceChat>())
            {
                if (log != null) log.Add("Adding VoiceChat component.");
                target.AddComponent<VoiceChat>();
            }

            if (log != null) log.Add("Setting VoiceChat component values.");
            target.GetComponent<VoiceChat>().GetType().GetField("isPlayer", E_Helpers.allBindings).SetValue(target.GetComponent<VoiceChat>(), true);
            if (target.GetComponent<VoiceChat>().speakerImage == null)
            {
                if (log != null) log.Add("Adding speaker image to VoiceChat component.");
                GameObject speakerIcon = E_Helpers.CreatePrefabFromPath("InvectorMultiplayer/UI/Voice/IsSpeakingImage.prefab");
                if (target.GetComponent<Animator>())
                {
                    speakerIcon.transform.SetParent(target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest));
                    speakerIcon.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                }
                else
                {
                    speakerIcon.transform.SetParent(target.transform);
                }
                target.GetComponent<VoiceChat>().speakerImage = speakerIcon;
            }

            if (log != null) log.Add("Setting PhotonVoiceView component values.");
            target.GetComponent<PhotonVoiceView>().AutoCreateRecorderIfNotFound = false;
            target.GetComponent<PhotonVoiceView>().UsePrimaryRecorder = true;
            target.GetComponent<PhotonVoiceView>().SetupDebugSpeaker = true;
            try
            {
                target.GetComponent<PhotonVoiceView>().SpeakerInUse = target.GetComponent<Speaker>();
            }
            catch(Exception ex)
            {
                log.Add("");
                log.Add("---------------------------------");
                log.Add(ex.Message);
                log.Add("Failed to add the `SpeakerInUse` value. You will need to do this manually.");
                log.Add("Assign the `Speaker` component into the `PhotonVoiceView`'s `SpeakerInUse` value.");
                log.Add("---------------------------------");
                log.Add("");
            }
        }
        public static void CB_COMP_GenericSync(GameObject target, ref List<string> log)
        {
            if (!target.GetComponent<GenericSync>())
            {
                if (log != null) log.Add("Adding GenericSync component.");
                target.AddComponent<GenericSync>();
            }
            if (log != null) log.Add("Adding GenericSync -> PhotonView");
            E_CompHelper.AddCompToPhotonView(target, target.GetComponent<GenericSync>());
        }
        #endregion

        #region Check Logic
        public static void CORE_GetConvertableComponents(GameObject target, ref Dictionary<string, Component> dict)
        {
            CB_CHECK_vGenericAction(target);
            if (target.GetComponent<vGenericAction>() && !CB_hasMPGenericAction)
            {
                dict.Add("* vGenericAction -> MP_vGenericAction", target.GetComponent<vGenericAction>());
            }
            if (target.GetComponent<vMeleeCombatInput>() && !target.GetComponent<MP_vMeleeCombatInput>())
            {
                dict.Add("* vMeleeCombatInput -> MP_vMeleeCombatInput", target.GetComponent<vMeleeCombatInput>());
            }
            if (target.GetComponent<vItemManager>())
            {
                dict.Add("* Add missing network event to vItemManager", target.GetComponent<vItemManager>());
            }
            if (target.GetComponent<vThirdPersonController>())
            {
                dict.Add("* Add missing network events to vThirdPersonController", target.GetComponent<vThirdPersonController>());
            }
            if (!target.GetComponent<SyncPlayer>())
            {
                dict.Add("* Add SyncPlayer component", target.GetComponent<SyncPlayer>());
            }
            if (CB_addNameBar == true && !target.GetComponent<PlayerNameBar>())
            {
                dict.Add("* Add PlayerNameBar component", null);
            }
            if (CB_addVoiceChat == true && !target.GetComponent<VoiceChat>())
            {
                dict.Add("* Add VoiceChat component", null);
            }
            if (target.GetComponentInChildren<vWeaponHolder>())
            {
                dict.Add("* Add missing SyncObject components", target.GetComponentInChildren<vWeaponHolder>());
            }
            if (target.GetComponent<vHeadTrack>())
            {
                dict.Add("* Replace vHeadTrack -> MP_HeadTrack", target.GetComponentInChildren<vHeadTrack>());
            }
        }
        #endregion
    }
}