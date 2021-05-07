using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
namespace Invector.vShooter
{
    using vCharacterController;
    public class vControlAimCanvas : MonoBehaviour
    {
        public static vControlAimCanvas instance;
        public RectTransform canvas;
        public List<vAimCanvas> aimCanvasCollection = new List<vAimCanvas>();
        public Camera scopeCamera { get { if (!currentAimCanvas) return null; return currentAimCanvas.scopeCamera; } }

        public bool isValid { get { if (!currentAimCanvas) return false; return currentAimCanvas.isValid; } set { currentAimCanvas.isValid = value; } }
        public bool isAimActive { get { if (!currentAimCanvas) return false; return currentAimCanvas.isAimActive; } set { currentAimCanvas.isAimActive = value; } }
        public bool isScopeCameraActive { get { if (!currentAimCanvas) return false; return currentAimCanvas.isScopeCameraActive; } set { currentAimCanvas.isScopeCameraActive = value; } }
        public bool isScopeUIActive { get { if (!currentAimCanvas) return false; return currentAimCanvas.isScopeUIActive; } set { currentAimCanvas.isScopeUIActive = value; } }
        public bool useScopeTransition { get { if (!currentAimCanvas) return false; return currentAimCanvas.useScopeTransition; } set { currentAimCanvas.useScopeTransition = value; } }
        protected bool scaleAimWithMovement { get { if (!currentAimCanvas) return false; return currentAimCanvas.scaleAimWithMovement; } }
        protected float movementSensibility { get { return currentAimCanvas.movementSensibility; } }
        protected float scaleWithMovement { get { return currentAimCanvas.scaleWithMovement; } }
        protected float smothChangeScale { get { return currentAimCanvas.smothChangeScale; } }

        protected RectTransform aimTarget { get { return currentAimCanvas.aimTarget; } }
        protected RectTransform aimCenter { get { return currentAimCanvas.aimCenter; } }
        protected Vector2 sizeDeltaTarget { get { return currentAimCanvas.sizeDeltaTarget; } }
        protected Vector2 sizeDeltaCenter { get { return currentAimCanvas.sizeDeltaCenter; } }
        
        protected vThirdPersonController cc;

        protected UnityEvent onEnableAim { get { return currentAimCanvas.onEnableAim; } }
        protected UnityEvent onDisableAim { get { return currentAimCanvas.onDisableAim; } }
        protected UnityEvent onCheckvalidAim { get { return currentAimCanvas.onCheckvalidAim; } }
        protected UnityEvent onCheckInvalidAim { get { return currentAimCanvas.onCheckInvalidAim; } }
        protected UnityEvent onEnableScopeCamera { get { return currentAimCanvas.onEnableScopeCamera; } }
        protected UnityEvent onDisableScopeCamera { get { return currentAimCanvas.onDisableScopeCamera; } }
        protected UnityEvent onEnableScopeUI { get { return currentAimCanvas.onEnableScopeUI; } }
        protected UnityEvent onDisableScopeUI { get { return currentAimCanvas.onDisableScopeUI; } }

        public vAimCanvas currentAimCanvas;

        protected int currentCanvasID;

        protected float scopeCameraTransformWeight;
        float scopeCameraTargetZoom, scopeCameraOriginZoom;
        Quaternion scopeCameraTargetRot, scopeCameraOriginRot;
        Vector3 scopeCameraTargetPos, scopeCameraOriginPos;
        bool updateScopeCameraTransition;
        public Camera mainCamera;
        public virtual void Init(vThirdPersonController cc)
        {
            mainCamera = Camera.main;
            instance = this;
            this.cc = cc;
            currentAimCanvas = aimCanvasCollection[currentCanvasID];
            isValid = true;
        }

        void FixedUpdate()
        {
            updateScopeCameraTransition = true;
        }

        void LateUpdate()
        {
            if (!updateScopeCameraTransition || !scopeCamera || !scopeCamera.gameObject.activeSelf || !useScopeTransition) return;
            updateScopeCameraTransition = false;
            if (isScopeCameraActive)
                scopeCameraTransformWeight = Mathf.Lerp(scopeCameraTransformWeight, 1.01f, 20 * Time.deltaTime);
            else
                scopeCameraTransformWeight = Mathf.Lerp(scopeCameraTransformWeight, -.01f, 20 * Time.deltaTime);

            scopeCameraTransformWeight = Mathf.Clamp(scopeCameraTransformWeight, 0, 1);
            if (!isScopeCameraActive && scopeCameraTransformWeight == 0)
                onDisableScopeCamera.Invoke();

            if (scopeCameraTransformWeight < 1)
            {
                scopeCamera.transform.rotation = Quaternion.Lerp(scopeCameraOriginRot, scopeCameraTargetRot, scopeCameraTransformWeight);
                scopeCamera.transform.position = Vector3.Lerp(scopeCameraOriginPos, scopeCameraTargetPos, scopeCameraTransformWeight);
                scopeCamera.fieldOfView = Mathf.Lerp(scopeCameraOriginZoom, scopeCameraTargetZoom, scopeCameraTransformWeight);
            }
        }

        /// <summary>
        /// Set Current Aim to Stay in Senter
        /// </summary>
        /// <param name="validPoint">Set if Aim is valid</param>
        public void SetAimToCenter(bool validPoint = true)
        {
            if (currentAimCanvas == null) return;
            if (validPoint != isValid)
            {
                isValid = validPoint;
                if (isValid) onCheckvalidAim.Invoke();
                else onCheckInvalidAim.Invoke();
            }
            if (!aimTarget || !aimCenter) return;
            aimTarget.anchoredPosition = aimCenter.anchoredPosition;
            aimTarget.sizeDelta = sizeDeltaTarget;
        }

