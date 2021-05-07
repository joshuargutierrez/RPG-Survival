using CBGames.Core;
using CBGames.UI;
using System.Collections;
using UnityEngine;

namespace CBGames.UI
{
    [AddComponentMenu("CB GAMES/Bonus/Set World Music")]
    public class SetWorldMusic : MonoBehaviour
    {
        [Tooltip("The music clip to use.")]
        [SerializeField] private AudioClip[] randomMusic = new AudioClip[] { };
        [Tooltip("The volume to set the audio source to.")]
        [SerializeField] private float volume = 0.5f;
        private UICoreLogic logic;

        /// <summary>
        /// Will play the set audio clip at the set volume only after there 
        /// is a found NetworkManager and UICoreLogic component attached to
        /// it in the scene. It will use the UICoreLogic functions to set 
        /// the audio clip on the UICoreLogic component to the volume and 
        /// audio clip to use.
        /// </summary>
        public void PlayWorldMusic()
        {
            StartCoroutine(WaitForLogic());
        }
        IEnumerator WaitForLogic()
        {
            if (randomMusic.Length < 1) yield return null;
            yield return new WaitUntil(() => NetworkManager.networkManager != null);
            yield return new WaitUntil(() => NetworkManager.networkManager.GetComponentInChildren<UICoreLogic>());
            logic = NetworkManager.networkManager.GetComponentInChildren<UICoreLogic>();
            Debug.Log(randomMusic[Random.Range(0, randomMusic.Length)]);
            logic.SetMusicAudio(randomMusic[Random.Range(0, randomMusic.Length)]);
            logic.SetMusicVolume(0);
            logic.SetFadeToVolume(volume);
            logic.FadeMusic(false);
            logic.PlayMusic();
        }
    }
}