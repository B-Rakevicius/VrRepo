using UnityEngine;
namespace Items
{
    public class Arrow : MonoBehaviour
    {
        [Header("Arrow Settings")]
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float knockbackForce = 1f;
        [SerializeField] private GameObject hitEffect;
        private Rigidbody rb;
        private bool hasHit = false;
        private string damageSource;
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (GetComponent<Collider>() == null)
            {
                Debug.Log("arrow collider not found fix pls");
            }
        }
        public void Initialize(Vector3 velocity, string source = "Player")
        {
            damageSource = source;

            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
            Destroy(gameObject, lifetime);
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;
            if (((1 << collision.gameObject.layer) & collisionMask) != 0)
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate hit direction (from arrow to target)
                    Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
                    damageable.TakeDamage(damage, hitDirection, knockbackForce, damageSource);
                }
                // hit vfx
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
                }
                // Sticking to surface
                if (rb != null)
                {
                    rb.isKinematic = true;
                    //rb.constraints = RigidbodyConstraints.FreezeAll;
                    transform.SetParent(collision.transform);
                }
                // Disable collider to prevent multihit ( ? maybe upgrade can allow multiple hits later on ) 
                Collider col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                hasHit = true;
                Destroy(gameObject, 2f);
            }
        }
        private void Update()
        {
            if (rb != null && rb.linearVelocity != Vector3.zero && !hasHit)
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            }
        }
    }
}