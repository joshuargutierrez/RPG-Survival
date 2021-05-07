using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
    [vClassHeader("Shooter Weapon", openClose = false)]
    public class vShooterWeaponBase : vMonoBehaviour
    {
        #region Variables

        [vEditorToolbar("Weapon Settings")]
        [Tooltip("The category of the weapon\n Used to the IK offset system. \nExample: HandGun, Pistol, Machine-Gun")]
        public string weaponCategory = "MyCategory";
        [Tooltip("Frequency of shots")]
        public float shootFrequency;

        [vEditorToolbar("Ammo")]
        public bool isInfinityAmmo;
        [Tooltip("Starting ammo")]
        [SerializeField, vHideInInspector("isInfinityAmmo", true)]
        public int ammo;

        [vEditorToolbar("Layer & Tag")]
        public List<string> ignoreTags = new List<string>();
        public LayerMask hitLayer = 1 << 0;

        [vEditorToolbar("Projectile")]
        [Tooltip("Prefab of the projectile")]
        public GameObject projectile;
        [Tooltip("Assign the muzzle of your weapon")]
        public Transform muzzle;
        [Tooltip("How many projectiles will spawn per shot")]
        [Range(1, 20)]
        public int projectilesPerShot = 1;
        [Range(0, 90)]
        [Tooltip("how much dispersion the weapon have")]
        public float dispersion = 0;
        [Range(0, 1000)]
        [Tooltip("Velocity of your projectile")]
        public float velocity = 380;
        [Tooltip("Use the DropOffStart and DropOffEnd to calc damage by distance using min and max damage")]
        public bool damageByDistance = true;
        [Tooltip("Min distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
        [UnityEngine.Serialization.FormerlySerializedAs("DropOffStart")]
        public float minDamageDistance = 8f;
        [Tooltip("Max distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
        [UnityEngine.Serialization.FormerlySerializedAs("DropOffEnd")]
        public float maxDamageDistance = 50f;
        [Tooltip("Minimum damage caused by the shot, regardless the distance")]
        public int minDamage;
        [Tooltip("Maximum damage caused by the close shot")]
        public int maxDamage;

        [vEditorToolbar("Audio & VFX")]
        [Header("Audio")]
        public AudioSource source;
        public AudioClip fireClip;
        public AudioClip emptyClip;

        [Header("Effects")]
        public bool testShootEffect;
        public Light lightOnShot;
        [SerializeField]
        public ParticleSystem[] emittShurykenParticle;
        protected Transform sender;

        [HideInInspector]
        public OnDestroyEvent onDestroy;
        [System.Serializable]
        public class OnDestroyEvent : UnityEvent<GameObject> { }
        [System.Serializable]
        public class OnInstantiateProjectile : UnityEvent<vProjectileControl> { }

        [vEditorToolbar("Events")]
        public UnityEvent onShot, onEmptyClip;

        public OnInstantiateProjectile onInstantiateProjectile;

        protected float _nextShootTime;
        protected float _nextEmptyClipTime;
        #endregion

        #region Public Methods
        /// <summary>
        /// Apply additional velocity to the Shot projectile 
        /// </summary>
        public virtual float velocityMultiplierMod { get; set; }

        /// <summary>
        /// Apply additional damage to the projectile
        /// </summary>
        public virtual float damageMultiplierMod { get; set; }

        /// <summary>
        /// Weapon Name
        /// </summary>
        public virtual string weaponName
        {
            get
            {
                var value = gameObject.name.Replace("(Clone)", string.Empty);
                return value;
            }
        }

        /// <summary>
        /// Shoot to direction of the muzzle forward
        /// </summary>
        public virtual void Shoot()
        {
            Shoot(muzzle.position + muzzle.forward * 100f);
        }

        /// <summary>
        /// Shoot to direction of the muzzle forward
        /// </summary>
        /// <param name="sender">Sender to reference of the damage</param>
        /// <param name="successfulShot">Action to check if shoot is sucessful</param>
        public virtual void Shoot(Transform _sender = null, UnityAction<bool> successfulShot = null)
        {
            Shoot(muzzle.position + muzzle.forward * 100f, _sender, successfulShot);
        }

        /// <summary>
        /// Shoot to direction of the aim Position
        /// </summary>
        /// <param name="aimPosition">Aim position to override direction of the projectile</param>
        /// <param name="sender">ender to reference of the damage</param>
        /// <param name="successfulShot">Action to check if shoot is sucessful</param>
        public virtual void Shoot(Vector3 aimPosition, Transform _sender = null, UnityAction<bool> successfulShot = null)
        {          
            if (HasAmmo())
            {
                if (!CanDoShot) return;
               
                UseAmmo();
                this.sender = _sender != null ? _sender : transform;
                HandleShot(aimPosition);
                if (successfulShot != null) successfulShot.Invoke(true);
                _nextShootTime = Time.time + shootFrequency;
                _nextEmptyClipTime = _nextShootTime;
            }
            else
            {
                if (!CanDoEmptyClip) return;
              
                EmptyClipEffect();
                if (successfulShot != null) successfulShot.Invoke(false);
                  _nextEmptyClipTime = Time.time + shootFrequency;
            }
        }

        /// <summary>
        /// Check if can shoot by <seealso cref="shootFrequency"/>
        /// </summary>
        public virtual bool CanDoShot
        {
            get
            {
                bool _canShot = _nextShootTime < Time.time;
                return _canShot;
            }
        }     

        protected virtual bool CanDoEmptyClip
        {
            get
            {
                bool _canShot = _nextEmptyClipTime < Time.time;
                return _canShot;
            }
        }

        /// <summary>
        /// Use weapon Ammo
        /// </summary>
        /// <param name="count">count to use</param>
        public virtual void UseAmmo(int count = 1)
        {
            if (ammo <= 0) return;
            ammo -= count;
            if (ammo <= 0) ammo = 0;
        }

        /// <summary>
        /// Check if Weapon Has Ammo
        /// </summary>
        /// <returns></returns>
        public virtual bool HasAmmo()
        {
            
            return isInfinityAmmo || ammo > 0;
        }
        #endregion

        #region Protected Methods

        protected virtual void OnDestroy()
        {
            onDestroy.Invoke(gameObject);
        }

        private void OnApplicationQuit()
        {
            onDestroy.RemoveAllListeners();
        }
        protected virtual void HandleShot(Vector3 aimPosition)
        {
            ShootBullet(aimPosition);
            ShotEffect();
        }

        protected virtual Vector3 Dispersion(Vector3 aim, float distance, float variance)
        {
            aim.Normalize();
            Vector3 v3 = Vector3.zero;
            do
            {
                v3 = Random.insideUnitSphere;
            }
            while (v3 == aim || v3 == -aim);
            v3 = Vector3.Cross(aim, v3);
            v3 = v3 * Random.Range(0f, variance);
            return aim * distance + v3;
        }

        protected virtual void ShootBullet(Vector3 aimPosition)
        {            
            var dir = aimPosition - muzzle.position;

            var rotation = Quaternion.LookRotation(dir);
            GameObject bulletObject = null;
            var velocityChanged = 0f;
            if (dispersion > 0 && projectile)
            {
                for (int i = 0; i < projectilesPerShot; i++)
                {
                    var dispersionDir = Dispersion(dir.normalized, maxDamageDistance, dispersion);
                    var spreadRotation = Quaternion.LookRotation(dispersionDir);
                    bulletObject = Instantiate(projectile, muzzle.transform.position, spreadRotation) as GameObject;

                    var pCtrl = bulletObject.GetComponent<vProjectileControl>();                    

                    pCtrl.shooterTransform = sender;
                    pCtrl.ignoreTags = ignoreTags;                    
                    pCtrl.hitLayer = hitLayer;                    
                    pCtrl.damage.sender = sender;
                    pCtrl.startPosition = bulletObject.transform.position;
                    pCtrl.damageByDistance = damageByDistance;
                    pCtrl.maxDamage = (int)((maxDamage / projectilesPerShot) * damageMultiplier);
                    pCtrl.minDamage = (int)((minDamage / projectilesPerShot) * damageMultiplier);
                    pCtrl.minDamageDistance = minDamageDistance;
                    pCtrl.maxDamageDistance = maxDamageDistance;
                    onInstantiateProjectile.Invoke(pCtrl);
                    velocityChanged = velocity * velocityMultiplier;
                    StartCoroutine(ApplyForceToBullet(bulletObject, dispersionDir, velocityChanged));

                    pCtrl = CreateProjectileData(aimPosition, velocityChanged, dispersionDir, pCtrl);
                }
            }
            else if (projectilesPerShot > 0 && projectile)
            {
                bulletObject = Instantiate(projectile, muzzle.transform.position, rotation) as GameObject;
                var pCtrl = bulletObject.GetComponent<vProjectileControl>();
                pCtrl.shooterTransform = sender;
                pCtrl.ignoreTags = ignoreTags;
                pCtrl.hitLayer = hitLayer;
                pCtrl.damage.sender = sender;
                pCtrl.startPosition = bulletObject.transform.position;
                pCtrl.damageByDistance = damageByDistance;
                pCtrl.maxDamage = (int)((maxDamage / projectilesPerShot) * damageMultiplier);
                pCtrl.minDamage = (int)((minDamage / projectilesPerShot) * damageMultiplier);
                pCtrl.minDamageDistance = minDamageDistance;
                pCtrl.maxDamageDistance = maxDamageDistance;
                onInstantiateProjectile.Invoke(pCtrl);
                velocityChanged = velocity * velocityMultiplier;

                StartCoroutine(ApplyForceToBullet(bulletObject, dir, velocityChanged));
            }
        }

        protected virtual vProjectileControl CreateProjectileData(Vector3 aimPosition, float velocityChanged, Vector3 dispersionDir, vProjectileControl pCtrl)
        {
            pCtrl.instantiateData = new vProjectileInstantiateData
            {
                aimPos = aimPosition,
                dir = dispersionDir,
                vel = velocityChanged
            };
            return pCtrl;
        }

        protected virtual IEnumerator ApplyForceToBullet(GameObject bulletObject, Vector3 direction, float velocityChanged)
        {
            yield return new WaitForSeconds(0.01f);
            try
            {
                var _rigidbody = bulletObject.GetComponent<Rigidbody>();
                _rigidbody.mass = _rigidbody.mass / projectilesPerShot;//Change mass per projectiles count.

                _rigidbody.AddForce((direction.normalized * velocityChanged), ForceMode.VelocityChange);
            }
            catch
            {

            }
        }

        protected virtual float damageMultiplier
        {
            get
            {
                return 1 + damageMultiplierMod;
            }
        }

        protected virtual float velocityMultiplier
        {
            get
            {
                return 1 + velocityMultiplierMod;
            }
        }

        #region Effects
        protected virtual void ShotEffect()
        {
            onShot.Invoke();

            StopCoroutine(LightOnShoot());
            if (source)
            {
                source.Stop();
                source.PlayOneShot(fireClip);
            }

            StartCoroutine(LightOnShoot(0.037f));
            StartEmitters();
        }

        protected virtual void StopSound()
        {
            if (source)
                source.Stop();
        }

        protected virtual IEnumerator LightOnShoot(float time = 0)
        {
            if (lightOnShot)
            {
                lightOnShot.enabled = true;

                yield return new WaitForSeconds(time);
                lightOnShot.enabled = false;
            }
        }

        protected virtual void StartEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                    pe.Emit(1);
            }
        }

        protected virtual void StopEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                    pe.Stop();
            }
        }

        protected virtual void EmptyClipEffect()
        {
            if (source)
            {
                source.Stop();
                source.PlayOneShot(emptyClip);
            }

            onEmptyClip.Invoke();
        }
        #endregion
        #endregion
    }
}