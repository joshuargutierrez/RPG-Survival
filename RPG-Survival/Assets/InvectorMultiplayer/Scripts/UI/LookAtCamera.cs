using CBGames.Core;
using Invector.vCharacterController;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/Camera/Look At Camera")]
    public class LookAtCamera : MonoBehaviour
    {
        protected Transform head_bone = null;
        /// <summary>
        /// Just always look at the main camera. If it's unable to find a suitable camera
        /// in the scene then it will find the owner player and origent to look at his 
        /// head bone. This is to account for when the main camera is deactivated when 
        /// the owning player is looking through a scope.
        /// </summary>
        protected virtual void Update()
        {
            if (Camera.main)
            {
                transform.LookAt(Camera.main.transform);
            }
            else if (!head_bone)
            {
                vThirdPersonController controller = NetworkManager.networkManager.GetYourPlayer();
                head_bone = controller.gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            }
            else
            {
                transform.LookAt(head_bone);
            }
        }
    }
}
