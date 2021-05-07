/*

using CBGames.Core;
using CBGames.Player;
using Invector;
using Invector.vShooter;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Weapons/Arrow View")]
    public class ArrowView : MonoBehaviour
    {
        [Tooltip("Reference value only, this will be set externally by different functions. " +
            "This is the unique id for this arrow that will match the networked versions.")]
        public int viewId = 0;
        [Tooltip("The transform owner of this arrow.")]
        public Transform owner;

        /// <summary>
        /// Set the position and rotation over the network if this arrow is the owner based
        /// on the raycast hit information.
        /// </summary>
        /// <param name="hit">RaycastHit type, Where to set the position and rotation to over the network.</param>
        public virtual void NetworkSetPosRot(RaycastHit hit)
        {
            if (!hit.transform.GetComponentInParent<SyncPlayer>()) return;
            if (hit.transform.GetComponentInParent<PhotonView>().IsMine == false) return;

            int[] tree = StaticMethods.BuildChildTree(hit.transform.root, hit.transform, false);
            
            hit.transform.GetComponentInParent<PhotonView>().RPC(
                "SetArrowPositionRotation",
                RpcTarget.Others,
                viewId,
                tree,
                JsonUtility.ToJson(transform.position),
                JsonUtility.ToJson(transform.rotation)
            );
        }
    }
}

*/
