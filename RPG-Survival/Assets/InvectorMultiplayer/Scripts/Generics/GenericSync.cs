using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static Photon.Pun.UtilityScripts.PlayerNumbering;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Player/Generic Sync")]
    [RequireComponent(typeof(PhotonView))]
    public class GenericSync : MonoBehaviourPun, IPunObservable
    {
        #region Modifiables
        [Tooltip("Sync the position of this transform across the network.")]
        public bool syncPosition = true;
        [Tooltip("Sync the rotation of this transform across the network.")]
        public bool syncRotation = true;
        [Tooltip("Sync the animations of the object across the network.")]
        public bool syncAnimations = true;
        [Tooltip("Attempt to sync triggers (if syncAnimations is true). If triggers are still not synced then you must use an RPC to do so, no other method will work.")]
        public bool syncTriggers = true;
        [Tooltip("The animator parameters that you don't want to sync across the network")]
        public List<string> ignoreParams = new List<string>();
        [Tooltip("Sync the animator layer weights across the network.")]
        public bool syncAnimatorWeights = true;
        [Tooltip("How fast to move the networked versions position to the desired location.")]
        public float positionLerpRate = 17.0f;
        [Tooltip("How fast to move the networked versions rotation to the desired rotation.")]
        public float rotationLerpRate = 17.0f;
        #endregion

        #region Internal Only
        protected Animator _anim = null;
        protected Vector3 _lastPos, _realPos, _velocity;
        protected Quaternion _realRot;
        protected Dictionary<string, AnimatorControllerParameterType> _animParams = new Dictionary<string, AnimatorControllerParameterType>();
        protected float _posTime = 0;
        protected bool _initialized = false;
        #endregion

        /// <summary>
        /// Attempts to find the animator, if none is found will disable syncing animations.
        /// Also builds the parameter dictionary to sync across the network as they change.
        /// Makes use of the `OnPlayerNumberingChanged` delegate provided by Photon to call
        /// `NewPlayerEnteredOrLeft` function.
        /// </summary>
        protected virtual void Start()
        {
            if (GetComponent<Animator>())
            {
                _anim = GetComponent<Animator>();
                BuildAnimatorParamsDict();
            }
            else
            {
                syncAnimations = false;
            }
            _lastPos = transform.position;
            StartCoroutine(WaitToInitialize());
            if (photonView.IsMine == true && (syncPosition == true || syncRotation == true))
            {
                OnPlayerNumberingChanged += NewPlayerEnteredOrLeft;
            }
        }

        /// <summary>
        /// This is only used to remove the need to call the `NewPlayerEnteredOrLeft` function
        /// from the `OnPlayerNumberingChanged` delegate
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (photonView.IsMine == true && (syncPosition == true || syncRotation == true))
            {
                OnPlayerNumberingChanged -= NewPlayerEnteredOrLeft;
            }
        }

        /// <summary>
        /// Will send the position and rotation of this object to all others on the network. 
        /// The networked versions will receive this information and will snap to the position
        /// and rotation received. Sent via RPC.
        /// </summary>
        protected virtual void NewPlayerEnteredOrLeft()
        {
            photonView.RPC("GenericSync_SetPosRot", RpcTarget.Others, transform.position, transform.rotation);
        }

        protected virtual IEnumerator WaitToInitialize()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            _initialized = true;
        }

        /// <summary>
        /// Will find all the animator parameters in the selected animator controller and store
        /// those types and names as a key value pair dictionary for later reading and syncing
        /// over the network. Also starts trigger watchers if `syncTriggers` is true.
        /// </summary>
        protected virtual void BuildAnimatorParamsDict()
        {
            if (GetComponent<Animator>())
            {
                foreach (var param in _anim.parameters)
                {
                    if (ignoreParams.Contains(param.name)) continue;
                    if (param.type != AnimatorControllerParameterType.Trigger) //Syncing triggers this way is unreliable, send trigger events via RPC
                    {
                        _animParams.Add(param.name, param.type);
                    }
                    else if (photonView.IsMine == true)
                    {
                        StartCoroutine(SyncTrigger(param.name));
                    }
                }
            }
        }

        /// <summary>
        /// A simple thread that is dedicated to watching for when a trigger is fired. When fired
        /// it will fire and RPC to the other networked versions to update theirs. This is unreliable
        /// as it doesn't always catch the trigger. For a more reliable method find where the trigger
        /// is being fired and in that function make this RPC call.
        /// </summary>
        /// <param name="triggerName">The trigger name to watch and update over the network</param>
        /// <returns></returns>
        protected virtual IEnumerator SyncTrigger(string triggerName)
        {
            yield return new WaitUntil(() => _anim.GetBool(triggerName) == true);
            photonView.RPC("GenericSync_SetTrigger", RpcTarget.Others, triggerName);
            StartCoroutine(SyncTrigger(triggerName));
            yield return null;
        }
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
        {
            if (_initialized == false || PhotonNetwork.InRoom == false) return;
            try
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(syncPosition);
                    stream.SendNext(syncRotation);
                    stream.SendNext(syncAnimations);
                    stream.SendNext(syncAnimatorWeights);
                    stream.SendNext(positionLerpRate);
                    stream.SendNext(rotationLerpRate);

                    if (syncPosition == true)
                    {
                        stream.SendNext(transform.position);
                        stream.SendNext((transform.position - _lastPos) * Time.deltaTime);
                    }
                    if (syncRotation == true)
                    {
                        stream.SendNext(transform.rotation);
                    }
                    if (syncAnimations == true)
                    {
                        foreach (var item in _animParams)
                        {
                            switch (item.Value)
                            {
                                case AnimatorControllerParameterType.Bool:
                                    stream.SendNext(_anim.GetBool(item.Key));
                                    break;
                                case AnimatorControllerParameterType.Float:
                                    stream.SendNext((_anim.GetFloat(item.Key) < 0.05f && _anim.GetFloat(item.Key) > -0.05f) ? 0 : _anim.GetFloat(item.Key));
                                    break;
                                case AnimatorControllerParameterType.Int:
                                    stream.SendNext(_anim.GetInteger(item.Key));
                                    break;
                            }
                        }
                    }
                    if (syncAnimatorWeights == true)
                    {
                        for (int i = 0; i < _anim.layerCount; i++)
                        {
                            stream.SendNext(_anim.GetLayerWeight(i));
                        }
                    }
                }
                else if (stream.IsReading)
                {
                    syncPosition = (bool)stream.ReceiveNext();
                    syncRotation = (bool)stream.ReceiveNext();
                    syncAnimations = (bool)stream.ReceiveNext();
                    syncAnimatorWeights = (bool)stream.ReceiveNext();
                    positionLerpRate = (float)stream.ReceiveNext();
                    rotationLerpRate = (float)stream.ReceiveNext();

                    if (syncPosition == true)
                    {
                        _realPos = (Vector3)stream.ReceiveNext();
                        _velocity = (Vector3)stream.ReceiveNext();
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        _realPos += (_velocity * lag);
                        if (_realPos != transform.position)
                        {
                            _lastPos = transform.position;
                            _posTime = 0;
                        }
                    }
                    if (syncRotation == true)
                    {
                        _realRot = (Quaternion)stream.ReceiveNext();
                    }
                    if (syncAnimations == true)
                    {
                        foreach (var item in _animParams)
                        {
                            switch (item.Value)
                            {
                                case AnimatorControllerParameterType.Bool:
                                    _anim.SetBool(item.Key, (bool)stream.ReceiveNext());
                                    break;
                                case AnimatorControllerParameterType.Float:
                                    _anim.SetFloat(item.Key, (float)stream.ReceiveNext());
                                    break;
                                case AnimatorControllerParameterType.Int:
                                    _anim.SetInteger(item.Key, (int)stream.ReceiveNext());
                                    break;
                            }
                        }
                    }
                    if (syncAnimatorWeights == true)
                    {
                        for (int i = 0; i < _anim.layerCount; i++)
                        {
                            _anim.SetLayerWeight(i, (float)stream.ReceiveNext());
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Used to smoothly rotate and move to the position of the owner. This is only run
        /// if this is the networked version.
        /// </summary>
        protected virtual void Update()
        {
            if (syncPosition == true && photonView.IsMine == false)
            {
                transform.position = Vector3.Lerp(_lastPos, _realPos, _posTime);
                if (_posTime < 1)
                {
                    _posTime += Time.deltaTime * positionLerpRate;
                    _posTime = (_posTime > 1) ? 1 : _posTime;
                }
            }
            if (syncRotation == true && photonView.IsMine == false)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, _realRot, Time.deltaTime * rotationLerpRate);
            }
        }

        [PunRPC]
        protected virtual void GenericSync_SetPosRot(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }
        [PunRPC]
        protected virtual void GenericSync_SetTrigger(string triggerName)
        {
            _anim.SetTrigger(triggerName);
        }
    }
}