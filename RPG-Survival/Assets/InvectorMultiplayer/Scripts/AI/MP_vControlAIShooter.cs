/*
using Photon.Pun;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    [AddComponentMenu("CB GAMES/AI/MP Components/MP vControlAIShooter")]
    public class MP_vControlAIShooter : vControlAIShooter, IPunObservable
    {
        protected Transform _leftUpperArm, _rightUpperArm, _leftHand, _rightHand;

        protected override void Start()
        {
            base.Start();
            _leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            try
            {
                if (PhotonNetwork.InRoom == false) return;
                if (stream.IsWriting)
                {
                    #region IK
                    stream.SendNext(upperArmRotation);
                    stream.SendNext(handRotation);
                    stream.SendNext(armAlignmentWeight);
                    stream.SendNext(handIKWeight);
                    stream.SendNext(handRotationAlignment);
                    stream.SendNext(upperArmRotationAlignment);
                    stream.SendNext(_onlyArmsLayerWeight);
                    stream.SendNext(weaponIKWeight);
                    stream.SendNext(rightRotationWeight);
                    #endregion

                    #region Actions
                    stream.SendNext(isCrouching);
                    stream.SendNext(ragdolled);
                    stream.SendNext(isInCombat);
                    stream.SendNext(isAiming);
                    stream.SendNext(isBlocking);
                    stream.SendNext(isJumping);
                    stream.SendNext(isGrounded);
                    stream.SendNext(isRolling);
                    stream.SendNext(IsReloading);
                    #endregion

                    #region Weapons/Targets
                    stream.SendNext(AimPosition);
                    stream.SendNext(aimTarget);
                    stream.SendNext(lastTargetPosition);
                    #endregion

                    stream.SendNext(currentHealth);
                }
                else if (stream.IsReading)
                {
                    #region IK
                    upperArmRotation = (Quaternion)stream.ReceiveNext();
                    handRotation = (Quaternion)stream.ReceiveNext();
                    armAlignmentWeight = (float)stream.ReceiveNext();
                    handIKWeight = (float)stream.ReceiveNext();
                    handRotationAlignment = (Quaternion)stream.ReceiveNext();
                    upperArmRotationAlignment = (Quaternion)stream.ReceiveNext();
                    _onlyArmsLayerWeight = (float)stream.ReceiveNext();
                    weaponIKWeight = (float)stream.ReceiveNext();
                    rightRotationWeight = (float)stream.ReceiveNext();
                    #endregion

                    #region Actions
                    isCrouching = (bool)stream.ReceiveNext();
                    ragdolled = (bool)stream.ReceiveNext();
                    isInCombat = (bool)stream.ReceiveNext();
                    isAiming = (bool)stream.ReceiveNext();
                    isBlocking = (bool)stream.ReceiveNext();
                    isJumping = (bool)stream.ReceiveNext();
                    isGrounded = (bool)stream.ReceiveNext();
                    isRolling = (bool)stream.ReceiveNext();
                    IsReloading = (bool)stream.ReceiveNext();
                    #endregion

                    #region Weapons/Targets
                    AimPosition = (Vector3)stream.ReceiveNext();
                    aimTarget = (Vector3)stream.ReceiveNext();
                    lastTargetPosition = (Vector3)stream.ReceiveNext();
                    #endregion

                    currentHealth = (float)stream.ReceiveNext();
                }
            }
            catch { }
        }

        #region IK
        /// <summary>
        /// Sets the IK positions, if not the owner will simply rotate IK placements
        /// according to received positions in the OnPhotonSerializeView function.
        /// </summary>
        protected override void UpdateAimBehaviour()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                base.UpdateAimBehaviour();
            }
            else
            {
                UpdateHeadTrack();
                HandleShots();
            }
        }

        /// <summary>
        /// Rotates the aim arm normally if owner, if networked version will only
        /// rotate to the values it receives in the OnPhotonSerializeView function.
        /// </summary>
        /// <param name="isUsingLeftHand"></param>
        protected override void RotateAimArm(bool isUsingLeftHand = false)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                base.RotateAimArm(isUsingLeftHand);
            }
            else if (CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightUpperArmToAim)
            {
                var upperArm = isUsingLeftHand ? _leftUpperArm : _rightUpperArm;
                if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
                    upperArm.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);
            }
        }

        /// <summary>
        /// Rotates the aim hand normally if owner, if networked version will only
        /// rotate to the values it receives in the OnPhotonSerializeView function.
        /// </summary>
        /// <param name="isUsingLeftHand"></param>
        protected override void RotateAimHand(bool isUsingLeftHand = false)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                base.RotateAimHand(isUsingLeftHand);
            }
            else if (CurrentActiveWeapon && armAlignmentWeight > 0.1f && _canAiming && CurrentActiveWeapon.alignRightHandToAim)
            {
                var hand = isUsingLeftHand ? _leftHand : _rightHand;
                if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
                    hand.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);
            }
        }
        #endregion

        #region Actions
        /// <summary>
        /// Attacks with hands/melee weapon. If owner will tell the networked version
        /// to do the exact same attack via RPC.
        /// </summary>
        /// <param name="strongAttack">bool type, Is this going to place the strong attack animation?</param>
        /// <param name="attackID">int type, The attack ID to play the correct animation type</param>
        /// <param name="forceCanAttack">bool type, *Read the invector documentation on this</param>
        public override void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                GetComponent<PhotonView>().RPC("vControlAIShooter_Attack", RpcTarget.Others, strongAttack, attackID, forceCanAttack);
                base.Attack(strongAttack, attackID, forceCanAttack);
            }
        }

        /// <summary>
        /// Rolls to a particular direction. If owner will tell the networked versions to roll to that 
        /// same direction. Call is done via RPC.
        /// </summary>
        /// <param name="direction"></param>
        public override void RollTo(Vector3 direction)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                if (!inTurn && !isRolling && !isJumping && isGrounded && !lockMovement && !customAction)
                {
                    GetComponent<PhotonView>().RPC("vControlAIShooter_RollTo", RpcTarget.Others, direction);
                }
                base.RollTo(direction);
            }
        }
        
        /// <summary>
        /// If owner will trigger the damage reaction animation and tell others to play that animation. 
        /// If networked player this will do nothing. Call to update others is done via RPC.
        /// </summary>
        /// <param name="damage">vDamage type, tells the type of hit reaction to do.</param>
        protected override void TriggerDamageRection(vDamage damage)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                if (!isRolling)
                {
                    if (animator != null && animator.enabled && !damage.activeRagdoll && currentHealth > 0)
                    {
                        if (damage.hitReaction)
                        {
                            if (triggerReactionHash.isValid && triggerResetStateHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerReactionHash, triggerResetStateHash });
                            }
                            else if (triggerReactionHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerReactionHash });
                            }
                            else if (triggerResetStateHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerResetStateHash });
                            }
                        }
                        else
                        {
                            if (triggerRecoilHash.isValid && triggerResetStateHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerRecoilHash, triggerResetStateHash });
                            }
                            else if (triggerRecoilHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerRecoilHash });
                            }
                            else if (triggerResetStateHash.isValid)
                            {
                                GetComponent<PhotonView>().RPC("vControlAIShooter_Triggers", RpcTarget.Others, new int[] { triggerResetStateHash });
                            }
                        }
                    }
                }
                base.TriggerDamageRection(damage);
            }
        }
        #endregion

        #region Set States
        /// <summary>
        /// Resets the attack trigger in the animator, if owner tell the others to do the same via RPC.
        /// </summary>
        public override void ResetAttackTriggers()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                GetComponent<PhotonView>().RPC("vControlAIShooter_ResetTriggers", RpcTarget.Others, new string[] { "StrongAttack", "WeakAttack" });
                base.ResetAttackTriggers();
            }
        }

        /// <summary>
        /// Heartbeat actions of the AI, this is only performed if the owner. If network player it only
        /// reacts to things the owner sends.
        /// </summary>
        protected override void UpdateAI()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                base.UpdateAI();
            }
        }

        /// <summary>
        /// Set the current target of the AI. If owner will tell the other networked versions to what 
        /// target they should update themselves to.
        /// </summary>
        /// <param name="target">Transform type, the target to target</param>
        /// <param name="overrideCanseTarget">bool type, force a sense on the target or not</param>
        public override void SetCurrentTarget(Transform target, bool overrideCanseTarget)
        {
            if (GetComponent<PhotonView>().IsMine == true && target.GetComponent<PhotonView>())
            {
                GetComponent<PhotonView>().RPC(
                    "vControlAIShooter_SetCurrentTarget", 
                    RpcTarget.Others, 
                    target.GetComponent<PhotonView>().ViewID, 
                    overrideCanseTarget
                );
            }
            base.SetCurrentTarget(target, overrideCanseTarget);
        }
        #endregion

        #region RPCs

        #region Triggers
        [PunRPC]
        void vControlAIShooter_Triggers(int[] triggerHashs)
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }
            foreach (int triggerHash in triggerHashs)
            {
                animator.SetTrigger(triggerHash);
            }
        }
        [PunRPC]
        void vControlAIShooter_Triggers(string[] triggerNames)
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }
            foreach (string triggerName in triggerNames)
            {
                animator.SetTrigger(triggerName);
            }
        }
        [PunRPC]
        void vControlAIShooter_ResetTriggers(string[] triggerNames)
        {
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }
            foreach (string triggerName in triggerNames)
            {
                animator.ResetTrigger(triggerName);
            }
        }
        #endregion

        #region Actions
        [PunRPC]
        void vControlAIShooter_RollTo(Vector3 direction)
        {
            RollTo(direction);
        }
        [PunRPC]
        void vControlAIShooter_Attack(bool strongAttack, int attackID, bool forceCanAttack)
        {
            Attack(strongAttack, attackID, forceCanAttack);
        }
        #endregion

        #region Sets
        [PunRPC]
        void vControlAIShooter_SetCurrentTarget(int viewId, bool overrideCanseTarget)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view)
            {
                SetCurrentTarget(view.transform, overrideCanseTarget);
            }
        }
        #endregion
        #endregion
    }
}
*/
