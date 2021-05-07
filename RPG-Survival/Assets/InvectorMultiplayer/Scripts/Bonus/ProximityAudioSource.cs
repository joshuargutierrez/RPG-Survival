using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Bonus/Audio On Proximity Source")]
    public class ProximityAudioSource : MonoBehaviour
    {
        [Tooltip("The source where the sound will be played from.")]
        public AudioSource source;
        [Tooltip("Can this audio source play sound?")]
        public bool canPlay = false;
        [Tooltip("How fast to fade in/out music. Higher values = Faster")]
        public float fadeSpeed = 0.25f;
        [Tooltip("Not modifiable in the inspector. This is modified by other scripts setting its starting volume.")]
        [HideInInspector] public float setVolume = 0;
        [Tooltip("Not modifiable in the inspector. If you want to start fading in or out the sound. True = Fade in, False = Fade out")]
        [HideInInspector] public bool fadeIn = false;

        /// <summary>
        /// Automatically sets the AudioSource to loop its sound.
        /// </summary>
        private void Start()
        {
            source.loop = true;
        }

        /// <summary>
        /// Will fade in the sound if the setVolume is greater than zero and 
        /// it is allowed to play. Will also fade out the sound if the setVolume 
        /// is lower than the current volume and is allowed to player. Otherwise 
        /// if it's zero it will stop the sound.
        /// </summary>
        private void Update()
        {
            if (canPlay == false) return;
            if (fadeIn == true && source.volume < setVolume)
            {
                source.volume += Time.deltaTime * fadeSpeed;
                if (source.volume > setVolume)
                {
                    source.volume = setVolume;
                }
            }
            else if (source.volume > setVolume)
            {
                source.volume -= Time.deltaTime * fadeSpeed;
                if (source.volume < setVolume)
                {
                    source.volume = setVolume;
                }
            }
            if (setVolume > 0 && source.isPlaying == false)
            {
                source.Play();
            }
            if (source.volume == 0)
            {
                source.clip = null;
                source.Stop();
            }
        }
    }
}