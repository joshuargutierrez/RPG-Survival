using Invector.vCharacterController;
using Photon.Pun;
using System;
using UnityEngine;

namespace CBGames.Objects
{
    [AddComponentMenu("CB GAMES/Bonus/Audio On Proximity")]
    public class AudioOnProximity : MonoBehaviour
    {
        [Serializable]
        class DistanceVolume
        {
            public float distance = 0.0f;
            public float volume = 0.0f;
        }

        [Tooltip("At what distance should the sound source volume be set to?")]
        [SerializeField] private DistanceVolume[] distances = new DistanceVolume[] { };
        [Tooltip("The audio clip to fade in/out based on distance to the target.")]
        [SerializeField] private AudioClip distanceClip = null;

        private float setVolume = 0.0f;
        private float _prevVolume = 0.0f;
        vThirdPersonController owningPlayer = null;
        ProximityAudioSource prxSrc;

        private void Start()
        {
            prxSrc = FindObjectOfType<ProximityAudioSource>();
            if (GetComponent<PhotonView>().IsMine == true) enabled = false;
        }

        /// <summary>
        /// Will keep track of the target to the source. If it falls within the particular
        /// distances Array it will fade in the volume with the selected audio clip.
        /// </summary>
        private void Update()
        {
            if (owningPlayer == null)
            {
                GetOwningPlayer();
                return;
            }
            for (int i = 0; i < distances.Length; i++)
            {
                if (Vector3.Distance(owningPlayer.transform.position, transform.position) <= distances[i].distance)
                {
                    setVolume = distances[i].volume;
                }
                else if (i == 0 && Vector3.Distance(owningPlayer.transform.position, transform.position) > distances[i].distance)
                {
                    setVolume = 0;
                    break;
                }
            }
            if (_prevVolume != setVolume)
            {
                _prevVolume = setVolume;
                if (setVolume == 0)
                {
                    prxSrc.setVolume = 0;
                    prxSrc.fadeIn = false;
                }
                else
                {
                    if (prxSrc.setVolume != setVolume)
                    {
                        prxSrc.setVolume = setVolume;
                        if (prxSrc.source.clip == null)
                        {
                            prxSrc.source.clip = distanceClip;
                        }
                        prxSrc.fadeIn = true;
                    }
                }
            }
        }
        void GetOwningPlayer()
        {
            foreach (vThirdPersonController player in FindObjectsOfType<vThirdPersonController>())
            {
                if (player.transform.GetComponent<PhotonView>().IsMine == true)
                {
                    owningPlayer = player;
                    break;
                }
            }
        }
    }
}