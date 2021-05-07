#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using UnityEditor;

namespace CBGames.Editors
{
    public class E_CompHelper : EditorWindow
    {
        public static void ReplaceWithComponent(GameObject target, Type org, Type mp_version, bool addToPhotonView = false)
        {
            if (!target.GetComponent(org)) return;
            Component orgComp = target.GetComponent(org);
            Component newComp = null;
            if (!target.GetComponent(mp_version))
            {
                target.AddComponent(mp_version);
            }
            newComp = target.GetComponent(mp_version);
            List<string> skips = E_Helpers.CopyComponentValues(orgComp, newComp);
            DestroyImmediate(orgComp);
            if (addToPhotonView == true)
            {
                AddCompToPhotonView(target, newComp);
            }
        }
        public static void AddCompToPhotonView(GameObject target, Component comp)
        {
            if (!target.GetComponent<PhotonView>())
            {
                target.AddComponent<PhotonView>();
            }
            List<Component> comps = new List<Component>();
            if (target.GetComponent<PhotonView>().ObservedComponents != null && target.GetComponent<PhotonView>().ObservedComponents.Count > 0)
            {
                comps = target.GetComponent<PhotonView>().ObservedComponents;
            }
            comps.Add(comp);
            target.GetComponent<PhotonView>().ObservedComponents = comps;
            if (target.GetComponent<PhotonView>().Synchronization == ViewSynchronization.Off)
            {
                target.GetComponent<PhotonView>().Synchronization = ViewSynchronization.UnreliableOnChange;
            }
        }
    }
}
#endif