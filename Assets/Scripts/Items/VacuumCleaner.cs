using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items
{
    public class VacuumCleaner : MonoBehaviour
    {
        [Header("Vacuum Settings")] 
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionRange = 10f;
        [SerializeField] private float suctionRadius = 3f;
        [SerializeField] private float suctionStrength = 20f;
        [SerializeField] private float orbCollectThreshold = 0.2f;
        [SerializeField] private LayerMask orbLayer;

        private bool isVacuuming = false;
        private HashSet<Rigidbody> affectedOrbs = new HashSet<Rigidbody>();
        

        public void VacuumOrbs()
        {
            // Cast an overlap sphere around an area
            Collider[] colliders = Physics.OverlapSphere(suctionPoint.position, suctionRange, orbLayer);
            
            foreach (Collider collider in colliders)
            {
                // Check for orb script
                Orb orb = collider.GetComponent<Orb>();
                if (orb is null) { continue; }
                
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                
                affectedOrbs.Add(rb);
                
                // Disable orb gravity while vacuuming
                rb.useGravity = false;
                
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
                    affectedOrbs.Remove(rb);
                    orb.Collect();

                    // Add score
                    // GameManager.Instance.OnScoreCollected
                }
            }
        }

        public void StopCleaner()
        {
            foreach (Rigidbody rb in affectedOrbs.ToList())
            {
                if(rb != null)
                    rb.useGravity = true;
            }
            affectedOrbs.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (suctionPoint)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(suctionPoint.position, suctionRange);
            }
        }
    }
}
