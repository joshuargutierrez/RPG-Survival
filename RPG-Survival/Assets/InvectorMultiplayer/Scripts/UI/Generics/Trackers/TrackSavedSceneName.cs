using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Saved Scene Name")]
    public class TrackSavedSceneName : MonoBehaviour
    {
        [Tooltip("The Texts to manipulate their string values to display" +
            " the current Saved Scene Name from the UICoreLogic component")]
        [SerializeField] protected Text[] texts = new Text[] { };

        protected UICoreLogic logic;
        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Dynamically sets the `texts` to be whatever the current Save Scene Name
        /// from the `UICoreLogic` component.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            SetText(logic.GetSavedSceneToLoadName());
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