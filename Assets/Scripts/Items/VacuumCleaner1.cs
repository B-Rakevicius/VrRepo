using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
namespace Items
{
    public class VacuumCleaner2 : MonoBehaviour
    {
        [Header("Vacuum Settings")]
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionRange = 10f;
        [SerializeField] private float suctionRadius = 3f;
        [SerializeField] private float suctionStrength = 20f;
        [SerializeField] private float orbCollectThreshold = 0.2f;
        [SerializeField] private LayerMask orbLayer;
        [Header("Cone Settings")]
        [SerializeField] private float coneAngle = 45f; // future upgrades increase cone angle?
        [SerializeField] private float startRadius = 0.05f; // if increase cone angle, me thinks this +0.01 for ease of use.
        [Header("Liquid Settings")]
        [SerializeField] private Material liquidMaterial;
        [SerializeField] private float emptyValue = 0.14f;
        [SerializeField] private float fullValue = -0.14f;
        private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");
        private HashSet<Rigidbody> affectedOrbs = new HashSet<Rigidbody>();
        private void Start()
        {
            PlayerManager.Instance.OnMoneyChanged += PlayerManager_MoneyChanged;
        }
        private void OnDestroy()
        {
            PlayerManager.Instance.OnMoneyChanged -= PlayerManager_MoneyChanged;
        }
        private void PlayerManager_MoneyChanged(object sender, PlayerManager.OnMoneyChangedEventArgs e)
        {
            // Update liquid shader visual
            // Remap Max money values
            float t = Mathf.InverseLerp(0, PlayerManager.Instance.MaxMoney, PlayerManager.Instance.CurrentMoney);
            float fillAmount = Mathf.Lerp(emptyValue, fullValue, t);
            liquidMaterial.SetFloat(FillAmount, fillAmount);
        }
        public void VacuumOrbs()
        {
            // First, check all currently affected orbs and restore gravity to those no longer in cone
            foreach (Rigidbody rb in affectedOrbs.ToList())
            {
                if (rb == null)
                {
                    affectedOrbs.Remove(rb);
                    continue;
                }

                if (!IsInCone(rb.position))
                {
                    rb.useGravity = true;
                    affectedOrbs.Remove(rb);
                }
            }

            // Now find new orbs to vacuum
            Collider[] colliders = Physics.OverlapSphere(suctionPoint.position, suctionRange, orbLayer);

            foreach (Collider collider in colliders)
            {
                Orb orb = collider.GetComponent<Orb>();
                if (orb is null) { continue; }
                if (!IsInCone(collider.transform.position))
                    continue;
                Rigidbody rb = collider.GetComponent<Rigidbody>();

                // If not already affected, add to set and disable gravity
                if (!affectedOrbs.Contains(rb))
                {
                    affectedOrbs.Add(rb);
                    rb.useGravity = false;
                }

                // Get the direction from orb to vacuum
                Vector3 direction = (suctionPoint.position - rb.position).normalized;
                direction += Vector3.up * 0.2f;
                direction.Normalize();
                float distance = Vector3.Distance(suctionPoint.position, rb.position);
                // Spiral movement
                Vector3 spiralDir = Vector3.Cross(direction, Vector3.up).normalized;
                float spiralStrength = Mathf.Lerp(0.1f, 0.03f, distance / suctionRange); // weaker when closer
                float pullSpeed = Mathf.Lerp(10f, 2f, distance / suctionRange);
                Vector3 velocity = (direction + spiralDir * spiralStrength).normalized * pullSpeed;
                rb.MovePosition(rb.position + velocity * Time.deltaTime);
                if (distance < orbCollectThreshold)
                {
                    if (PlayerManager.Instance.CurrentMoney >= PlayerManager.Instance.MaxMoney)
                    {
                        continue;
                    }
                    else
                    {
                        affectedOrbs.Remove(rb);
                        orb.Collect();
                    }
                }
            }
        }
        private bool IsInCone(Vector3 orbPosition)
        {
            Vector3 localPos = suctionPoint.InverseTransformPoint(orbPosition);
            // If behind the suction point, ignore
            if (localPos.z < 0) return false;
            // Calculate the radius at this distance based on cone shape
            float currentRadius = Mathf.Lerp(startRadius, Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * suctionRange, localPos.z / suctionRange);

            // Check if within the circular cross-section at this distance
            float distanceFromCenter = Mathf.Sqrt(localPos.x * localPos.x + localPos.y * localPos.y);

            return distanceFromCenter <= currentRadius && localPos.z <= suctionRange;
        }
        public void StopCleaner()
        {
            foreach (Rigidbody rb in affectedOrbs.ToList())
            {
                if (rb != null)
                    rb.useGravity = true;
            }
            affectedOrbs.Clear();
        }
        private void OnDrawGizmosSelected()
        {
            if (suctionPoint)
            {
                // Draw cone
                Gizmos.color = Color.cyan;
                float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * suctionRange;
                Vector3 startPoint = suctionPoint.position;
                Vector3 endPoint = suctionPoint.position + suctionPoint.forward * suctionRange;
                // Draw start circle
                DrawGizmoCircle(startPoint, suctionPoint.rotation, startRadius, 16);
                // Draw end circle
                DrawGizmoCircle(endPoint, suctionPoint.rotation, endRadius, 16);
                // Draw connecting lines between circles
                Vector3[] startCirclePoints = GetCirclePoints(startPoint, suctionPoint.rotation, startRadius, 8);
                Vector3[] endCirclePoints = GetCirclePoints(endPoint, suctionPoint.rotation, endRadius, 8);
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.DrawLine(startCirclePoints[i], endCirclePoints[i]);
                }
                // Draw wireframe box representing the cone volume
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                DrawConeWireframe();
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
            float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * suctionRange;
            Vector3 startPoint = suctionPoint.position;
            Vector3 endPoint = suctionPoint.position + suctionPoint.forward * suctionRange;
            Vector3 startUp = suctionPoint.up * startRadius;
            Vector3 startRight = suctionPoint.right * startRadius;
            Vector3 endUp = suctionPoint.up * endRadius;
            Vector3 endRight = suctionPoint.right * endRadius;
            Vector3[] startCorners = new Vector3[4];
            Vector3[] endCorners = new Vector3[4];
            startCorners[0] = startPoint + startUp + startRight;  // Top-right
            startCorners[1] = startPoint + startUp - startRight;  // Top-left
            startCorners[2] = startPoint - startUp - startRight;  // Bottom-left
            startCorners[3] = startPoint - startUp + startRight;  // Bottom-right
            endCorners[0] = endPoint + endUp + endRight;  // Top-right
            endCorners[1] = endPoint + endUp - endRight;  // Top-left
            endCorners[2] = endPoint - endUp - endRight;  // Bottom-left
            endCorners[3] = endPoint - endUp + endRight;  // Bottom-right
            // Connect start to end corners
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], endCorners[i]);
            }
            // Connect start corners
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], startCorners[(i + 1) % 4]);
            }
            // Connect end corners
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(endCorners[i], endCorners[(i + 1) % 4]);
            }
        }
    }
}