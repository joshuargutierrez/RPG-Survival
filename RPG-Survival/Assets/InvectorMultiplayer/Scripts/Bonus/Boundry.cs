using Invector.vCharacterController;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Bonus/Level Boundary")]
    public class Boundry : MonoBehaviour
    {
        [Tooltip("If you left the boundary box it will reset your characters position to this point.")]
        public Transform resetPoint;

        /// <summary>
        /// If you leave the boundary box it will reset that vThirdPersonController's
        /// position to the 'resetPoint' position.
        /// </summary>
        /// <param name="other">Collider type, checks to make sure it only reacts to vThirdPersonController's.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.transform.GetComponentInParent<vThirdPersonController>())
            {
                other.transform.GetComponentInParent<vThirdPersonController>().transform.position = resetPoint.position;
            }
            else
            {
                Destroy(other.gameObject);
            }

        }
    }
}