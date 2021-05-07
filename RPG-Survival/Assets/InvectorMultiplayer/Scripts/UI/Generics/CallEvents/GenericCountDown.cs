using CBGames.Core;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CBGames.UI
{
    public enum CounterStartType { Immediately, OnCall };
    public enum NumberType { WholeNumber, FullTime, AbbreviatedTime, Raw }

    [AddComponentMenu("CB GAMES/UI/Generics/Call Events/Generic Count Down")]
    public class GenericCountDown : MonoBehaviour
    {
        [Tooltip("Only perform the UnityEvents if you are the owner/not the owner.")]
        [SerializeField] protected bool useRoomOwnerShip = false;
        [Tooltip("If you are the owner then only perform the OnZero event.")]
        [SerializeField] protected bool ifIsOwner = false;
        [Tooltip("The time the timer will start counting down from.")]
        [SerializeField] protected float startTime = 6.0f;
        [Tooltip("How fast to count down? Higher = faster, Lower = Slower")]
        [SerializeField] protected float countSpeed = 1.0f;
        [Tooltip("If you want to countdown based off the time on the photon server. Great for " +
            "having everyone keep the same time. However this is not responsible for starting " +
            "everyone at the same time. Just counting down at the same speed!")]
        [SerializeField] protected bool syncWithPhotonServer = false;
        [Tooltip("When do you want to start counting?\n" +
            "Immediately=The OnStart call will trigger the counting sequence.\n" +
            "OnCall=An outside source will have to call the StartCounting function.")]
        [SerializeField] protected CounterStartType startType = CounterStartType.OnCall;
        [Tooltip("How do you want the number to be displayed? \n" +
            "WholeNumber = Display in integer format\n\n" +
            "FullTime = Display like 00:00:00\n\n" +
            "AbbreviatedTime = Display like 00:00\n\n" +
            "Raw = Display the raw float value")]
        [SerializeField] protected NumberType numberType = NumberType.WholeNumber;
        [Tooltip("(Optional)Texts to overwrite and show the current counted number.")]
        [SerializeField] protected Text[] texts = new Text[] { };
        [Tooltip("(Optional) The audio source to play the tick clip sound.")]
        [SerializeField] protected AudioSource soundSource = null;
        [Tooltip("(Optional) The audio clip to play every time a whole number goes down by 1.")]
        [SerializeField] protected AudioClip tickClip = null;
        [Tooltip("UnityEvent. Called when the counting starts.")]
        public UnityEvent OnStartCounting = new UnityEvent();
        [Tooltip("UnityEvent. Called when the countind stops.")]
        public UnityEvent OnStopCounting = new UnityEvent();
        [Tooltip("UnityEvent. Called when the current number the counter is at changes.")]
        public FloatUnityEvent OnNumberChange = new FloatUnityEvent();
        [Tooltip("UnityEvent. Called when the current number reaches zero.")]
        public UnityEvent OnZero = new UnityEvent();
        [Tooltip("If you want verbose logging into the console as to what is happening in this script.")]
        [SerializeField] protected bool debugging = false;

        protected bool _startCounting;
        protected bool _invoked = false;
        [HideInInspector] [SerializeField] protected float _time = 0.0f;
        protected float _actualTime = 0.0f;
        protected float _prevTime = 0.0f;
        protected TimeSpan _ts;
        protected string _timeString;
        protected float _startTime = 0;
        protected double _syncedStartTime = 0;

        #region EditorVars
        [HideInInspector] public bool showUnityEvents = false;
        #endregion

        /// <summary>
        /// If the `startType` is `Immediately` it will call the `StartCounting` function.
        /// Also sets the `tickClip` if there is a `soundSource` and a `tickClip` specified.
        /// </summary>
        protected virtual void Start()
        {
            if (startType == CounterStartType.Immediately)
            {
                StartCounting();
            }
            if (soundSource != null && tickClip != null)
            {
                soundSource.clip = tickClip;
            }
        }

        /// <summary>
        /// Set the current time number the counter is at.
        /// </summary>
        /// <param name="incomingTime">float type, the current time number to set.</param>
        public virtual void SetTime(float incomingTime)
        {
            if (debugging == true) Debug.Log("Setting time: " + incomingTime);
            _time = incomingTime;
        }

        /// <summary>
        /// The time that the timer should start at when first starting to count down.
        /// </summary>
        /// <param name="timeToStart">float type, the start counting from time.</param>
        public virtual void SetStartTime(float timeToStart)
        {
            if (debugging == true) Debug.Log("Setting Start time: " + timeToStart);
            startTime = timeToStart;
        }

        /// <summary>
        /// Remove time from the current countdown time.
        /// </summary>
        /// <param name="subtractTime">float type, the amount of time to subtract.</param>
        public virtual void SubtractTime(float subtractTime)
        {
            if (debugging == true) Debug.Log("Subtracting "+ subtractTime + " amount of time");
            _time -= subtractTime;
        }

        /// <summary>
        /// Sets the `tickClip`.
        /// </summary>
        /// <param name="clip">AudioClip type, the tick clip to use.</param>
        public virtual void SetClip(AudioClip clip)
        {
            if (debugging == true) Debug.Log("Setting tick clip: "+clip);
            tickClip = clip;
        }

        /// <summary>
        /// Sets the `soundSource`.
        /// </summary>
        /// <param name="source">AudioSource type, the audiosource to use.</param>
        public virtual void SetAudioSource(AudioSource source)
        {
            if (debugging == true) Debug.Log("Setting Audio Source: " + source);
            soundSource = source;
        }

        /// <summary>
        /// Start the countdown. Calls `OnStartCounting` UnityEvent.
        /// </summary>
        public virtual void StartCounting()
        {
            if (_startCounting == true || enabled == false) return;
            _time = startTime;
            if (syncWithPhotonServer == true && PhotonNetwork.IsMasterClient == true)
            {
                Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                if (props.ContainsKey("SYNC_TIME"))
                {
                    props["SYNC_TIME"] = PhotonNetwork.ServerTimestamp;
                }
                else
                {
                    props.Add("SYNC_TIME", PhotonNetwork.ServerTimestamp);
                }
                if (!props.ContainsKey("SYNC_TIMER_STARTED"))
                {
                    props.Add("SYNC_TIMER_STARTED", true);
                }
            }
            _actualTime = _time;
            _prevTime = _time;
            _startCounting = true;
            if (debugging == true) Debug.Log("Start counting! Sending OnStartCounting UnityEvent!");
            OnStartCounting.Invoke();
        }


        /// <summary>
        /// Stop the countdown. Calls `OnStopCounting` UntiyEvent.
        /// </summary>
        public virtual void StopCounting()
        {
            if (_startCounting == false) return;
            _invoked = false;
            _time = startTime;
            _actualTime = _time;
            _prevTime = _time;
            _startCounting = false;
            if (debugging == true) Debug.Log("Stop Counting! Send OnStopCounting UnityEvent!");
            RemoveRoomProperties();
            OnStopCounting.Invoke();
        }

        /// <summary>
        /// This is called when the timer reachs zero. It will call the `RemoveSyncTime`
        /// function and the `OnZero` UnityEvent. It also resets this timer so it can 
        /// be used again.
        /// </summary>
        protected virtual void TimerEnded()
        {
            _time = startTime;
            _actualTime = _time;
            _prevTime = _time;
            _startCounting = false;
            _invoked = false;
            if (debugging == true) Debug.Log("Reached Zero, sending OnZero UnityEvent!");
            RemoveRoomProperties();
            OnZero.Invoke();
        }

        /// <summary>
        /// Removes all the custom room properties that this component has added to sync the times.
        /// </summary>
        public virtual void RemoveRoomProperties()
        {
            if (PhotonNetwork.IsMasterClient == true && syncWithPhotonServer == true)
            {
                Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                if (props.ContainsKey("SYNC_TIME"))
                {
                    props.Remove("SYNC_TIME");
                }
                if (props.ContainsKey("SYNC_TIMER_STARTED"))
                {
                    props.Remove("SYNC_TIMER_STARTED");
                }
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
        
        /// <summary>
        /// Set the `texts` values to be whatever the input string is.
        /// </summary>
        /// <param name="value">string type, the string to display</param>
        protected virtual void SetTexts(string value)
        {
            foreach(Text text in texts)
            {
                text.text = value;
            }
        }

        /// <summary>
        /// Start playing the `soundSource`.
        /// </summary>
        protected virtual void PlayAudioClip()
        {
            if (soundSource != null && soundSource.clip != null)
            {
                if (debugging == true) Debug.Log("Play Audio Clip!");
                soundSource.Play();
            }
        }

        /// <summary>
        /// Responsible for subtracting time from the current time in an settings the 
        /// display value parameter based on the `numberType` parameter. Also calls the 
        /// `PlayAudioClip` function when the time changes. It also calls the `SetTexts`
        /// function to set the display counting values. Finally it calls the `OnZero` 
        /// UnityEvent when reacing zero and the `StopCounting` function when it reaches
        /// zero.
        /// </summary>
        protected virtual void Update()
        {
            if (_startCounting == true)
            {
                if (syncWithPhotonServer == true)
                {
                    if (_startTime == 0)
                    {
                        _startTime = _time;
                    }
                    _time = _startTime - (float)(PhotonNetwork.Time - _syncedStartTime);
                }
                else
                {
                    _time -= Time.deltaTime * countSpeed;
                }
                switch (numberType)
                {
                    case NumberType.WholeNumber:
                        _actualTime = Mathf.Round(_time);
                        _timeString = _actualTime.ToString();
                        break;
                    case NumberType.FullTime:
                        _ts = TimeSpan.FromSeconds(_time);
                        _actualTime = _ts.Seconds;
                        _timeString = string.Format("{0:00}:{1:00}:{1:00}", _ts.Minutes, _ts.Seconds, _ts.Milliseconds);
                        break;
                    case NumberType.AbbreviatedTime:
                        _ts = TimeSpan.FromSeconds(_time);
                        _actualTime = _ts.Seconds;
                        _timeString = string.Format("{0:00}:{1:00}", _ts.Minutes, _ts.Seconds);
                        break;
                    case NumberType.Raw:
                        _actualTime = _time;
                        _timeString = _actualTime.ToString();
                        break;
                }
                if (_actualTime != _prevTime)
                {
                    _prevTime = _actualTime;
                    PlayAudioClip();
                    OnNumberChange.Invoke(_time);
                }
                if (texts.Length > 0)
                {
                    SetTexts(_timeString);
                }
                if (_time <= 0)
                {
                    if (_invoked == false &&
                        (useRoomOwnerShip == false || (
                            useRoomOwnerShip == true && PhotonNetwork.IsMasterClient == ifIsOwner
                            )
                         )
                    )
                    {
                        TimerEnded();
                    }
                    else
                    {
                        StopCounting();
                    }
                }
            }
        }

        /// <summary>
        /// Callback method. This is called when the properties of the current room
        /// have changed. Will start the timer and set the sync values based on what 
        /// is received.
        /// </summary>
        /// <param name="propertiesThatChanged"></param>
        protected virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
            if (props.ContainsKey("SYNC_TIME"))
            {
                _syncedStartTime = (double)props["SYNC_TIME"];
            }
            if (props.ContainsKey("SYNC_TIMER_STARTED"))
            {
                StartCounting();
            }
        }
    }
}