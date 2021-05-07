using UnityEngine;
using UnityEditor;
using CBGames.Objects;
using Invector.vCamera;
using CBGames.Core;

namespace CBGames.Editors
{
    public class E_PreviewCamera : EditorWindow
    {
        [MenuItem("CB Games/Camera/Add Preview Camera", false, 0)]
        public static void CB_PreviewCamComponent()
        {
            if (FindObjectOfType<PreviewCamera>())
            {
                if (EditorUtility.DisplayDialog("Duplicate PreviewCamera", "There is already a PreviewCamera component in the scene this could cause issues with adding a camera point from the menu. Do you really want to add another \"PreviewCamera\" component to this scene?",
                                "Yes", "No"))
                {
                    AddPreviewComp();
                }
                else
                {
                    Debug.Log("Skipping adding preview component to scene.");
                }
            }
            else
            {
                AddPreviewComp();
            }

            void AddPreviewComp()
            {
                GameObject previewObj = new GameObject("CameraPoints");
                previewObj.AddComponent<PreviewCamera>();
                if (FindObjectOfType<vThirdPersonCamera>())
                {
                    previewObj.GetComponent<PreviewCamera>().targetCam = FindObjectOfType<vThirdPersonCamera>().transform;
                }
                if (FindObjectOfType<NetworkManager>())
                {
                    previewObj.GetComponent<PreviewCamera>().networkManager = FindObjectOfType<NetworkManager>();
                }
            }
        }


        [MenuItem("CB Games/Camera/Add Camera Point", false, 0)]
        public static void CB_PreviewCamPoint()
        {
            if (!FindObjectOfType<PreviewCamera>())
            {
                if (EditorUtility.DisplayDialog("Missing \"PreviewCamera\" Component!", "Unable to find the \"PreviewCamera\" component in the scene. Please run \"CB Games/Add/Camera/Preview Camera Component\" before running this.",
                                "Okay"))
                {
                }
            }
            else
            {
                PreviewCamera cam = FindObjectOfType<PreviewCamera>();
                GameObject point = new GameObject("CameraPoint");
                E_Helpers.SetObjectIcon(point, E_Core.h_cameraPath);
                point.transform.SetParent(cam.transform);
                cam.cameraPoints.Add(point.transform);
                Debug.Log("Successfully added camera point to the scene.");
            }
        }
    }
}