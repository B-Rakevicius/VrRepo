using System.Collections.Generic;
using UnityEngine;
namespace Items
{
    public class CrossBuh : MonoBehaviour
    {
        [Header("Crossbow Settings")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float shootForce = 20f;
        [SerializeField] private float shootRange = 10f;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private int maxArrows = 10;
        [SerializeField] private float arrowLifetime = 5f;
        [Header("Cone Settings")]
        [SerializeField] private float coneAngle = 15f;
        [SerializeField] private float startRadius = 0.05f;
        private float nextFireTime;
        private Queue<GameObject> activeArrows = new Queue<GameObject>();
        public void Shoot()
        {
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + 1f / fireRate;
            // Calculate random direction within cone
            Vector3 shootDirection = GetRandomDirectionInCone();
            // Instantiate arrow
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
            // Add arrow to tracking queue
            activeArrows.Enqueue(arrow);
            // Limit number of active arrows
            if (activeArrows.Count > maxArrows)
            {
                GameObject oldArrow = activeArrows.Dequeue();
                if (oldArrow != null)
                    Destroy(oldArrow);
            }
            // Set up arrow physics
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            }
            // Set up arrow destruction
            Destroy(arrow, arrowLifetime);
        }
        private Vector3 GetRandomDirectionInCone()
        {
            // Start with forward direction
            Vector3 direction = shootPoint.forward;
            // Apply random rotation within cone angle
            float randomAngle = Random.Range(0f, coneAngle * 0.5f);
            Vector3 randomAxis = Random.onUnitSphere;
            // Ensure the random axis is perpendicular to forward direction for more natural spread
            randomAxis = Vector3.Cross(randomAxis, direction).normalized;
            if (randomAxis.magnitude < 0.1f)
                randomAxis = Vector3.Cross(Vector3.up, direction).normalized;

            // Rotate the direction by random angle
            direction = Quaternion.AngleAxis(randomAngle, randomAxis) * direction;

            return direction.normalized;
        }
        private void OnDrawGizmosSelected()
        {
            if (shootPoint)
            {
                // Draw cone
                Gizmos.color = Color.red;
                float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * shootRange;
                Vector3 startPoint = shootPoint.position;
                Vector3 endPoint = shootPoint.position + shootPoint.forward * shootRange;
                // Draw start circle
                DrawGizmoCircle(startPoint, shootPoint.rotation, startRadius, 16);
                // Draw end circle
                DrawGizmoCircle(endPoint, shootPoint.rotation, endRadius, 16);
                // Draw connecting lines between circles
                Vector3[] startCirclePoints = GetCirclePoints(startPoint, shootPoint.rotation, startRadius, 8);
                Vector3[] endCirclePoints = GetCirclePoints(endPoint, shootPoint.rotation, endRadius, 8);
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.DrawLine(startCirclePoints[i], endCirclePoints[i]);
                }
                // Draw wireframe box representing the cone volume
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                DrawConeWireframe();
                Gizmos.color = Color.yellow;
                for (int i = 0; i < 10; i++)
                {
                    Vector3 sampleDir = GetRandomDirectionInCone();
                    Gizmos.DrawRay(shootPoint.position, sampleDir * shootRange);
                }
            }
        }
        private Vector3[] GetCirclePoints(Vector3 center, Quaternion rotation, float radius, int segments)
        {
            Vector3[] points = new Vector3[segments];
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            float angleIncrement = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleIncrement * Mathf.Deg2Rad;
                points[i] = center + (up * Mathf.Sin(angle) + right * Mathf.Cos(angle)) * radius;
            }

            return points;
        }
        private void DrawGizmoCircle(Vector3 center, Quaternion rotation, float radius, int segments)
        {
            Vector3[] points = GetCirclePoints(center, rotation, radius, segments);

            for (int i = 0; i < segments; i++)
            {
                int nextIndex = (i + 1) % segments;
                Gizmos.DrawLine(points[i], points[nextIndex]);
            }
        }
        private void DrawConeWireframe()
        {
            float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * shootRange;
            Vector3 startPoint = shootPoint.position;
            Vector3 endPoint = shootPoint.position + shootPoint.forward * shootRange;
            Vector3 startUp = shootPoint.up * startRadius;
            Vector3 startRight = shootPoint.right * startRadius;
            Vector3 endUp = shootPoint.up * endRadius;
            Vector3 endRight = shootPoint.right * endRadius;
            Vector3[] startCorners = new Vector3[4];
            Vector3[] endCorners = new Vector3[4];
            startCorners[0] = startPoint + startUp + startRight;
            startCorners[1] = startPoint + startUp - startRight;
            startCorners[2] = startPoint - startUp - startRight;
            startCorners[3] = startPoint - startUp + startRight;
            endCorners[0] = endPoint + endUp + endRight;
            endCorners[1] = endPoint + endUp - endRight;
            endCorners[2] = endPoint - endUp - endRight;
            endCorners[3] = endPoint - endUp + endRight;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], endCorners[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], startCorners[(i + 1) % 4]);
            }
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(endCorners[i], endCorners[(i + 1) % 4]);
            }
        }
    }
}