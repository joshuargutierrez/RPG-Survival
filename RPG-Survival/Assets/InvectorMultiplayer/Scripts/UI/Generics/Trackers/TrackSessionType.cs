using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{ 
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Session Type")]
    public class TrackSessionType : MonoBehaviour
    {
        [Tooltip("The Text values to manipulate to display what session type your currently in")]
        [SerializeField] protected Text[] texts = new Text[] { };

        /// <summary>
        /// Dynamically sets all the `texts` values to display what type of session
        /// your currently in. Public/Private
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                SetText((string)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.RoomType]);
            }
            else
            {
                SetText("");
            }
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