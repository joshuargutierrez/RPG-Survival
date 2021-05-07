/*
using Invector;
using Photon.Pun;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Objects/MP Simple Trigger")]
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(vSimpleTrigger))]
    public class MP_vSimpleTrigger : MonoBehaviour
    {
        private vSimpleTrigger trigger;
        private bool networkEnabled = false;

        private void Awake()
        {
            trigger = GetComponent<vSimpleTrigger>();
            networkEnabled = trigger.onTriggerEnter.GetPersistentEventCount() > 0 || trigger.onTriggerExit.GetPersistentEventCount() > 0;
        }

        /// <summary>
        /// On top of doing the normal actions when entering the trigger it will also trigger these actions
        /// on all networked versions of the charcter entering it
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (networkEnabled == false) return;
            bool inLayer = trigger.layersToDetect == (trigger.layersToDetect | (1 << other.gameObject.layer));
            if (trigger.tagsToDetect.Contains(other.tag) || inLayer)
            {
                if (other.GetComponent<PhotonView>())
                {
                    GetComponent<PhotonView>().RPC("SyncTriggerEnter", RpcTarget.Others, other.GetComponent<PhotonView>().ViewID);
                }
            }
        }

        /// <summary>
        /// On top of doing the normal actions when exiting the trigger it will also trigger these actions
        /// on all networked versions of the charcter exiting it
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (networkEnabled == false) return;
            bool inLayer = trigger.layersToDetect == (trigger.layersToDetect | (1 << other.gameObject.layer));
            if (trigger.tagsToDetect.Contains(other.tag) || inLayer)
            {
                if (other.GetComponent<PhotonView>())
                {
                    GetComponent<PhotonView>().RPC("SyncTriggerExit", RpcTarget.Others, other.GetComponent<PhotonView>().ViewID);
                }
            }
        }

        [PunRPC]
        public void SyncTriggerEnter(int viewId)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view)
            {
                Collider col = view.transform.GetComponent<Collider>();
                if (col)
                {
                    trigger.onTriggerEnter.Invoke(col);
                }
            }
        }
        [PunRPC]
        public void SyncTriggerExit(int viewId)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view)
            {
                Collider col = view.transform.GetComponent<Collider>();
                if (col)
                {
                    trigger.onTriggerExit.Invoke(col);
                }
            }
        }
    }
}
*/
