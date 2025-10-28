using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Arrow : MonoBehaviour
    {
        [Tooltip("How long should the shot arrow exist in the world?")]
        [SerializeField] private float arrowLifetime = 5f;
        
        private Rigidbody _rb;
        private bool m_hasBeenShot = false;
        private bool m_hasHit = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }
        
        private void FixedUpdate()
        {
            if (m_hasBeenShot && !m_hasHit && _rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.forward = _rb.linearVelocity.normalized;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Arrow already hit something or wasn't shot. Don't calculate collisions.
            if (m_hasHit || !m_hasBeenShot) { return; }
            
            m_hasHit = true;

            StickToSurface(collision);
        }

        public void Shoot(Vector3 direction, float force)
        {
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.AddForce(direction * force, ForceMode.Impulse);
            m_hasBeenShot = true;
            
            Destroy(gameObject, arrowLifetime);
        }

        private void StickToSurface(Collision collision)
        {
            // Set speed to 0
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            _rb.isKinematic = true;
            
            // Get collision contacts and move the arrow slightly into the surface
            ContactPoint contact = collision.contacts[0];
            transform.position = contact.point + transform.forward * -0.37f;
            
            // Set a parent so that arrow would follow it
            transform.SetParent(collision.transform, true);
            
            // Disable interactions with this arrow
            GetComponent<XRGrabInteractable>().enabled = false;
            GetComponent<XRGeneralGrabTransformer>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
