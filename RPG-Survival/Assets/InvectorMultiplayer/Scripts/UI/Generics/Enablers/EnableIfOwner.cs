using CBGames.Core;
using Photon.Pun;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Enablers/Enable If Owner")]
    public class EnableIfOwner : MonoBehaviour
    {
        [Tooltip("The `targets` will only enable if you're the MasterClient or not.")]
        [SerializeField] protected bool isOwner = true;
        [Tooltip("The list of gameobjects to enable or not if you're the MasterClient.")]
        [SerializeField] protected GameObject[] targets = new GameObject[] { };

        /// <summary>
        /// Enable or disables the targets based on if your the MasterClient, in a photon room, and have 
        /// a `NetworkManager`. This is done by calling the `SetIsActive` function.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (NetworkManager.networkManager && PhotonNetwork.InRoom == true && PhotonNetwork.IsMasterClient == isOwner)
            {
                SetIsActive(true);
            }
            else
            {
                SetIsActive(false);
            }
        }

        /// <summary>
        /// Sets all the `targets` to be active or not based on the input value.
        /// </summary>
        /// <param name="isActive">bool type, enable all the targets?</param>
        protected virtual void SetIsActive(bool isActive)
        {
            foreach (GameObject target in targets)
            {
                if (target.activeInHierarchy != isActive)
                {
                    target.SetActive(isActive);
                }
            }
        }
    }
}