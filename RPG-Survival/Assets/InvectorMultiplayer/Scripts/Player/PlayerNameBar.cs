using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.Player
{
    [RequireComponent(typeof(PhotonView))]
    [DisallowMultipleComponent]
    public class PlayerNameBar : MonoBehaviour
    {
        [Tooltip("The text to modify with the Network Nickname.")]
        public Text playerName;
        [Tooltip("The holder object for the player name bar. Will disable this if not a network version of this player.")]
        public GameObject playerBar;

        /// <summary>
        /// Removes the namebar if you're the owner player. Also sets the
        /// name on your networked versions via `SetPlayerName` function.
        /// Sets this player's name on the name bar based on the owner id
        /// if the photon view. Will find the player that owns this PhotonView
        /// and attach that players Nickname to this name bar.
        /// </summary>
        public virtual void Start()
        {
            if (GetComponent<PhotonView>().IsMine == true)
            {
                Destroy(playerBar);
            }
            else
            {
                int thisNumber = GetComponent<PhotonView>().OwnerActorNr;
                List<Photon.Realtime.Player> playerList = PhotonNetwork.PlayerList.ToList<Photon.Realtime.Player>();
                SetPlayerName(playerList.Find(x => x.ActorNumber == thisNumber).NickName);
            }
        }

        /// <summary>
        /// Sets the name shown on the name bar to whatever is passed
        /// in via the input.
        /// </summary>
        /// <param name="nameText">string type, the input name</param>
        public virtual void SetPlayerName(string nameText)
        {
            playerName.text = nameText;
        }
    }
}
