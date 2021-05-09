using UnityEngine;
using Photon.Pun;
using Invector;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Objects/Sync Health Controller")]
    [RequireComponent(typeof(PhotonView))]
    [DisallowMultipleComponent]
    public class SyncHealthController : MonoBehaviour
    {
        protected vHealthController hc;
        protected bool waitingResponse = false;

        public virtual void Awake()
        {
            hc = GetComponent<vHealthController>();
        }

        /// <summary>
        /// Used to send the `NetworkOnReceiveDamage` RPC. This has this damage
        /// be received by all networked versions across the network.
        /// </summary>
        /// <param name="damage"></param>
        public virtual void SendDamageOverNetwork(vDamage damage)
        {
            if (waitingResponse)
            {
                waitingResponse = false;
                return;
            }
            GetComponent<PhotonView>().RPC("NetworkOnReceiveDamage", RpcTarget.OthersBuffered, JsonUtility.ToJson(damage));
        }

        [PunRPC]
        protected virtual void NetworkOnReceiveDamage(string Damage)
        {
            vDamage _recievedDamage = JsonUtility.FromJson<vDamage>(Damage);
            waitingResponse = true;
            hc.TakeDamage(_recievedDamage);
        }
    }
}
