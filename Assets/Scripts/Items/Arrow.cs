using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private GameObject hitEffect; //vfx, need make then add
        private Rigidbody _rb;
        private bool m_hasHit = false;
        private bool m_hasBeenShot = false;
        private string damageSource;
        private GameObject instantVFX;
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
            damageSource = source;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.AddForce(direction * force, ForceMode.Impulse);
            m_hasBeenShot = true;
            Destroy(gameObject, lifetime);
            
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (m_hasHit || !m_hasBeenShot) { return; }
            m_hasHit = true;
            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                Vector3 hitDirection = transform.forward;
                damageable.TakeDamage(damage, hitDirection, knockbackForce, damageSource);
            }
            if (hitEffect != null)
            {
                instantVFX = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
                Destroy(instantVFX, 2f);
            }
            StickToSurface(collision);
        }
        private void StickToSurface(Collision collision)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            ContactPoint contact = collision.contacts[0];
            transform.position = contact.point + transform.forward * -0.37f;
            transform.SetParent(collision.transform, true);
            GetComponent<XRGrabInteractable>().enabled = false;
            GetComponent<XRGeneralGrabTransformer>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}