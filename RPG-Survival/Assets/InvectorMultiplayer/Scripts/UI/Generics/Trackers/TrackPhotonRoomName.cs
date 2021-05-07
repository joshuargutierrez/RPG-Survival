using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/UI/Generics/Trackers/Track Photon Room Name")]
    public class TrackPhotonRoomName : MonoBehaviour
    {
        [Tooltip("List of Text values to set.")]
        [SerializeField] protected Text[] texts = new Text[] { };

        /// <summary>
        /// Set the text values according to the current photon room name dynamically.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                SetText(PhotonNetwork.CurrentRoom.Name);
            }
            else
            {
                SetText("");
            }
        }

        /// <summary>
        /// Set the `texts` values according to the input value.
        /// </summary>
        /// <param name="inputText">string type, the text string to set</param>
        protected virtual void SetText(string inputText)
        {
            foreach (Text text in texts)
            {
                text.text = inputText;
            }
        }
    }
}