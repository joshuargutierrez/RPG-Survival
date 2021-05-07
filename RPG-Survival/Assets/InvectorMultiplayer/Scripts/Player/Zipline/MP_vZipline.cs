/*
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Photon.Pun;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vZipline")]
    public class MP_vZipline : vZipLine
    {
        private bool mp_isUsingZipline = false;
        private bool mp_inExitZipline;
        private Transform mp_nearestPoint;
        private vThirdPersonInput mp_tpInput;

        /// <summary>
        /// Only called by the owner player. Calls the `MP_InitiateZipline` function.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (GetComponent<PhotonView>().IsMine == true && other.gameObject.CompareTag(ziplineTag) && !mp_isUsingZipline)
            {
                var ap = other.gameObject.GetComponent<vZiplineAnchorPoints>();
                if (!mp_tpInput) mp_tpInput = GetComponent<vThirdPersonInput>();
                mp_nearestPoint = getNearPoint(ap.ziplineDirectionRef, mp_tpInput.transform.position + Vector3.up * (mp_tpInput.cc._capsuleCollider.height + heightOffSet));
                if (!enterZipline.useInput)
                    MP_InitiateZipline(other);
            }
        }


        /// <summary>
        /// Only called by the owner player. Calls the `MP_ExitZipline` function.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (GetComponent<PhotonView>().IsMine == true && other.gameObject.CompareTag(ziplineTag) && mp_isUsingZipline)
            {
                mp_inExitZipline = false;
                if (mp_isUsingZipline)
                {
                    mp_isUsingZipline = false;
                    MP_ExitZipline();
                }
            }
        }

        /// <summary>
        /// Only called by the owner player. Calls `MP_InitiateZipline`, `MP_ExitZipline`, or `MP_UsingZipline`
        /// based on the current stat of the owner player.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other)
        {
            if (GetComponent<PhotonView>().IsMine == true && other.gameObject.CompareTag(ziplineTag))
            {
                // enter the zipline only if you press the enterZipline input
                if (enterZipline.GetButton() && !mp_isUsingZipline && !mp_inExitZipline)
                    MP_InitiateZipline(other);
                // exit the zipline by pressing the exitZipline input
                if (exitZipline.GetButtonDown() && mp_isUsingZipline && !mp_inExitZipline)
                {
                    mp_inExitZipline = true;
                    MP_ExitZipline();
                }
                MP_UsingZipline(other);
            }
        }

        /// <summary>
        /// Sets the animation clip, rigidbody settings, and calls the `NetworkOnZiplineEnter`
        /// RPC for all networked players.
        /// </summary>
        /// <param name="other"></param>
        void MP_InitiateZipline(Collider other)
        {
            if (mp_tpInput && mp_nearestPoint)
            {
                mp_isUsingZipline = true;
                GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.Others, animationClip, 0.2f);
                GetComponent<PhotonView>().RPC("UseRigidBodyGravity", RpcTarget.OthersBuffered, false);
                GetComponent<PhotonView>().RPC("FreezeRigidbodyContraints", RpcTarget.Others, true);
                GetComponent<PhotonView>().RPC("NetworkOnZiplineEnter", RpcTarget.Others);
            }
        }

        /// <summary>
        /// Sets the animation, rigidbody settings, and calls the `NetworkOnZiplineExit` RPC for all
        /// networked players.
        /// </summary>
        void MP_ExitZipline()
        {
            if (!useExitTrigger) return;
            if (mp_tpInput)
            {
                GetComponent<PhotonView>().RPC("UseRigidBodyGravity", RpcTarget.OthersBuffered, true);
                GetComponent<PhotonView>().RPC("UseRigidBodyGravity", RpcTarget.Others, true);
                GetComponent<PhotonView>().RPC("FreezeRigidbodyContraints", RpcTarget.Others, false);
                GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.Others, "Falling", 0.2f);
                mp_isUsingZipline = false;
                GetComponent<PhotonView>().RPC("NetworkOnZiplineExit", RpcTarget.Others);
            }
        }

        /// <summary>
        /// Calls the `NetworkOnZiplineUsing` RPC for all networked players.
        /// </summary>
        /// <param name="other"></param>
        void MP_UsingZipline(Collider other)
        {
            if (!mp_isUsingZipline) return;
            if (mp_tpInput)
            {
                GetComponent<PhotonView>().RPC("NetworkOnZiplineUsing", RpcTarget.Others);
            }
        }
    }
}
*/
