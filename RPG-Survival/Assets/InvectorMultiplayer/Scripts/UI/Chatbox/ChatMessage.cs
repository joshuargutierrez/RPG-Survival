using UnityEngine;
using UnityEngine.UI;
using CBGames.Core;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Chatbox/Chat Message")]
    public class ChatMessage : MonoBehaviour
    {
        [Tooltip("A Text value to display the players name.")]
        [SerializeField] private Text playerName = null;
        [Tooltip("A Text value to display the players message.")]
        [SerializeField] private Text message = null;

        private string userId = null;

        /// <summary>
        /// Sets the `playerName` and `message` text values based on the
        /// values in the `SentChatMessage` object.
        /// </summary>
        /// <param name="incoming">SentChatMessage type, this contains the player name and message</param>
        public virtual void SetMessage(SentChatMessage incoming)
        {
            userId = incoming.playerName;
            playerName.text = incoming.playerName.Split(':')[0];
            message.text = incoming.message;
        }
    }
}