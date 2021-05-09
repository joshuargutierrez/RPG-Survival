/*
using Invector.vCharacterController.vActions;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vFreeClimb")]
    [RequireComponent(typeof(SyncPlayer))]

    public class MP_vFreeClimb : vFreeClimb
    {
        /// <summary>
        /// Disables this component if you're a networked player and not the owner player.
        /// </summary>
        protected override void Start()
        {
            if (GetComponent<PhotonView>().IsMine == false)
            {
                this.enabled = false;
            }
            else
            {
                base.Start();
            }
        }

        /// <summary>
        /// Calls `CrossFadeInFixedTime` RPC with the `ClimbJump` Animation. This makes 
        /// all the networked players play the `ClimbJump` animation.
        /// </summary>
        protected override void ClimbJump()
        {
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.OthersBuffered, "ClimbJump", 0.2f);
            base.ClimbJump();
        }

        /// <summary>
        /// Calls the `CrossFadeInFixedTime` RPC with `EnterClimbGrounded` or `EnterClimbAir`.
        /// This is to have the network players mimic climbing animations of the owner.
        /// </summary>
        protected override void EnterClimb()
        {
            var dragPosition = new Vector3(dragInfo.position.x, transform.position.y, dragInfo.position.z) + transform.forward * -TP_Input.cc._capsuleCollider.radius;
            var castObstacleUp = Physics.Raycast(dragPosition + transform.up * TP_Input.cc._capsuleCollider.height, transform.up, TP_Input.cc._capsuleCollider.height * 0.5f, obstacle);
            var castDragableWallForward = Physics.Raycast(dragPosition + transform.up * (TP_Input.cc._capsuleCollider.height * climbUpHeight), transform.forward, out hit, 1f, draggableWall) && draggableTags.Contains(hit.collider.gameObject.tag);
            var climbUpConditions = TP_Input.cc.isGrounded && !castObstacleUp && castDragableWallForward;
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.OthersBuffered, climbUpConditions ? "EnterClimbGrounded" : "EnterClimbAir", 0.2f);
            base.EnterClimb();
        }

        /// <summary>
        /// Calls the `CrossFadeInFixedTime` RPC with `ExitGrounded` or `ExitAir`. 
        /// This is to have the network players mimic the climbing animations of the owner.
        /// </summary>
        protected override void ExitClimb()
        {
            if (!inClimbUp)
            {
                bool nextGround = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.5f, TP_Input.cc.groundLayer);
                GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.OthersBuffered, nextGround ? "ExitGrounded" : "ExitAir", 0.2f);
            }
            base.ExitClimb();
        }

        /// <summary>
        /// Calls the `CrossFadeInFixedTime` RPC with `ClimbUpWall`. This is to have
        /// the network players mimic the climbing animatiosn of the owner.
        /// </summary>
        protected override void ClimbUp()
        {
            StartCoroutine(WaitForCrossFade());
            base.ClimbUp();
        }
        IEnumerator WaitForCrossFade()
        {
            var transition = 0f;
            var dir = transform.forward;
            dir.y = 0;
            var angle = Vector3.Angle(Vector3.up, transform.forward);

            var targetRotation = Quaternion.LookRotation(-dragInfo.normal);
            var targetPosition = ((dragInfo.position + dir * -TP_Input.cc._capsuleCollider.radius + Vector3.up * 0.1f) - transform.rotation * handTarget.localPosition);
            while (transition < 1 && Vector3.Distance(targetRotation.eulerAngles, transform.rotation.eulerAngles) > 0.2f && angle < 60)
            {
                yield return null;
            }
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.OthersBuffered, "ClimbUpWall", 0.1f);
        }
    }
}
*/
