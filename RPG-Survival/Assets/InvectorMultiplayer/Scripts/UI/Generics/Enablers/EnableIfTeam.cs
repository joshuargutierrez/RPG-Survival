using CBGames.Core;
using Photon.Pun;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Enablers/Enable If On Team")]
    public class EnableIfTeam : MonoBehaviour
    {
        public enum ChangeType { ForEveryone, IfOwner, IfNotOwner }

        [Tooltip("ForEveryone=Does the NetworkManager teamName variable match this teamName variable?\n" +
            "IfOwner=Are you the master client and does the NetworkManager teamName variable match this teamName variable?\n" +
            "IfNotOwner=Are you NOT the master client and does the NetworkManager teamName variable match this teamName variable?")]
        [SerializeField] protected ChangeType checkType = ChangeType.ForEveryone;
        [Tooltip("Enable the selected items if you are on the team name.")]
        [SerializeField] protected string teamName = "";
        [Tooltip("List of items to enable or disable.")]
        [SerializeField] protected GameObject[] items = new GameObject[] { };

        /// <summary>
        /// Enable or disable the objects dynamically if the specified settings match.
        /// </summary>
        protected virtual void Update()
        {
            switch (checkType)
            {
                case ChangeType.ForEveryone:
                    EnableItems(NetworkManager.networkManager.teamName == teamName);
                    break;
                case ChangeType.IfNotOwner:
                    if (PhotonNetwork.IsMasterClient == false)
                    {
                        EnableItems(NetworkManager.networkManager.teamName == teamName);
                    }
                    break;
                case ChangeType.IfOwner:
                    if (PhotonNetwork.IsMasterClient == true)
                    {
                        EnableItems(NetworkManager.networkManager.teamName == teamName);
                    }
                    break;
            }
        }

        /// <summary>
        /// Enable all the items or disable all the items based on the input value.
        /// </summary>
        /// <param name="isEnabled">bool type, enable all the items?</param>
        protected virtual void EnableItems(bool isEnabled)
        {
            foreach (GameObject item in items)
            {
                item.SetActive(isEnabled);
            }
        }
    }
}