using Player;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        
        [Header("Hand Pose Settings")]
        [Tooltip("Defines which hands animation blend tree to use.")]
        [SerializeField] private int triggerPoseID = 3;
        
        // Attach points
        [SerializeField] private Transform leftHandTriggerAttach;
        [SerializeField] private Transform rightHandTriggerAttach;
        
        // Identify hand grab points between main and secondary
        private AttachPointType _leftHandAttachPointType = AttachPointType.None;
        private AttachPointType _rightHandAttachPointType = AttachPointType.None;
        
        // Interactors to keep track of which are active for this object, a.k.a which hands are holding the object
        private IXRSelectInteractor _leftHandInteractor;
        private IXRSelectInteractor _rightHandInteractor;
        
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
                _grabInteractable.selectEntered.AddListener(OnPickedUp);
                _grabInteractable.selectExited.AddListener(OnThrown);
                _grabInteractable.hoverEntered.AddListener(OnHover);
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
        /// Sets attach points prior to grabbing actual object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnHover(HoverEnterEventArgs arg0)
        {
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;

            if (handedness == InteractorHandedness.Right)
            {
                _grabInteractable.attachTransform = rightHandTriggerAttach;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                _grabInteractable.attachTransform = leftHandTriggerAttach;
            }
        }
        /// <summary>
        /// Gets called then the object is released. Checks which hand was holding it to properly clear variables, change poses.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnThrown(SelectExitEventArgs arg0)
        {
            // Grenade logic
            _rb.isKinematic = false;
            if (_isInShop) return;
            if (!_isArmed)
            {
                ArmGrenade();
            }
            
            // Hand animation
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
            
            // Check if released hand was holding the handle.
            if (handedness == InteractorHandedness.Right)
            {
                handAnimator.ClearHandPose();
                _rightHandAttachPointType = AttachPointType.None;
                _rightHandInteractor = null;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                handAnimator.ClearHandPose();
                _leftHandAttachPointType = AttachPointType.None;
                _leftHandInteractor = null;
            }
        }
        /// <summary>
        /// Sets hand pose depending on which hand picked up the object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnPickedUp(SelectEnterEventArgs arg0)
        {
            // Grenade logic
            _rb.isKinematic = false;
            if (_isInShop) return;
            if (_isArmed && !_hasExploded)
            {
                DisarmGrenade();
            }
            
            // Hand animation
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
            
            if (handedness == InteractorHandedness.Right)
            {
                // Left hand is holding it. Reset its pose
                if (_leftHandInteractor != null)
                {
                    HandAnimator animator = _leftHandInteractor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
                    animator.ClearHandPose();
                    _leftHandAttachPointType = AttachPointType.None;
                    _leftHandInteractor = null;
                }
                handAnimator.SetHandPose(triggerPoseID);
                _rightHandAttachPointType = AttachPointType.Main;
                _rightHandInteractor = interactor;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                // Right hand is holding it. Reset its pose
                if (_leftHandInteractor != null)
                {
                    HandAnimator animator = _rightHandInteractor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
                    animator.ClearHandPose();
                    _rightHandAttachPointType = AttachPointType.None;
                    _rightHandInteractor = null;
                }
                handAnimator.SetHandPose(triggerPoseID);
                _leftHandAttachPointType = AttachPointType.Main;
                _leftHandInteractor = interactor;
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
                _grabInteractable.hoverEntered.RemoveListener(OnHover);
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