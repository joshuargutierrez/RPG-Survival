/*
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vSwimming")]
    [RequireComponent(typeof(SyncPlayer))]
    public class MP_vSwimming : vSwimming
    {
        [Header("UnityEvent. Called when you enter the water. This event is called over the network.")]
        public UnityEvent NetworkOnEnterWater;
        [Header("UnityEvent. Called when you exit the water. This event is called over the network.")]
        public UnityEvent NetworkOnExitWater;
        [Header("UnityEvent. Called when you are above the water. This event is called over the network.")]
        public UnityEvent NetworkOnAboveWater;
        [Header("UnityEvent. Called when you are under the water. This event is called over the network.")]
        public UnityEvent NetworkOnUnderWater;
     
        vThirdPersonInput mp_tpInput = null;
        private float mp_timer;
        private float mp_waterRingSpawnFrequency;
        private float mp_waterHeightLevel;
        private bool mp_currentlyInWater = false;
        private bool mp_triggerSwimState;
        private bool mp_triggerUnderWater;
        private bool mp_triggerAboveWater;

        bool currentlyUnderWater
        {
            get
            {
                if (mp_tpInput.cc._capsuleCollider.bounds.max.y >= mp_waterHeightLevel + 0.25f)
                    return false;
                else
                    return true;
            }
        }

        protected override void Start()
        {
            if (GetComponent<PhotonView>().IsMine == false) enabled = false;
            mp_tpInput = GetComponentInParent<vThirdPersonInput>();
            base.Start();
        }

        /// <summary>
        /// Will send the `WaterImpactEffect` over the network via the `NetworkWaterImpactEffect` RPC.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(waterTag) && GetComponent<PhotonView>().IsMine == true)
            {
                mp_currentlyInWater = true;
                mp_waterHeightLevel = other.transform.position.y;
                if (mp_tpInput.cc.verticalVelocity <= velocityToImpact)
                {
                    var newPos = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
                    GetComponent<PhotonView>().RPC(
                        "NetworkWaterImpactEffect", 
                        RpcTarget.Others, 
                        JsonUtility.ToJson(newPos), 
                        JsonUtility.ToJson(mp_tpInput.transform.rotation)
                    );
                }
            }
        }

        /// <summary>
        /// Will send the `WaterDropsEffect` over the network via the `NetworkWaterDropsEffect` RPC.
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(waterTag) && GetComponent<PhotonView>().IsMine == true)
            {
                if (debugMode) Debug.Log("Player left the Water");

                mp_currentlyInWater = false;
                //MP_ExitSwimState();
                if (waterDrops)
                {
                    var newPos = new Vector3(transform.position.x, transform.position.y + waterDropsYOffset, transform.position.z);
                    GetComponent<PhotonView>().RPC(
                        "NetworkWaterDropsEffect",
                        RpcTarget.Others,
                        JsonUtility.ToJson(newPos),
                        JsonUtility.ToJson(mp_tpInput.transform.rotation)
                    );
                }
            }
        }

        /// <summary>
        /// Keeps the same logic as the based invector code but also calls the `MP_UnderWaterBehaviour`
        /// and `MP_SwimmingBehaviour` functions.
        /// </summary>
        protected override void UpdateSwimmingBehavior()
        {
            base.UpdateSwimmingBehavior();
            MP_UnderWaterBehaviour();
            MP_SwimmingBehaviour();
        }

        /// <summary>
        /// Calls the `MP_EnterSwimState` or `MP_ExitSwimState` based on your position in the water.
        /// </summary>
        private void MP_SwimmingBehaviour()
        {
            if (mp_tpInput.cc._capsuleCollider.bounds.center.y + heightOffset < mp_waterHeightLevel)
            {
                if (mp_tpInput.cc.currentHealth > 0)
                {
                    if (!mp_triggerSwimState) MP_EnterSwimState();
                }
                else
                    MP_EnterSwimState();
            }
            else
                MP_ExitSwimState();
        }

        /// <summary>
        /// Calls the `NetworkOnEnterWater` UnityEvent for everyone in the photon room. Also
        /// plays the `Swimming` animation for everyone in the photon room.
        /// </summary>
        private void MP_EnterSwimState()
        {
            mp_triggerSwimState = true;
            GetComponent<PhotonView>().RPC("InvokeOnEnterWater", RpcTarget.Others);
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.Others, "Swimming", 0.25f);
        }

        /// <summary>
        /// Calls the `NetworkOnExitWater` UnityEvent for everyone in the photon room.
        /// </summary>
        private void MP_ExitSwimState()
        {
            if (!mp_triggerSwimState) return;
            mp_triggerSwimState = false;
            GetComponent<PhotonView>().RPC("InvokeOnExitWater", RpcTarget.Others);
        }

        /// <summary>
        /// Calls the `NetworkOnUnderWater` UnityEvent for everyone in the photon room.
        /// </summary>
        void MP_UnderWaterBehaviour()
        {
            if (currentlyUnderWater)
            {
                if (!mp_triggerUnderWater)
                {
                    mp_triggerUnderWater = true;
                    mp_triggerAboveWater = false;
                    GetComponent<PhotonView>().RPC("InvokeOnUnderWater", RpcTarget.Others);
                }
            }
            else
            {
                MP_WaterRingEffect();
                if (!mp_triggerAboveWater && mp_triggerSwimState)
                {
                    mp_triggerUnderWater = false;
                    mp_triggerAboveWater = true;
                    GetComponent<PhotonView>().RPC("InvokeOnAboveWater", RpcTarget.Others);
                }
            }
        }

        /// <summary>
        /// Plays the `WaterRingEffect` for all networked versions in the photon room.
        /// </summary>
        private void MP_WaterRingEffect()
        {
            if (mp_tpInput.cc.input != Vector3.zero) mp_waterRingSpawnFrequency = waterRingFrequencySwim;
            else mp_waterRingSpawnFrequency = waterRingFrequencyIdle;

            mp_timer += Time.deltaTime;
            if (mp_timer >= mp_waterRingSpawnFrequency)
            {
                GetComponent<PhotonView>().RPC(
                    "NetworkWaterRingEffect",
                    RpcTarget.Others,
                    mp_waterHeightLevel,
                    JsonUtility.ToJson(mp_tpInput.transform.rotation)
                );
                mp_timer = 0f;
            }
        }

    }
}
*/
