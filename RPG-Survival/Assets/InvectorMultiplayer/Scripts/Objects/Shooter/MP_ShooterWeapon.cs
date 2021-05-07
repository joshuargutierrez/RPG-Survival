/*
using UnityEngine;
using Photon.Pun;
using Invector.vShooter;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Weapons/MP Shooter Weapon")]
    [RequireComponent(typeof(vShooterWeapon))]
    public class MP_ShooterWeapon : MP_BaseShooterWeapon
    {
        #region Parameters
        protected MP_vShooterManager shooterManager;
        #endregion

        #region Initializations
        /// <summary>
        /// Assign the MP_vShooterManager component that should be in the root of this object
        /// </summary>
        protected override void Start()
        {
            shooterManager = transform.GetComponentInParent<MP_vShooterManager>();
            base.Start();
        }
        #endregion

        #region Send Network Events
        /// <summary>
        /// (Legacy) This simply returns and is no longer used but is kept as to not break 
        /// current unity events that reference this. This logic has 
        /// now been integrated into the MP_ShooterMeleeInput component.
        /// </summary>
        public virtual void SendNetworkShot()
        {
            return;
            //ChildTreeCheck();
            //if (transform.GetComponentInParent<PhotonView>().IsMine == false) return;
            //if (weapon.ammo > 0)
            //{
            //    if (weapon.transform.GetComponent<vBowControl>())
            //    {
            //        view.RPC("SetTriggers", RpcTarget.Others, new string[1] { "Shot" });
            //    }
            //    byte prevGroup = GetComponentInParent<PhotonView>().Group;
            //    GetComponentInParent<PhotonView>().Group = 0;
            //    view.RPC("ShooterWeaponShotWithPosition", RpcTarget.Others, childTree, shooterManager.lastAimPos);
            //    PhotonNetwork.SendAllOutgoingCommands();
            //    GetComponentInParent<PhotonView>().Group = prevGroup;
            //}
        }
        #endregion

    }
}
*/
