using UnityEngine;

namespace Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        [Tooltip("What sound clip to play?")]
        [SerializeField] private AudioClip soundClip;
        
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            _audioSource.spatialize = true;
            _audioSource.spatialBlend = 1;
        }

        public void PlaySound(float volume = 1.0f)
        {
            _audioSource.PlayOneShot(soundClip, volume);
        }
    }
}
