﻿using UnityEngine;

namespace Invector.vCharacterController
{
    using vEventSystems;
    using vMelee;

    // here you can modify the Melee Combat inputs
    // if you want to modify the Basic Locomotion inputs, go to the vThirdPersonInput
    [vClassHeader("MELEE INPUT MANAGER", iconName = "inputIcon")]
    public class vMeleeCombatInput : vThirdPersonInput, vIMeleeFighter
    {
        #region Variables                

        [vEditorToolbar("Inputs")]
        [Header("Melee Inputs")]
        public GenericInput weakAttackInput = new GenericInput("Mouse0", "RB", "RB");
        public GenericInput strongAttackInput = new GenericInput("Alpha1", false, "RT", true, "RT", false);
        public GenericInput blockInput = new GenericInput("Mouse1", "LB", "LB");        

        internal vMeleeManager meleeManager;
        protected bool _isAttacking;
        public bool isAttacking { get => _isAttacking || cc.IsAnimatorTag("Attack"); protected set { _isAttacking = value; } }
        public bool isBlocking { get; protected set; }
        public bool isArmed { get { return meleeManager != null && (meleeManager.rightWeapon != null || (meleeManager.leftWeapon != null && meleeManager.leftWeapon.meleeType != vMeleeType.OnlyDefense)); } }
        public bool isEquipping { get; protected set; }

        [HideInInspector]
        public bool lockMeleeInput;

        public void SetLockMeleeInput(bool value)
        {
            lockMeleeInput = value;

            if (value)
            {
                isAttacking = false;
                isBlocking = false;            
            }
        }

        public override void SetLockAllInput(bool value)
        {
            base.SetLockAllInput(value);
            SetLockMeleeInput(value);
        }

        #endregion

