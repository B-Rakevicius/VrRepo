using UnityEngine;

/// <summary>
/// Component that keeps loot hovering above the ground
/// </summary>
public class GroundHover : MonoBehaviour
{
    public float minHeight = 0.3f;
    public LayerMask groundLayer;
    public float hoverForce = 10f;
    public float hoverDamping = 5f;
    private Rigidbody rb;
    private bool hasSettled = false;
    private float settleTimer = 0f;
    private float settleTime = 2f;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (rb == null) return;
        settleTimer += Time.fixedDeltaTime;
        if (settleTimer < settleTime) return;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, minHeight + 1f, groundLayer))
        {
            float currentHeight = hit.distance;
            float heightDifference = minHeight - currentHeight;
            if (!hasSettled && rb.linearVelocity.magnitude < 0.5f)
            {
                // Once mostly stopped, disable gravity and switch to kinematic hovering
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                hasSettled = true;
                // Position it exactly at min height
                Vector3 targetPosition = hit.point + Vector3.up * minHeight;
                transform.position = targetPosition;
            }
            else if (hasSettled)
            {
                // Keep it locked at the hover height
                Vector3 targetPosition = hit.point + Vector3.up * minHeight;
                transform.position = targetPosition;
            }
            else if (currentHeight < minHeight)
            {
                // Apply upward force to maintain hover height while still falling
                float forceAmount = heightDifference * hoverForce;
                rb.AddForce(Vector3.up * forceAmount, ForceMode.Force);
                // Dampen downward velocity
                if (rb.linearVelocity.y < 0)
                {
                    Vector3 dampingForce = -rb.linearVelocity.y * Vector3.up * hoverDamping;
                    rb.AddForce(dampingForce, ForceMode.Force);
                }
            }
        }
    }
}