        /// <summary>
        /// Set WordPosition of TargetAim 
        /// </summary>
        /// <param name="wordPosition">Word Position</param>
        /// <param name="validPoint">Set if Aim is Valid</param>
        public void SetWordPosition(Vector3 wordPosition, bool validPoint = true)
        {
            if (currentAimCanvas == null) return;

            if (validPoint != isValid)
            {
                isValid = validPoint;
                if (isValid) onCheckvalidAim.Invoke();
                else onCheckInvalidAim.Invoke();
            }
            if (validPoint == false) return;
            if (!aimTarget || !aimCenter) return;

            Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(wordPosition);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f)));
            if (currentAimCanvas.aimCenterToAimTarget)
                aimCenter.anchoredPosition = WorldObject_ScreenPosition;
            aimTarget.anchoredPosition = WorldObject_ScreenPosition;

            if (scaleAimWithMovement && (cc.input.magnitude > movementSensibility || Input.GetAxis("Mouse X") > movementSensibility || Input.GetAxis("Mouse Y") > movementSensibility))
            {
                aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * scaleWithMovement, smothChangeScale * Time.deltaTime);
                aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * scaleWithMovement, smothChangeScale * Time.deltaTime);
            }
            else
            {
                aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * 1, smothChangeScale * Time.deltaTime);
                aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * 1, smothChangeScale * Time.deltaTime);
            }
        }

        /// <summary>
        /// Enable or Disable the current Aim
        /// </summary>
        /// <param name="value"> active value</param>
        public void SetActiveAim(bool value)
        {
            if (currentAimCanvas == null) return;
            if (value != isAimActive)
            {
             
                isAimActive = value;
                if (value)
                {
                    isValid = true;
                    onEnableAim.Invoke();
                }
                else onDisableAim.Invoke();
            }
        }

        /// <summary>
        /// Enable or Disable the current Scope
        /// </summary>
        /// <param name="value">active value</param>
        /// <param name="useUI">set if scope camera use the Scope UI </param>
        public void SetActiveScopeCamera(bool value, bool useUI = false)
        {
            if (currentAimCanvas == null) return;
            if (isScopeCameraActive != value || isScopeUIActive != useUI)
            {
                mainCamera.enabled=!value;
                isScopeUIActive = useUI;
                if (value)
                {
                    onEnableScopeCamera.Invoke();
                    isScopeCameraActive = true;
                    if (value && useUI)
                    {
                        onEnableScopeUI.Invoke();
                        isScopeUIActive = true;
                    }
                    else
                    {
                        onDisableScopeUI.Invoke();
                        isScopeUIActive = false;
                    }
                }
                else
                {
                    if (!useScopeTransition) onDisableScopeCamera.Invoke();
                    onDisableScopeUI.Invoke();
                    isScopeUIActive = false;
                    isScopeCameraActive = false;
                }
            }
        }

        /// <summary>
        /// Update Word properties and zoom ("FieldOfView") of the Scope Camera
        /// </summary>
        /// <param name="position">word position</param>
        /// <param name="lookPosition">Word target LookAt</param>
        /// <param name="zoom">FieldOfView</param>
        public void UpdateScopeCamera(Vector3 position, Vector3 lookPosition, float zoom = 60)
        {
            if (currentAimCanvas == null || !scopeCamera) return;

            var _zoom = Mathf.Clamp(60 - zoom, 1, 179);
            if (scopeCameraTransformWeight < 1 && useScopeTransition)
            {
                scopeCameraTargetPos = position;
                scopeCameraTargetRot = Quaternion.LookRotation(lookPosition - scopeCamera.transform.position);
                scopeCameraTargetZoom = _zoom;
                scopeCameraOriginPos = mainCamera.transform.position;
                scopeCameraOriginRot = mainCamera.transform.rotation;
                scopeCameraOriginZoom = mainCamera.fieldOfView;
                if (!scopeCamera.isActiveAndEnabled && useScopeTransition)
                {
                    scopeCamera.transform.position = mainCamera.transform.position;
                    scopeCamera.transform.rotation = mainCamera.transform.rotation;
                    scopeCamera.fieldOfView = mainCamera.fieldOfView;
                }
            }
            else
            {
                scopeCamera.fieldOfView = _zoom;
                scopeCamera.transform.position = position;
                var rot = Quaternion.LookRotation(lookPosition - scopeCamera.transform.position);
                scopeCamera.transform.rotation = rot;
            }


        }

        /// <summary>
        /// Set AimCanvas ID
        /// if id do not exist,this change to defaultAimCanvas id 0
        /// </summary>
        /// <param name="id">index of AimCanvasCollection</param>
        public void SetAimCanvasID(int id)
        {
            if (aimCanvasCollection.Count > 0 && currentCanvasID != id)
            {
                if (currentAimCanvas != null) currentAimCanvas.DisableAll();
                if (id < aimCanvasCollection.Count)
                {
                    currentAimCanvas = aimCanvasCollection[id];
                    currentCanvasID = id;
                }
                else
                {
                    currentAimCanvas = aimCanvasCollection[0];
                    currentCanvasID = 0;
                }
            }
        }
    }
}