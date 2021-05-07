/*
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController
{
    [AddComponentMenu("CB GAMES/Player/MP Components/MP vHeadTrack")]
    public class MP_HeadTrack : vHeadTrack, IPunObservable
    {
        private bool _updated = false;
        private bool updateIK = true;
        private bool sendPosition = false;
        private bool _initialized = false;

        void Awake()
        {
            StartCoroutine(WaitToInitialize());
        }
        IEnumerator WaitToInitialize()
        {
            yield return new WaitForSeconds(0.05f);
            _initialized = true;
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
        {
            try
            {
                if (_initialized == false) return;
                if (stream.IsWriting && GetComponent<PhotonView>().IsMine == true)
                {
                    if (sendPosition == true)
                    {
                        stream.SendNext(currentLookPosition);
                    }
                    stream.SendNext(_currentHeadWeight);
                    stream.SendNext(_currentbodyWeight);
                }
                else if (stream.IsReading)
                {
                    if (sendPosition == true)
                    {
                        currentLookPosition = (Vector3)stream.ReceiveNext();
                    }
                    _currentHeadWeight = (float)stream.ReceiveNext();
                    _currentbodyWeight = (float)stream.ReceiveNext();
                }
            }
            catch { }
        }

        /// <summary>
        /// Used to have the network player update its head rotation based on the 
        /// rotations and weights that are received by the owner player.
        /// </summary>
        void LateUpdate()
        {
            if (sendPosition == false)
            {
                sendPosition = true;
            }
            if (GetComponent<PhotonView>().IsMine == false && updateIK == true)
            {
                if (_updated == false)
                {
                    _updated = true;
                    GetComponent<vThirdPersonInput>().onLateUpdate -= this.UpdateHeadTrack;
                }
                SetLookAtPosition(currentLookPosition, _currentHeadWeight, _currentbodyWeight);
                updateIK = false;
            }
        }

        void FixedUpdate()
        {
            updateIK = true;
        }
    }
}
*/
