using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;
using System.Text.RegularExpressions;

namespace CBGames.Player
{
    [Serializable]
    public class CullDistance
    {
        [Tooltip("The interest group to subscribe to when another player is within that distance.")]
        public byte interest_group = 0;
        [Tooltip("The distance the other player must be at or less than to subscribe to be a part " +
            "of this photon interest group.")]
        public float distance = 10;
        [Tooltip("This only applies to the color you want this circle to be in the gizmo in the editor.")]
        public Color circleColor = Color.green;
        [Tooltip("How fast to move the networked players position when they're in this interest group.")]
        [Range(0,30)]
        public float moveSpeed = 5.0f;
        [Tooltip("How fast to move the networked players rotation when they're in this interest group.")]
        [Range(0, 30)]
        public float rotateSpeed = 5.0f;

        public CullDistance(byte i_group, float i_distance, Color i_color, float i_move, float i_rot)
        {
            interest_group = i_group;
            distance = i_distance;
            circleColor = i_color;
            moveSpeed = i_move;
            rotateSpeed = i_rot;
        }
    }
    [AddComponentMenu("CB GAMES/Player/Network Culling")]
    [RequireComponent(typeof(PhotonView))]
    public class NetworkCulling : MonoBehaviourPun, IPunObservable
    {
        #region Parameters

        #region Custom Inspector Variables
        [HideInInspector] public bool show_culling = true;
        [HideInInspector] public bool show_lastGroup = false;
        [HideInInspector] public bool auto_set_groups = true;
        #endregion

        #region Modifiables
        [Tooltip("The distances that will be used to send to certain interest groups. This is to help " +
            "return the network load. Objects/Players that are greater distance than the last item in the " +
            "list will always receive the least amount of network packets. \n\n" +
            "EX: If the other " +
            "player is less than or equal to 10 units away from this object and you have an " +
            "item in this list with a distance of 10 that means they will be in that specified group " +
            "for sending network data. The lower on the list/greater the distance means the less network " +
            "packets that player will receive from this object.")]
        [SerializeField] protected List<CullDistance> cullDistances = new List<CullDistance>()
        {
            new CullDistance(1, 15, new Color32(0, 255, 81, 255), 6, 6),
            new CullDistance(2, 30, new Color32(0, 6, 114, 255), 5, 5),
            new CullDistance(3, 45, new Color32(255, 255, 0, 255), 4, 4),
        };
        [Tooltip("The interest group to use when the player is further than the greatest cullDistance.")]
        [SerializeField] protected byte last_group = 255;
        [Tooltip("How fast to move the networked players position when they're in this interest group.")]
        [Range(0,30)]
        public float last_moveSpeed = 3.0f;
        [Tooltip("How fast to move the networked players rotation when they're in this interest group.")]
        [Range(0, 30)]
        public float last_rotateSpeed = 3.0f;
        #if UNITY_EDITOR
        [Tooltip("Disable or enable the distance gizmos.")]
        [SerializeField] private bool drawGizmos = true;
        #endif
        #endregion

        #region Gizmo Parameters

        #if UNITY_EDITOR
        private Color guiTextColor = Color.white;
        private Color orgGizmoColor = Color.black;
        #endif

        #endregion

        #region Internal Use Values
        protected List<byte> _subToGroups = new List<byte>();
        protected int _cullIndex = 0;
        protected int _finalIndex = 0;
        protected List<SyncPlayer> _allPlayers = new List<SyncPlayer>();
        protected List<SyncPlayer> _tmpList = new List<SyncPlayer>();
        protected int _loops = 0;
        
        #if UNITY_EDITOR
        private bool _isOwner = true;
        #endif
        #endregion

        #endregion

        #region Startup Logic
        protected void Start()
        {
            if (photonView.IsMine == false)
            {
                enabled = false;

                #if UNITY_EDITOR
                _isOwner = false;
                #endif
            }
        }
        #endregion

