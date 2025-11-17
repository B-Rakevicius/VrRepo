using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Grenade : MonoBehaviour
    {
        [SerializeField] private float fuseTime = 3f, explosionRadius = 5f;
        [SerializeField] private float baseDamage = 50f, knockbackForce = 15f, upwardForce = 5f;
        [SerializeField] private GameObject explosionEffect, fuseLightEffect;
        [SerializeField] private AudioClip explosionSound, fuseSound, disarmSound;
        private Rigidbody _rb;
        private XRGrabInteractable _grabInteractable;
        private bool _hasExploded = false;
        private bool _isArmed = false;
        private float _armTime;
        private string _damageSource = "Player";
        private AudioSource _audioSource;
        private Material _originalMaterial;
        private Color _armedColor = Color.red;
        // public check if grenade is armed
        public bool IsArmed => _isArmed;
        private bool _isInShop = false;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1f; // for 3d sound
            }
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                _originalMaterial = renderer.material;
            }
            SetupXRInteractable();
        }
        private void SetupXRInteractable()
        {
            if (_grabInteractable != null)
            {
                // events for actions
                _grabInteractable.selectExited.AddListener(OnThrown);
                _grabInteractable.selectEntered.AddListener(OnPickedUp);
                _grabInteractable.activated.AddListener(OnActivated); // remove if manual arming doesnt feel good
            }
        }
        private void Update()
        {
            if (_isArmed && !_hasExploded && !_isInShop)
            {
                float timeSinceArm = Time.time - _armTime;
                float timeLeft = fuseTime - timeSinceArm;
                if (timeLeft < 1f)
                {
                    PulseVFX(timeLeft);
                }
            }
        }
        /// <summary>
        /// Method on throw
        /// </summary>
        /// <param name="args"></param>
        private void OnThrown(SelectExitEventArgs args)
        {
            _rb.isKinematic = false;
            if (_isInShop) return;
            if (!_isArmed)
            {
                ArmGrenade();
            }
        }
        /// <summary>
        /// Method on pick up
        /// </summary>
        /// <param name="args"></param>
        private void OnPickedUp(SelectEnterEventArgs args)
        {
            _rb.isKinematic = false;
            if (_isInShop) return;
            if (_isArmed && !_hasExploded)
            {
                DisarmGrenade();
            }
        }
        /// <summary>
        /// Method for manual arming via pulling a trigger or something similar 
        /// ( test pulling a trigger, test clicking a button, if feels bad remove )
        /// </summary>
        /// <param name="args"></param>
        private void OnActivated(ActivateEventArgs args)
        {
            if (_isInShop) return;
            if (!_isArmed)
            {
                ArmGrenade();
            }
        }
        /// <summary>
        /// Method to arm grenade, starting timers
        /// </summary>
        /// <param name="damageSource"></param>
        public void ArmGrenade(string damageSource = "Player")
        {
            if (_isArmed || _isInShop) return;
            _isArmed = true;
            _damageSource = damageSource;
            _armTime = Time.time;
            if (fuseSound != null)
            {
                _audioSource.clip = fuseSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
            if (fuseLightEffect != null)
            {
                fuseLightEffect.SetActive(true);
            }
            SetArmedVisuals(true);
            Invoke(nameof(Explode), fuseTime);
            Debug.Log("grenade 'ArmGrenade' called, exploding in " + fuseTime + " seconds");
        }
        /// <summary>
        /// Method to cancel explosion on pick up again
        /// </summary>
        public void DisarmGrenade()
        {
            if (_isInShop) return;
            if (_isArmed && !_hasExploded)
            {
                _isArmed = false;
                CancelInvoke(nameof(Explode));
                _audioSource.Stop();
                if (disarmSound != null)
                {
                    _audioSource.PlayOneShot(disarmSound);
                }
                if (fuseLightEffect != null)
                {
                    fuseLightEffect.SetActive(false);
                }
                SetArmedVisuals(false);
                Debug.Log("grenade disarm 11111111111111111111");
            }
        }
        /// <summary>
        /// Method to change visual appearance
        /// </summary>
        /// <param name="armed"></param>
        private void SetArmedVisuals(bool armed)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (armed)
                {
                    Material armedMaterial = new Material(_originalMaterial);
                    armedMaterial.color = _armedColor;
                    armedMaterial.EnableKeyword("_EMISSION");
                    armedMaterial.SetColor("_EmissionColor", _armedColor * 0.5f);
                    renderer.material = armedMaterial;
                }
                else
                {
                    renderer.material = _originalMaterial;
                }
            }
        }
        /// <summary>
        /// Method for a pulse VFX
        /// </summary>
        /// <param name="timeLeft"></param>
        private void PulseVFX(float timeLeft)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                float pulseRate = timeLeft * 10f;
                float pulse = Mathf.PingPong(Time.time * pulseRate, 1f);
                Color pulseColor = Color.Lerp(_armedColor, Color.white, pulse);
                renderer.material.color = pulseColor;
            }
        }
        /// <summary>
        /// Method to... explode...
        /// </summary>
        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;
            if (explosionEffect != null)
            {
                GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(explosion, 3f);
            }
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position, 2f);
            }
            _audioSource.Stop();
            ApplyExplosionDamage();
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            _grabInteractable.enabled = false;
            Destroy(gameObject, 0.1f);
        }
        /// <summary>
        /// Method to apply damage
        /// </summary>
        private void ApplyExplosionDamage()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hitCollider in hitColliders)
            {
                // skip grenade itself
                if (hitCollider.gameObject == gameObject) continue;
                // distance from explosion center
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float distanceFactor = Mathf.Clamp01(1f - (distance / explosionRadius));
                // damage with falloff
                if (hitCollider.gameObject.TryGetComponent(out IDamageable damageable))
                {
                    Vector3 explosionDirection = (hitCollider.transform.position - transform.position).normalized;
                    float calculatedDamage = baseDamage * distanceFactor;
                    float calculatedKnockback = knockbackForce * distanceFactor;
                    damageable.TakeDamage(calculatedDamage, explosionDirection, calculatedKnockback, _damageSource);
                    Debug.Log($"grenade explosion damaged {hitCollider} object 22222222");
                }
                Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
                /*
                if (hitRb != null && !hitRb.isKinematic)
                {
                    Vector3 explosionDirection = (hitCollider.transform.position - transform.position).normalized;
                    Vector3 explosionForce = explosionDirection * knockbackForce * distanceFactor;
                    explosionForce.y += upwardForce;
                    hitRb.AddForce(explosionForce, ForceMode.Impulse);
                }
                */
            }
            Debug.Log($"grenade explosion affected {hitColliders.Length} objects");
        }
        /// <summary>
        /// Method to manual explosion trigger, maybe add bool for impact grenades? ( throwable beehive ??? )
        /// </summary>
        public void TriggerExplosion()
        {
            if (!_hasExploded)
            {
                Explode();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            /*
            //impact detonation
            if (_isArmed && collision.relativeVelocity.magnitude > 10f)
            {
                Explode();
            }
            */
        }
        private void OnDestroy()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.RemoveListener(OnThrown);
                _grabInteractable.selectEntered.RemoveListener(OnPickedUp);
                _grabInteractable.activated.RemoveListener(OnActivated);
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
        public void SetShopState(bool isShopper)
        { 
            _isInShop = isShopper;
        }
    }
}