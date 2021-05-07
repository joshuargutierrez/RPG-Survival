using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vCharacterController.vActions
{
    [vClassHeader("Trigger Generic Action", false, iconName = "triggerIcon")]
    public class vTriggerGenericAction : vMonoBehaviour
    {
        [vEditorToolbar("Input", order = 1)]

        public InputType inputType = InputType.GetButtonDown;

        [Tooltip("Input to make the action")]
        public GenericInput actionInput = new GenericInput("E", "A", "A");

        public enum InputType
        {
            GetButtonDown,
            GetDoubleButton,
            GetButtonTimer,
            AutoAction
        };
        
        [vHelpBox("Time you have to hold the button *Only for GetButtonTimer*")]
        public float buttonTimer = 3f;
        [vHelpBox("Add delay to start the input count *Only for GetButtonTimer*")] 
        public float inputDelay = 0.1f;
        [vHelpBox("*Only for GetButtonTimer* \n\n<b>TRUE: </b> Play the animation while you're holding the button \n" +
            "<b>FALSE: </b>Play the animation after you finish holding the button")]
        public bool playAnimationWhileHoldingButton = true;
        
        [vHelpBox("Time to press the button twice *Only for GetDoubleButton*")]
        public float doubleButtomTime = 0.25f;

        [vEditorToolbar("Trigger", order = 2)]
        public string actionName = "Action";
        public string actionTag = "Action";
        [vHelpBox("Disable this trigger OnStart")]
        public bool disableOnStart = false;
        [vHelpBox("Disable the Player's Capsule Collider Collision, useful for animations with closer interactions")]
        public bool disableCollision;
        [vHelpBox("Disable the Player's Rigidbody Gravity, useful for on air animations")]
        public bool disableGravity;
        [vHelpBox("It will only use the trigger if the forward of the character is close to the forward of this transform")]
        public bool activeFromForward;
        [vHelpBox("Max angle between character forward and trigger forward to active trigger"), Range(5, 180)]
        public float forwardAngle = 30;
        [vHelpBox("Rotate Character to the Forward Rotation of this Trigger")]
        public bool useTriggerRotation;
        [vHelpBox("Destroy this Trigger after pressing the Input or AutoAction or finishing the Action")]
        public bool destroyAfter = false;
        [vHideInInspector("destroyAfter")]
        public float destroyDelay = 0f;
        [vHelpBox("Change your CameraState to a Custom State while playing the animation")]
        public string customCameraState;

        [vEditorToolbar("Animation", order = 2)]

        [vHelpBox("Trigger a Animation - Use the exactly same name of the AnimationState you want to trigger, " +
            "don't forget to add a vAnimatorTag to your State")]
        public string playAnimation;

        [vHelpBox("Check the Exit Time of your animation (if it doesn't loop) and insert here. \n\n" +
            "For example if your Exit Time is 0.8 and the Transition Duration is 0.2 you need to insert 0.5 or lower as the final value. " +
            "\n\nAlways check with the Debug of the GenericAction if your animation is finishing correctly, " +
            "otherwise the controller won't reset to the default physics and collision.", vHelpBoxAttribute.MessageType.Warning)]
        [Tooltip("You can use this to make a persistent action, and finish the action calling FinishAction method of the vGenericAction  component in your character")]
        public bool endActionManualy = false;
        [vHideInInspector("endActionManualy",invertValue =true)]
        public float endExitTimeAnimation = 0.8f;
        [vHelpBox("Use a ActionState value to apply special conditions for your AnimatorController transitions")]
        public int animatorActionState = 0;
        [vHelpBox("Reset the ActionState parameter to 0 after playing the animation")]
        public bool resetAnimatorActionState = true;
        public bool useAnimatorMatchTarget = true;
        [vHelpBox("Use a empty transform as reference for the MatchTarget")]
        public Transform matchTarget;
        [vHelpBox("Select the bone you want to use as reference to the Match Target")]       
        public AvatarTarget avatarTarget;
        [Header("Curve Match target system")]
        public bool useLocalX = false;       
        public bool useLocalZ = true;      
        public AnimationCurve matchPositionXZCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 1));   
        public AnimationCurve matchPositionYCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 1));      
        public AnimationCurve matchRotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 1));

        [vHelpBox("Check what positions XYZ you want the matchTarget to work")]
        [Header("Animator Match target system")]  
        [vHelpBox("<b>These properties are related to the animator's matchtarget system</b>.\n<i> To use our new system curve based, <color=red>uncheck the UseAnimatorMatchTarget</color></i>\n <b>This properties will be removed in next update</b>",messageType:vHelpBoxAttribute.MessageType.Info)]
        [Space(30)]

        [FormerlySerializedAs("matchTargetMask")]
        public Vector3 matchPos;
        [vHelpBox("Rotate Weight for your character to use the matchTarget rotation")]
        [Range(0, 1f)]
        public float matchRot;      
        [vHelpBox("Time of the animation to start the MatchTarget goes from 0 to 1")]
        public float startMatchTarget;
        [vHelpBox("Time of the animation to end the MatchTarget goes from 0 to 1")]
        public float endMatchTarget;

        [vEditorToolbar("Events", order = 3)]

        [Tooltip("Delay to run the OnDoAction Event")]
        [FormerlySerializedAs("onDoActionDelay")]
        public float onPressActionDelay;

        [Header("--- INPUT EVENTS ---")]
        [FormerlySerializedAs("OnDoAction")]
        public UnityEvent OnPressActionInput;
        public OnDoActionWithTarget onPressActionInputWithTarget;

        [Header("--- ONLY FOR GET BUTTON TIMER ---")]
        public UnityEvent OnCancelActionInput;
        public UnityEvent OnFinishActionInput;
        public OnUpdateValue OnUpdateButtonTimer;

        [Header("--- ANIMATION EVENTS ---")]
        public UnityEvent OnStartAnimation;
        public UnityEvent OnEndAnimation;

        [Header("--- PLAYER AND TRIGGER DETECTION ---")]
        public OnDoActionWithTarget OnPlayerEnter;
        public OnDoActionWithTarget OnPlayerStay;
        public OnDoActionWithTarget OnPlayerExit;
        [Header("--- ACTION VALIDATION  ---")]
        public OnDoActionWithTarget OnValidate;
        public OnDoActionWithTarget OnInvalidate;
        private float currentButtonTimer;
        internal Collider _collider;

        protected virtual void Start()
        {
            this.gameObject.tag = actionTag;
            this.gameObject.layer = LayerMask.NameToLayer("Triggers");
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            if (disableOnStart)
                this.enabled = false;
        }       

        public virtual IEnumerator OnPressActionDelay(GameObject obj)
        {
            yield return new WaitForSeconds(onPressActionDelay);
            OnPressActionInput.Invoke();
            if (obj)
                onPressActionInputWithTarget.Invoke(obj);
        }

        public void UpdateButtonTimer(float value)
        {
            if (value != currentButtonTimer)
            {
                currentButtonTimer = value;
                OnUpdateButtonTimer.Invoke(value);
            }
        }

        [System.Serializable]
        public class OnUpdateValue : UnityEvent<float>
        {

        }
    }

    [System.Serializable]
    public class OnDoActionWithTarget : UnityEvent<GameObject>
    {

    }
  
}