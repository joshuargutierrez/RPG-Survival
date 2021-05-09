using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Random Scene List")]
    public class TrackRandomSceneList : MonoBehaviour
    {
        [Tooltip("The index of the `_randomRoomList` on the UICoreLogic to track")]
        [SerializeField] protected int indexNumberToTrack = 0;
        [Tooltip("The images to manipulate.")]
        [SerializeField] protected Image[] images = new Image[] { };
        [Tooltip("The Text values to manipulate.")]
        [SerializeField] protected Text[] texts = new Text[] { };
        protected UICoreLogic logic;
        protected SceneOption option;

        protected virtual void Start()
        {
            logic = FindObjectOfType<UICoreLogic>();
        }

        /// <summary>
        /// Sets the `images` and the `texts` to whatever the current 
        /// `indexNumberToTrack` integer is set to. Will get that info
        /// from the `UICoreLogic`'s `_randomRoomList` variable.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            option = logic.GetRandomSceneNumber(indexNumberToTrack);
            SetText(option.sceneName);
            SetSprite(option.sceneSprite);
        }

        /// <summary>
        /// Used to set the `texts` values to be whatever the input value is
        /// </summary>
        /// <param name="inputText">string type, the string value to set to the `texts`</param>
        protected virtual void SetText(string inputText)
        {
            if (string.IsNullOrEmpty(inputText)) return;
            foreach (Text text in texts)
            {
                text.text = inputText;
            }
        }

        /// <summary>
        /// Sets the input Sprite image value into all the `images`
        /// </summary>
        /// <param name="newImage">Sprite type, the image to set all the `images` to</param>
        protected virtual void SetSprite(Sprite newImage)
        {
            if (newImage == null) return;
            foreach (Image image in images)
            {
                image.sprite = newImage;
            }
        }
    }
}