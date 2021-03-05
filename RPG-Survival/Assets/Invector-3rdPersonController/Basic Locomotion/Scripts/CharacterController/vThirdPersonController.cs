using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("THIRD PERSON CONTROLLER", iconName = "controllerIcon")]
    public class vThirdPersonController : vThirdPersonAnimator
    {
        /// <summary>
        /// Move the controller to a specific Position
        /// </summary>
        /// <param name="targetPosition"></param>
        public virtual void MoveToPosition(Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0;
            //moveDirection = dir.normalized;
            input = transform.InverseTransformDirection(dir.normalized);
            // calculate input smooth
            inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
        }

        /// <summary>
        /// Handle RootMotion movement and specific Actions
        /// </summary>
        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled)
            {
                return;
            }

            if (isRolling)
            {
                RollBehavior();
                return;
            }

            if (customAction || lockAnimMovement)
            {
                StopCharacterWithLerp();

                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            else if (inputSmooth == Vector3.zero && isGrounded)
            {
                animator.ApplyBuiltinRootMotion();
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
            {
                MoveCharacter(moveDirection);
            }
        }

        /// <summary>
        /// Set the Controller movement speed (rigidbody, animator and root motion)
        /// </summary>
        public virtual void ControlLocomotionType()
        {
            if (lockAnimMovement || lockMovement || customAction || isRolling)
            {
                return;
            }

            if (!lockSetMoveSpeed)
            {
                if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
                {
                    SetControllerMoveSpeed(freeSpeed);
                    SetAnimatorMoveSpeed(freeSpeed);
                }
                else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
                {
                    isStrafing = true;
                    SetControllerMoveSpeed(strafeSpeed);
                    SetAnimatorMoveSpeed(strafeSpeed);
                }
            }

            if (!useRootMotion)
            {
                MoveCharacter(moveDirection);
            }
        }

        /// <summary>
        /// Manage the Control Rotation Type of the Player
        /// </summary>
        public virtual void ControlRotationType()
        {
            if (lockAnimRotation || lockRotation || customAction || isRolling)
            {
                return;
            }

            bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            if (validInput)
            {
                if (lockAnimMovement)
                {
                    // calculate input smooth
                    inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                }
                Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection(dir);
            }
        }

        /// <summary>
        /// Use it to keep the direction the Player is moving (most used with CCV camera)
        /// </summary>
        public virtual void ControlKeepDirection()
        {
            // update oldInput to compare with current Input if keepDirection is true
            if (!keepDirection)
            {
                oldInput = input;
            }
            else if ((input.magnitude < 0.01f || Vector3.Distance(oldInput, input) > 0.9f) && keepDirection)
            {
                keepDirection = false;
            }
        }

        /// <summary>
        /// Determine the direction the player will face based on input and the referenceTransform
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (isRolling && !rollControl || input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        /// <summary>
        /// Set the isSprinting bool and manage the Sprint Behavior 
        /// </summary>
        /// <param name="value"></param>
        public virtual void Sprint(bool value)
        {
            var sprintConditions = (currentStamina > 0 && hasMovementInput && isGrounded && !customAction &&
                !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f) && !sprintOnlyFree));

            if (value && sprintConditions)
            {
                if (currentStamina > (finishStaminaOnSprint ? sprintStamina : 0) && hasMovementInput)
                {
                    finishStaminaOnSprint = false;
                    if (isGrounded && useContinuousSprint)
                    {
                        isCrouching = false;
                        isSprinting = !isSprinting;
                        if (isSprinting)
                        {
                            OnStartSprinting.Invoke();
                        }
                        else
                        {
                            OnFinishSprinting.Invoke();
                        }
                    }
                    else if (!isSprinting)
                    {
                        OnStartSprinting.Invoke();
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    if (currentStamina <= 0)
                    {
                        finishStaminaOnSprint = true;
                        OnFinishSprintingByStamina.Invoke();
                    }
                    isSprinting = false;
                    OnFinishSprinting.Invoke();
                }
            }
            else if (isSprinting && (!useContinuousSprint || !sprintConditions))
            {
                if (currentStamina <= 0)
                {
                    finishStaminaOnSprint = true;
                    OnFinishSprintingByStamina.Invoke();
                }

                isSprinting = false;
                OnFinishSprinting.Invoke();
            }
        }

        /// <summary>
        /// Manage the isCrouching bool
        /// </summary>
        public virtual void Crouch()
        {
            if (isGrounded && !customAction)
            {
                AutoCrouch();
                if (isCrouching && CanExitCrouch())
                {
                    isCrouching = false;
                }
                else
                {
                    isCrouching = true;
                    isSprinting = false;
                }
            }
        }

        /// <summary>
        /// Set the isStrafing bool
        /// </summary>
        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        /// <summary>
        /// Triggers the Jump Animation and set the necessary variables to make the Jump behavior in the <seealso cref="vThirdPersonMotor"/>
        /// </summary>
        /// <param name="consumeStamina">Option to consume or not the stamina</param>
        public virtual void Jump(bool consumeStamina = false)
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;
            OnJump.Invoke();

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
            {
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            }
            else
            {
                animator.CrossFadeInFixedTime("JumpMove", .2f);
            }

            // reduce stamina
            if (consumeStamina)
            {
                ReduceStamina(jumpStamina, false);
                currentStaminaRecoveryDelay = 1f;
            }
        }

        /// <summary>
        /// Triggers the Roll Animation and set the stamina cost for this action
        /// </summary>
        public virtual void Roll()
        {
            isRolling = true;
            animator.CrossFadeInFixedTime("Roll", 0.1f);
            ReduceStamina(rollStamina, false);
            currentStaminaRecoveryDelay = 2f;
        }

        #region Check Action Triggers 

        /// <summary>
        /// Call this in OnTriggerEnter or OnTriggerStay to check if enter in triggerActions     
        /// </summary>
        /// <param name="other">collider trigger</param>                         
        protected override void OnTriggerStay(Collider other)
        {
            try
            {
                CheckForAutoCrouch(other);
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e.Message);
            }
            base.OnTriggerStay(other);
        }

        /// <summary>
        /// Call this in OnTriggerExit to check if exit of triggerActions 
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerExit(Collider other)
        {
            AutoCrouchExit(other);
            base.OnTriggerExit(other);
        }

        #endregion
    }
}