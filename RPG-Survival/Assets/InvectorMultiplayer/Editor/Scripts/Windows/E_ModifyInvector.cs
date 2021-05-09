using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace CBGames.Editors
{
    public class E_ModifyInvector : EditorWindow
    {
        [MenuItem("CB Games/Convert/Invector Files", false, 0)]
        public static void CB_MENU_ModifyInvectorFiles()
        {
            CB_ModifyInvectorFiles(false);
        }
        [MenuItem("Window/Reset/Invector Files")]
        public static void CB_MENU_ResetInvectorFiles()
        {
            CB_ModifyInvectorFiles(true);
        }

        public static void CB_ModifyInvectorFiles(bool disable_files = false)
        {
            List<E_Helpers.CB_Additions> modifications = new List<E_Helpers.CB_Additions>();

            #region HeadTrack
            modifications.Add(new E_Helpers.CB_Additions(
                in_target: "private float _currentHeadWeight, _currentbodyWeight;",
                in_add: "protected float _currentHeadWeight, _currentbodyWeight;",
                in_nextline: "",
                in_type: E_Helpers.CB_FileAddtionType.Replace
            ));
            E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/HeadTrack/Scripts/vHeadTrack.cs", modifications);
            Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/HeadTrack/Scripts/vHeadTrack.cs");
            #endregion

            #region vItemManagerUtilities_Shooter
            try
            {
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "if (equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) && equipPointL.onInstantiateEquiment.GetPersistentMethodName(i).Equals(\"SetLeftWeapon\"))",
                    in_add: "if ((equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) || equipPointL.onInstantiateEquiment.GetPersistentTarget(i).GetType().IsSubclassOf(typeof(vShooterManager))) && equipPointL.onInstantiateEquiment.GetPersistentMethodName(i).Equals(\"SetLeftWeapon\"))",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "if (equipPointR.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) && equipPointR.onInstantiateEquiment.GetPersistentMethodName(i).Equals(\"SetRightWeapon\"))",
                    in_add: "if ((equipPointR.onInstantiateEquiment.GetPersistentTarget(i).GetType().Equals(typeof(vShooterManager)) || equipPointR.onInstantiateEquiment.GetPersistentTarget(i).GetType().IsSubclassOf(typeof(vShooterManager))) && equipPointR.onInstantiateEquiment.GetPersistentMethodName(i).Equals(\"SetRightWeapon\"))",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/Editor/vItemManagerUtilities_Shooter.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/Editor/vItemManagerUtilities_Shooter.cs");
            }
            catch(Exception ex)
            {
                Debug.Log("FAILED TO CONVERT Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/Editor/vItemManagerUtilities_Shooter.cs");
                Debug.Log("If you don't have the Shooter Template imported this is why you're recieving this error and can safely ignore it if you're not using this.");
                Debug.Log(ex);
            }
            #endregion

            #region E_ConvertPlayer
            E_Helpers.CommentOutRegionInFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_ConvertPlayer.cs", "InvectorModification", disable_files);
            #endregion

            #region MP_HeadTrack
            string results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Basic/MP_HeadTrack.cs", disable_files);
            Debug.Log(results);
            #endregion

            #region vGenericAction
            modifications.Clear();
            modifications.Add(new E_Helpers.CB_Additions(
                in_target: "private float animationBehaviourDelay;",
                in_add: "protected float animationBehaviourDelay;",
                in_nextline: "",
                in_type: E_Helpers.CB_FileAddtionType.Replace
            ));
            E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/CharacterController/Actions/vGenericAction.cs", modifications);
            Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/CharacterController/Actions/vGenericAction.cs");
            #endregion

            #region MP_vGenericActions
            results = E_Helpers.CommentOutFile("InvectorMultiplayer/Scripts/Player/Basic/MP_vGenericAction.cs", disable_files);
            Debug.Log(results);
            #endregion

            #region E_InvectorConvertPlayer
            results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/ConvertPlayer/E_InvectorConvertPlayer.cs", disable_files);
            Debug.Log(results);
            #endregion

            #region E_InvectorTests
            results = E_Helpers.CommentOutFile("InvectorMultiplayer/Editor/Scripts/Windows/SceneTests/E_InvectorTests.cs", disable_files);
            Debug.Log(results);
            #endregion

            #region vShooterWeaponBase
            try
            {
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "protected virtual bool CanDoEmptyClip",
                    in_add: "public virtual bool CanDoEmptyClip",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Shooter/Scripts/Weapon/vShooterWeaponBase.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Weapon/vShooterWeaponBase.cs");
            }
            catch
            {
                Debug.Log("Failed to modify file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Weapon/vShooterWeaponBase.cs");
                Debug.Log("If you don't have the Shooter template in your project you can safetly ignore this.");
            }
            #endregion

            #region vShooterManager
            try
            {
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "private bool usingThirdPersonController;",
                    in_add: "protected bool usingThirdPersonController;",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterManager.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterManager.cs");
            }
            catch
            {
                Debug.Log("Failed to modify file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterManager.cs");
                Debug.Log("If you don't have the Shooter template in your project you can safetly ignore this.");
            }
            #endregion

            #region vRagdoll
            try
            {
                modifications.Clear();
                modifications.Add(new E_Helpers.CB_Additions(
                    in_target: "public void ActivateRagdoll(vDamage damage)",
                    in_add: "public virtual void ActivateRagdoll(vDamage damage)",
                    in_nextline: "",
                    in_type: E_Helpers.CB_FileAddtionType.Replace
                ));
                E_Helpers.ModifyFile(@"Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/Ragdoll/vRagdoll.cs", modifications);
                Debug.Log("Modified file at: Assets/Invector-3rdPersonController/Basic Locomotion/Scripts/Ragdoll/vRagdoll.cs");
            }
            catch
            {
                Debug.Log("Failed to modify file at: Assets/Invector-3rdPersonController/Shooter/Scripts/Shooter/vShooterManager.cs");
                Debug.Log("If you don't have the Shooter template in your project you can safetly ignore this.");
            }
            #endregion

            if (results.Contains("Success"))
            {
                if (EditorUtility.DisplayDialog("Success!",
                            "You have successfully modified the invector files. You may have to click out of unity and back in (or close it " +
                            "and open it again) to re-compile the scripts. If you see the little gear icon in the lower right corner spin " +
                            "that means that unity is compiling.",
                                        "Ok, thanks"))
                { }
            }
        }
    }
}