using CBGames.Core;
using Invector;
using Invector.vCharacterController;
using Invector.vEventSystems;
using Photon.Pun;
using UnityEngine;

namespace Invector.vCharacterController
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vMeleeCombatInput")]
    public class MP_vMeleeCombatInput : vMeleeCombatInput
    {
        #region Initializations
        private void Awake()
        {
            if (GetComponent<PhotonView>().IsMine == false && PhotonNetwork.IsConnected == true)
            {
                enabled = false;
            }
            cc = (cc == null) ? GetComponent<vThirdPersonController>() : cc;
        }
        protected override void Start()
        {
            base.Start();
            meleeManager = GetComponent<MP_vMeleeManager>();
        }

        public override void OnAnimatorMoveEvent()
        {
            if (cc == null)
            {
                cc = GetComponent<vThirdPersonController>();
            }
            if (cc != null)
            {
                base.OnAnimatorMoveEvent();
            }
        }
        public override bool lockInventory
        {
            get
            {
                if (cc == null)
                {
                    vThirdPersonController yourPlayer = NetworkManager.networkManager.GetYourPlayer();
                    if (yourPlayer)
                    {
                        cc = yourPlayer;
                    }
                    if (cc == null) return true;
                }
                return isAttacking || cc.isDead;
            }
        }
        #endregion

        #region Attacks
        /// <summary>
        /// This makes this action only callable by the owner and networked
        /// players will not react to the input of the owner players.
        /// </summary>
        public override void OnEnableAttack()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            base.OnEnableAttack();
        }

        /// <summary>
        /// This makes this action only callable by the owner and networked
        /// players will not react to the input of the owner players.
        /// </summary>
        public override void OnDisableAttack()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            base.OnDisableAttack();
        }

        /// <summary>
        /// This makes this action only callable by the owner and networked
        /// players will not react to the input of the owner players. Also
        /// Calls the `ResetTriggers` with `WeakAttack` and `StrongAttack`
        /// over the network for the networked players to execute.
        /// </summary>
        public override void ResetAttackTriggers()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            string[] triggers = new string[2] { "WeakAttack", "StrongAttack" };
            GetComponent<PhotonView>().RPC("ResetTriggers", RpcTarget.Others, (object)triggers);

            base.ResetAttackTriggers();
        }

        /// <summary>
        /// This makes this action only callable by the owner and networked
        /// players will not react to the input of the owner players.
        /// </summary>
        public override void BreakAttack(int breakAtkID)
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            base.BreakAttack(breakAtkID);
        }

        /// <summary>
        /// When the owner player recoils it calls `SetTriggers` RPC and 
        /// `ResetTriggers` RPC for all network players to mimic what the
        /// owner player is doing.
        /// </summary>
        /// <param name="recoilID"></param>
        public override void OnRecoil(int recoilID)
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            string[] triggers = new string[2] { "TriggerRecoil", "StrongAttack" };
            GetComponent<PhotonView>().RPC("SetTriggers", RpcTarget.Others, (object)triggers);

            string[] resettriggers = new string[2] { "WeakAttack", "StrongAttack" };
            GetComponent<PhotonView>().RPC("ResetTriggers", RpcTarget.Others, (object)resettriggers);

            base.OnRecoil(recoilID);
        }

        /// <summary>
        /// This makes this action only callable by the owner and networked
        /// players will not react to the input of the owner players.
        /// </summary>
        public override void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            base.OnReceiveAttack(damage, attacker);
        }

        /// <summary>
        /// When the owner makes a weak attack it calls `SetTriggers` RPC 
        /// which has the network players make a weak attack to mimic what 
        /// the owner player is doing.
        /// </summary>
        public override void TriggerWeakAttack()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            string[] triggers = new string[1] { "WeakAttack" };
            GetComponent<PhotonView>().RPC("SetTriggers", RpcTarget.Others, (object)triggers);

            base.TriggerWeakAttack();
        }

        /// <summary>
        /// When the owner makes a weak attack it calls `SetTriggers` RPC 
        /// which has the network players make a strong attack to mimic what 
        /// the owner player is doing.
        /// </summary>
        public override void TriggerStrongAttack()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            string[] triggers = new string[1] { "StrongAttack" };
            GetComponent<PhotonView>().RPC("SetTriggers", RpcTarget.Others, (object)triggers);
            
            base.TriggerStrongAttack();
        }
        #endregion
    }
}
