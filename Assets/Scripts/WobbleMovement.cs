using System;
using UnityEngine;

[ExecuteInEditMode]
public class WobbleMovement : MonoBehaviour
{
    // Shader material
    [SerializeField] Material material;
    
    // Sums of velocities
    float wobbleAmountX;
    float wobbleAmountZ;

    private float sinWobbleX;
    private float sinWobbleZ;
    
    // Wobble limits
    [SerializeField] float wobbleLimit = 0.05f;
    
    [SerializeField] float wobbleDecaySpeed = 5f;

    [SerializeField] private float wobbleWaveSpeed = 5f;
    
    // Rotation velocity
    private Vector3 rotationDelta; // Difference between previous and current rotation
    private Vector3 rotationLast; // Last rotation values
    
    // Movement velocity
    private Vector3 velocityDelta;
    private Vector3 velocityLast;

    void Start()
    {
        rotationLast = transform.rotation.eulerAngles;
        velocityLast = transform.position;
    }
    
    void Update()
    {
        // Lerp to 0 over time
        wobbleAmountX = Mathf.Lerp(wobbleAmountX, 0, Time.deltaTime * wobbleDecaySpeed);
        wobbleAmountZ = Mathf.Lerp(wobbleAmountZ, 0, Time.deltaTime * wobbleDecaySpeed);
        
        // Make a sine wave out of wobbleAmounts
        float sinWobbleX = wobbleAmountX * Mathf.Sin(wobbleWaveSpeed * Time.time);
        float sinWobbleZ = wobbleAmountZ * Mathf.Sin(wobbleWaveSpeed * Time.time);
        
        material.SetFloat("_WobbleX", sinWobbleZ);
        material.SetFloat("_WobbleZ", sinWobbleX);
        
        // Sum velocity and rotation, but clamp it so it wouldn't reach high values
        wobbleAmountX += Mathf.Clamp(velocityDelta.x + rotationDelta.z, -wobbleLimit, wobbleLimit);
        wobbleAmountZ += Mathf.Clamp(velocityDelta.z + rotationDelta.x, -wobbleLimit, wobbleLimit);

        
        // VELOCITY CALCULATION
        // Set difference and last rotation to current rotation
        rotationDelta = transform.rotation.eulerAngles - rotationLast;
        rotationLast = transform.rotation.eulerAngles;
        
        // Set difference and last position to current position
        velocityDelta = (velocityLast - transform.position) / Time.deltaTime;
        velocityLast = transform.position;
    }
}