        #region Dynamically Setting Interest Groups & Sync Values
        /// <summary>
        /// Used to network cull based on registered player distances. Will only send certain messages 
        /// to certain interest groups. The furthest interest group will receive the least amount of 
        /// messages while the first interest group will receive all the messages.
        /// </summary>
        /// <param name="stream">Used by photon</param>
        /// <param name="info">Used by Photon</param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (photonView.IsMine)
            {
                SetInterestGroups();

                _cullIndex++;
                if (_cullIndex > _finalIndex)
                {
                    _cullIndex = 0;
                    _finalIndex--;
                    if (_finalIndex < 0)
                    {
                        _finalIndex = cullDistances.Count;
                    }
                }
                if (_cullIndex == 0 && _loops == 0)
                {
                    PhotonNetwork.SetSendingEnabled(new byte[0], null);
                    photonView.Group = 0;
                }
                else if (_cullIndex < cullDistances.Count && _subToGroups.Count > 0 && _subToGroups.Contains(cullDistances[_cullIndex].interest_group))
                {
                    PhotonNetwork.SetSendingEnabled(new byte[0], new byte[1] { cullDistances[_cullIndex].interest_group });
                    photonView.Group = cullDistances[_cullIndex].interest_group;
                }
                else if (_cullIndex == cullDistances.Count && _finalIndex == cullDistances.Count)
                {
                    _loops = (_loops + 1 > 3) ? 0 : _loops + 1;
                    PhotonNetwork.SetSendingEnabled(new byte[0], new byte[1] { cullDistances[0].interest_group });
                    photonView.Group = cullDistances[0].interest_group;
                }
            }
        }

        /// <summary>
        /// Used to register the groups you would like to send/receive messages to/from (doesn't send the messages
        /// this is just to tell what groups you can actually send to/receive from).It is also responsible for 
        /// setting the other players (that you see locally) speed and rotation as well as what group they should
        /// receive network messages from.
        /// </summary>
        protected void SetInterestGroups()
        {
            _subToGroups.Clear();
            _allPlayers.Clear();
            _tmpList.Clear();
            _allPlayers.AddRange(FindObjectsOfType<SyncPlayer>());
            foreach (CullDistance item in cullDistances)
            {
                foreach (SyncPlayer player in _allPlayers)
                {
                    if (Vector3.Distance(transform.position, player.transform.position) <= item.distance)
                    {
                        _tmpList.Add(player);
                    }
                }
                if (_tmpList.Count > 0)
                {
                    foreach (SyncPlayer player in _tmpList)
                    {
                        _allPlayers.Remove(player);
                        player.photonView.Group = item.interest_group;
                        player._positionLerpRate = item.moveSpeed;
                        player._rotationLerpRate = item.rotateSpeed;
                    }
                    _tmpList.Clear();
                    _subToGroups.Add(item.interest_group);
                }
            }
            if (_allPlayers.Count > 0)
            {
                foreach (SyncPlayer player in _allPlayers)
                {
                    player.photonView.Group = last_group;
                    player._positionLerpRate = last_moveSpeed;
                    player._rotationLerpRate = last_rotateSpeed;
                }
                _subToGroups.Add(last_group);
            }
            PhotonNetwork.SetInterestGroups(new byte[0], _subToGroups.ToArray());
        }
        #endregion

        #region Editor Debugging
        #if UNITY_EDITOR
        /// <summary>
        /// This is used to draw the wire spheres that is helpful to visualize the distances you're setting for
        /// player network culling.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (drawGizmos == false) return;
            orgGizmoColor = Gizmos.color;
            guiTextColor = GUI.color;

            if (_isOwner)
            {
                int index = 0;
                float additional = 1;
                for (int i = 0; i < cullDistances.Count; i++)
                {
                    Gizmos.color = cullDistances[i].circleColor;
                    Gizmos.DrawWireSphere(transform.position, cullDistances[i].distance);

                    GUI.color = cullDistances[i].circleColor;
                    index = (i - 1 < 0) ? 0 : i - 1;
                    additional = (i == 0) ? 0.5f : 1f;
                    Handles.Label(transform.position + transform.forward * ((cullDistances[index].distance) * additional) + new Vector3(0,i,0), "Interest Group: " + cullDistances[i].interest_group);
                }
                GUI.color = Color.red;
                Handles.Label(transform.position + transform.forward * (cullDistances[cullDistances.Count - 1].distance + 2), "Interest Group: " + last_group);

                GUI.color = guiTextColor;
                Gizmos.color = orgGizmoColor;
            }

            Handles.Label(transform.position + (transform.up * 2.5f), "Current Group: " + photonView.Group);
        }
        #endif
        #endregion
    }
}