using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
    [vClassHeader("Input Manager", iconName = "inputIcon")]
    public class vThirdPersonInput : vMonoBehaviour, vIAnimatorMoveReceiver
    {
        public delegate void OnUpdateEvent();
        public event OnUpdateEvent onUpdate;
        public event OnUpdateEvent onLateUpdate;
        public event OnUpdateEvent onFixedUpdate;
        public event OnUpdateEvent onAnimatorMove;

        #region Variables        

        [vEditorToolbar("Inputs")]
        [vHelpBox("Check these options if you need to use the mouse cursor, ex: <b>2.5D, Topdown or Mobile</b>", vHelpBoxAttribute.MessageType.Info)]
        public bool unlockCursorOnStart = false;
        public bool showCursorOnStart = false;
        [vHelpBox("PC only - use it to toggle between run/walk", vHelpBoxAttribute.MessageType.Info)]
        public KeyCode toggleWalk = KeyCode.CapsLock;
        private bool _toogleWalk;

        [Header("Movement Input")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput crouchInput = new GenericInput("C", "Y", "Y");
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput jumpInput = new GenericInput("Space", "X", "X");
        public GenericInput rollInput = new GenericInput("Q", "B", "B");

        [HideInInspector] public bool lockInput;

        [vEditorToolbar("Camera Settings")]
        public bool lockCameraInput;
        public bool invertCameraInputVertical, invertCameraInputHorizontal;
        [vEditorToolbar("Inputs")]
        [Header("Camera Input")]
        public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");
        public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");
        public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");

        [vEditorToolbar("Events")]
        public UnityEvent OnLockCamera;
        public UnityEvent OnUnlockCamera;
        public UnityEvent onEnableAnimatorMove = new UnityEvent();
        public UnityEvent onDisableDisableAnimatorMove = new UnityEvent();

        [HideInInspector]
        public vCamera.vThirdPersonCamera tpCamera;         // access tpCamera info
        [HideInInspector]
        public bool ignoreTpCamera;                         // controls whether update the cameraStates of not                
        [HideInInspector]
        public string customCameraState;                    // generic string to change the CameraState
        [HideInInspector]
        public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode
        [HideInInspector]
        public bool changeCameraState;                      // generic bool to change the CameraState
        [HideInInspector]
        public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp
        [HideInInspector]
        public vThirdPersonController cc;                   // access the ThirdPersonController component
        [HideInInspector]
        public vHUDController hud;                          // acess vHUDController component
        protected bool updateIK = false;
        protected bool isInit;
        [HideInInspector] public bool lockMoveInput;
        protected InputDevice inputDevice { get { return vInput.instance.inputDevice; } }

        protected Camera _cameraMain;
        protected bool withoutMainCamera;
        internal bool lockUpdateMoveDirection;                // lock the method UpdateMoveDirection

        public Camera cameraMain
        {
            get
            {
                if (!_cameraMain && !withoutMainCamera)
                {
                    if (!Camera.main)
                    {
                        Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                        withoutMainCamera = true;
                    }
                    else
                    {
                        _cameraMain = Camera.main;
                        cc.rotateTarget = _cameraMain.transform;
                    }
                }
                return _cameraMain;
            }
            set
            {
                _cameraMain = value;
            }
        }

        public Animator animator
        {
            get
            {
                if (cc == null)
                {
                    cc = GetComponent<vThirdPersonController>();
                }

                if (cc.animator == null)
                {
                    return GetComponent<Animator>();
                }

                return cc.animator;
            }
        }

        #endregion

        #region Initialize Character, Camera & HUD when LoadScene

        protected virtual void Start()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
            {
                cc.Init();
            }

            StartCoroutine(CharacterInit());

            ShowCursor(showCursorOnStart);
            LockCursor(unlockCursorOnStart);
            EnableOnAnimatorMove();
        }

        protected virtual IEnumerator CharacterInit()
        {
            FindCamera();
            yield return new WaitForEndOfFrame();
            FindHUD();
        }

        public virtual void FindHUD()
        {
            if (hud == null && vHUDController.instance != null)
            {
                hud = vHUDController.instance;
                hud.Init(cc);
            }
        }

        public virtual void FindCamera()
        {
            var tpCameras = FindObjectsOfType<vCamera.vThirdPersonCamera>();

            if (tpCameras.Length > 1)
            {
                tpCamera = System.Array.Find(tpCameras, tp => !tp.isInit);

                if (tpCamera == null)
                {
                    tpCamera = tpCameras[0];
                }

                if (tpCamera != null)
                {
                    for (int i = 0; i < tpCameras.Length; i++)
                    {
                        if (tpCamera != tpCameras[i])
                        {
                            Destroy(tpCameras[i].gameObject);
                        }
                    }
                }
            }
            else if (tpCameras.Length == 1)
            {
                tpCamera = tpCameras[0];
            }

            if (tpCamera && tpCamera.mainTarget != transform)
            {
                tpCamera.SetMainTarget(this.transform);
            }
        }

        #endregion

        protected virtual void LateUpdate()
        {
            if (cc == null || Time.timeScale == 0)
            {
                return;
            }

            if (!updateIK)
            {
                return;
            }

            if (onLateUpdate != null)
            {
                onLateUpdate.Invoke();
            }

            CameraInput();                      // update camera input
            UpdateCameraStates();               // update camera states                        
            updateIK = false;
        }

        protected virtual void FixedUpdate()
        {
            if (onFixedUpdate != null)
            {
                onFixedUpdate.Invoke();
            }

            Physics.SyncTransforms();
            cc.UpdateMotor();                                                   // handle the ThirdPersonMotor methods            
            cc.ControlLocomotionType();                                         // handle the controller locomotion type and movespeed   
            ControlRotation();
            cc.UpdateAnimator();                                                // handle the ThirdPersonAnimator methods
            updateIK = true;
        }

        protected virtual void Update()
        {
            if (cc == null || Time.timeScale == 0)
            {
                return;
            }

            if (onUpdate != null)
            {
                onUpdate.Invoke();
            }

            InputHandle();                      // update input methods                        
            UpdateHUD();                        // update hud graphics            
        }

        public virtual void OnAnimatorMoveEvent()
        {
            if (cc == null)
            {
                return;
            }

            cc.ControlAnimatorRootMotion();
            if (onAnimatorMove != null)
            {
                onAnimatorMove.Invoke();
            }
        }

        #region Generic Methods
        // you can call this methods anywhere in the inspector or third party assets to have better control of the controller or cutscenes

        /// <summary>
        /// Lock all Basic  Input from the Player
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockBasicInput(bool value)
        {
            lockInput = value;
            if (value)
            {
                cc.input = Vector2.zero;
                cc.isSprinting = false;
                cc.animator.SetFloat("InputHorizontal", 0, 0.25f, Time.deltaTime);
                cc.animator.SetFloat("InputVertical", 0, 0.25f, Time.deltaTime);
                cc.animator.SetFloat("InputMagnitude", 0, 0.25f, Time.deltaTime);
            }
        }

        /// <summary>
        /// Lock all Inputs 
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockAllInput(bool value)
        {
            SetLockBasicInput(value);
        }

        /// <summary>
        /// Show/Hide Cursor
        /// </summary>
        /// <param name="value"></param>
        public virtual void ShowCursor(bool value)
        {
            Cursor.visible = value;
        }

        /// <summary>
        /// Lock/Unlock the cursor to the center of screen
        /// </summary>
        /// <param name="value"></param>
        public virtual void LockCursor(bool value)
        {
            if (!value)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Lock the Camera Input
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockCameraInput(bool value)
        {
            lockCameraInput = value;

            if (lockCameraInput)
            {
                OnLockCamera.Invoke();
            }
            else
            {
                OnUnlockCamera.Invoke();
            }
        }

        /// <summary>
        /// If you're using the MoveCharacter method with a custom targetDirection, check this true to align the character with your custom targetDirection
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLockUpdateMoveDirection(bool value)
        {
            lockUpdateMoveDirection = value;
        }

        /// <summary>
        /// Limits the character to walk only, useful for cutscenes and 'indoor' areas
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetWalkByDefault(bool value)
        {
            cc.freeSpeed.walkByDefault = value;
            cc.strafeSpeed.walkByDefault = value;
        }

        /// <summary>
        /// Set the character to Strafe Locomotion
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetStrafeLocomotion(bool value)
        {
            cc.lockInStrafe = value;
            cc.isStrafing = value;
        }

        /// <summary>
        /// OnAnimatorMove Event Sender 
        /// </summary>
        internal virtual vAnimatorMoveSender animatorMoveSender { get; set; }

        /// <summary>
        /// Use Animator Move Event Sender <seealso cref="vAnimatorMoveSender"/>
        /// </summary>
        protected bool _useAnimatorMove { get; set; }

        /// <summary>
        /// Check if OnAnimatorMove is Enabled
        /// </summary>
        public virtual bool UseAnimatorMove
        {
            get
            {
                return _useAnimatorMove;
            }
            set
            {

                if (_useAnimatorMove != value)
                {
                    if (value)
                    {
                        animatorMoveSender = gameObject.AddComponent<vAnimatorMoveSender>();
                        onEnableAnimatorMove?.Invoke();
                    }
                    else
                    {
                        if (animatorMoveSender)
                        {
                            Destroy(animatorMoveSender);
                        }

                        onEnableAnimatorMove?.Invoke();
                    }
                }
                _useAnimatorMove = value;
            }
        }

        /// <summary>
        /// Enable OnAnimatorMove event
        /// </summary>
        public virtual void EnableOnAnimatorMove()
        {
            UseAnimatorMove = true;
        }

        /// <summary>
        /// Disable OnAnimatorMove event
        /// </summary>
        public virtual void DisableOnAnimatorMove()
        {
            UseAnimatorMove = false;
        }

        #endregion

        #region Basic Locomotion Inputs

        protected virtual void InputHandle()
        {
            if (lockInput || cc.ragdolled)
            {
                return;
            }

            MoveInput();
            SprintInput();
            CrouchInput();
            StrafeInput();
            JumpInput();
            RollInput();
        }

        public virtual void MoveInput()
        {
            if (!lockMoveInput)
            {
                // gets input
                cc.input.x = horizontalInput.GetAxisRaw();
                cc.input.z = verticallInput.GetAxisRaw();
            }

            if (Input.GetKeyDown(toggleWalk))
            {
                _toogleWalk = !_toogleWalk;
                SetWalkByDefault(_toogleWalk);
            }

            cc.ControlKeepDirection();
        }

        public virtual void ControlRotation()
        {
            if (cameraMain && !lockUpdateMoveDirection)
            {
                if (!cc.keepDirection)
                {
                    cc.UpdateMoveDirection(cameraMain.transform);
                }
            }

            if (tpCamera != null && tpCamera.lockTarget && cc.isStrafing)
            {
                cc.RotateToPosition(tpCamera.lockTarget.position);          // rotate the character to a specific target
            }
            else
            {
                cc.ControlRotationType();                                   // handle the controller rotation type (strafe or free)
            }
        }

        protected virtual void StrafeInput()
        {
            if (strafeInput.GetButtonDown())
            {
                cc.Strafe();
            }
        }

        protected virtual void SprintInput()
        {
            if (sprintInput.useInput)
            {
                cc.Sprint(cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton());
            }
        }

        protected virtual void CrouchInput()
        {
            cc.AutoCrouch();

            if (crouchInput.useInput && crouchInput.GetButtonDown())
            {
                cc.Crouch();
            }
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return !cc.customAction && !cc.isCrouching && cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && cc.currentStamina >= cc.jumpStamina && !cc.isJumping && !cc.isRolling;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if (jumpInput.GetButtonDown() && JumpConditions())
            {
                cc.Jump(true);
            }
        }

        /// <summary>
        /// Conditions to trigger the Roll animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool RollConditions()
        {
            return (!cc.isRolling || cc.canRollAgain) && cc.input != Vector3.zero && !cc.customAction && cc.isGrounded && cc.currentStamina > cc.rollStamina && !cc.isJumping;
        }

        /// <summary>
        /// Input to trigger the Roll
        /// </summary>
        protected virtual void RollInput()
        {
            if (rollInput.GetButtonDown() && RollConditions())
            {
                cc.Roll();
            }
        }

        #endregion       

        #region Camera Methods

        public virtual void CameraInput()
        {
            if (!cameraMain)
            {
                return;
            }

            if (tpCamera == null)
            {
                return;
            }

            var Y = lockCameraInput ? 0f : rotateCameraYInput.GetAxis();
            var X = lockCameraInput ? 0f : rotateCameraXInput.GetAxis();
            if (invertCameraInputHorizontal)
            {
                X *= -1;
            }

            if (invertCameraInputVertical)
            {
                Y *= -1;
            }

            var zoom = cameraZoomInput.GetAxis();

            tpCamera.RotateCamera(X, Y);
            if (!lockCameraInput)
            {
                tpCamera.Zoom(zoom);
            }
        }

        public virtual void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
            if (ignoreTpCamera)
            {
                return;
            }

            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vCamera.vThirdPersonCamera>();
                if (tpCamera == null)
                {
                    return;
                }

                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }

            if (changeCameraState)
            {
                tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
            }
            else if (cc.isCrouching)
            {
                tpCamera.ChangeState("Crouch", true);
            }
            else if (cc.isStrafing)
            {
                tpCamera.ChangeState("Strafing", true);
            }
            else
            {
                tpCamera.ChangeState("Default", true);
            }
        }

        public virtual void ChangeCameraState(string cameraState, bool useLerp = true)
        {
            if (useLerp)
            {
                ChangeCameraStateWithLerp(cameraState);
            }
            else
            {
                ChangeCameraStateNoLerp(cameraState);
            }
        }

        public virtual void ResetCameraAngle()
        {
            if (tpCamera)
            {
                tpCamera.ResetAngle();
            }
        }

        public virtual void ChangeCameraStateWithLerp(string cameraState)
        {
            changeCameraState = true;
            customCameraState = cameraState;
            smoothCameraState = true;
        }

        public virtual void ChangeCameraStateNoLerp(string cameraState)
        {
            changeCameraState = true;
            customCameraState = cameraState;
            smoothCameraState = false;
        }

        public virtual void ResetCameraState()
        {
            changeCameraState = false;
            customCameraState = string.Empty;
        }

        #endregion

        #region HUD       

        public virtual void UpdateHUD()
        {
            if (hud == null)
            {
                if (vHUDController.instance != null)
                {
                    hud = vHUDController.instance;
                    hud.Init(cc);
                }
                else
                {
                    return;
                }
            }

            hud.UpdateHUD(cc);
        }

        #endregion
    }

    /// <summary>
    /// Interface to receive events from <seealso cref="vAnimatorMoveSender"/>
    /// </summary>
    public interface vIAnimatorMoveReceiver
    {
        /// <summary>
        /// Check if Component is Enabled
        /// </summary>
        bool enabled { get; set; }
        /// <summary>
        /// Method Called from <seealso cref="vAnimatorMoveSender"/>
        /// </summary>
        void OnAnimatorMoveEvent();
    }

    /// <summary>
    /// OnAnimatorMove Event Sender 
    /// </summary>
    class vAnimatorMoveSender : MonoBehaviour
    {
        private void Awake()
        {
            ///Hide in Inpector
            this.hideFlags = HideFlags.HideInInspector;
            vIAnimatorMoveReceiver[] animatorMoves = GetComponents<vIAnimatorMoveReceiver>();
            for (int i = 0; i < animatorMoves.Length; i++)
            {
                var receiver = animatorMoves[i];
                animatorMoveEvent += () =>
                {
                    if (receiver.enabled)
                    {
                        receiver.OnAnimatorMoveEvent();
                    }
                };
            }
        }

        /// <summary>
        /// AnimatorMove event called using  default unity OnAnimatorMove
        /// </summary>
        public System.Action animatorMoveEvent;

        private void OnAnimatorMove()
        {
            animatorMoveEvent?.Invoke();
        }
    }
}