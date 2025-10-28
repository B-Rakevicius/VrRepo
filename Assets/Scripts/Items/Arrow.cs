using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Arrow : MonoBehaviour
    {
        [Header("Arrow Settings")]
        [Tooltip("How long should the shot arrow exist in the world?")]
        [SerializeField] private float lifetime = 5f;
        
        [Tooltip("How much damage should the arrow inflict?")]
        [SerializeField] private float damage = 10f;
        
        [Tooltip("How strong should the damageable item be knocked back?")]
        [SerializeField] private float knockbackForce = 1f;
        
        // Particles?
        [SerializeField] private GameObject hitEffect;
        private Rigidbody _rb;
        private bool m_hasHit = false;
        private bool m_hasBeenShot = false;
        private string damageSource;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (GetComponent<Collider>() == null)
            {
                Debug.LogError("Collider not found. Make sure it is attached to the arrow game object.");
            }
        }
        
        private void FixedUpdate()
        {
            if (m_hasBeenShot && !m_hasHit && _rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.forward = _rb.linearVelocity.normalized;
            }
        }
        
        public void Shoot(Vector3 direction, float force, string source = "Player")
        {
            // Set damage source
            damageSource = source;
            
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.AddForce(direction * force, ForceMode.Impulse);
            m_hasBeenShot = true;
            
            Destroy(gameObject, lifetime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Arrow already hit something or wasn't shot. Don't calculate collisions.
            if (m_hasHit || !m_hasBeenShot) { return; }
            
            m_hasHit = true;
            
            // Try to check if hit collision is damageable
            if(collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                // Collision is damageable. Take damage and apply knockback
                Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
                damageable.TakeDamage(damage, hitDirection, knockbackForce, damageSource);
            }
            
            // Hit VFX
            if (hitEffect != null)
            {
                Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
            }
            
            StickToSurface(collision);
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