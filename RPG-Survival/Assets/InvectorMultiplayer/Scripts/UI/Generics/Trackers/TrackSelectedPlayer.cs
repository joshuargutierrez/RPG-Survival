using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Selected Players")]
    public class TrackSelectedPlayer : MonoBehaviour
    {
        [Tooltip("The Text values to manipulate to show your own currently selected player choice.")]
        [SerializeField] protected Text[] texts = new Text[] { };

        protected UICoreLogic logic;
        protected string playerName = "";
        
        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Dynamically sets the `texts` values to set the currently selected player name.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            SetText(logic.GetSetPlayer());
        }

        /// <summary>
        /// Used to set the `texts` values to be whatever the input value is
        /// </summary>
        /// <param name="inputText">string type, the string value to set to the `texts`</param>
        protected virtual void SetText(GameObject player)
        {
            if (player == null) return;
            playerName = player.name;

            foreach(Text text in texts)
            {
                text.text = playerName;
            }
        }
    }
}