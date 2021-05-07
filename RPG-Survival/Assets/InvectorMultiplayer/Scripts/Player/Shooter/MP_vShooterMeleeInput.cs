using CBGames.Core;
using CBGames.Objects;
using Invector.vEventSystems;
using Invector.vShooter;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vShooterMeleeInput")]
    public class MP_vShooterMeleeInput : vShooterMeleeInput, IPunObservable
    {
        #region Parameters
        protected bool initialized = false;
        protected Vector3 _targetArmAligmentDirection = Vector3.zero;
        protected Vector3 _targetArmAlignmentPosition = Vector3.zero;
        protected bool primaryTrigger = false;
        protected bool secondaryTrigger = false;
        protected vShooterWeapon _targetWeapon = null;
        protected MP_vShooterManager _shooterManager = null;
        protected vThirdPersonController controller = null;
        protected bool receivedTriggerRPC = false;
        protected bool sentTriggerRPC = false;
        protected bool _sendTriggerRunning = false;
        protected int _priorShotAmmot = 30;
        protected PhotonView pv = null;
        #endregion

        #region Initializations
        protected override void Start()
        {
            pv = GetComponent<PhotonView>();
            _shooterManager = GetComponent<MP_vShooterManager>();
            if (pv.IsMine == true)
            {
                base.Start();
            }
            else
            {
                controller = GetComponent<vThirdPersonController>();
                shooterManager = GetComponent<vShooterManager>();
                leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

                onlyArmsLayer = animator.GetLayerIndex("OnlyArms");
                aimAngleReference = new GameObject("aimAngleReference");
                aimAngleReference.tag = ("Ignore Ragdoll");
                aimAngleReference.transform.rotation = transform.rotation;
                var chest = animator.GetBoneTransform(HumanBodyBones.Head);
                aimAngleReference.transform.SetParent(chest);
                aimAngleReference.transform.localPosition = Vector3.zero;
                defaultStrafeWalk = cc.strafeSpeed.walkByDefault;
                headTrack = GetComponent<vHeadTrack>();
                lastRotateWithCamera = cc.strafeSpeed.rotateWithCamera;
                if (!controlAimCanvas)
                    Debug.LogWarning("Missing the AimCanvas, drag and drop the prefab to this scene in order to Aim", gameObject);
            }
            cc = (cc == null) ? GetComponent<vThirdPersonController>() : cc;
            StartCoroutine(WaitToInitialize());
        }
        IEnumerator WaitToInitialize()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            initialized = true;
        }
        #endregion

        #region Melee Attacks
        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player.
        /// </summary>
        public override void OnEnableAttack()
        {
            if (pv.IsMine == false) return;
            base.OnEnableAttack();
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player.
        /// </summary>
        public override void OnDisableAttack()
        {
            if (pv.IsMine == false) return;
            base.OnDisableAttack();
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Als will reset the 
        /// triggers for the networked players when called via the `ResetTriggers` RPC.
        /// </summary>
        public override void ResetAttackTriggers()
        {
            if (pv.IsMine == false) return;
            string[] triggers = new string[2] { "WeakAttack", "StrongAttack" };
            pv.RPC("ResetTriggers", RpcTarget.Others, (object)triggers);

            base.ResetAttackTriggers();
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player.
        /// </summary>
        public override void BreakAttack(int breakAtkID)
        {
            if (pv.IsMine == false) return;
            base.BreakAttack(breakAtkID);
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Will also set the triggers
        /// for recoil and strong attack and reset the triggers for weak attack and strong attack
        /// for the networked players.
        /// </summary>
        public override void OnRecoil(int recoilID)
        {
            if (pv.IsMine == false) return;
            string[] triggers = new string[2] { "TriggerRecoil", "StrongAttack" };
            pv.RPC("SetTriggers", RpcTarget.Others, (object)triggers);

            string[] resettriggers = new string[2] { "WeakAttack", "StrongAttack" };
            pv.RPC("ResetTriggers", RpcTarget.Others, (object)resettriggers);

            base.OnRecoil(recoilID);
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Triggers the weak attack
        /// animation for networked players to mimic the owner player via the `SetTriggers` RPC.
        /// </summary>
        public override void TriggerWeakAttack()
        {
            if (pv.IsMine == false) return;
            string[] triggers = new string[1] { "WeakAttack" };
            pv.RPC("SetTriggers", RpcTarget.Others, (object)triggers);
            PhotonNetwork.SendAllOutgoingCommands();

            base.TriggerWeakAttack();
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Triggers the strong attack
        /// animation for networked players to mimic the owner player via the `SetTriggers` RPC.
        /// </summary>
        public override void TriggerStrongAttack()
        {
            if (pv.IsMine == false) return;
            string[] triggers = new string[1] { "StrongAttack" };
            pv.RPC("SetTriggers", RpcTarget.Others, (object)triggers);
            PhotonNetwork.SendAllOutgoingCommands();

            base.TriggerStrongAttack();
        }
        public override void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            if (cc == null)
            {
                cc = GetComponent<vThirdPersonController>();
            }
            base.OnReceiveAttack(damage, attacker);
        }
        #endregion

        #region Animator Weights
        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Sets the animator layer
        /// weights for the networked players to mimic the owner player via the `SetAnimatorLayerWeights`
        /// RPC.
        /// </summary>
        public override void ResetShooterAnimations()
        {
            if (pv.IsMine == false) return;
            base.ResetShooterAnimations();
            pv.RPC("SetAnimatorLayerWeights", RpcTarget.Others, onlyArmsLayer, onlyArmsLayerWeight);
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Sets the animator layer
        /// weights for the networked players to mimic the owner player via the `SetAnimatorLayerWeights`
        /// RPC.
        /// </summary>
        protected override void UpdateShooterAnimations()
        {
            if (pv.IsMine == false) return;
            base.UpdateShooterAnimations();
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, (CurrentActiveWeapon) ? 1f : 0f, 6f * Time.deltaTime);
            pv.RPC("SetAnimatorLayerWeights", RpcTarget.Others, onlyArmsLayer, onlyArmsLayerWeight);
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Otherwise the functionality is 
        /// the same but it also sets the animator layer weights for the networked players to mimic 
        /// the owner player via the `SetAnimatorLayerWeights`
        /// RPC.
        /// </summary>
        protected override void UpdateMeleeAnimations()
        {
            if (pv.IsMine == false) return;
            if (!animator) return;

            if (cc.customAction)
            {
                ResetMeleeAnimations();
                ResetShooterAnimations();
                UpdateCameraStates();
                CancelAiming();
                return;
            }
            if ((shooterManager == null || !CurrentActiveWeapon) && meleeManager)
            {
                base.UpdateMeleeAnimations();
                onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0, 6f * Time.deltaTime);
                animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
                pv.RPC("SetAnimatorLayerWeights", RpcTarget.Others, onlyArmsLayer, onlyArmsLayerWeight);
                animator.SetBool(vAnimatorParameters.IsAiming, false);
                isReloading = false;
            }
            else if (shooterManager && CurrentActiveWeapon)
                UpdateShooterAnimations();
            else
            {
                ResetMeleeAnimations();
                ResetShooterAnimations();
            }
        }
        #endregion

        #region Heartbeat
        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player.
        /// </summary>
        public override void OnAnimatorMoveEvent()
        {
            if (cc == null || pv.IsMine == false) return;
            base.OnAnimatorMoveEvent();
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player. Also calls the 
        /// `ReceivePulledTrigger` function if a networked version of a player or the 
        /// `SendPulledTrigger` function if the owner player.
        /// </summary>
        protected override void Update()
        {
            if (pv.IsMine == false)
            {
                ReceivePulledTrigger();
            }
            else
            {
                StartCoroutine(SendPulledTrigger());
                base.Update();
            }
        }

        /// <summary>
        /// Overrides default functionality of invector to only work if you're the owner player
        /// and will not work if this is called by a networked player.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (pv.IsMine == true)
            {
                base.FixedUpdate();
            }
            else
            {
                updateIK = true;
            }
        }

        /// <summary>
        /// Overrides default functionality of invector to perform the same if you're the 
        /// owner player. Otherwise will only update its aiming location based on what
        /// is received over the network.
        /// </summary>
        protected override void LateUpdate()
        {
            if (pv.IsMine == true)
            {
                base.LateUpdate();
            }
            else if (initialized == true)
            {
                if ((!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;
                UpdateAimBehaviour();
                updateIK = false;
            }
        }
        #endregion

        #region IK
        protected override Vector3 targetArmAlignmentPosition
        {
            get
            {
                if (pv.IsMine == true)
                {
                    if (cameraMain == null)
                    {
                        return Vector3.zero;
                    }
                    else
                    {
                        return base.targetArmAlignmentPosition;
                    }
                }
                else
                {
                    return _targetArmAlignmentPosition;
                }
            }
        }
        protected override Vector3 targetArmAligmentDirection
        {
            get
            {
                if (pv.IsMine == true)
                {
                    if (cameraMain == null)
                    {
                        return Vector3.zero;
                    }
                    else
                    {
                        return base.targetArmAligmentDirection;
                    }
                }
                else
                {
                    return _targetArmAligmentDirection;
                }
            }
        }
        protected override void UpdateAimBehaviour()
        {
            if (pv.IsMine == true)
            {
                base.UpdateAimBehaviour();
            }
            else
            {
                UpdateHeadTrack();
                if (shooterManager && CurrentActiveWeapon && !controller.isDead)
                {
                    UpdateIKAdjust(shooterManager.IsLeftWeapon);
                    RotateAimArm(shooterManager.IsLeftWeapon);
                    RotateAimHand(shooterManager.IsLeftWeapon);
                    UpdateArmsIK(shooterManager.IsLeftWeapon);
                }
            }
        }
        #endregion

        #region Shooter Weapons
        #region Scope UI
        public override vControlAimCanvas controlAimCanvas
        {
            get
            {
                if (!_controlAimCanvas && pv.IsMine)
                {
                    _controlAimCanvas = FindObjectOfType<vControlAimCanvas>();
                    if (_controlAimCanvas)
                        _controlAimCanvas.Init(cc);
                }

                return _controlAimCanvas;
            }
        }
        protected override void UpdateAimHud()
        {
            if (pv.IsMine)
            {
                base.UpdateAimHud();
            }
        }
        protected override void UpdateAimPosition()
        {
            if (pv.IsMine)
            {
                base.UpdateAimPosition();
            }
        }
        public override void AimInput()
        {
            if (pv.IsMine)
            {
                base.AimInput();
            }
        }
        public override void EnableScopeView()
        {
            if (pv.IsMine)
            {
                base.EnableScopeView();
            }
        }
        public override void DisableScopeView()
        {
            if (pv.IsMine)
            {
                base.DisableScopeView();
            }
        }
        public override void ScopeViewInput()
        {
            if (pv.IsMine)
            {
                base.ScopeViewInput();
            }
        } 
        #endregion

        #region Shots
        /// <summary>
        /// This is only used by networked versions of players to receive when the owner player
        /// is pulling or releasing the trigger to the shooter weapon. It is responsible for 
        /// firing the target weapon or to stop firing the target weapon.
        /// </summary>
        protected virtual void ReceivePulledTrigger()
        {
            if (receivedTriggerRPC || primaryTrigger || secondaryTrigger)
            {
                if (_targetWeapon != null)
                {
                    if (_targetWeapon.ammoCount > 0)
                    {
                        _targetWeapon.Shoot(_shooterManager.lastAimPos, transform);
                        receivedTriggerRPC = false;
                    }
                    else if (_targetWeapon.emptyClip != null)
                    {
                        _targetWeapon.source.Stop();
                        _targetWeapon.source.PlayOneShot(_targetWeapon.emptyClip);
                        receivedTriggerRPC = false;
                    }
                    if (!_targetWeapon.automaticWeapon || _targetWeapon.ammoCount < 1)
                    {
                        primaryTrigger = false;
                        secondaryTrigger = false;
                    }
                }
            }
        }

        /// <summary>
        /// This is only called by the owner player. This will send when the weapon trigger is 
        /// pulled or released. This is used in combination with the `ReceivePulledTrigger`
        /// function. It will send the weapon that is firing/not firing, the ammo for it, and 
        /// where it is firing to.
        /// </summary>
        protected virtual IEnumerator SendPulledTrigger()
        {
            if (_sendTriggerRunning) yield return null;
            if (shooterManager.CurrentWeapon != null && (shooterManager.hipfireShot || IsAiming) && shotInput.GetButtonDown())
            {
                if (shooterManager.CurrentWeapon.CanDoShot && shooterManager.CurrentWeapon.ammoCount > 0 ||
                    shooterManager.CurrentWeapon.CanDoEmptyClip && shooterManager.CurrentWeapon.ammoCount < 1)
                {
                    _priorShotAmmot = shooterManager.CurrentWeapon.ammoCount;
                    yield return new WaitUntil(() => _shooterManager.firedShot == true);
                    if (shooterManager.CurrentWeapon.automaticWeapon)
                    {
                        sentTriggerRPC = true;
                    }
                    if (!shooterManager.CurrentWeapon.transform.GetComponent<MP_ShooterWeapon>())
                    {
                        Debug.LogError("The current weapon doesn't have a \"MP_ShooterWeapon\" component on it. Open your ItemListData that you're using and make sure you're using the MP versions! Run the Convert > Prefabs menu to help with this.", shooterManager.CurrentWeapon.transform);
                    }
                    else
                    {
                        PhotonNetwork.SetSendingEnabled(0, true);
                        pv.RPC(
                            "PullTrigger",
                            RpcTarget.Others,
                            true,
                            _priorShotAmmot,
                            _shooterManager.lastAimPos,
                            shooterManager.CurrentWeapon.transform.GetComponent<MP_ShooterWeapon>().childTree
                        );
                        PhotonNetwork.SendAllOutgoingCommands();
                    }
                    _shooterManager.firedShot = false;
                }
            }
            else if (sentTriggerRPC == true && shooterManager.CurrentWeapon != null && shotInput.GetButtonUp())
            {
                sentTriggerRPC = false;
                if (!shooterManager.CurrentWeapon.transform.GetComponent<MP_ShooterWeapon>())
                {
                    Debug.LogError("The current weapon doesn't have a \"MP_ShooterWeapon\" component on it. Open your ItemListData that you're using and make sure you're using the MP versions! Run the Convert > Prefabs menu to help with this.", shooterManager.CurrentWeapon.transform);
                }
                else
                {
                    PhotonNetwork.SetSendingEnabled(0, true);
                    pv.RPC(
                        "PullTrigger",
                        RpcTarget.Others,
                        false,
                        shooterManager.CurrentWeapon.ammoCount,
                        _shooterManager.lastAimPos,
                        shooterManager.CurrentWeapon.transform.GetComponent<MP_ShooterWeapon>().childTree
                    );
                    PhotonNetwork.SendAllOutgoingCommands();
                }
                _shooterManager.firedShot = false;
            }
            _sendTriggerRunning = false;
        }
        #endregion

        #endregion

        #region Input
        protected override void InputHandle()
        {
            if (pv.IsMine)
            {
                base.InputHandle();
            }
        }
        #endregion

        #region RPCs
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
        {
            try
            {
                if (initialized == false) return;
                if (stream.IsWriting)
                {
                    stream.SendNext(targetArmAligmentDirection);
                    stream.SendNext(targetArmAlignmentPosition);
                    stream.SendNext(_isAiming);
                    stream.SendNext(isReloading);
                    stream.SendNext(isEquipping);
                    stream.SendNext(cc.isStrafing);
                    stream.SendNext(AimPosition);
                    stream.SendNext(_aimTimming);
                    stream.SendNext(aimConditions);
                    stream.SendNext(isUsingScopeView);
                    stream.SendNext(isCameraRightSwitched);
                    stream.SendNext(onlyArmsLayerWeight);
                    stream.SendNext(supportIKWeight);
                    stream.SendNext(weaponIKWeight);
                    stream.SendNext(armAlignmentWeight);
                    stream.SendNext(aimWeight);
                    stream.SendNext(lastAimDistance);
                    stream.SendNext(lastRotateWithCamera);
                    stream.SendNext(aimAngleReference.transform.position);
                    stream.SendNext(aimAngleReference.transform.rotation);
                    stream.SendNext(aimAngleReference.transform.localPosition);
                    stream.SendNext(_shooterManager.lastAimPos);
                }
                else if (stream.IsReading)
                {
                    _targetArmAligmentDirection = (Vector3)stream.ReceiveNext();
                    _targetArmAlignmentPosition = (Vector3)stream.ReceiveNext();
                    _isAiming = (bool)stream.ReceiveNext();
                    isReloading = (bool)stream.ReceiveNext();
                    isEquipping = (bool)stream.ReceiveNext();
                    cc.isStrafing = (bool)stream.ReceiveNext();
                    AimPosition = (Vector3)stream.ReceiveNext();
                    _aimTimming = (float)stream.ReceiveNext();
                    aimConditions = (bool)stream.ReceiveNext();
                    isUsingScopeView = (bool)stream.ReceiveNext();
                    isCameraRightSwitched = (bool)stream.ReceiveNext();
                    onlyArmsLayerWeight = (float)stream.ReceiveNext();
                    supportIKWeight = (float)stream.ReceiveNext();
                    weaponIKWeight = (float)stream.ReceiveNext();
                    armAlignmentWeight = (float)stream.ReceiveNext();
                    aimWeight = (float)stream.ReceiveNext();
                    lastAimDistance = (float)stream.ReceiveNext();
                    lastRotateWithCamera = (bool)stream.ReceiveNext();
                    aimAngleReference.transform.position = (Vector3)stream.ReceiveNext();
                    aimAngleReference.transform.rotation = (Quaternion)stream.ReceiveNext();
                    aimAngleReference.transform.localPosition = (Vector3)stream.ReceiveNext();
                    _shooterManager.lastAimPos = (Vector3)stream.ReceiveNext();
                }
            }
            catch { }
        }

        [PunRPC]
        void PullTrigger(bool primary, int ammoAmount, Vector3 aimPos, int[] childTree)
        {
            _shooterManager.lastAimPos = aimPos;
            Transform target = StaticMethods.FindTargetChild(childTree, transform);
            if (target != null && target.GetComponent<vShooterWeapon>())
            {
                _targetWeapon = target.GetComponent<vShooterWeapon>();
                if (ammoAmount != _targetWeapon.ammoCount)
                {
                    if (ammoAmount < _targetWeapon.ammoCount)
                    {
                        if (_targetWeapon.ammoCount > 0 && ammoAmount > 0)
                        {
                            _targetWeapon.UseAmmo(_targetWeapon.ammoCount % ammoAmount);
                        }
                        else if (ammoAmount == 0 && _targetWeapon.ammoCount > 0)
                        {
                            _targetWeapon.UseAmmo(_targetWeapon.ammoCount);
                        }
                    }
                    else if (ammoAmount > _targetWeapon.ammoCount)
                    {
                        if (_targetWeapon.ammoCount > 0)
                        {
                            _targetWeapon.AddAmmo(ammoAmount % _targetWeapon.ammoCount);
                        }
                        else
                        {
                            _targetWeapon.AddAmmo(ammoAmount);
                        }
                    }
                }
            }
            primaryTrigger = primary;
            //secondaryTrigger = secondary;
            //if (primary || secondary)
            //{
            //    receivedTriggerRPC = true;
            //}
        }
        #endregion
    }
}
