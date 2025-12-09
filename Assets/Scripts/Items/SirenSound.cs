using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SirenSound : MonoBehaviour
{
    [Tooltip("What sound should siren play?")]
    [SerializeField] private AudioClip audioClip;
    [Tooltip("How long should the sound be active for?")]
    [SerializeField] private float activeDuration = 8.5f;

    private float m_activeTill;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        _audioSource.spatialize = true;
        _audioSource.spatialBlend = 1f;
    }
    
    private void Start()
    {
        GameManager.Instance.OnRoundStarted += GameManager_OnRoundStarted;
    }

    private void GameManager_OnRoundStarted(object sender, EventArgs e)
    {
        PlaySirenSound();
    }

    private async void PlaySirenSound()
    {
        _audioSource.PlayOneShot(audioClip, 10f);
        
        m_activeTill = Time.time + activeDuration;
        
        while (m_activeTill > Time.time)
        {
            await Task.Yield();
        }
        
        _audioSource.Stop();
    }
}
