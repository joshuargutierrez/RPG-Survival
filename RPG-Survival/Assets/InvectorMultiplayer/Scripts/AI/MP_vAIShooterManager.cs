/*
using CBGames.Core;
using CBGames.Objects;
using Photon.Pun;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    [AddComponentMenu("CB GAMES/AI/MP Components/MP vAIShooterManager")]
    public class MP_vAIShooterManager : vAIShooterManager
    {
        [HideInInspector] public Vector3 lastAimPos; //Variable is referenced by weapons to know where to fire its projectile
        protected Animator _animator;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        #region Set Weapon
        /// <summary>
        /// Sets the left weapon in the vAIShooterManager component but also 
        /// tells the other networked versions to do the same and with what 
        /// weapon.
        /// </summary>
        /// <param name="weapon">GameObject type, the weapon to set</param>
        public new void SetLeftWeapon(GameObject weapon)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                int[] childTree = StaticMethods.BuildChildTree(transform, weapon.transform);
                GetComponent<PhotonView>().RPC("vAIShooterManager_SetLeftWeapon", RpcTarget.Others, (object)childTree);
            }
            base.SetLeftWeapon(weapon);
        }
        /// <summary>
        /// Sets the right weapon in the vAIShooterManager component but also 
        /// tells the other networked versions to do the same and with what 
        /// weapon.
        /// </summary>
        /// <param name="weapon">GameObject type, the weapon to set</param>
        public new void SetRightWeapon(GameObject weapon)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                int[] childTree = StaticMethods.BuildChildTree(transform, weapon.transform);
                GetComponent<PhotonView>().RPC("vAIShooterManager_SetRightWeapon", RpcTarget.Others, (object)childTree);
            }
            base.SetRightWeapon(weapon);
        }
        #endregion

        #region IK Placements
        /// <summary>
        /// Load the IK adjust catagory into vAIShooterManager but also tells
        /// the other networked versions to do the same if you're the owner.
        /// </summary>
        /// <param name="category">string type, the ik catagory to set</param>
        public override void LoadIKAdjust(string category)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                GetComponent<PhotonView>().RPC("vAIShooterManager_LoadIKAdjust", RpcTarget.Others, category);
            }
            base.LoadIKAdjust(category);
        }
        #endregion

        #region Actions
        /// <summary>
        /// Shoot your equipped weapon at the target point. If owner will set the
        /// aim position that is referenced by the MP_ShooterWeapon component. Which
        /// triggers firing the networked versions weapon at that point.
        /// </summary>
        /// <param name="aimPosition">Vector3 type, the point to fire a shot at</param>
        public override void Shoot(Vector3 aimPosition)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                lastAimPos = aimPosition;
            }
            base.Shoot(aimPosition);
        }

        /// <summary>
        /// Reloads the weapon, if owner will tell the other networked versions
        /// to play the reload animations.
        /// </summary>
        public override void ReloadWeapon()
        {
            if (GetComponent<PhotonView>().IsMine == false) return;
            var weapon = CurrentWeapon;

            if (!weapon || !weapon.gameObject.activeSelf) return;

            //bool primaryWeaponAnim = false;
            if (!((weapon.ammoCount >= weapon.clipSize)) && !weapon.dontUseReload)
            {
                if (_animator)
                {
                    _animator.SetInteger("ReloadID", GetReloadID());
                    _animator.SetTrigger("Reload");
                    GetComponent<PhotonView>().RPC("AI_SetTrigger", RpcTarget.Others, "Reload");
                }
                //primaryWeaponAnim = true;
            }
            //if (weapon.secundaryWeapon && !((weapon.secundaryWeapon.ammoCount >= weapon.secundaryWeapon.clipSize)) && !weapon.secundaryWeapon.dontUseReload)
            //{
            //    if (!primaryWeaponAnim)
            //    {
            //        if (_animator)
            //        {
            //            _animator.SetInteger("ReloadID", weapon.secundaryWeapon.reloadID);
            //            _animator.SetTrigger("Reload");
            //            GetComponent<PhotonView>().RPC("AI_SetTrigger", RpcTarget.Others, "Reload");
            //        }
            //    }
            //}
            base.ReloadWeapon();
        }
        #endregion

        #region RPCs

        #region Set Weapon
        [PunRPC]
        void vAIShooterManager_SetLeftWeapon(int[] treeToWeapon)
        {
            Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
            if (weapon)
            {
                SetLeftWeapon(weapon.gameObject);
            }
        }
        [PunRPC]
        void vAIShooterManager_SetRightWeapon(int[] treeToWeapon)
        {
            Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
            if (weapon)
            {
                SetRightWeapon(weapon.gameObject);
            }
        }
        #endregion

        #region IK
        [PunRPC]
        void vAIShooterManager_LoadIKAdjust(string catagory)
        {
            LoadIKAdjust(catagory);
        }
        #endregion

        #region Weapon Actions
        [PunRPC]
        void vAIShooterManager_Shoot(int[] treeToWeapon, Vector3 aimPos)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkShot(aimPos);
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponEmptyClip(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkEmptyClip();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnFinishReload(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnFinishReload();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnFullPower(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnFullPower();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnFinishAmmo(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnFinishAmmo();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponReload(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkReload();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnEnableAim(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnEnableAim();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnDisableAim(int[] treeToWeapon)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnDisableAim();
        }
        [PunRPC]
        void vAIShooterManager_ShooterWeaponOnChangerPowerCharger(int[] treeToWeapon, float amount)
        {
            Transform weaponTransform = StaticMethods.FindTargetChild(treeToWeapon, transform);
            weaponTransform.gameObject.GetComponent<MP_BaseShooterWeapon>().RecieveNetworkOnChangerPowerCharger(amount);
        }
        #endregion
        #endregion
    }
}
*/
