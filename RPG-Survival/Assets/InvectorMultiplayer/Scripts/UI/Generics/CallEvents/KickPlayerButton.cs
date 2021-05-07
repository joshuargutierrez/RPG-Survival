using Photon.Pun;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Call Events/Kick Player")]
    public class KickPlayerButton : MonoBehaviour
    {
        [Tooltip("The object that holds all the player data that can be referenced by this component.")]
        [SerializeField] private PlayerListObject playerInfo = null;
        
        protected UICoreLogic logic;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();    
        }

        /// <summary>
        /// This is only callable if you're the MasterClient. Gets the player data from the 
        /// `playerInfo` and calls the `KickPlayer` function from the `UICoreLogic`.
        /// </summary>
        public virtual void KickPlayer()
        {
            if (!playerInfo) return;
            if (!string.IsNullOrEmpty(playerInfo.userId) && PhotonNetwork.IsMasterClient == true)
            {
                logic.KickPlayer(playerInfo.userId);
            }
        }
    }
}