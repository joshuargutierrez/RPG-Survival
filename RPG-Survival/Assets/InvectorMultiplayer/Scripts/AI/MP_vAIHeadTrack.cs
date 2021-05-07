/*
using Invector.vCharacterController.AI;
using Photon.Pun;
using UnityEngine;

namespace Invector
{
    [AddComponentMenu("CB GAMES/AI/MP Components/MP vAIHeadTrack")]
    [RequireComponent(typeof(PhotonView))]
    public class MP_vAIHeadTrack : vAIHeadtrack, IPunObservable
    {
        #region Internal Only Variables
        protected bool _updateIK = false;
        protected Animator _animator = null;
        protected vIControlAI _character = null;
        protected Vector3 _lookAtPoint = Vector3.zero;
        #endregion

        #region Network Serialization
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            try
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(_currentHeadWeight);
                    stream.SendNext(_currentbodyWeight);
                    stream.SendNext(_lookAtPoint);
                }
                else if (stream.IsReading)
                {
                    _currentHeadWeight = (float)stream.ReceiveNext();
                    _currentbodyWeight = (float)stream.ReceiveNext();
                    _lookAtPoint = (Vector3)stream.ReceiveNext();
                }
            }
            catch { }
        }
        #endregion

        #region Initializations
        protected override void Start()
        {
            _animator = GetComponent<Animator>();
            _character = GetComponent<vIControlAI>();
            base.Start();
        }
        #endregion

        #region Heartbeat
        /// <summary>
        /// Override or original FixedUpdate so I can control when to call fixed update 
        /// with a variable that I have access to.
        /// </summary>
        protected override void FixedUpdate()
        {
            _updateIK = true;
            base.FixedUpdate();
        }

        /// <summary>
        /// Only perform the late update if you're the owner. Otherwise only rotate
        /// to specified locations and weights.
        /// </summary>
        protected override void LateUpdate()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                base.LateUpdate();
            }
            else
            {
                if (_animator == null || _character.currentHealth <= 0 || _character.isDead || _character.ragdolled || !_animator.enabled || (!_updateIK && _animator.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;

                _updateIK = false;
                if (onPreUpdateSpineIK != null) onPreUpdateSpineIK.Invoke();
                if (_lookAtPoint != Vector3.zero)
                {
                    LookAtIK(_lookAtPoint, _currentHeadWeight, _currentbodyWeight);
                }
                if (onPosUpdateSpineIK != null && !IgnoreHeadTrackFromAnimator()) onPosUpdateSpineIK.Invoke();
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// When the main look target changes notify all the network versions to 
        /// switch to the new target.
        /// </summary>
        /// <param name="target"></param>
        public override void SetMainLookTarget(Transform target)
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                if (target.GetComponent<PhotonView>())
                {
                    GetComponent<PhotonView>().RPC(
                        "vAIHeadTrack_SetMainLookTarget",
                        RpcTarget.Others,
                        target.GetComponent<PhotonView>().ViewID
                    );
                }
            }
            base.SetMainLookTarget(target);
        }
        
        /// <summary>
        /// When the main look target is removed, notify all the network versions 
        /// to remove their target as well.
        /// </summary>
        public override void RemoveMainLookTarget()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                GetComponent<PhotonView>().RPC(
                    "vAIHeadTrack_RemoveMainLookTarget",
                    RpcTarget.Others
                );
            }
            base.RemoveMainLookTarget();
        }

        /// <summary>
        /// Set the _lookAtPoint if you're the owner 
        /// </summary>
        /// <returns></returns>
        protected override Vector3 GetLookPoint()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                _lookAtPoint = base.GetLookPoint();
            }
            return base.GetLookPoint();
        }
        #endregion

        #region RPCs
        [PunRPC]
        void vAIHeadTrack_SetMainLookTarget(int viewId)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view)
            {
                Debug.Log(view.transform, view.transform);
                SetMainLookTarget(view.transform);
            }
        }
        [PunRPC]
        void vAIHeadTrack_RemoveMainLookTarget()
        {
            RemoveMainLookTarget();
        }
        #endregion
    }
}
*/
