using CBGames.Core;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Player Count")]
    public class TrackPlayerCount : MonoBehaviour
    {
        [Tooltip("The series of text objects to modify with the number of players in the room.")]
        [SerializeField] protected Text[] texts = new Text[] { };
        [Tooltip("If you ony want to count the number of players that are on a certain team.")]
        [SerializeField] protected string teamName = "";
        [Tooltip("Trigger the ReachPlayerCount unityevent when the player count = reachPlayerCount.")]
        [SerializeField] protected bool executeEventAtPlayerCount = false;
        [Tooltip("Only execute these unity events based on whether or not you're the room owner.")]
        [SerializeField] protected bool useRoomOwnerShip = false;
        [Tooltip("Excute if are you/are not the owner.")]
        [SerializeField] protected bool isOwner = false;
        [Tooltip("The number of players to reach before executing this ReachPlayerCount UnityEvent.")]
        [SerializeField] protected int reachPlayerCount = 4;
        [Tooltip("The number of players to fall to to excute the ReachedFallPlayerCount UnityEvent.")]
        [SerializeField] protected int fallBelowCount = 2;
        [Tooltip("UnityEvent. Called when you reach the specified `reachPlayerCount`.")]
        public UnityEvent ReachedPlayerCount = new UnityEvent();
        [Tooltip("UnityEvent. Called when you hit the `fallBelowCount` when you were originally at a higher value.")]
        public UnityEvent ReachedFallPlayerCount = new UnityEvent();
        [Tooltip("UnityEvent. Called when the player count changes")]
        public IntUnityEvent OnCountChanged = new IntUnityEvent();

        protected UICoreLogic logic;
        protected bool _reachedCount = false;
        protected int _prevCount = 0;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();    
        }

        /// <summary>
        /// Dynamically sets the `texts` values to be what the currented connected
        /// player count is. Will only update these values if you're currently 
        /// connected to a photon room. Will call the `ReachedFallPlayerCount` and
        /// `ReachedPlayerCount` UnityEvents.
        /// </summary>
        protected virtual void Update()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                DisplayCount(PhotonNetwork.CurrentRoom.PlayerCount);
                if (executeEventAtPlayerCount == true)
                {
                    if (_reachedCount == false && PhotonNetwork.CurrentRoom.PlayerCount >= reachPlayerCount)
                    {
                        _reachedCount = true;
                        if (useRoomOwnerShip == false || useRoomOwnerShip == true && PhotonNetwork.IsMasterClient == isOwner)
                        {
                            ReachedPlayerCount.Invoke();
                        }
                    }
                    else if (_reachedCount == true && PhotonNetwork.CurrentRoom.PlayerCount < fallBelowCount)
                    {
                        _reachedCount = false;
                        if ((useRoomOwnerShip == false || useRoomOwnerShip == true && PhotonNetwork.IsMasterClient == isOwner))
                        {
                            ReachedFallPlayerCount.Invoke();
                        }
                    }
                }
            }
            else
            {
                SetText("0");
            }
        }

        /// <summary>
        /// Calls the `SetText` function to set the values of the `texts` to be 
        /// what the current input count is. If the input count was different from
        /// the last time you called this function it will execute the `OnCountChanged`
        /// UnityEvent.
        /// </summary>
        /// <param name="count">int type, the count to display on all the texts.</param>
        protected virtual void DisplayCount(int count)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                SetText(count.ToString());
            }
            else
            {
                int total_count = 0;
                foreach (KeyValuePair<int, Photon.Realtime.Player> target_player in PhotonNetwork.CurrentRoom.Players)
                {
                    if (logic.GetUserTeamName(target_player.Value.UserId) == teamName)
                    {
                        total_count += 1;
                    }
                }
                SetText(total_count.ToString());
                if (_prevCount != total_count)
                {
                    _prevCount = total_count;
                    OnCountChanged.Invoke(total_count);
                }
            }
        }

        /// <summary>
        /// Sets the value of the `texts` to be whatever the input value is. 
        /// </summary>
        /// <param name="inputText">string type, the string to display on all the `texts`</param>
        protected virtual void SetText(string inputText)
        {
            foreach (Text text in texts)
            {
                text.text = inputText;
            }
        }

        /// <summary>
        /// Used to make sure that the fall and reached unity events are fired
        /// only once. This is calls as part of the `Update` function.
        /// </summary>
        /// <param name="isEnabled"></param>
        public virtual void EnableReachPlayerCountEvent(bool isEnabled)
        {
            executeEventAtPlayerCount = isEnabled;
            if (isEnabled == false)
            {
                _reachedCount = false;
            }
        }
    }
}