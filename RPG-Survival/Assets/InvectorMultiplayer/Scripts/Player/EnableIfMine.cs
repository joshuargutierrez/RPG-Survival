using Photon.Pun;
using UnityEngine;

namespace CBGames.Player
{
    [AddComponentMenu("CB GAMES/Player/Enable Objects If Mine")]
    public class EnableIfMine : MonoBehaviour
    {
        [Tooltip("The list of gameobjects to enable if you are the " +
            "owner player or not.")]
        [SerializeField] protected GameObject[] targets = new GameObject[] { };

        /// <summary>
        /// Enables/Disables the list of `targets` if you're the owner
        /// or not. This is done by calling the `EnableTargets` 
        /// function.
        /// </summary>
        protected virtual void Start()
        {
            if (GetComponent<PhotonView>())
            {
                EnableTargets(GetComponent<PhotonView>().IsMine);
            }
            else if (GetComponentInChildren<PhotonView>())
            {
                EnableTargets(GetComponentInChildren<PhotonView>().IsMine);
            }
        }

        /// <summary>
        /// Loops through the `targets` and disables/enables them
        /// based on the input value.
        /// </summary>
        /// <param name="isEnabled">bool type, enable or disable the targets?</param>
        protected virtual void EnableTargets(bool isEnabled)
        {
            foreach (GameObject target in targets)
            {
                target.SetActive(isEnabled);
            }
        }
    }
}