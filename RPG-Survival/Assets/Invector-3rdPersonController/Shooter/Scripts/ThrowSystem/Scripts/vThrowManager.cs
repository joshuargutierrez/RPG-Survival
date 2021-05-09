using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController
{
    [RequireComponent(typeof(LineRenderer))]
    [vClassHeader("THROW MANAGER")]
    public class vThrowManager : vMonoBehaviour
    {
        #region variables

        public GenericInput throwInput = new GenericInput("Mouse0", "RB", "RB");
        public GenericInput aimThrowInput = new GenericInput("G", "LB", "LB");
        public bool aimHoldingButton = true;
        public enum CameraStyle
        {
            ThirdPerson, TopDown, SideScroll
        }

        public CameraStyle cameraStyle;
        public Transform throwStartPoint;
        public GameObject throwEnd;
        public Rigidbody objectToThrow;

        [System.Serializable]
        public class ThrowObject
        {
            Rigidbody objectToThrow;
            int id;
            int count;
        }

        public LayerMask obstacles = 1 << 0;
        public float throwMaxForce = 15f;
        public float throwDelayTime = .25f;
        public float lineStepPerTime = .1f;
        public float lineMaxTime = 10f;

        [Tooltip("Delay to exit the Throw Aiming Mode and get back to default locomotion")]
        public float exitThrowModeDelay = 0.5f;
        public string throwAnimation = "ThrowObject";
        public string holdingAnimation = "HoldingObject";
        public string cancelAnimation = "CancelThrow";
        public int maxThrowObjects = 6;
        public int currentThrowObject;
        public bool canUseThrow;
        public bool debug;
        public UnityEngine.Events.UnityEvent onEnableAim;
        public UnityEngine.Events.UnityEvent onCancelAim;
        public UnityEngine.Events.UnityEvent onThrowObject;
        public UnityEngine.Events.UnityEvent onCollectObject;
        public UnityEngine.Events.UnityEvent onFinishThrow;
        public vThrowUI ui
        {
            get
            {
                if (!_ui)
                {
                    _ui = FindObjectOfType<vThrowUI>();
                    if (_ui)
                    {
                        _ui.UpdateCount(this);
                    }
                }
                return _ui;
            }
        }

        private bool isAiming;
        private bool inThrow;
        private bool isThrowInput;
        private Transform rightUpperArm;
        private LineRenderer lineRenderer;
        private vThrowUI _ui;

        vThirdPersonInput tpInput;
        RaycastHit hit;

        #endregion       

        public void CanUseThrow(bool value)
        {
            canUseThrow = value;
        }

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if (ui != null)
            {
                ui.UpdateCount(this);
            }

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer)
            {
                lineRenderer.useWorldSpace = true;
            }

            canUseThrow = true;

            tpInput = GetComponentInParent<vThirdPersonInput>();
            if (tpInput)
            {
                tpInput.onUpdate -= UpdateThrowBehavior;
                tpInput.onUpdate += UpdateThrowBehavior;
                tpInput.onFixedUpdate -= UpdateThrow;
                tpInput.onFixedUpdate += UpdateThrow;

                rightUpperArm = tpInput.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

                if (cameraStyle == CameraStyle.SideScroll)
                {
                    tpInput.cc.strafeSpeed.rotateWithCamera = true;
                }
            }
        }

        void UpdateThrowBehavior()
        {
            if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction || !canUseThrow)
            {
                isAiming = false;
                inThrow = false;
                isThrowInput = false;
                return;
            }

            UpdateThrowInput();
            MoveAndRotate();
        }

        void UpdateThrowInput()
        {
            if (aimThrowInput.GetButtonDown() && !isAiming && !inThrow)
            {
                PrepareControllerToThrow(true);
                tpInput.animator.CrossFadeInFixedTime(holdingAnimation, 0.2f);
                onEnableAim.Invoke();
                return;
            }
            if (aimThrowInput.GetButtonUp() && aimHoldingButton && isAiming)
            {
                PrepareControllerToThrow(false);
                tpInput.animator.CrossFadeInFixedTime(cancelAnimation, 0.2f);
                onCancelAim.Invoke();
                onFinishThrow.Invoke();
            }

            if (throwInput.GetButtonDown() && isAiming && !inThrow)
            {
                isAiming = false;
                isThrowInput = true;
            }
            else if (!aimHoldingButton && aimThrowInput.GetButtonDown() && !isThrowInput && isAiming)
            {
                PrepareControllerToThrow(false);
                tpInput.animator.CrossFadeInFixedTime(cancelAnimation, 0.2f);
                onCancelAim.Invoke();
                onFinishThrow.Invoke();
            }
        }

        private void MoveAndRotate()
        {
            if (isAiming || inThrow)
            {
                tpInput.MoveInput();
                switch (cameraStyle)
                {
                    case CameraStyle.ThirdPerson:
                        tpInput.cc.RotateToDirection(tpInput.cameraMain.transform.forward);
                        break;
                    case CameraStyle.TopDown:
                        var dir = aimDirection;
                        dir.y = 0;
                        tpInput.cc.RotateToDirection(dir);
                        break;
                    case CameraStyle.SideScroll:
                        ///
                        break;
                }
            }
        }

        void LaunchObject(Rigidbody projectily)
        {
            projectily.AddForce(StartVelocity, ForceMode.VelocityChange);
        }

        void UpdateThrow()
        {
            if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction)
            {
                isAiming = false;
                inThrow = false;
                isThrowInput = false;
                if (lineRenderer && lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                }

                if (throwEnd && throwEnd.activeSelf)
                {
                    throwEnd.SetActive(false);
                }

                return;
            }

            if (isAiming)
            {
                DrawTrajectory();
            }
            else
            {
                if (lineRenderer && lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                }

                if (throwEnd && throwEnd.activeSelf)
                {
                    throwEnd.SetActive(false);
                }
            }

            if (isThrowInput)
            {
                inThrow = true;
                isThrowInput = false;
                tpInput.animator.CrossFadeInFixedTime(throwAnimation, 0.2f);
                currentThrowObject -= 1;
                StartCoroutine(Launch());
            }
        }

        void DrawTrajectory()
        {
            var points = GetTrajectoryPoints(throwStartPoint.position, StartVelocity, lineStepPerTime, lineMaxTime);
            if (lineRenderer)
            {
                if (!lineRenderer.enabled)
                {
                    lineRenderer.enabled = true;
                }

                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
            }
            if (throwEnd)
            {
                if (!throwEnd.activeSelf)
                {
                    throwEnd.SetActive(true);
                }

                if (points.Count > 1)
                {
                    throwEnd.transform.position = points[points.Count - 1];
                }
            }
        }

        IEnumerator Launch()
        {
            yield return new WaitForSeconds(throwDelayTime);
            var obj = Instantiate(objectToThrow, throwStartPoint.position, throwStartPoint.rotation);
            obj.isKinematic = false;
            LaunchObject(obj);
            if (ui)
            {
                ui.UpdateCount(this);
            }

            onThrowObject.Invoke();

            yield return new WaitForSeconds(2 * lineStepPerTime);
            var coll = obj.GetComponent<Collider>();
            if (coll)
            {
                coll.isTrigger = false;
            }

            inThrow = false;

            if (currentThrowObject <= 0)
            {
                objectToThrow = null;
            }

            yield return new WaitForSeconds(exitThrowModeDelay);
            PrepareControllerToThrow(false);
            onFinishThrow.Invoke();
        }

        private void PrepareControllerToThrow(bool value)
        {
            isAiming = value;
            tpInput.SetLockAllInput(value);
            tpInput.SetStrafeLocomotion(value);
            if (cameraStyle == CameraStyle.SideScroll)
            {
                tpInput.cc.strafeSpeed.rotateWithCamera = true;
            }
        }

        Vector3 thirdPersonAimPoint
        {
            get
            {
                return throwStartPoint.position + tpInput.cameraMain.transform.forward * throwMaxForce;
            }
        }

        Vector3 topdownAimPoint
        {
            get
            {
                var pos = vMousePositionHandler.Instance.WorldMousePosition(obstacles);
                pos.y = transform.position.y;
                return pos;
            }
        }

        Vector3 sideScrollAimPoint
        {
            get
            {
                var localPos = transform.InverseTransformPoint(vMousePositionHandler.Instance.WorldMousePosition(obstacles));
                localPos.x = 0;

                return transform.TransformPoint(localPos);
            }
        }

        Vector3 StartVelocity
        {
            get
            {
                RaycastHit hit;
                var dist = Vector3.Distance(transform.position, aimPoint);
                if (debug)
                {
                    Debug.DrawLine(transform.position, aimPoint);
                }

                if (cameraStyle == CameraStyle.ThirdPerson)
                {
                    if (Physics.Raycast(throwStartPoint.position, aimDirection.normalized, out hit, obstacles))
                    {
                        dist = hit.distance;
                    }
                }

                if (cameraStyle != CameraStyle.SideScroll)
                {
                    var force = Mathf.Clamp(dist, 0, throwMaxForce);
                    var rotation = Quaternion.LookRotation(aimDirection.normalized, Vector3.up);
                    var dir = Quaternion.AngleAxis(rotation.eulerAngles.NormalizeAngle().x, transform.right) * transform.forward;
                    return dir * force;
                }
                else
                {
                    var force = Mathf.Clamp(dist, 0, throwMaxForce);
                    return aimDirection.normalized * force;
                }
            }
        }

        Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
        {
            return start + startVelocity * time + Physics.gravity * time * time * 0.5f;
        }

        List<Vector3> GetTrajectoryPoints(Vector3 start, Vector3 startVelocity, float timestep, float maxTime)
        {
            Vector3 prev = start;
            List<Vector3> points = new List<Vector3>();
            points.Add(prev);
            for (int i = 1; ; i++)
            {
                float t = timestep * i;
                if (t > maxTime)
                {
                    break;
                }

                Vector3 pos = PlotTrajectoryAtTime(start, startVelocity, t);
                RaycastHit hit;
                if (Physics.Linecast(prev, pos, out hit, obstacles))
                {
                    points.Add(hit.point);
                    break;
                }
                if (debug)
                {
                    Debug.DrawLine(prev, pos, Color.red);
                }

                points.Add(pos);
                prev = pos;

            }
            return points;
        }

        public virtual Vector3 aimPoint
        {
            get
            {
                switch (cameraStyle)
                {
                    case CameraStyle.ThirdPerson: return thirdPersonAimPoint;
                    case CameraStyle.TopDown: return topdownAimPoint;
                    case CameraStyle.SideScroll: return sideScrollAimPoint;
                }
                return throwStartPoint.position + tpInput.cameraMain.transform.forward * throwMaxForce;
            }
        }

        public virtual Vector3 aimDirection
        {
            get
            {
                return aimPoint - rightUpperArm.position;
            }
        }

        public virtual void SetAmount(int value)
        {
            currentThrowObject += value;
            if (ui)
            {
                ui.UpdateCount(this);
            }

            onCollectObject.Invoke();
        }
    }
}