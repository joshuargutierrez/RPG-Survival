/*
using CBGames.AI;
using CBGames.Objects;
using Invector;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_TestScene
    {
        partial void AI_GenericSyncTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (GenericSync sync in FindObjectsOfType<GenericSync>())
            {
                if (sync.transform.GetComponent<vFSMBehaviourController>())
                {
                    if (sync.syncTriggers == true)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "GenericSync - Sync Triggers should be false for AI sync this is taken care of in other components. " +
                            "It wont break anything but its a small optimization you can perform.",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (sync.syncAnimations == false)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "GenericSync - Sync Animations is false. This should be true otherwise the AI animations will not " +
                            "be synced across the network.",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (sync.syncPosition == false)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "GenericSync - Sync Position is false. This should be true otherwise the AI will not move over the network.",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (sync.syncRotation == false)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "GenericSync - Sync Rotation is false. This should be true otherwise the AI will not rotate over the network.",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (sync.syncAnimatorWeights == false)
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "GenericSync - Sync Animator Layer Weights is false. This should be true otherwise the AI " +
                            "will have his animations look off over the network.",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                    if (!sync.gameObject.GetComponent<PhotonView>().ObservedComponents.Contains(sync))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "GenericSync - The GenericSync component isn't in the PhotonView's \"ObservedComponents\". " +
                            "This will throw a bunch of errors on you at runtime if this component isn't placed in " +
                            "the \"ObservedComponents\".",
                            sync
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void AI_ShooterManagerTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vAIShooterManager manager in FindObjectsOfType<vAIShooterManager>())
            {
                if (!manager.transform.GetComponent<MP_vAIShooterManager>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vAIShooterManager - You have a vAIShooterManager that has not been converted to MP_vAIShooterManager. " +
                        "You can either reconvert the entire character using the CBGames menu or convert this one component by " +
                        "using the cog wheel (context menu) on the component.",
                        manager
                    ));
                }
                else
                {
                    passed += 1;
                }
            }
        }
        partial void AI_ControlAIShooterTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vControlAIShooter shooter in FindObjectsOfType<vControlAIShooter>())
            {
                if (shooter.isImmortal == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" is marked as immortal. Are you sure you want this?",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (shooter.fillHealthOnStart == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" marked \"Fill Health On Start\" as true. This should be false " +
                        "as this causes strange issues sometimes with multiplayer. Sometimes this is okay but it's unreliable to rely on it.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (shooter.removeComponentsAfterDie == true)
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" marked \"Remove Components After Die\" as true. This should be false " +
                        "because there are ondead functions that are called.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                LayerMask detectLayer = (LayerMask)shooter.GetType().GetField("_detectLayer", E_Helpers.allBindings).GetValue(shooter);
                if (!detectLayer.Contains("Player"))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components \"Detect Layer\" doesn't have the \"Player\". " +
                        "That means this AI will never react to the player.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                vTagMask detectTags = (vTagMask)shooter.GetType().GetField("_detectTags", E_Helpers.allBindings).GetValue(shooter);
                if (!detectTags.Contains("Player"))
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components \"Detect Tags\" doesn't have the \"Player\". " +
                        "That means this AI will never react to the player.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (shooter.debugAim == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components has \"Debug Aim\" enabled. You probably want to " +
                        "disable this before your final build.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (shooter.lockAimDebug == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components has \"Lock Aim Debug\" enabled. You probably want to " +
                        "disable this before your final build.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((bool)shooter.GetType().GetField("_debugVisualDetection", E_Helpers.allBindings).GetValue(shooter) == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components has \"Debug Visual Detection\" enabled. You probably want to " +
                        "disable this before your final build.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((bool)shooter.GetType().GetField("_debugRaySight", E_Helpers.allBindings).GetValue(shooter) == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components has \"Debug Ray Sight\" enabled. You probably want to " +
                        "disable this before your final build.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if ((bool)shooter.GetType().GetField("_debugLastTargetPosition", E_Helpers.allBindings).GetValue(shooter) == true)
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "vControlAIShooter - The \"vControlAIShooter\" components has \"Debug Last Target Position\" enabled. You probably want to " +
                        "disable this before your final build.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!shooter.transform.GetComponent<MP_vControlAIShooter>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vControlAIShooter - You are missing the MP_vControlAIShooter component. You can either " +
                        "reconvert the entire character using the CBGames menu or convert this single component " +
                        "using the cog wheel (context menu) on the component.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                }
                if (!shooter.transform.GetComponent<PhotonView>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vControlAIShooter - You are missing the required PhotonView component from the gameobject. " +
                        "Add this manually or reconvert the character.",
                        shooter
                    ));
                }
                else
                {
                    passed += 1;
                    if (!shooter.transform.GetComponent<PhotonView>().ObservedComponents.Contains(shooter))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vControlAIShooter - The PhotonView is missing the MP_vControlAIShooter component " +
                            "in its \"Observed Components\" parameter.",
                            shooter
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void AI_MPAITests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vFSMBehaviourController controller in FindObjectsOfType<vFSMBehaviourController>())
            {
                if (!controller.transform.GetComponent<MP_AI>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "MP_AITests - You are missing the MP_AI component from this AI controller. " +
                        "Either add it manually or reconvert your character using the CBGames menu.",
                        controller
                    ));
                }
                else
                {
                    passed += 1;
                    if (!controller.transform.GetComponent<MP_AI>().components.Contains(controller))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "MP_AITests - The components is missing the \"vFSMBehaviourController\". This is required " +
                            "to have the AI function properly over the network.",
                            controller
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void AI_AIHeadTrackTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach(vAIHeadtrack headtrack in FindObjectsOfType<vAIHeadtrack>())
            {
                if (!headtrack.transform.GetComponent<MP_vAIHeadTrack>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vAIHeadTrackTests - vAIHeadtrack is not converted to the MP version. You can do this " +
                        "by either reconverting the AI character using the CBGames menu or convert the individual " +
                        "component using the cog wheel (context menu).",
                        headtrack
                    ));
                }
                else
                {
                    passed += 1;
                    if (!headtrack.transform.GetComponent<PhotonView>().ObservedComponents.Contains(headtrack.transform.GetComponent<MP_vAIHeadTrack>()))
                    {
                        failed += 1;
                        failures.Add(new DebugFormat(
                            "vAIHeadTrackTests - MP_vAIHeadtrack is not in the PhotonView's \"Observed Components\". " +
                            "This will not sync the head/spine rotations across the network. Simply drag the \"MP_vAIHeadtrack\" component " +
                            "into the PhotonView's \"Observed Components\".",
                            headtrack
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
    }
}
*/
