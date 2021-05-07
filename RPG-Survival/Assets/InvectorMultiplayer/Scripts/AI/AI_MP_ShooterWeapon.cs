/*
using CBGames.Objects;
using Invector.vCharacterController.AI;
using Invector.vShooter;
using Photon.Pun;
using UnityEngine;

namespace CBGames.AI
{
    [AddComponentMenu("CB GAMES/AI/MP Components/MP Shooter Weapon")]
    public class AI_MP_ShooterWeapon : MP_BaseShooterWeapon
    {
        protected MP_vAIShooterManager ai_shooterManager = null;

        /// <summary>
        /// Find the parent MP_vAIShooterManager component. Also makes sure
        /// this weapon has a valid vShooterWeapon component. Finally, makes 
        /// sure the root transform has a photon view.
        /// </summary>
        protected override void Start()
        {
            ai_shooterManager = transform.GetComponentInParent<MP_vAIShooterManager>();
            base.Start();
        }

        #region Sends
        /// <summary>
        /// Send the 'Shoot' trigger over the network and tells the other network
        /// versions of this object to play their weapon fire function.
        /// </summary>
        public virtual void SendNetworkShot()
        {
            if (transform.GetComponentInParent<PhotonView>().IsMine == false) return;
            if (weapon.ammo > 0)
            {
                if (weapon.transform.GetComponent<vBowControl>())
                {
                    view.RPC("SetTriggers", RpcTarget.Others, new string[1] { "Shoot" });
                }

                view.RPC("vAIShooterManager_Shoot", RpcTarget.Others, (object)childTree, ai_shooterManager.lastAimPos);
            }
        }
        #endregion
    }
}
*/
