/*
using Invector.vCharacterController.vActions;
using System.Collections.Generic;
using CBGames.Player;
using Invector.vCharacterController;
using Photon.Pun;
using UnityEngine;

namespace CBGames.Editors
{
    public partial class E_TestScene
    {
        partial void CORE_PerformMPHeadTrackTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (MP_HeadTrack comp in FindObjectsOfType<MP_HeadTrack>())
            {
                if (_autoFixTests)
                {
                    if (!comp.transform.GetComponent<PhotonView>())
                    {
                        comp.transform.gameObject.AddComponent<PhotonView>();
                    }
                    if (!comp.transform.GetComponent<PhotonView>().ObservedComponents.Contains(comp))
                    {
                        comp.transform.GetComponent<PhotonView>().ObservedComponents.Add(comp);
                    }
                }
                if (!comp.transform.GetComponent<PhotonView>())
                {
                    failed += 1;
                    warnings.Add(new DebugFormat(
                        "MP_HeadTrack - \"PhotonView\" is not found on the same object as \"MP_HeadTrack\" component. " +
                        "The \"PhotonView\" component needs to exist and be observing the \"MP_HeadTrack\" component " +
                        "to work properly.",
                        comp
                    ));
                }
                else
                {
                    passed += 1;
                    if (!comp.transform.GetComponent<PhotonView>().ObservedComponents.Contains(comp))
                    {
                        failed += 1;
                        warnings.Add(new DebugFormat(
                            "MP_HeadTrack - \"PhotonView\" is not observing the \"MP_HeadTrack\" component. " +
                            "While the head motion will work locally it will not be replicated over the network.",
                            comp
                        ));
                    }
                    else
                    {
                        passed += 1;
                    }
                }
            }
        }
        partial void CORE_PerformGenericActionTests(ref int passed, ref int failed, ref List<DebugFormat> failures, ref List<DebugFormat> warnings)
        {
            foreach (vGenericAction ga in FindObjectsOfType<vGenericAction>())
            {
                if (_autoFixTests)
                {
                    if (!ga.GetComponent<MP_vGenericAction>())
                    {
                        ga.gameObject.AddComponent<MP_vGenericAction>();
                    }
                }
                if (!ga.GetComponent<MP_vGenericAction>())
                {
                    failed += 1;
                    failures.Add(new DebugFormat(
                        "vGenericAction - \"vGenericAction\" needs to be replaced with \"MP_vGenericAction\" to sync actions correctly.",
                        ga
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
*/
