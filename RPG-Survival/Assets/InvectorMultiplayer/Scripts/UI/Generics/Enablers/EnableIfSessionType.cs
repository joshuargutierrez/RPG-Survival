using CBGames.Core;
using Photon.Pun;
using UnityEngine;

namespace CBGames.UI
{ 
    public enum SessionType { Public, Private }

    [AddComponentMenu("CB GAMES/UI/Generics/Enablers/Enable If Session Type")]
    public class EnableIfSessionType : MonoBehaviour
    {
        [Tooltip("Enable these targets if you're the owner or not")]
        [SerializeField] protected bool isOwner = true;
        [Tooltip("What type of photon session this has to be for these targets to enable.")]
        [SerializeField] protected SessionType type = SessionType.Public;
        [Tooltip("The list of targets to enable or disable.")]
        [SerializeField] protected GameObject[] targets = new GameObject[] { };

        /// <summary>
        /// Will enable or disable objects by calling the `SetIsActive` function. Will only enable
        /// them if the photon session type and isOwner match.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (NetworkManager.networkManager && PhotonNetwork.InRoom == true && PhotonNetwork.IsMasterClient == isOwner)
            {
                switch(type)
                {
                    case SessionType.Private:
                        if ((string)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.RoomType] == RoomProperty.PrivateRoomType)
                        {
                            SetIsActive(true);
                        }
                        else
                        {
                            SetIsActive(false);
                        }
                        break;
                    case SessionType.Public:
                        if ((string)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.RoomType] == RoomProperty.PublicRoomType)
                        {
                            SetIsActive(true);
                        }
                        else
                        {
                            SetIsActive(false);
                        }
                        break;
                }
            }
            else
            {
                SetIsActive(false);
            }
        }

        /// <summary>
        /// Enable or disable all the targets based on the input value.
        /// </summary>
        /// <param name="isActive">bool type, active all the targets?</param>
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