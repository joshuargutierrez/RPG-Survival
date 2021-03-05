using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCamera
{
    public class vThirdPersonCamera : MonoBehaviour
    {

        private static vThirdPersonCamera _instance;
        public static vThirdPersonCamera instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<vThirdPersonCamera>();

                    //Tell unity not to destroy this object when loading a new scene!
                    //DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        #region inspector properties    
        public Transform mainTarget;

        [Tooltip("Lerp speed between Camera States")]
        public float smoothBetweenState = 6f;
        public float smoothCameraRotation = 6f;
        public float scrollSpeed = 10f;

        [Tooltip("What layer will be culled")]
        public LayerMask cullingLayer = 1 << 0;
        [Tooltip("Change this value If the camera pass through the wall")]
        public float clipPlaneMargin;
        public float checkHeightRadius;
        public bool showGizmos;
        public bool startUsingTargetRotation = true;
        public bool startSmooth = false;
        [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
        public bool lockCamera;

        public Vector2 offsetMouse;

        #endregion

        #region hide properties    
        [HideInInspector]
        public int indexList, indexLookPoint;
        [HideInInspector]
        public float offSetPlayerPivot;
        [HideInInspector]
        public float distance = 5f;
        [HideInInspector]
        public string currentStateName;
        [HideInInspector]
        public Transform currentTarget;
        [HideInInspector]
        public vThirdPersonCameraState currentState;
        [HideInInspector]
        public vThirdPersonCameraListData CameraStateList;
        [HideInInspector]
        public Transform lockTarget;
        [HideInInspector]
        public Vector2 movementSpeed;
        [HideInInspector]
        public vThirdPersonCameraState lerpState;

        protected float lockTargetSpeed;
        protected float lockTargetWeight;
        protected Transform targetLookAt;
        protected Vector3 currentTargetPos;
        protected Vector3 lookPoint;
        protected Vector3 current_cPos;
        protected Vector3 desired_cPos;
        protected Vector3 lookTargetAdjust;

        internal float mouseY = 0f;
        internal float mouseX = 0f;
        protected float currentHeight;
        protected float currentZoom;
        protected float cullingHeight;
        protected float cullingDistance;
        protected float switchRight, currentSwitchRight;
        protected float heightOffset;
        internal bool isInit;
        protected bool useSmooth;
        protected bool isNewTarget;
        protected bool firstStateIsInit;
        protected Quaternion fixedRotation;
        internal Camera targetCamera;

        protected float transformWeight;
        protected float mouseXStart;
        protected float mouseYStart;
        protected Vector3 startPosition;
        protected Quaternion startRotation;

        protected private Vector3 cameraVelocityDamp;
        protected private bool firstUpdated;

        #endregion
        /// <summary>
        /// Lock camera angle based to the <seealso cref="currentTarget"/>. if you need just to reset angle use <seealso cref="ResetAngle"/>
        /// </summary>
        public bool LockCamera
        {
            get
            {
                return lockCamera;
            }
            set
            {
                lockCamera = false;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (showGizmos)
            {
                if (currentTarget)
                {
                    var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
                    Gizmos.DrawWireSphere(targetPos + Vector3.up * cullingHeight, checkHeightRadius);
                    Gizmos.DrawLine(targetPos, targetPos + Vector3.up * cullingHeight);
                }
            }
        }

        protected virtual void Start()
        {
            Init();
        }
        /// <summary>
        /// Init camera.
        /// </summary>
        public virtual void Init()
        {
            if (mainTarget == null)
            {
                return;
            }

            firstUpdated = true;
            useSmooth = true;
            if (!targetLookAt)
            {
                targetLookAt = new GameObject("targetLookAt").transform;
            }

            targetLookAt.rotation = startUsingTargetRotation ? mainTarget.rotation : transform.rotation;
            targetLookAt.position = mainTarget.position;
            targetLookAt.hideFlags = HideFlags.HideInHierarchy;
            startPosition = transform.position;
            startRotation = transform.rotation;
            if (!targetCamera)
            {
                targetCamera = Camera.main;
            }

            currentTarget = mainTarget;
            switchRight = 1f;
            currentSwitchRight = 1f;
            mouseXStart = transform.eulerAngles.NormalizeAngle().y;
            mouseYStart = transform.eulerAngles.NormalizeAngle().x;

            if (startSmooth)
            {
                distance = Vector3.Distance(targetLookAt.position, transform.position);
            }
            else
            {
                transformWeight = 1;
            }

            if (startUsingTargetRotation)
            {
                mouseY = currentTarget.eulerAngles.NormalizeAngle().x;
                mouseX = currentTarget.eulerAngles.NormalizeAngle().y;
            }
            else
            {
                mouseY = transform.eulerAngles.NormalizeAngle().x;
                mouseX = transform.eulerAngles.NormalizeAngle().y;
            }


            ChangeState("Default", startSmooth);
            currentZoom = currentState.defaultDistance;
            currentHeight = currentState.height;

            currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z) + currentTarget.transform.up * lerpState.height;
            targetLookAt.position = currentTargetPos;

            isInit = true;
        }

        protected virtual void FixedUpdate()
        {
            if (mainTarget == null || targetLookAt == null || currentState == null || lerpState == null || !isInit)
            {
                return;
            }

            switch (currentState.cameraMode)
            {
                case TPCameraMode.FreeDirectional:
                    CameraMovement();
                    break;
                case TPCameraMode.FixedAngle:
                    CameraMovement();
                    break;
                case TPCameraMode.FixedPoint:
                    CameraFixed();
                    break;
            }
        }
        /// <summary>
        /// Set a <seealso cref="lockTarget"/> to the  camera  auto rotate to look to.
        /// </summary>
        public virtual void SetLockTarget(Transform lockTarget)
        {
            if (this.lockTarget != null && this.lockTarget == lockTarget)
            {
                return;
            }

            isNewTarget = lockTarget != this.lockTarget;
            this.lockTarget = lockTarget;
            lockTargetWeight = 0;
            this.lockTargetSpeed = 1;
        }
        /// <summary>
        /// Set a <seealso cref="lockTarget"/> to the  camera  auto rotate to look to.
        /// </summary>
        /// <param name="lockTarget">Target to look</param>
        /// <param name="heightOffset">Height offset</param>
        /// <param name="lockSpeed">speed to look</param>
        public virtual void SetLockTarget(Transform lockTarget, float heightOffset, float lockSpeed = 1)
        {
            if (this.lockTarget != null && this.lockTarget == lockTarget)
            {
                return;
            }

            isNewTarget = lockTarget != this.lockTarget;
            this.lockTarget = lockTarget;
            this.heightOffset = heightOffset;
            lockTargetWeight = 0;
            this.lockTargetSpeed = lockSpeed;
        }
        /// <summary>
        /// Remove the <seealso cref="lockTarget"/>
        /// </summary>
        public virtual void RemoveLockTarget()
        {
            lockTargetWeight = 0;
            lockTarget = null;
        }

        /// <summary>
        /// Set <seealso cref="currentTarget"/>. If you need to retorn to <seealso cref="mainTarget"/>, use <seealso cref="ResetTarget"/>
        /// </summary>
        /// <param name="newTarget"></param>
        public virtual void SetTarget(Transform newTarget)
        {
            lockTargetWeight = 0;
            currentTarget = newTarget ? newTarget : mainTarget;
        }
        /// <summary>
        /// Set<seealso cref="mainTarget"/> and<seealso cref= "currentTarget" />
        /// </summary>
        /// <param name= "newTarget" ></ param>
        public virtual void SetMainTarget(Transform newTarget)
        {
            mainTarget = newTarget;
            currentTarget = newTarget;
            if (!isInit)
            {
                Init();
            }
        }

        /// <summary>
        /// Set <seealso cref="currentTarget"/> to <seealso cref="mainTarget"/>
        /// </summary>
        public virtual void ResetTarget()
        {
            if (currentTarget != mainTarget)
            {
                currentTarget = mainTarget;
                if (!isInit)
                {
                    Init();
                }
            }
        }

        /// <summary>
        /// Set the camera angle based to <seealso cref="currentTarget"/>
        /// </summary>
        public virtual void ResetAngle()
        {
            if (currentTarget)
            {
                mouseY = currentTarget.eulerAngles.NormalizeAngle().x;
                mouseX = currentTarget.eulerAngles.NormalizeAngle().y;
            }
            else
            {
                mouseY = 0;
                mouseX = 0;
            }
        }

        /// <summary>    
        /// Convert a point in the screen in a Ray for the world
        /// </summary>
        /// <param name="Point"></param>
        /// <returns></returns>
        public virtual Ray ScreenPointToRay(Vector3 Point)
        {
            return this.GetComponent<Camera>().ScreenPointToRay(Point);
        }

        /// <summary>
        /// Change CameraState
        /// </summary>
        /// <param name="stateName"></param>       
        public virtual void ChangeState(string stateName)
        {
            ChangeState(stateName, true);
        }

        /// <summary>
        /// Change CameraState
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="Use smoth"></param>
        public virtual void ChangeState(string stateName, bool hasSmooth)
        {
            if (currentState != null && currentState.Name.Equals(stateName) || !isInit && firstStateIsInit)
            {
                if (firstStateIsInit)
                {
                    useSmooth = hasSmooth;
                }

                return;
            }
            useSmooth = !firstStateIsInit ? startSmooth : hasSmooth;
            // search for the camera state string name
            vThirdPersonCameraState state = CameraStateList != null ? CameraStateList.tpCameraStates.Find(delegate (vThirdPersonCameraState obj) { return obj.Name.Equals(stateName); }) : new vThirdPersonCameraState("Default");

            if (state != null)
            {
                currentStateName = stateName;
                currentState.cameraMode = state.cameraMode;
                lerpState = state; // set the state of transition (lerpstate) to the state finded on the list
                if (!firstStateIsInit)
                {
                    currentState.defaultDistance = Vector3.Distance(targetLookAt.position, transform.position);
                    currentState.forward = lerpState.forward;
                    currentState.height = state.height;
                    currentState.fov = state.fov;
                    if (useSmooth)
                    {
                        StartCoroutine(ResetFirstState());
                    }
                    else
                    {
                        distance = lerpState.defaultDistance;
                        firstStateIsInit = true;
                    }
                }
                // in case there is no smooth, a copy will be make without the transition values
                if (currentState != null && !useSmooth)
                {
                    currentState.CopyState(state);
                }
            }
            else
            {
                // if the state choosed if not real, the first state will be set up as default
                if (CameraStateList != null && CameraStateList.tpCameraStates.Count > 0)
                {
                    if (lerpState != null)
                    {
                        return;
                    }

                    state = CameraStateList.tpCameraStates[0];
                    currentStateName = state.Name;
                    currentState.cameraMode = state.cameraMode;
                    lerpState = state;

                    if (currentState != null && !useSmooth)
                    {
                        currentState.CopyState(state);
                    }
                }
            }
            // in case a list of states does not exist, a default state will be created
            if (currentState == null)
            {
                currentState = new vThirdPersonCameraState("Null");
                currentStateName = currentState.Name;
            }
            if (CameraStateList != null)
            {
                indexList = CameraStateList.tpCameraStates.IndexOf(state);
            }

            currentZoom = state.defaultDistance;

            if (currentState.cameraMode == TPCameraMode.FixedAngle)
            {
                mouseX = currentState.fixedAngle.x;
                mouseY = currentState.fixedAngle.y;
            }

            currentState.fixedAngle = new Vector3(mouseX, mouseY);
            indexLookPoint = 0;
            if (!isInit)
            {
                CameraMovement(true);
            }
        }

        /// <summary>
        /// Change State using look at point if the cameraMode is FixedPoint  
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="pointName"></param>
        /// <param name="hasSmooth"></param>
        public virtual void ChangeState(string stateName, string pointName, bool hasSmooth)
        {
            useSmooth = hasSmooth;
            if (!currentState.Name.Equals(stateName))
            {
                // search for the camera state string name
                var state = CameraStateList.tpCameraStates.Find(delegate (vThirdPersonCameraState obj)
                {
                    return obj.Name.Equals(stateName);
                });

                if (state != null)
                {
                    currentStateName = stateName;
                    currentState.cameraMode = state.cameraMode;
                    lerpState = state; // set the state of transition (lerpstate) to the state finded on the list
                                       // in case there is no smooth, a copy will be make without the transition values
                    if (currentState != null && !hasSmooth)
                    {
                        currentState.CopyState(state);
                    }
                }
                else
                {
                    // if the state choosed if not real, the first state will be set up as default
                    if (CameraStateList.tpCameraStates.Count > 0)
                    {
                        state = CameraStateList.tpCameraStates[0];
                        currentStateName = state.Name;
                        currentState.cameraMode = state.cameraMode;
                        lerpState = state;
                        if (currentState != null && !hasSmooth)
                        {
                            currentState.CopyState(state);
                        }
                    }
                }
                // in case a list of states does not exist, a default state will be created
                if (currentState == null)
                {
                    currentState = new vThirdPersonCameraState("Null");
                    currentStateName = currentState.Name;
                }

                indexList = CameraStateList.tpCameraStates.IndexOf(state);
                currentZoom = state.defaultDistance;
                currentState.fixedAngle = new Vector3(mouseX, mouseY);
                indexLookPoint = 0;
            }

            if (currentState.cameraMode == TPCameraMode.FixedPoint)
            {
                var point = currentState.lookPoints.Find(delegate (LookPoint obj)
                {
                    return obj.pointName.Equals(pointName);
                });
                if (point != null)
                {
                    indexLookPoint = currentState.lookPoints.IndexOf(point);
                }
                else
                {
                    indexLookPoint = 0;
                }
            }
        }

        protected virtual IEnumerator ResetFirstState()
        {
            yield return new WaitForEndOfFrame();
            firstStateIsInit = true;
        }

        /// <summary>
        /// Change the lookAtPoint of current state if cameraMode is FixedPoint
        /// </summary>
        /// <param name="pointName"></param>
        public virtual void ChangePoint(string pointName)
        {
            if (currentState == null || currentState.cameraMode != TPCameraMode.FixedPoint || currentState.lookPoints == null)
            {
                return;
            }

            var point = currentState.lookPoints.Find(delegate (LookPoint obj) { return obj.pointName.Equals(pointName); });
            if (point != null)
            {
                indexLookPoint = currentState.lookPoints.IndexOf(point);
            }
            else
            {
                indexLookPoint = 0;
            }
        }

        /// <summary>    
        /// Zoom baheviour 
        /// </summary>
        /// <param name="scroolValue"></param>
        /// <param name="zoomSpeed"></param>
        public virtual void Zoom(float scroolValue)
        {
            currentZoom -= scroolValue * scrollSpeed;
        }

        /// <summary>
        /// Camera Rotation behaviour
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void RotateCamera(float x, float y)
        {

            if (currentState.cameraMode.Equals(TPCameraMode.FixedPoint) || !isInit)
            {
                return;
            }

            if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
            {
                // lock into a target            
                if (!lockTarget)
                {
                    // free rotation 
                    mouseX += x * currentState.xMouseSensitivity;
                    mouseY -= y * currentState.yMouseSensitivity;

                    movementSpeed.x = x;
                    movementSpeed.y = -y;
                    if (!lockCamera)
                    {
                        mouseY = vExtensions.ClampAngle(mouseY, lerpState.yMinLimit, lerpState.yMaxLimit);
                        mouseX = vExtensions.ClampAngle(mouseX, lerpState.xMinLimit, lerpState.xMaxLimit);
                    }
                    else
                    {
                        mouseY = currentTarget.root.eulerAngles.NormalizeAngle().x;
                        mouseX = currentTarget.root.eulerAngles.NormalizeAngle().y;
                    }
                }
            }
            else
            {
                // fixed rotation
                var _x = lerpState.fixedAngle.x;
                var _y = lerpState.fixedAngle.y;
                mouseX = useSmooth ? Mathf.LerpAngle(mouseX, _x, smoothBetweenState * Time.deltaTime) : _x;
                mouseY = useSmooth ? Mathf.LerpAngle(mouseY, _y, smoothBetweenState * Time.deltaTime) : _y;
            }

        }

        /// <summary>
        /// Switch Camera Right 
        /// </summary>
        /// <param name="value"></param>
        public virtual void SwitchRight(bool value = false)
        {
            switchRight = value ? -1 : 1;
        }

        protected virtual void CalculeLockOnPoint()
        {
            if (currentState.cameraMode.Equals(TPCameraMode.FixedAngle) && lockTarget)
            {
                return;   // check if angle of camera is fixed         
            }

            var collider = lockTarget.GetComponent<Collider>();                                  // collider to get center of bounds

            if (collider == null)
            {
                return;
            }

            var _point = collider.bounds.center;
            Vector3 relativePos = _point - (desired_cPos);                      // get position relative to transform
            Quaternion rotation = Quaternion.LookRotation(relativePos);         // convert to rotation

            //convert angle (360 to 180)
            var y = 0f;
            var x = rotation.eulerAngles.y;
            if (rotation.eulerAngles.x < -180)
            {
                y = rotation.eulerAngles.x + 360;
            }
            else if (rotation.eulerAngles.x > 180)
            {
                y = rotation.eulerAngles.x - 360;
            }
            else
            {
                y = rotation.eulerAngles.x;
            }

            if (lockTargetWeight < 1f)
            {
                lockTargetWeight += Time.deltaTime * lockTargetSpeed;
            }

            mouseY = Mathf.LerpAngle(mouseY, vExtensions.ClampAngle(y, currentState.yMinLimit, currentState.yMaxLimit), lockTargetWeight);
            mouseX = Mathf.LerpAngle(mouseX, vExtensions.ClampAngle(x, currentState.xMinLimit, currentState.xMaxLimit), lockTargetWeight);
        }

        protected virtual void CameraMovement(bool forceUpdate = false)
        {
            if (currentTarget == null || targetCamera == null || (!firstStateIsInit && !forceUpdate))
            {
                return;
            }

            transformWeight = Mathf.Clamp(transformWeight += Time.deltaTime, 0f, 1f);
            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState * Time.deltaTime);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            if (currentState.useZoom)
            {
                currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
                distance = useSmooth ? Mathf.Lerp(distance, currentZoom, lerpState.smooth * Time.deltaTime) : currentZoom;
            }
            else
            {
                distance = useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance, lerpState.smooth * Time.deltaTime) : currentState.defaultDistance;
                currentZoom = currentState.defaultDistance;
            }
            targetCamera.fieldOfView = currentState.fov;
            cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.deltaTime);
            currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothBetweenState * Time.deltaTime);
            var camDir = (currentState.forward * targetLookAt.forward) + ((currentState.right * currentSwitchRight) * targetLookAt.right);

            camDir = camDir.normalized;

            var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, currentTarget.position.z) + currentTarget.transform.up * offSetPlayerPivot;
            currentTargetPos = targetPos;
            desired_cPos = targetPos + currentTarget.transform.up * currentState.height;
            current_cPos = firstUpdated ? targetPos + currentTarget.transform.up * currentHeight : Vector3.SmoothDamp(current_cPos, targetPos + currentTarget.transform.up * currentHeight, ref cameraVelocityDamp, lerpState.smoothDamp * Time.deltaTime);
            firstUpdated = false;
            RaycastHit hitInfo;

            ClipPlanePoints planePoints = targetCamera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
            ClipPlanePoints oldPoints = targetCamera.NearClipPlanePoints(desired_cPos + (camDir * currentZoom), clipPlaneMargin);
            //Check if Height is not blocked 
            if (Physics.SphereCast(targetPos, checkHeightRadius, currentTarget.transform.up, out hitInfo, currentState.cullingHeight + 0.2f, cullingLayer))
            {
                var t = hitInfo.distance - 0.2f;
                t -= currentState.height;
                t /= (currentState.cullingHeight - currentState.height);
                cullingHeight = Mathf.Lerp(currentState.height, currentState.cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            }
            else
            {
                cullingHeight = useSmooth ? Mathf.Lerp(cullingHeight, currentState.cullingHeight, smoothBetweenState * Time.deltaTime) : currentState.cullingHeight;
            }
            //Check if desired target position is not blocked       
            if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, currentZoom + 0.2f, cullingLayer, Color.blue))
            {
                var dist = hitInfo.distance;
                if (dist < currentState.defaultDistance)
                {
                    var t = dist;
                    t -= currentState.cullingMinDist;
                    t /= (currentZoom - currentState.cullingMinDist);
                    currentHeight = Mathf.Lerp(cullingHeight, currentState.height, Mathf.Clamp(t, 0.0f, 1.0f));
                    current_cPos = targetPos + currentTarget.transform.up * currentHeight;
                }
            }
            else
            {
                currentHeight = useSmooth ? Mathf.Lerp(currentHeight, currentState.height, smoothBetweenState * Time.deltaTime) : currentState.height;
            }

            if (cullingDistance < distance)
            {
                distance = cullingDistance;
            }

            //Check if target position with culling height applied is not blocked
            if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan))
            {
                distance = Mathf.Clamp(cullingDistance, 0.0f, currentState.defaultDistance);
            }

            var lookPoint = current_cPos + targetLookAt.forward * 2f;
            lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
            targetLookAt.position = current_cPos;

            float _mouseY = Mathf.LerpAngle(mouseYStart, mouseY, transformWeight);
            float _mouseX = Mathf.LerpAngle(mouseXStart, mouseX, transformWeight);
            Quaternion newRot = Quaternion.Euler(_mouseY + offsetMouse.y, _mouseX + offsetMouse.x, 0);
            targetLookAt.rotation = useSmooth ? Quaternion.Lerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime) : newRot;
            transform.position = Vector3.Lerp(startPosition, current_cPos + (camDir * (distance)), transformWeight);
            var rotation = Quaternion.LookRotation((lookPoint) - transform.position);

            if (lockTarget)
            {
                CalculeLockOnPoint();

                if (!(currentState.cameraMode.Equals(TPCameraMode.FixedAngle)))
                {
                    var collider = lockTarget.GetComponent<Collider>();
                    if (collider != null)
                    {
                        var point = (collider.bounds.center + Vector3.up * heightOffset) - transform.position;
                        var euler = Quaternion.LookRotation(point).eulerAngles - rotation.eulerAngles;
                        if (isNewTarget)
                        {
                            lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, euler.x, lockTargetWeight);
                            lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, euler.y, lockTargetWeight);
                            lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, euler.z, lockTargetWeight);
                            // Quaternion.LerpUnclamped(lookTargetAdjust, Quaternion.Euler(euler), currentState.smoothFollow * Time.deltaTime);
                            if (Vector3.Distance(lookTargetAdjust, euler) < .5f)
                            {
                                isNewTarget = false;
                            }
                        }
                        else
                        {
                            lookTargetAdjust = euler;
                        }
                    }
                }
            }
            else
            {
                lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, 0, currentState.smooth * Time.deltaTime);
                lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, 0, currentState.smooth * Time.deltaTime);
                lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, 0, currentState.smooth * Time.deltaTime);
                //lookTargetAdjust = Quaternion.LerpUnclamped(lookTargetAdjust, Quaternion.Euler(Vector3.zero), 1 * Time.deltaTime);
            }
            var _euler = rotation.eulerAngles + lookTargetAdjust;
            _euler.z = 0;
            var _rot = Quaternion.Euler(_euler + currentState.rotationOffSet);
            transform.rotation = Quaternion.Lerp(startRotation, _rot, transformWeight);
            movementSpeed = Vector2.zero;
        }

        protected virtual void CameraFixed()
        {
            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            transformWeight = Mathf.Clamp(transformWeight += Time.deltaTime, 0f, 1f);
            var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot + currentState.height, currentTarget.position.z);
            currentTargetPos = useSmooth ? Vector3.MoveTowards(currentTargetPos, targetPos, currentState.smooth * Time.deltaTime) : targetPos;
            current_cPos = currentTargetPos;
            var pos = isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : transform.position;
            transform.position = Vector3.Lerp(startPosition, useSmooth ? Vector3.Lerp(transform.position, pos, currentState.smooth * Time.deltaTime) : pos, transformWeight);
            targetLookAt.position = current_cPos;
            if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
            {
                var rot = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
                transform.rotation = Quaternion.Lerp(startRotation, useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smooth * 0.5f) * Time.deltaTime) : rot, transformWeight);
            }
            else if (isValidFixedPoint)
            {
                var rot = Quaternion.LookRotation(currentTargetPos - transform.position);
                transform.rotation = Quaternion.Lerp(startRotation, useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smooth) * Time.deltaTime) : rot, transformWeight);
            }
            targetCamera.fieldOfView = currentState.fov;
        }

        protected virtual bool isValidFixedPoint
        {
            get
            {
                return (currentState.lookPoints != null && currentState.cameraMode.Equals(TPCameraMode.FixedPoint) && (indexLookPoint < currentState.lookPoints.Count || currentState.lookPoints.Count > 0));
            }
        }

        protected virtual bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
        {
            bool value = false;
            if (showGizmos)
            {
                Debug.DrawRay(from, _to.LowerLeft - from, color);
                Debug.DrawLine(_to.LowerLeft, _to.LowerRight, color);
                Debug.DrawLine(_to.UpperLeft, _to.UpperRight, color);
                Debug.DrawLine(_to.UpperLeft, _to.LowerLeft, color);
                Debug.DrawLine(_to.UpperRight, _to.LowerRight, color);
                Debug.DrawRay(from, _to.LowerRight - from, color);
                Debug.DrawRay(from, _to.UpperLeft - from, color);
                Debug.DrawRay(from, _to.UpperRight - from, color);
            }
            if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                cullingDistance = hitInfo.distance;
            }

            if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance)
                {
                    cullingDistance = hitInfo.distance;
                }
            }

            if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance)
                {
                    cullingDistance = hitInfo.distance;
                }
            }

            if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
            {
                value = true;
                if (cullingDistance > hitInfo.distance)
                {
                    cullingDistance = hitInfo.distance;
                }
            }

            return hitInfo.collider && value;
        }
    }
}