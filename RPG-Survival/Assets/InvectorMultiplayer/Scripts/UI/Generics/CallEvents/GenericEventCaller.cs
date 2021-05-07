using UnityEngine;
using UnityEngine.Events;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Call Events/Generic Event Caller")]
    public class GenericEventCaller : MonoBehaviour
    {
        [Tooltip("Call these events when this objects calls onAwake (before on start)")]
        [SerializeField] protected bool onAwake = false;
        [Tooltip("Call these events when starting this object")]
        [SerializeField] protected bool onStart = false;
        [Tooltip("Call these events when this gameobject is enabled.")]
        [SerializeField] protected bool onEnable = false;
        [Tooltip("Call these events when this gameobject is disabled")]
        [SerializeField] protected bool onDisable = false;
        [Tooltip("UnityEvent. List of user-defined events to call based on the specified settings.")]
        [SerializeField] protected UnityEvent EventsToCall = new UnityEvent();

        /// <summary>
        /// If the `onEnable` is true it will call the `EventsToCall` UnityEvent.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (onEnable == true)
            {
                EventsToCall.Invoke();
            }
        }

        /// <summary>
        /// If the `onDisable` is true it will call the `EventsToCall` UnityEvent.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (onDisable == true)
            {
                EventsToCall.Invoke();
            }
        }

        /// <summary>
        /// If the `onAwake` is true it will call the `EventsToCall` UnityEvent.
        /// </summary>
        protected virtual void Awake()
        {
            if (onAwake == true)
            {
                EventsToCall.Invoke();
            }
        }

        /// <summary>
        /// If the `onStart` is true it will call the `EventsToCall` UnityEvent.
        /// </summary>
        protected virtual void Start()
        {
            if (onStart == true)
            {
                EventsToCall.Invoke();
            }
        }
    }
}