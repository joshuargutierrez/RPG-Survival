using CBGames.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Enablers/Enable If Chat Connected")]
    public class EnableIfChatConnected : MonoBehaviour
    {
        [Tooltip("List of buttons to enable or disable.")]
        [SerializeField] protected Button[] buttons = new Button[] { };
        [Tooltip("List of GameObjects to enable or disable.")]
        [SerializeField] protected GameObject[] gameObjects = new GameObject[] { };
        [Tooltip("False = perform normal, True = Will enable `buttons` and `gameObjects` if the chat is NOT enabled.")]
        [SerializeField] protected bool invertActions = false;
        protected bool _isenabled = false;

        /// <summary>
        /// Will enable the buttons or gameobjects if you're connected/not connected
        /// to the ChatBox data channel. Actions are opposite if `invertActions` is true.
        /// Objects are enabled/disabled by calling `EnableTargets` function.
        /// </summary>
        protected virtual void Update()
        {
            if (_isenabled == false)
            {
                if (NetworkManager.networkManager && NetworkManager.networkManager.GetChabox() &&
                    NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel())
                {
                    _isenabled = true;
                    EnableTargets((invertActions == true) ? false : true);
                }
            }
            else if (NetworkManager.networkManager.GetChabox().IsConnectedToDataChannel() == false)
            {
                _isenabled = false;
                EnableTargets((invertActions == true) ? true : false);
            }
        }

        /// <summary>
        /// Will enable or disable all of the `buttons` and `gameObjects` based on
        /// the input value.
        /// </summary>
        /// <param name="isEnabled">bool type, enable the targets?</param>
        protected virtual void EnableTargets(bool isEnabled)
        {
            foreach(Button button in buttons)
            {
                button.interactable = isEnabled;
            }
            foreach(GameObject go in gameObjects)
            {
                go.SetActive(isEnabled);
            }
        }
    }
}