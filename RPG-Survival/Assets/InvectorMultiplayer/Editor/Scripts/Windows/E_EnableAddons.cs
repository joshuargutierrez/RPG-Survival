using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBGames.Editors
{
    public class E_EnableAddons : EditorWindow
    {
        #region Disable Menu
        [MenuItem("Window/Reset/Swimming Add-On")]
        public static void CB_MENU_ResetSwimming()
        {
            CB_AddonSwimming(true);
        }
        [MenuItem("Window/Reset/FreeClimb Add-On")]
        public static void CB_MENU_ResetFreeClimb()
        {
            CB_AddonFreeClimb(true);
        }
        [MenuItem("Window/Reset/Zipline Add-On")]
        public static void CB_MENU_ResetZipline()
        {
            CB_AddonZipLine(true);
        }
        [MenuItem("Window/Reset/Shooter Template Add-On")]
        public static void CB_MENU_ResetShooter()
        {
            CB_AddonShooter(true);
        }
        [MenuItem("Window/Reset/FSM AI Template Add-On")]
        public static void CB_MENU_ResetFSMAI()
        {
            CB_AddonFSMAI(true);
        }
        #endregion

        #region Enable Menu
        [MenuItem("CB Games/Enable Support/Enable Free Climb Add-on Support", false, 0)]
        public static void CB_MENU_AddonFreeClimb()
        {
            CB_AddonFreeClimb(false);
        }
        [MenuItem("CB Games/Enable Support/Enable Swimming Add-on Support", false, 0)]
        public static void CB_MENU_AddonSwimming()
        {
            CB_AddonSwimming(false);
        }

        [MenuItem("CB Games/Enable Support/Enable Zipline Add-on Support", false, 0)]
        public static void CB_MENU_AddonZipline()
        {
            CB_AddonZipLine(false);
        }

        [MenuItem("CB Games/Enable Support/Enable Shooter Template Support", false, 0)]
        public static void CB_MENU_AddonShooter()
        {
            CB_AddonShooter(false);
        }
        [MenuItem("CB Games/Enable Support/(Beta) Enable FSM AI Template Add-On", false, 0)]
        public static void CB_MENU_AddonFSMAI()
        {
            CB_AddonFSMAI(false);
        }
        #endregion

        #region Enable/Disable Logic
        private static void CB_AddonShooter(bool e_disable_shooter = false)
        {
            if (EditorUtility.DisplayDialog("Do you have the \"Shooter Template\"?",
               "WARNING: This is a one way operation.\n\n" +
               "Do you have the \"Shooter Template\" imported? If you don't own this or don't " +
               "have this imported into your project yet DO NOT CONTINUE! If you do have it imported " +
               "into your project you may safetly continue.",
                       "Continue", "Cancel"))
            {
                List<string> failures = new List<string>();
                bool failure = false;
                bool AIEnabled = !E_Helpers.FileContainsText(
                    @"InvectorMultiplayer/Scripts/AI/MP_vAIHeadTrack.cs",
                    "/*"
                );
                Debug.Log("AI ENABLED: " + AIEnabled);
                List<E_Helpers.CB_Additions> modifications = new List<E_Helpers.CB_Additions>();

                #region MP_vSimpleTrigger
                string results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/SceneTests/E_ShooterTests.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region E_ShooterConvertPlayer
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_ShooterConvertPlayer.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region SyncPlayer
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Scripts/Player/SyncPlayer.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template region in SyncPlayer.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region E_ShooterConvertPrefabs
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPrefabs/E_ShooterConvertPrefabs.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region SyncItemCollection
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Scripts/Objects/SyncItemCollection.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template in file SyncItemCollection.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region E_ConvertScene
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Windows/E_ConvertScene.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template region in file E_ConvertScene.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region MP_ShooterWeaponInspector
                if (AIEnabled == true && e_disable_shooter == true)
                {
                    Debug.Log("Skipping disabling MP_ShooterWeaponInspector.cs because FSM AI depends on it.");
                }
                else
                { 
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Inspector/Components/Shooter/MP_ShooterWeaponInspector.cs", e_disable_shooter);
                    Debug.Log(results);
                    failure = !results.Contains("Success");
                    if (failure == true)
                    {
                        failures.Add(results);
                    }
                }
                #endregion

                #region MP_ShooterWeapon
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/MP_ShooterWeapon.cs", e_disable_shooter);
                Debug.Log(results);
                #endregion

                #region MP_BaseShooterWeapon
                if (AIEnabled == true && e_disable_shooter == true)
                {
                    Debug.Log("Skipping disabling MP_BaseShooterWeapon.cs because FSM AI depends on it.");
                }
                else
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/MP_BaseShooterWeapon.cs", e_disable_shooter);
                    Debug.Log(results);
                }
                #endregion

                #region MP_vShooterManager
                if (AIEnabled == true && e_disable_shooter == true)
                {
                    Debug.Log("Skipping disabling MP_vShooterManager.cs because FSM AI depends on it.");
                }
                else
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Shooter/MP_vShooterManager.cs", e_disable_shooter);
                    Debug.Log(results);
                    failure = !results.Contains("Success");
                    if (failure == true)
                    {
                        failures.Add(results);
                    }
                }
                #endregion

                #region MP_vSimpleTriggerInspector
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Inspector/Components/Shooter/MP_vSimpleTriggerInspector.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region MP_vSimpleTrigger
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/MP_vSimpleTrigger.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region vShooterMeleeInput
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private vControlAimCanvas _controlAimCanvas;",
                    in_add: "protected vControlAimCanvas _controlAimCanvas;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "public vControlAimCanvas controlAimCanvas",
                    in_add: "public virtual vControlAimCanvas controlAimCanvas",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterMeleeInput.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterMeleeInput.cs");
                #endregion

                #region MP_vShooterMeleeInput
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Shooter/MP_vShooterMeleeInput.cs", e_disable_shooter);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region ArrowView
                if (AIEnabled == true && e_disable_shooter == true)
                {
                    Debug.Log("Skipping disabling ArrowView.cs because FSM AI depends on it.");
                }
                else
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/ArrowView.cs", e_disable_shooter);
                    Debug.Log(results);
                    failure = !results.Contains("Success");
                    if (failure == true)
                    {
                        failures.Add(results);
                    }
                }
                #endregion

                #region E_Helpers
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Helpers/E_Helpers.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template region in E_Helpers.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region E_PlayerEvents
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Helpers/E_PlayerEvents.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template region in file E_PlayerEvents.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region MP_vRagdoll
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Scripts/Player/Melee/MP_vRagdoll.cs", "Shooter Template", e_disable_shooter);
                Debug.Log("Comment out Shooter Template region in file MP_vRagdoll.cs: " + e_disable_shooter);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region Shooter FSM AI Shared
                CB_AddonShooterFSMAIShared();
                #endregion

                #region Results Popup Window
                if (failure == false)
                {
                    if (EditorUtility.DisplayDialog("Enable Results",
                        "Successfully added Shooter Template to multiplayer conversion! \n\n" +
                        "You should now be able to convert a player made with the shooter template. \n\n" +
                        "IMPORTANT NOTE: You may need to minimize Unity and Re-Open it for the script to recompile " +
                        "with this addon now supported.", "Okay")) { }
                }
                else
                {
                    string formatted_failures = "The following failures occured while attempting to enable this addon:\n\n";
                    foreach (string line in failures)
                    {
                        formatted_failures += line + "\n\n";
                    }
                    if (EditorUtility.DisplayDialog("Failures Occured", formatted_failures, "Okay")) { }
                }
                #endregion
            }
        }

        private static void CB_AddonFreeClimb(bool e_disable_freeclimb = false)
        {
            if (EditorUtility.DisplayDialog("Do you have the \"Free Climb\" addon?",
                "WARNING: This is a one way operation.\n\n" +
                "Do you have this addon imported? If you don't own this addon or don't " +
                "have this imported into your project yet DO NOT CONTINUE! If you do have it " +
                "imported into your project already you can safetly continue.",
                        "Continue", "Cancel"))
            {
                List<string> failures = new List<string>();
                bool failure = false;

                #region MP_vFreeClimb
                string results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/FreeClimb/MP_vFreeClimb.cs", e_disable_freeclimb);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region E_FreeClimbConvertPlayer
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_FreeClimbConvertPlayer.cs", e_disable_freeclimb);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region Popup Results Window
                if (failure == false)
                {
                    if (EditorUtility.DisplayDialog("Enable Results",
                        "Successfully added free climb to multiplayer conversion! \n\n" +
                        "You should now be able to convert a player with the free climb addon. \n\n" +
                        "IMPORTANT NOTE: You may need to minimize Unity and Re-Open it for the script to recompile " +
                        "with this addon now supported.", "Okay")) { }
                }
                else
                {
                    string formatted_failures = "The following failures occured while attempting to enable this addon:\n\n";
                    foreach (string line in failures)
                    {
                        formatted_failures += line + "\n\n";
                    }
                    if (EditorUtility.DisplayDialog("Failures Occured", formatted_failures, "Okay")) { }
                }
                #endregion
            }
            else
            {
                return;
            }
        }

        private static void CB_AddonSwimming(bool e_disable_swimming = false)
        {
            if (EditorUtility.DisplayDialog("Do you have the \"Swimming\" addon?",
                "WARNING: This is a one way operation.\n\n" +
                "Do you have the \"Swimming\" add-on imported? If you don't own this add-on or don't " +
                "have this imported into your project yet DO NOT CONTINUE! If you do have it imported " +
                "into your project you may safetly continue.",
                        "Continue", "Cancel"))
            {
                List<string> failures = new List<string>();
                bool failure = false;

                #region MP_vSwimming
                string results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Swimming/MP_vSwimming.cs", e_disable_swimming);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region E_SwimmingConvertPlayer
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_SwimmingConvertPlayer.cs", e_disable_swimming);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region SyncPlayer
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Scripts/Player/SyncPlayer.cs", "Swimming Addon", e_disable_swimming);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region Results Popup Window
                if (failure == false)
                {
                    if (EditorUtility.DisplayDialog("Enable Results",
                        "Successfully added swimming add-on to multiplayer conversion! \n\n" +
                        "You should now be able to convert a player with the swimming add-on. \n\n" +
                        "IMPORTANT NOTE: You may need to minimize Unity and Re-Open it for the script to recompile " +
                        "with this addon now supported.", "Okay")) { }
                }
                else
                {
                    string formatted_failures = "The following failures occured while attempting to enable this addon:\n\n";
                    foreach (string line in failures)
                    {
                        formatted_failures += line + "\n\n";
                    }
                    if (EditorUtility.DisplayDialog("Failures Occured", formatted_failures, "Okay")) { }
                }
                #endregion
            }
            else
            {
                return;
            }
        }

        private static void CB_AddonZipLine(bool e_disable_zipline = false)
        {
            if (EditorUtility.DisplayDialog("Do you have the \"Swimming\" addon?",
                "WARNING: This is a one way operation.\n\n" +
                "Do you have the \"Swimming\" add-on imported? If you don't own this add-on or don't " +
                "have this imported into your project yet DO NOT CONTINUE! If you do have it imported " +
                "into your project you may safetly continue.",
                        "Continue", "Cancel"))
            {
                List<string> failures = new List<string>();
                bool failure = false;
                
                #region MP_vZipline
                string results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Zipline/MP_vZipline.cs", e_disable_zipline);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region E_ZiplineConvertPlayer
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_ZiplineConvertPlayer.cs", e_disable_zipline);
                Debug.Log(results);
                failure = !results.Contains("Success");
                if (failure == true)
                {
                    failures.Add(results);
                }
                #endregion

                #region SyncPlayer
                results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Scripts/Player/SyncPlayer.cs", "Zipline Addon", e_disable_zipline);
                Debug.Log(results);
                failure = (failure == true || !results.Contains("Success")) ? true : false;
                if (!results.Contains("Success"))
                {
                    failures.Add(results);
                }
                #endregion

                #region Popup Results Window
                if (failure == false)
                {
                    if (EditorUtility.DisplayDialog("Enable Results",
                        "Successfully added zipline add-on to multiplayer conversion! \n\n" +
                        "You should now be able to convert a player with the zipline add-on. \n\n" +
                        "IMPORTANT NOTE: You may need to minimize Unity and Re-Open it for the script to recompile " +
                        "with this addon now supported.", "Okay")) { }
                }
                else
                {
                    string formatted_failures = "The following failures occured while attempting to enable this addon:\n\n";
                    foreach (string line in failures)
                    {
                        formatted_failures += line + "\n\n";
                    }
                    if (EditorUtility.DisplayDialog("Failures Occured", formatted_failures, "Okay")) { }
                }
                #endregion
            }
            else
            {
                return;
            }
        }

        private static void CB_AddonFSMAI(bool e_disable_fsm_ai = false)
        {
            if (EditorUtility.DisplayDialog("Do you have the \"FSM AI\" addon?",
                "**WARNING: This is a one way operation. Please be aware that this is in \"Beta\".\n\n" +
                "That means some of this is subject to change in the future. There also may be some missing " +
                "features. If that IS the case, please let me know immediately on Discord!**\n\n" +
                "Do you have the \"FSM AI\" add-on imported? If you don't own this add-on or don't " +
                "have this imported into your project yet DO NOT CONTINUE! If you do have it imported " +
                "into your project you may safetly continue.",
                        "Continue", "Cancel"))
            {
                bool ShooterEnabled = !E_Helpers.FileContainsText(
                    @"InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_ShooterConvertPlayer.cs",
                    "/*"
                );
                List<E_Helpers.CB_Additions> modifications = new List<E_Helpers.CB_Additions>();
                string results = "";

                #region ArrowView
                if (ShooterEnabled == false && e_disable_fsm_ai == true || e_disable_fsm_ai == false)
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/ArrowView.cs", e_disable_fsm_ai);
                    Debug.Log(results);
                }
                else
                {
                    Debug.Log("ArrowView.cs is needed for shooter template, skipping.");
                }
                #endregion

                #region AI_MP_ShooterWeapon
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/AI_MP_ShooterWeapon.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region MP_BaseShooterWeapon
                if (ShooterEnabled == false)
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Objects/Shooter/MP_BaseShooterWeapon.cs", e_disable_fsm_ai);
                    Debug.Log(results);
                }
                #endregion

                #region MP_vShooterManager
                if (ShooterEnabled == false && e_disable_fsm_ai == true || ShooterEnabled == true && e_disable_fsm_ai == false)
                {
                    results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Shooter/MP_vShooterManager.cs", e_disable_fsm_ai);
                    Debug.Log(results);
                }
                else
                {
                    Debug.Log("MP_vShooterManager.cs is needed for shooter template, skipping.");
                }
                #endregion

                #region vAIHeadTrack
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private float _currentHeadWeight, _currentbodyWeight;",
                    in_add: "protected float _currentHeadWeight, _currentbodyWeight;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-AIController/Scripts/AI/vAIHeadTrack.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-AIController/Scripts/AI/vAIHeadTrack.cs");
                #endregion

                #region vAIShooterManager
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private float currentShotTime;",
                    in_add: "protected float currentShotTime;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "public void ReloadWeapon()",
                    in_add: "public virtual void ReloadWeapon()",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-AIController/Scripts/AI/vAIShooterManager.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-AIController/Scripts/AI/vAIShooterManager.cs");
                #endregion

                #region MP_vAIShooterManager
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/MP_vAIShooterManager.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region MP_AI
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/MP_AI.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region MP_vAIHeadTrack
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/MP_vAIHeadTrack.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region MP_vControlAIShooter
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/MP_vControlAIShooter.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region E_ConvertPlayer
                E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Windows/E_ConvertPlayer.cs", "vAIHeadTrack", e_disable_fsm_ai);
                #endregion

                #region MP_vAIHeadTrack
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/AI/MP_vAIHeadTrack.cs", e_disable_fsm_ai);
                Debug.Log(results);
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertAI/E_ConvertAI.cs", e_disable_fsm_ai);
                Debug.Log(results);
                results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/SceneTests/E_AITests.cs", e_disable_fsm_ai);
                Debug.Log(results);
                #endregion

                #region vControlAIShooter
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private Quaternion upperArmRotation, handRotation;",
                    in_add: "protected Quaternion upperArmRotation, handRotation;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private float handIKWeight;",
                    in_add: "protected float handIKWeight;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private float _onlyArmsLayerWeight;",
                    in_add: "protected float _onlyArmsLayerWeight;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private float weaponIKWeight;",
                    in_add: "protected float weaponIKWeight;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private readonly float rightRotationWeight;",
                    in_add: "protected float rightRotationWeight;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private Vector3 aimTarget;",
                    in_add: "protected Vector3 aimTarget;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-AIController/Scripts/AI/AI Controllers/vControlAIShooter.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-AIController/Scripts/AI/AI Controllers/vControlAIShooter.cs");
                #endregion

                #region Shooter FSM AI Shared
                CB_AddonShooterFSMAIShared();
                #endregion
                
                if (results.Contains("Success"))
                {
                    if (EditorUtility.DisplayDialog("Success!",
                                "You have successfully enabled FSM AI support! You may have to click out of unity and back in (or close it " +
                                "and open it again) to re-compile the scripts. If you see the little gear icon in the lower right corner spin " +
                                "that means that unity is compiling.",
                                            "Ok, thanks"))
                    { }
                }
            }
        }
        
        private static void CB_AddonShooterFSMAIShared()
        {
            bool ShooterEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/Objects/Shooter/MP_ShooterWeapon.cs",
                "/*"
            );
            bool AIEnabled = !E_Helpers.FileContainsText(
                @"InvectorMultiplayer/Scripts/AI/MP_vAIHeadTrack.cs",
                "/*"
            );
            bool disable_regions = false;
            if (ShooterEnabled == false && AIEnabled == false)
            {
                disable_regions = true;
            }

            #region E_PlayerEvents
            string results = E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Helpers/E_PlayerEvents.cs", "Shooter FSMAI Shared", disable_regions);
            Debug.Log(results);
            #endregion
        }
        #endregion
    }
}