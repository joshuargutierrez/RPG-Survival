using Photon.Pun;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Enablers/Enable If All Players Ready")]
    public class EnableIfAllPlayersReady : MonoBehaviour
    {
        [Tooltip("Will only be enabled if you're also the MasterClient of the photon room.")]
        [SerializeField] protected bool mustBeOnwer = false;
        [Tooltip("The list of GameObjects to enable or disable.")]
        [SerializeField] protected GameObject[] targets = new GameObject[] { };
        protected UICoreLogic logic;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Will constantely set all the targets to be active based on the return 
        /// value from the `UICoreLogic`'s `AllPlayersReady` function. Calls
        /// `EnableTargets` to enable or disable all the targets
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (mustBeOnwer == true && PhotonNetwork.IsMasterClient != mustBeOnwer)
            {
                EnableTargets(false);
            }
            else
            {
                EnableTargets(logic.AllPlayersReady());
            }
        }

        /// <summary>
        /// Enable or disable all the `targets` based on the input value.
        /// </summary>
        /// <param name="isEnabled"></param>
        protected virtual void EnableTargets(bool isEnabled)
        {
            foreach(GameObject target in targets)
            {
                if (target.activeInHierarchy != isEnabled)
                {
                    target.SetActive(isEnabled);
                }
            }
        }
    }
}