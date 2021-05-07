using Invector.vCamera;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/Camera/Player Death Camera")]
    public class DeathCamera : MonoBehaviour
    {
        [Tooltip("The key to press in order to have the camera switch to a new player " +
            "target if \"allowSwitching\" is true.")]
        [SerializeField] protected string keyToSwitchPrevious = "";
        [Tooltip("The key to press in order to have the camera switch to a new player " +
            "target if \"allowSwitching\" is true.")]
        [SerializeField] protected string keyToSwitchNext = "";
        [Tooltip("(Optional)The UI GameObject to enable when you die.")]
        [SerializeField] protected GameObject deathVisual = null;
        [Tooltip("UnityEvent. Called when you allow the owner player to switch camera targets.")]
        public UnityEvent OnEnableSwitching = new UnityEvent();
        [Tooltip("UnityEvent. Called when you disallow the owner player to switch camera targets.")]
        public UnityEvent OnDisableSwitching = new UnityEvent();

        protected int _targetIndex = 0;
        protected bool _canSwitch = false;

        /// <summary>
        /// Makes sure the death visual is inactive by default.
        /// </summary>
        protected virtual void Awake()
        {
            if (deathVisual != null)
            {
                deathVisual.SetActive(false);
            }
        }

        /// <summary>
        /// Changes the invector camera target to target another player in the room.
        /// </summary>
        /// <param name="target">Transform type, the transform that you want the camera to target</param>
        /// <returns>true</returns>
        public virtual bool SwitchCameraTarget(Transform target)
        {
            if (target == null) return false;
            FindObjectOfType<vThirdPersonCamera>().SetTarget(target);
            return true;
        }

        /// <summary>
        /// Sets the potential next target the camera can switch to. Does not
        /// switch the camera target.
        /// </summary>
        public virtual void SelectNextTarget()
        {
            vThirdPersonController[] lookTargets = FindObjectsOfType<vThirdPersonController>();
            _targetIndex += 1;
            if (_targetIndex >= lookTargets.Length)
            {
                _targetIndex = 0;
            }
            if (SwitchCameraTarget(lookTargets[_targetIndex].transform) == false)
            {
                SelectNextTarget();
            }
        }

        /// <summary>
        /// Sets the potential next target the camera can switch to. Does not 
        /// switch the camera target.
        /// </summary>
        public virtual void SelectPreviousTarget()
        {
            vThirdPersonController[] lookTargets = FindObjectsOfType<vThirdPersonController>();
            _targetIndex -= 1;
            if (_targetIndex < 0)
            {
                _targetIndex = lookTargets.Length-1;
            }
            if (SwitchCameraTarget(lookTargets[_targetIndex].transform) == false)
            {
                SelectPreviousTarget();
            }
        }

        /// <summary>
        /// Allow the owner player to switch the camera target.
        /// </summary>
        /// <param name="isEnabled">bool type, allow camera switching?</param>
        public virtual void EnableSwitching(bool isEnabled)
        {
            _canSwitch = isEnabled;
            if (deathVisual != null)
            {
                deathVisual.SetActive(isEnabled);
            }
        }

        /// <summary>
        /// If switching is allowed, this will capture the owner input and
        /// switch the camera to target another player when the owner presses
        /// the `keyToSwitchPrevious` or `keyToSwitchNext` parameter value.
        /// </summary>
        protected virtual void Update()
        {
            if (_canSwitch)
            {
                if (Input.GetButtonUp(keyToSwitchPrevious))
                {
                    SelectPreviousTarget();
                }
                else if (Input.GetButtonUp(keyToSwitchNext))
                {
                    SelectNextTarget();
                }
            }
        }
    }
}