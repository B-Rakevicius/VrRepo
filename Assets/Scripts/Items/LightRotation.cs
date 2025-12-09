using System;
using UnityEngine;

public class LightRotation : MonoBehaviour
{
    private Light _light;

    [Tooltip("Light's rotation speed")]
    [SerializeField] private float speed = 5f;
    [Tooltip("How long should the light be active for?")]
    [SerializeField] private float activeDuration = 8.5f;

    private float m_activeDuration; // Temp value, acts as a timer.
    private bool m_isActive;        // Is the light active
    

    private void Awake()
    {
        _light = GetComponent<Light>();
        StopRotateLight();
    }

    private void Start()
    {
        GameManager.Instance.OnRoundStarted += GameManager_OnRoundStarted;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoundStarted -= GameManager_OnRoundStarted;
    }

    private void GameManager_OnRoundStarted(object sender, EventArgs e)
    {
        StartRotateLight();
    }

    private void Update()
    {
        if (!m_isActive) { return; }
        
        // Start rotating the light for activeDuration
        if (m_activeDuration > 0)
        {
            transform.RotateAround(transform.position, transform.right, speed * 100f * Time.deltaTime);
            m_activeDuration -= Time.deltaTime;
        }
        else
        {
            StopRotateLight();
        }

    }

    /// <summary>
    /// Activates siren light rotation. Light rotates for set duration, defined on its component.
    /// </summary>
    private void StartRotateLight()
    {
        m_isActive = true;
        _light.enabled = true;
        m_activeDuration = activeDuration;
    }

    /// <summary>
    /// Stops siren light rotation.
    /// </summary>
    private void StopRotateLight()
    {
        m_isActive = false;
        _light.enabled = false;
    }
}
