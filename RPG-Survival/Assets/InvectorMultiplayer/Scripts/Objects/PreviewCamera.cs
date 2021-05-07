using CBGames.Core;
using Invector.vCamera;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Camera/Preview Camera")]
    public class PreviewCamera : MonoBehaviour
    {
        #region Modifiables
        [Tooltip("Whether or not you want to move the camera using the camera points before the player joins a game.")]
        public bool moveImmediatly = true;
        [Tooltip("Stop the preview camera when you successfully join a room.")]
        public bool stopOnJoinRoom = true;
        [Tooltip("List of points the camera will travel do while waiting for the player to join the game.")]
        public List<Transform> cameraPoints = new List<Transform>();
        [Tooltip("How fast the camera will move between points.")]
        public float cameraMoveSpeed = 0.01f;
        [Tooltip("How close to the point before considering it close enough?")]
        public float cameraCloseEnough = 0.1f;
        [Tooltip("The Network Manager component to see if you have joined a room or not")] 
        public NetworkManager networkManager = null;
        [Tooltip("The camera that you want to manipulate.")]
        public Transform targetCam = null;
        #endregion

        #region Internal Variables
        protected Transform camMoveToTransform = null;
        protected float _startTime;
        protected float _journeyLength;
        protected int _cameraPointIndex = 0;
        protected bool _playCamera = false;
        #endregion

        /// <summary>
        /// Sets the needed variables for later use inside of this class
        /// </summary>
        protected virtual void Start()
        {
            networkManager = (networkManager == null) ? FindObjectOfType<NetworkManager>() : networkManager;
            targetCam = (targetCam == null) ? FindObjectOfType<vThirdPersonCamera>().transform : targetCam;
            _playCamera = (moveImmediatly == true) ? true : false;
        }
        
        /// <summary>
        /// Used to smoothly transition the cameras rotations and position based 
        /// on the current target point it's headed towards.
        /// </summary>
        public virtual void Update()
        {
            if (networkManager.IsInRoom() == true && stopOnJoinRoom == true && _playCamera == true)
            {
                _playCamera = false;
            }
            if (_playCamera == true)
            {
                if (camMoveToTransform)
                {
                    float distCovered = (Time.time - _startTime) * cameraMoveSpeed;
                    float fractionOfJourney = distCovered / _journeyLength;
                    targetCam.position = Vector3.Lerp(targetCam.position, camMoveToTransform.position, fractionOfJourney);
                    targetCam.rotation = Quaternion.Lerp(targetCam.rotation, camMoveToTransform.rotation, fractionOfJourney);
                    if (Vector3.Distance(targetCam.position, camMoveToTransform.position) <= cameraCloseEnough)
                    {
                        camMoveToTransform = GetCameraPoint();
                    }
                }
                else
                {
                    camMoveToTransform = GetCameraPoint();
                    targetCam.position = camMoveToTransform.position;
                    targetCam.rotation = camMoveToTransform.rotation;
                    camMoveToTransform = GetCameraPoint();
                }
            }
        }

        /// <summary>
        /// Will remove the current target point and make the target index to 
        /// be zero.
        /// </summary>
        public virtual void ResetPreviewCameraValues()
        {
            camMoveToTransform = null;
            _cameraPointIndex = 0;
        }

        /// <summary>
        /// Get the target transform point to move towards, set the target index,
        /// the start time, and the length from this to that point for smooth
        /// transitioning.
        /// </summary>
        /// <returns>The target transform point</returns>
        public virtual Transform GetCameraPoint()
        {
            Transform target = cameraPoints[_cameraPointIndex];
            _cameraPointIndex = (_cameraPointIndex + 1 > cameraPoints.Count - 1) ? 0 : _cameraPointIndex + 1;
            _startTime = Time.time;
            _journeyLength = Vector3.Distance(targetCam.position, target.position);
            return target;
        }

        /// <summary>
        /// Make the camera start moving.
        /// </summary>
        public virtual void StartPreviewCamera()
        {
            _playCamera = true;
        }

        /// <summary>
        /// Make the camera stop moving and call `ResetPreviewCameraValues` function.
        /// </summary>
        public virtual void StopPreviewCamera()
        {
            _playCamera = false;
            ResetPreviewCameraValues();
        }

        #region Editor Helpers
        protected virtual void OnDrawGizmos()
        {
            if (cameraPoints.Count > 2 && cameraPoints[0] && cameraPoints[1])
            {
                Vector3 right;
                Vector3 left;
                Vector3 direction;
                float arrowHeadAngle = 20.0f;
                float arrowHeadLength = 1.50f;
                float lookLineLength = 2.0f;

                //Draw starting line
                Gizmos.color = (moveImmediatly == true) ? Color.blue : Color.red;
                Gizmos.DrawLine(cameraPoints[0].position, cameraPoints[1].position);

                for (int i = 1; i < cameraPoints.Count; i++)
                {
                    if (cameraPoints[i].GetInstanceID() == cameraPoints[i - 1].GetInstanceID() || cameraPoints[i] == null || cameraPoints[i-1] == null) continue;

                    //Draw move arrow
                    Gizmos.color = (moveImmediatly == true) ? Color.blue : Color.red;
                    direction = cameraPoints[i].position - cameraPoints[i - 1].position;
                    right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                    left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                    Gizmos.DrawLine(cameraPoints[i - 1].position, cameraPoints[i].position);
                    Gizmos.DrawRay(cameraPoints[i - 1].position + direction, right * arrowHeadLength);
                    Gizmos.DrawRay(cameraPoints[i - 1].position + direction, left * arrowHeadLength);

                    //Draw look arrow
                    Gizmos.color = (moveImmediatly == true) ? Color.yellow : Color.red;
                    direction = cameraPoints[i].position + cameraPoints[i].forward * lookLineLength;
                    Gizmos.DrawLine(cameraPoints[i].position, direction);
                }

                //Draw start over arrow
                if (cameraPoints[cameraPoints.Count - 1])
                {
                    Gizmos.color = (moveImmediatly == true) ? Color.grey : Color.red;
                    direction = cameraPoints[0].position - cameraPoints[cameraPoints.Count - 1].position;
                    right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                    left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
                    Gizmos.DrawLine(cameraPoints[0].position, cameraPoints[cameraPoints.Count - 1].position);
                    Gizmos.DrawRay(cameraPoints[cameraPoints.Count - 1].position + direction, right * arrowHeadLength);
                    Gizmos.DrawRay(cameraPoints[cameraPoints.Count - 1].position + direction, left * arrowHeadLength);
                }
                //Draw start look arrow
                Gizmos.color = (moveImmediatly == true) ? Color.yellow : Color.red;
                direction = cameraPoints[0].position + cameraPoints[0].forward * lookLineLength;
                Gizmos.DrawLine(cameraPoints[0].position, direction);
            }
        }
        #endregion
    }
}