        public virtual bool lockInventory
        {
            get
            {
                return isAttacking || cc.isDead || cc.customAction || cc.isRolling;
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void LateUpdate()
        {
            UpdateMeleeAnimations();
            base.LateUpdate();
        }

        protected override void FixedUpdate()
        {            
            base.FixedUpdate();
        }

        protected override void InputHandle()
        {
            if (cc == null || cc.isDead)
                return;

            base.InputHandle();

            if (MeleeAttackConditions() && !lockMeleeInput)
            {
                MeleeWeakAttackInput();
                MeleeStrongAttackInput();
                BlockingInput();
            }
            else
            {
                ResetAttackTriggers();
                isBlocking = false;
            }            
        }

        #region MeleeCombat Input Methods

        /// <summary>
        /// WEAK ATK INPUT
        /// </summary>
        public virtual void MeleeWeakAttackInput()
        {            
            if (cc.animator == null) return;

            if (weakAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
            {                
                TriggerWeakAttack();
            }
        }

        public virtual void TriggerWeakAttack()
        {                
            cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
            cc.animator.SetTrigger(vAnimatorParameters.WeakAttack);
        }

        /// <summary>
        /// STRONG ATK INPUT
        /// </summary>
        public virtual void MeleeStrongAttackInput()
        {
            if (cc.animator == null) return;

            if (strongAttackInput.GetButtonDown() && (!meleeManager.CurrentActiveAttackWeapon || meleeManager.CurrentActiveAttackWeapon.useStrongAttack) && MeleeAttackStaminaConditions())
            {
                TriggerStrongAttack();
            }
        }

        public virtual void TriggerStrongAttack()
        {
            cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
            cc.animator.SetTrigger(vAnimatorParameters.StrongAttack);
        }

        /// <summary>
        /// BLOCK INPUT
        /// </summary>
        public virtual void BlockingInput()
        {
            if (cc.animator == null) return;

            isBlocking = blockInput.GetButton() && cc.currentStamina > 0 && !cc.customAction && !isAttacking;
        }

        /// <summary>
        /// Override the Sprint method to cancel Sprinting when Attacking
        /// </summary>
        protected override void SprintInput()
        {
            if (sprintInput.useInput)
            {
                bool canSprint = cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton();
                cc.Sprint(canSprint && !isAttacking);
            }                
        }

        #endregion

        #region Conditions

        protected virtual bool MeleeAttackStaminaConditions()
        {
            var result = cc.currentStamina - meleeManager.GetAttackStaminaCost();
            return result >= 0;
        }

        protected virtual bool MeleeAttackConditions()
        {           
            if (meleeManager == null) meleeManager = GetComponent<vMeleeManager>();
            return meleeManager != null && cc.isGrounded && !cc.customAction && !cc.isJumping && !cc.isCrouching && !cc.isRolling && !isEquipping && !cc.animator.IsInTransition(cc.baseLayer);
        }

        protected override bool JumpConditions()
        {
            return !isAttacking && !cc.customAction && !cc.isCrouching && cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && cc.currentStamina >= cc.jumpStamina && !cc.isJumping;
        }

        protected override bool RollConditions()
        {
            return (!cc.isRolling || cc.canRollAgain) && 
                cc.input != Vector3.zero && !cc.customAction && cc.isGrounded && cc.currentStamina > cc.rollStamina && 
                !cc.isJumping && !isAttacking && !cc.animator.IsInTransition(cc.upperBodyLayer) && !cc.animator.IsInTransition(cc.fullbodyLayer);
        }

        #endregion

        #region Update Animations        

        protected virtual void UpdateMeleeAnimations()
        {
            if (cc.animator == null || meleeManager == null) return;

            cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
            cc.animator.SetInteger(vAnimatorParameters.DefenseID, DefenseID);
            cc.animator.SetBool(vAnimatorParameters.IsBlocking, isBlocking);
            cc.animator.SetFloat(vAnimatorParameters.MoveSet_ID, meleeMoveSetID, .2f, vTime.deltaTime);
            isEquipping = cc.IsAnimatorTag("IsEquipping");
        }

        /// <summary>
        /// Default moveset id used when is without weapon
        /// </summary>
        public virtual int defaultMoveSetID { get; set; }   
        
        /// <summary>
        /// Used to ignore the Weapon moveset id and use the <seealso cref="defaultMoveSetID"/>
        /// </summary>
        public virtual bool overrideWeaponMoveSetID { get; set; }

        /// <summary>
        /// Current moveset id based if is using weapon or not
        /// </summary>
        public virtual int meleeMoveSetID
        {
            get
            {
                int id = meleeManager.GetMoveSetID();
                if (id == 0  || overrideWeaponMoveSetID)id = defaultMoveSetID;
                return id;
            }
        } 
        
        public virtual void ResetMeleeAnimations()
        {
            if (meleeManager == null || !animator) return; 
            cc.animator.SetBool(vAnimatorParameters.IsBlocking, false);            
        }
        
        public virtual int AttackID
        {
            get
            {
                return meleeManager ? meleeManager.GetAttackID() : 0;
            }
        }

        public virtual int DefenseID
        {
            get
            {
                return meleeManager ? meleeManager.GetDefenseID() : 0;
            }
        }
        #endregion

        #region Melee Methods

        public virtual void OnEnableAttack()
        {
            if (meleeManager == null) meleeManager = GetComponent<vMeleeManager>();
            if (meleeManager == null) return;

            cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
            cc.currentStamina -= meleeManager.GetAttackStaminaCost();
            isAttacking = true;
            cc.isSprinting = false;
        }

        public virtual void OnDisableAttack()
        {
            isAttacking = false;
        }

        public virtual void ResetAttackTriggers()
        {
            cc.animator.ResetTrigger(vAnimatorParameters.WeakAttack);
            cc.animator.ResetTrigger(vAnimatorParameters.StrongAttack);
        }

        public virtual void BreakAttack(int breakAtkID)
        {
            ResetAttackTriggers();
            OnRecoil(breakAtkID);
        }

        public virtual void OnRecoil(int recoilID)
        {
            cc.animator.SetInteger(vAnimatorParameters.RecoilID, recoilID);
            cc.animator.SetTrigger(vAnimatorParameters.TriggerRecoil);
            cc.animator.SetTrigger(vAnimatorParameters.ResetState);
            cc.animator.ResetTrigger(vAnimatorParameters.WeakAttack);
            cc.animator.ResetTrigger(vAnimatorParameters.StrongAttack);
        }

        public virtual void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            // character is blocking
            if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(damage.sender.position))
            {
                var damageReduction = meleeManager.GetDefenseRate();
                if (damageReduction > 0)
                    damage.ReduceDamage(damageReduction);
                if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                    attacker.BreakAttack(meleeManager.GetDefenseRecoilID());
                meleeManager.OnDefense();
                cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
                cc.currentStamina -= damage.staminaBlockCost;
            }
            // apply damage
            damage.hitReaction = !isBlocking || damage.ignoreDefense;
            cc.TakeDamage(damage);
        }

        public virtual vICharacter character
        {
            get { return cc; }
        }

        #endregion

    }

    public static partial class vAnimatorParameters
    {
        public static int AttackID = Animator.StringToHash("AttackID");
        public static int DefenseID = Animator.StringToHash("DefenseID");
        public static int IsBlocking = Animator.StringToHash("IsBlocking");
        public static int MoveSet_ID = Animator.StringToHash("MoveSet_ID");
        public static int RecoilID = Animator.StringToHash("RecoilID");
        public static int TriggerRecoil = Animator.StringToHash("TriggerRecoil");
        public static int WeakAttack = Animator.StringToHash("WeakAttack");
        public static int StrongAttack = Animator.StringToHash("StrongAttack");
    }
}