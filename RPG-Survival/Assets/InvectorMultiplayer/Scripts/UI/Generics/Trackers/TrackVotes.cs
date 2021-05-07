using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Votes")]
    public class TrackVotes : MonoBehaviour
    {
        [Tooltip("The index of the `sceneVotes` variable to track.")]
        [SerializeField] protected int indexNumberToTrack = 0;
        [Tooltip("The Text values to manipulate to show how many votes at the selected index there currently are.")]
        [SerializeField] protected Text[] texts = new Text[] { };

        protected UICoreLogic logic;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Dynamically sets the `texts` values to be what the current number of votes there 
        /// are at the selected `indexNumberToTrack` index.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            SetText(logic.GetSceneVotes(indexNumberToTrack).ToString());
        }

        /// <summary>
        /// Used to set the `texts` values to be whatever the input value is
        /// </summary>
        /// <param name="inputText">string type, the string value to set to the `texts`</param>
        protected virtual void SetText(string inputText)
        {
            foreach (Text text in texts)
            {
                text.text = inputText;
            }
        }
    }
}