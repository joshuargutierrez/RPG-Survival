using Invector.vCharacterController;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    public enum FloatingBarType { Health, Stamina }
    public enum NumberDisplayType { Whole, Percent, Raw }

    [AddComponentMenu("CB GAMES/UI/Player/Floating Bar")]
    public class FloatingBar : MonoBehaviour
    {
        [Tooltip("What type of floating bar is this?\n\n" +
            "Health = Track the vThirdPersonController's health.\n" +
            "Stamina = Track the vThirdPersonController's stamina\n" +
            "Custom = Input a custom component that exposes a function called \"CustomOutputValue\" " +
            "that outputs a float value for you to track.")]
        public FloatingBarType type = FloatingBarType.Health;
        [Tooltip("How to display your tracked number in the displayBarNumber field.\n\n" +
            "Whole = Convert float to int.\n" +
            "Percent = Convert the number to a percent value\n" +
            "Raw = Display the raw tracked number")]
        [SerializeField] protected NumberDisplayType displayType = NumberDisplayType.Percent;
        [Tooltip("If you want to have this function be the thing that is keeping " +
            "track of the values. Uses the update method. Less efficent but saves " +
            "you some extra work.")]
        [SerializeField] protected bool realTimeTracking = true;
        [Space(10)]
        [Tooltip("All texts to fade in and out")]
        [SerializeField] protected Text[] allTexts = new Text[] { };
        [Tooltip("All images to fade in and out")]
        [SerializeField] protected Image[] allImages = new Image[] { };
        [Tooltip("The actual colored bar to resize according to the selected number.")]
        [SerializeField] protected Image coloredBar = null;
        [Tooltip("If the bars are not quite the same size then you need to have the bars at a different fill amount." +
            " Use this offset to achieve this")]
        [SerializeField] protected float colorBarFillOffset = 0.076f;
        [Tooltip("Will adjust a set delay later after the colored bar.")]
        [SerializeField] protected Image fillBar = null;
        [Tooltip("How long to wait before adjusting the fill bar.")]
        [SerializeField] protected float fillDelay = 1.5f;
        [Tooltip("How fast to adjust the fill bar.")]
        [SerializeField] protected float fillSpeed = 0.4f;
        [Tooltip("The text to show based on the current value of your bar.")]
        [SerializeField] protected Text displayBarNumber = null;
        [Tooltip("Required if using real time tracking. Will always keep track of the " +
            "values in the controller and update the bars for you.")]
        public vThirdPersonController controller = null;
        [Space(10)]
        [Tooltip("Will only display for set amount of time only if the tracked number changes.")]
        [SerializeField] protected bool startHidden = true;
        [Tooltip("How long to display this UI before disabling it after a change (Only matters if fade out is true).")]
        [SerializeField] protected float displayTime = 6.0f;
        [Tooltip("Instead of just turning off the elements you want to slowly fade this images out.")]
        [SerializeField] protected bool fadeOut = true;
        [Tooltip("How fast to fade the texts/images.")]
        [SerializeField] protected float fadeSpeed = 0.4f;
        [Tooltip("Only show this bar if this is a networked player that you are not controlling. (i.e. other players that are not you)")]
        [SerializeField] protected bool onlyEnableForNoneOwner = true;

        protected float _trackedNumber = 0.0f;
        protected float _maxNumber = 0.0f;
        protected bool _useRealTimeTracking = false;
        protected bool _adjustFillBar = false;
        protected bool _fade = false;
        protected float _alpha = 0.0f;
        protected float _prevFillAmount = 1.0f;
        protected float _preNumberValue = 0.0f;
        protected bool _updateNow = false;
        protected float _currentDisplayTime;
        // 0 = faded
        // 1 = visible

        /// <summary>
        /// Sets the starting fill amount and the starting alpha values.
        /// </summary>
        protected virtual void Awake()
        {
            if (startHidden == false)
            {
                _alpha = 1;
            }
            else
            {
                _alpha = 0;
            }
            SetElementsAlpha(_alpha);
            _prevFillAmount = coloredBar.fillAmount;
        }

        /// <summary>
        /// Finds and sets the bar values based on the current settings in the `vThirdPersonController`
        /// </summary>
        protected virtual void Start()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<vThirdPersonController>();
                if (controller == null)
                {
                    controller = GetComponent<vThirdPersonController>();
                    if (controller == null)
                    {
                        controller = GetComponentInChildren<vThirdPersonController>();
                    }
                }
            }
            if (controller != null)
            {
                _useRealTimeTracking = true;
                switch (type)
                {
                    case FloatingBarType.Health:
                        _maxNumber = controller.maxHealth;
                        _trackedNumber = controller.currentHealth;
                        break;
                    case FloatingBarType.Stamina:
                        _maxNumber = controller.maxStamina;
                        _trackedNumber = controller.currentStamina;
                        break;
                }
                _preNumberValue = _trackedNumber;
                if (onlyEnableForNoneOwner == true && controller.GetComponent<PhotonView>().IsMine == true)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Dynamically updates the alpha and fill amount values based on changes in the `vThirdPersonController`
        /// and the settings on this component.
        /// </summary>
        protected virtual void Update()
        {
            if (realTimeTracking == true && _useRealTimeTracking == true)
            {
                if (type == FloatingBarType.Health && controller.currentHealth != _preNumberValue)
                {
                    _updateNow = true;
                    _trackedNumber = controller.currentHealth;
                }
                else if (type == FloatingBarType.Stamina && controller.currentStamina != _preNumberValue)
                {
                    _updateNow = true;
                    _trackedNumber = controller.currentStamina;
                }
                if (_updateNow == true)
                {
                    _updateNow = false;
                    _preNumberValue = controller.currentHealth;
                    SetBarValue();
                    if (_prevFillAmount != coloredBar.fillAmount)
                    {
                        _prevFillAmount = coloredBar.fillAmount;
                        _alpha = 1;
                        SetElementsAlpha(_alpha);
                        _currentDisplayTime = displayTime;
                        StartCoroutine(DelayFadeElements());
                    }
                }
            }
            if (_adjustFillBar == true)
            {
                if (fillBar.fillAmount < coloredBar.fillAmount - colorBarFillOffset)
                {
                    fillBar.fillAmount = coloredBar.fillAmount - colorBarFillOffset;
                    _adjustFillBar = false;
                }
                else if (fillBar.fillAmount > coloredBar.fillAmount - colorBarFillOffset)
                {
                    fillBar.fillAmount -= Time.deltaTime * fillSpeed;
                    if (fillBar.fillAmount <= 0)
                    {
                        fillBar.fillAmount = 0;
                        _adjustFillBar = false;
                    }
                }
            }
            if (_fade == true)
            {
                _alpha -= Time.deltaTime * fadeSpeed;
                SetElementsAlpha(_alpha);
                if (_alpha <= 0 )
                {
                    _alpha = 0;
                    _fade = false;
                }
            }
            if (_currentDisplayTime > 0)
            {
                _currentDisplayTime -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Can override the value to be displayed. This is normally the `currentHealth`
        /// or `currentStamina` in the `vThirdPersonController`
        /// </summary>
        /// <param name="value"></param>
        public virtual void UpdateTrackedValue(float value)
        {
            _trackedNumber = value;
        }

        /// <summary>
        /// Can override the max value to compare against. This value is used to display
        /// percentages. This is normally the `maxStamina` or `maxHealth` values in the
        /// `vThirdPersonController`.
        /// </summary>
        /// <param name="value"></param>
        public virtual void UpdateMaxValue(float value)
        {
            _maxNumber = value;
        }

        /// <summary>
        /// Returns a percentage float comparing the current with the max values.
        /// </summary>
        /// <returns>float percentage</returns>
        protected virtual float GetRemaining()
        {
            return (_trackedNumber / _maxNumber);
        }

        /// <summary>
        /// Returns the current value as a whole number
        /// </summary>
        /// <returns>whole number representing the current value</returns>
        protected virtual int GetWholeNumber()
        {
            return Mathf.RoundToInt(_trackedNumber);
        }

        /// <summary>
        /// Sets the text and fill amounts based on the `displayType` parameter.
        /// </summary>
        protected virtual void SetBarValue()
        {
            switch (displayType)
            {
                case NumberDisplayType.Percent:
                    coloredBar.fillAmount = GetRemaining() + colorBarFillOffset;
                    displayBarNumber.text = (GetRemaining() * 100).ToString()+"%";
                    break;
                case NumberDisplayType.Raw:
                    coloredBar.fillAmount = GetRemaining() + colorBarFillOffset;
                    displayBarNumber.text = _trackedNumber.ToString();
                    break;
                case NumberDisplayType.Whole:
                    coloredBar.fillAmount = GetRemaining() + colorBarFillOffset;
                    displayBarNumber.text = GetWholeNumber().ToString();
                    break;
            }
            StartCoroutine(DelayAdjustFillBar());
        }

        /// <summary>
        /// Makes it so the fill amount doesn't adjust to the new amount for a few seconds.
        /// The wait time is based on the `fillDelay` parameter.
        /// </summary>
        protected virtual IEnumerator DelayAdjustFillBar()
        {
            _adjustFillBar = false;
            yield return new WaitForSeconds(fillDelay);
            _adjustFillBar = true;
        }

        /// <summary>
        /// Will not fade the elements after changing for a few seconds. Then calls the 
        /// `SetElementsAlpha` function.
        /// </summary>
        protected virtual IEnumerator DelayFadeElements()
        {
            _fade = false;
            yield return new WaitUntil(() => _currentDisplayTime <= 0);
            if (fadeOut == true)
            {
                _fade = true;
            }
            else
            {
                _alpha = 0;
                SetElementsAlpha(_alpha);
            }
        }

        /// <summary>
        /// Sets the alpha values on the images and text elements in the `allTexts`
        /// and `allImages` list.
        /// </summary>
        /// <param name="setAlpha">float type, the alpha value to set the elements to.</param>
        protected virtual void SetElementsAlpha(float setAlpha)
        {
            Color temp;
            foreach(Text text in allTexts)
            {
                temp = text.color;
                temp.a = setAlpha;
                text.color = temp;
            }
            foreach (Image image in allImages)
            {
                temp = image.color;
                temp.a = setAlpha;
                image.color = temp;
            }
        }
    }
}