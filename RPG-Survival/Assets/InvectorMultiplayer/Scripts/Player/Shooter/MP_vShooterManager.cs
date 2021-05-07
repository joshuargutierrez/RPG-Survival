using CBGames.Core;
using Invector.vItemManager;
using Photon.Pun;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Invector.vShooter
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vShooterManager")]
    public class MP_vShooterManager : vShooterManager, IPunObservable
    {
        #region Properties
        protected BindingFlags allBindings;
        protected bool mp_cancelReload = false;
        protected bool mp_isReloading = false;
        protected bool mp_isReloadingWeapon = false;
        protected bool _isLeftWeapon, _isShooting;
        protected bool _initialized = false;
        protected PhotonView pv = null;
        [HideInInspector] public Vector3 lastAimPos;
        [HideInInspector] public bool firedShot = false;
        #endregion

        #region Initializations
        public override void Start()
        {
            pv = GetComponent<PhotonView>();
            allBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            useAmmoDisplay = (pv.IsMine) ? useAmmoDisplay : false;
            if (!pv.IsMine)
            {
                onEquipWeapon.AddListener(NoneOwnerOnEnableWeapon);
            }
            base.Start();
            StartCoroutine(WaitToInitialize());
        }
        IEnumerator WaitToInitialize()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            _initialized = true;
        }
        public virtual void NoneOwnerOnEnableWeapon(GameObject obj, bool value)
        {
            if (obj.GetComponentInChildren<Camera>())
            {
                obj.GetComponentInChildren<Camera>().enabled = false;
            }
        }
        #endregion

        #region Network Sync Animations
        /// <summary>
        /// When the owner player cancels the reload animations it calls
        /// `ResetTriggers` RPC which has the network players mimic the 
        /// owner player by canceling their reload animations.
        /// </summary>
        IEnumerator NetworkCancelReloadRoutine()
        {
            if (CurrentWeapon != null)
            {
                string[] triggers = new string[1] { "Reload" };
                pv.RPC("ResetTriggers", RpcTarget.Others, (object)triggers);
                mp_cancelReload = true;
                yield return new WaitForSeconds(CurrentWeapon.reloadTime + 0.1f);
                mp_cancelReload = false;
                if (mp_isReloadingWeapon)
                {
                    mp_isReloadingWeapon = false;
                }
            }
        }

        protected override IEnumerator CancelReloadRoutine()
        {
            StartCoroutine(NetworkCancelReloadRoutine());
            StartCoroutine(base.CancelReloadRoutine());
            yield return null;
        }

        /// <summary>
        /// When the owner player triggers the reload animation it calls
        /// `SetTriggers` RPC which makes the network players play the 
        /// reload animation to mimic the owner player.
        /// </summary>
        IEnumerator NetworkReloadWeapon()
        {
            var weapon = rWeapon ? rWeapon : lWeapon;
            if (!weapon || !weapon.gameObject.activeInHierarchy || mp_isReloading) yield return null;
            //bool mp_primaryWeaponAnim = false;
            if (weapon.ammoCount < weapon.clipSize && (weapon.isInfinityAmmo || WeaponHasUnloadedAmmo()) && !weapon.dontUseReload)
            {
                if (GetComponent<Animator>())
                {
                    pv.RPC("SetTriggers", RpcTarget.Others, new int[1] { Reload });
                }
                //mp_primaryWeaponAnim = true;
            }
            //if (weapon.secundaryWeapon && weapon.secundaryWeapon.ammoCount >= weapon.secundaryWeapon.clipSize && (weapon.secundaryWeapon.isInfinityAmmo || WeaponHasUnloadedAmmo(true)) && !weapon.secundaryWeapon.dontUseReload)
            //{
            //    if (!mp_primaryWeaponAnim)
            //    {
            //        if (GetComponent<Animator>())
            //        {
            //            mp_primaryWeaponAnim = true;
            //            pv.RPC("SetTriggers", RpcTarget.Others, new int[1] { Reload });
            //        }
            //    }
            //}
            yield return null;
        }

        /// <summary>
        /// Calls the `NetworkReloadWeapon` IEnumerator
        /// </summary>
        public override void ReloadWeapon()
        {
            StartCoroutine(NetworkReloadWeapon());
            base.ReloadWeapon();
        }

        /// <summary>
        /// Calls the `NetworkAddAmmoToWeapon` IEnumerator on top of all its normal functions.
        /// </summary>
        /// <param name="weapon">Read the invector docs.</param>
        /// <param name="delayTime">Read the invector docs.</param>
        /// <param name="ignoreEffects">Read the invector docs.</param>
        /// <returns></returns>
        protected override IEnumerator AddAmmoToWeapon(vShooterWeapon weapon, float delayTime, bool ignoreEffects = false)
        {
            StartCoroutine(NetworkAddAmmoToWeapon(weapon, delayTime, ignoreEffects));
            StartCoroutine(base.AddAmmoToWeapon(weapon, delayTime, ignoreEffects));
            yield return null;
        }

        /// <summary>
        /// When the owner player adds ammo to the weapon it triggers the network players
        /// to play the Reload animations via the `SetTriggers` RPC.
        /// </summary>
        /// <param name="weapon">Takes same inputs as `AddAmmoToWeapon` function</param>
        /// <param name="delayTime">Takes same inputs as `AddAmmoToWeapon` function</param>
        /// <param name="ignoreEffects">Takes same inputs as `AddAmmoToWeapon` function</param>
        /// <returns></returns>
        IEnumerator NetworkAddAmmoToWeapon(vShooterWeapon weapon, float delayTime, bool ignoreEffects = false)
        {
            mp_isReloading = true;
            if (weapon.ammoCount < weapon.clipSize && (weapon.isInfinityAmmo || WeaponHasUnloadedAmmo()) && !weapon.dontUseReload && !mp_cancelReload)
            {
                yield return new WaitForSeconds(delayTime);
                if (!mp_cancelReload)
                {
                    if (weapon.reloadOneByOne && weapon.ammoCount < weapon.clipSize && WeaponHasUnloadedAmmo())
                    {
                        if (WeaponAmmo(weapon).count == 0)
                        {
                            if (!ignoreEffects)
                            {
                                mp_isReloadingWeapon = false;
                            }
                        }
                        else
                        {
                            if (!ignoreEffects) mp_isReloadingWeapon = true;
                            if (!mp_cancelReload)
                            {
                                if (!ignoreEffects)
                                {
                                    pv.RPC("SetTriggers", RpcTarget.Others, new int[1] { Reload });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!ignoreEffects)
                        {
                            mp_isReloadingWeapon = false;
                        }
                    }
                }
            }
            mp_isReloading = false;
        }

        /// <summary>
        /// Calls the `NetworkRecoil` function on top of all it's normal functionality.
        /// </summary>
        /// <param name="horizontal">Read the invector docs.</param>
        /// <param name="up">Read the invector docs.</param>
        protected override IEnumerator Recoil(float horizontal, float up)
        {
            StartCoroutine(NetworkRecoil(horizontal, up));
            StartCoroutine(base.Recoil(horizontal, up));
            yield return null;
        }

        /// <summary>
        /// The owner sends the `SetTriggers` RPC for the network players to play the Shooting 
        /// recoil animation.
        /// </summary>
        /// <param name="horizontal">Read the invector docs.</param>
        /// <param name="up">Read the invector docs.</param>
        IEnumerator NetworkRecoil(float horizontal, float up)
        {
            if (GetComponent<Animator>())
            {
                yield return new WaitForSeconds(0.02f);
                pv.RPC("SetTriggers", RpcTarget.Others, new int[1] { IsShoot });
            }
            yield return null;
        }
        #endregion

        #region Shot Position
        /// <summary>
        /// Sets the `lastAimPos` parameter. This parameter is referenced by the `MP_ShooterWeapon`
        /// class. So when the Shot function on that component is called it knows where to fire
        /// based on the Vector3 position stored in the `lastAimPos` parameter in this class.
        /// </summary>
        /// <param name="aimPosition">Read the invector docs.</param>
        /// <param name="applyHipfirePrecision">Read the invector docs.</param>
        /// <param name="useSecundaryWeapon">Read the invector docs.</param>
        public override void Shoot(Vector3 aimPosition, bool applyHipfirePrecision = false)
        {
            lastAimPos = applyHipfirePrecision ? aimPosition + HipFirePrecision(aimPosition) : aimPosition;
            firedShot = true;
            base.Shoot(lastAimPos, false);
        }
        #endregion

        #region IK
        /// <summary>
        /// When the owner changes its IK adjust catagory it will call the `LoadIKAdjustCatagory`
        /// RPC which has the networked players set their catagory to the same thing as the owner.
        /// </summary>
        /// <param name="category"></param>
        public override void LoadIKAdjust(string category)
        {
            if (pv.IsMine == true)
            {
                pv.RPC("LoadIKAdjustCatagory", RpcTarget.Others, category);
            }
            base.LoadIKAdjust(category);
        }
        #endregion

        #region Weapon Placements
        /// <summary>
        /// When the owner changes their left weapon it calls the `vShooterManager_ReceiveSetLeftWeapon`
        /// RPC which has the network players set their left weapon to the same thing that the owner
        /// player has.
        /// </summary>
        /// <param name="weapon">GameObject type, the weapon to set as the left weapon</param>
        public override void SetLeftWeapon(GameObject weapon)
        {
            bool orgValue = usingThirdPersonController;
            if (pv.IsMine == false)
            {
                usingThirdPersonController = false;
            }
            base.SetLeftWeapon(weapon);
            if (pv.IsMine == true)
            {
                pv.RPC(
                    "vShooterManager_ReceiveSetLeftWeapon",
                    RpcTarget.Others,
                    (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
                );
            }
            else
            {
                usingThirdPersonController = orgValue;
            }
        }

        /// <summary>
        /// When the owner changes their right weapon it calls the `vShooterManager_ReceiveSetRightWeapon`
        /// RPC which has the network players set their right weapon to the same thing that the owner
        /// player has.
        /// </summary>
        /// <param name="weapon">GameObject type, the weapon to set as the right weapon</param>
        public override void SetRightWeapon(GameObject weapon)
        {
            bool orgValue = usingThirdPersonController;
            if (pv.IsMine == false)
            {
                usingThirdPersonController = false;
            }
            base.SetRightWeapon(weapon);
            if (pv.IsMine == true)
            {
                pv.RPC(
                    "vShooterManager_ReceiveSetRightWeapon",
                    RpcTarget.Others,
                    (weapon) ? StaticMethods.BuildChildTree(transform, weapon.transform, false) : new int[] { }
                );
            }
            else
            {
                usingThirdPersonController = orgValue;
            }
        }
        #endregion

        #region Override Checks
        /// <summary>
        /// This overrideds the default functionality of the invector logic to only work if
        /// you're the owner player and will not work if you're a networked player
        /// </summary>
        public override bool IsLeftWeapon
        {
            get
            {
                return (pv.IsMine == true) ? base.IsLeftWeapon : _isLeftWeapon;
            }
        }

        /// <summary>
        /// This overrideds the default functionality of the invector logic to only work if
        /// you're the owner player and will not work if you're a networked player
        /// </summary>
        public override bool isShooting
        {
            get
            {
                return (pv.IsMine == true) ? base.isShooting : _isShooting;
            }
        }
        #endregion

        #region Ammo UI
        protected override void UpdateAmmoDisplay(int displayId)
        {
            if (pv.IsMine)
            {
                base.UpdateAmmoDisplay(displayId);
            }
        }
        protected override void HideAmmoDisplay(vAmmoDisplay ammoDisplay)
        {
            if (pv.IsMine)
            {
                base.HideAmmoDisplay(ammoDisplay);
            }
        }
        protected override void HideLeftAmmoDisplay()
        {
            if (pv.IsMine)
            {
                base.HideLeftAmmoDisplay();
            }
        }
        protected override void HideRightAmmoDisplay()
        {
            if (pv.IsMine)
            {
                base.HideRightAmmoDisplay();
            }
        }
        #endregion

        #region RPCs
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
        {
            if (_initialized == false) return;
            if (stream.IsWriting)
            {
                stream.SendNext(IsLeftWeapon);
                stream.SendNext(isShooting);
                stream.SendNext(smoothArmIKRotation);
            }
            else if (stream.IsReading)
            {
                try
                {
                    _isLeftWeapon = (bool)stream.ReceiveNext();
                    _isShooting = (bool)stream.ReceiveNext();
                    smoothArmIKRotation = (float)stream.ReceiveNext();
                }
                catch { }
            }
        }

        [PunRPC]
        void LoadIKAdjustCatagory(string catagory)
        {
            LoadIKAdjust(catagory);
        }
        [PunRPC]
        void vShooterManager_ReceiveSetLeftWeapon(int[] treeToWeapon)
        {
            if (treeToWeapon.Length > 0)
            {
                Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
                SetLeftWeapon(weapon.gameObject);
            }
            else
            {
                SetLeftWeapon(null);
            }
        }
        [PunRPC]
        void vShooterManager_ReceiveSetRightWeapon(int[] treeToWeapon)
        {
            if (treeToWeapon.Length > 0)
            {
                Transform weapon = StaticMethods.FindTargetChild(treeToWeapon, transform);
                SetRightWeapon(weapon.gameObject);
            }
            else
            {
                SetRightWeapon(null);
            }
        }
        #endregion
    }
}
