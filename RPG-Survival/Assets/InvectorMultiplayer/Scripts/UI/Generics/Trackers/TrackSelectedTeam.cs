using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Selected Team")]
    public class TrackSelectedTeam : MonoBehaviour
    {
        [Tooltip("The Text values to manipulate to show what team you're currently on.")]
        [SerializeField] protected Text[] texts = new Text[] { };
        protected UICoreLogic logic;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();    
        }

        /// <summary>
        /// Dynamically update all the `texts` values to display what team you're currently on.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            SetText(logic.GetMyTeamName());
        }

        /// <summary>
        /// Used to set the `texts` values to be whatever the input value is
        /// </summary>
        /// <param name="inputText">string type, the string value to set to the `texts`</param>
        protected virtual void SetText(string inputText)
        {
            foreach(Text text in texts)
            {
                text.text = inputText;
            }
        }
    }
}