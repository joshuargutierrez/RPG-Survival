using UnityEngine;
using Photon.Pun;
using Invector.vShooter;
using CBGames.Core;
using CBGames.Player;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Weapons/MP Shooter Weapon")]
    [RequireComponent(typeof(vShooterWeapon))]
    public class MP_BaseShooterWeapon : MonoBehaviour
    {
        #region Parameters
        protected PhotonView view;
        protected vShooterWeapon weapon;
        public int[] childTree = null;
        #endregion

        #region Initializations
        /// <summary>
        /// Makes sure there is a valid photon view at the root of this object where the 
        /// vThirdPersonController is
        /// </summary>
        protected virtual void Awake()
        {
            ViewCheck();
        }

        /// <summary>
        /// Makes sure there is an index tree to the PhotonView for this weapon and it
        /// has a valid vShooterWeapon component attached.
        /// </summary>
        protected virtual void Start()
        {
            WeaponCheck();
            ChildTreeCheck();
        }

        /// <summary>
        /// Sets the target PhotonView to the one that is on the root of this object
        /// tree where the vThirdPersonController is.
        /// </summary>
        protected virtual void ViewCheck()
        {
            if (!view)
            {
                if (GetComponent<PhotonView>())
                {
                    view = GetComponent<PhotonView>();
                }
                else if (transform.GetComponentInParent<PhotonView>())
                {
                    view = transform.GetComponentInParent<PhotonView>();
                }
            }
        }

        /// <summary>
        /// Builds a child tree index to the targeted photon view if one is not already
        /// made.
        /// </summary>
        protected virtual void ChildTreeCheck()
        {
            if (childTree == null || childTree.Length == 0)
            {
                childTree = StaticMethods.BuildChildTree(transform.root, transform, false);
            }
        }

        /// <summary>
        /// Make sure there is a vShooterWeapon component targeted on this weapon.
        /// </summary>
        protected virtual void WeaponCheck()
        {
            if (!weapon)
            {
                weapon = GetComponent<vShooterWeapon>();
            }
        }
        #endregion

        #region Bow
        /// <summary>
        /// This is called from the OnInstantiateProjectile UnityEvent to set the ArrowView
        /// components viewId on the instantiated arrow.
        /// </summary>
        /// <param name="control">vProjectileControl type, just to make this work with the UnityEvent, not used otherwise.</param>
        public virtual void SetArrowView(vProjectileControl control)
        {
            control.GetComponentInChildren<ArrowView>().viewId = transform.GetComponentInParent<SyncPlayer>().GetArrowId();
            control.GetComponentInChildren<ArrowView>().owner = transform.GetComponentInParent<SyncPlayer>().transform;
        }
        #endregion

        #region Send Network Events
        /// <summary>
        /// If owner will send an RPC to all networked versions to reload this weapon
        /// </summary>
        public virtual void SendNetworkReload()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponReload", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// (Legacy) This function simply returns. It is now only kept as to not break
        /// current weapons that have unity events referencing this. This logic has 
        /// now been integrated into the MP_ShooterMeleeInput component.
        /// </summary>
        public virtual void SendNetworkEmptyClip()
        {
            //ViewCheck();
            //ChildTreeCheck();
            //if (view.IsMine == false) return;
            //view.RPC("ShooterWeaponEmptyClip", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// If owner will send an RPC to all networked versiosn to play the 
        /// OnFinishedReload function.
        /// </summary>
        public virtual void SendNetworkOnFinishReload()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponOnFinishReload", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// If owner will send an RPC to all networked versions to play the
        /// OnFullPower function.
        /// </summary>
        public virtual void SendNetworkOnFullPower()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponOnFullPower", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// If owner will send an RPC to all networked versions to play the
        /// OnFinishAmmo function.
        /// </summary>
        public virtual void SendNetworkOnFinishAmmo()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            if (weapon.onFinishAmmo.GetPersistentEventCount() > 0)
            {
                view.RPC("ShooterWeaponOnFinishAmmo", RpcTarget.Others, childTree);
            }
        }

        /// <summary>
        /// If owner will send an RPC to all networked versions to play the
        /// OnEnableAim function.
        /// </summary>
        public virtual void SendOnEnableAim()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponOnEnableAim", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// If owner will send an RPC to all networked versions to play the
        /// OnDisableAim function.
        /// </summary>
        public virtual void SendOnDisableAim()
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponOnDisableAim", RpcTarget.Others, childTree);
        }

        /// <summary>
        /// If owner will send an RPC to all networked versions to play the
        /// OnChangerPowerCharger function with the amount of charge.
        /// </summary>
        public virtual void SendOnChangerPowerCharger(float amount)
        {
            ViewCheck();
            ChildTreeCheck();
            if (view.IsMine == false) return;
            view.RPC("ShooterWeaponOnChangerPowerCharger", RpcTarget.Others, childTree, amount);
        }
        #endregion

        #region Recieve Network Events
        /// <summary>
        /// (Legacy) This function now simply returns. This logic has 
        /// now been integrated into the MP_ShooterMeleeInput component.
        /// </summary>
        /// <param name="aimPos">Vector3 type, The position to shoot at.</param>
        public virtual void RecieveNetworkShot(Vector3 aimPos)
        {
            return;
            //ViewCheck();
            //if (view.IsMine == true) return;
            //WeaponCheck();
            //weapon.isInfinityAmmo = true;
            //weapon.AddAmmo(1);
            //weapon.Shoot(aimPos, transform);
        }

        /// <summary>
        /// (Legacy) This function now simply returns. This logic has 
        /// now been integrated into the MP_ShooterMeleeInput component.
        /// </summary>
        public virtual void RecieveNetworkShot()
        {
            return;
            //ViewCheck();
            //if (view.IsMine == true) return;
            //WeaponCheck();
            //weapon.isInfinityAmmo = true;
            //weapon.AddAmmo(1);
            //weapon.Shoot(transform);
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the reload effect.
        /// </summary>
        public virtual void RecieveNetworkReload()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.ReloadEffect();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the empty clip effect.
        /// </summary>
        public virtual void RecieveNetworkEmptyClip()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            if (weapon.source)
            {
                weapon.source.Stop();
                weapon.source.PlayOneShot(weapon.emptyClip);
            }
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on finished reload effect.
        /// </summary>
        public virtual void RecieveNetworkOnFinishReload()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.FinishReloadEffect();
            weapon.onFinishReload.Invoke();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on full power effect.
        /// </summary>
        public virtual void RecieveNetworkOnFullPower()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.onFullPower.Invoke();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on finished ammo effect.
        /// </summary>
        public virtual void RecieveNetworkOnFinishAmmo()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.onFinishAmmo.Invoke();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on enable aim effect.
        /// </summary>
        public virtual void RecieveNetworkOnEnableAim()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.onEnableAim.Invoke();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on disable aim effect.
        /// </summary>
        public virtual void RecieveNetworkOnDisableAim()
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.onDisableAim.Invoke();
        }

        /// <summary>
        /// This is called by the RPC when the networked versions receives a
        /// request from the owner for them to play the on change power charger effect.
        /// </summary>
        public virtual void RecieveNetworkOnChangerPowerCharger(float amount)
        {
            ViewCheck();
            if (view.IsMine == true) return;
            WeaponCheck();
            weapon.onPowerChargerChanged.Invoke(amount);
        }
        #endregion

    }
}
