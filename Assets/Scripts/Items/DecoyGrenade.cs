using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class DecoyGrenade : MonoBehaviour
    {
        [SerializeField] private float activationTime = 2f, activeDuration = 5f, attractionRadius = 10f;
        [SerializeField] private int maxBites = 5;
        [SerializeField] private GameObject activationEffect, hayVisual;
        [SerializeField] private AudioClip activationSound, attractSound;
        private Rigidbody _rb;
        private XRGrabInteractable _grabInteractable;
        private bool _hasActivated = false, _isActive = false, _isArmed = false;
        private float _activationTime;
        private int _currentBites = 0;
        private AudioSource _audioSource;
        private Dictionary<EnemyAI, Transform> _originalTargets = new Dictionary<EnemyAI, Transform>();
        private HashSet<EnemyAI> _enemiesThatBit = new HashSet<EnemyAI>();
        public bool IsActive => _isActive;
        public Vector3 Position => transform.position;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 1f;
            }
            SetupXRInteractable();
        }
        private void SetupXRInteractable()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.AddListener(OnThrown);
                _grabInteractable.selectEntered.AddListener(OnPickedUp);
            }
        }
        private void Update()
        {
            if (_isActive)
            {
                PulseActiveVisual();
                AttractEnemies();
                CleanupEnemies();
            }
        }
        private void OnThrown(SelectExitEventArgs args)
        {
            _rb.isKinematic = false;
            if (!_isArmed)
            {
                ArmDecoy();
            }
        }
        private void OnPickedUp(SelectEnterEventArgs args)
        {
            _rb.isKinematic = false;
            if (_isArmed && !_hasActivated)
            {
                DisarmDecoy();
            }
        }
        public void ArmDecoy()
        {
            if (_isArmed) return;
            _isArmed = true;
            Invoke(nameof(ActivateDecoy), activationTime);
            Debug.Log("decoy armed");
        }
        public void DisarmDecoy()
        {
            if (_isArmed && !_hasActivated)
            {
                _isArmed = false;
                CancelInvoke(nameof(ActivateDecoy));
                transform.localScale = Vector3.one;
                Debug.Log("decoy disarmed");
            }
        }
        private void ActivateDecoy()
        {
            if (_hasActivated) return;
            _hasActivated = true;
            _isActive = true;
            _activationTime = Time.time;
            if (activationEffect != null)
            {
                GameObject effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }
            if (activationSound != null)
            {
                _audioSource.PlayOneShot(activationSound);
            }
            StartCoroutine(AttractSounds());
            Invoke(nameof(DeactivateDecoy), activeDuration);
            Debug.Log("attracting enemies for " + activeDuration + " seconds");
        }
        private System.Collections.IEnumerator AttractSounds()
        {
            while (_isActive)
            {
                if (attractSound != null && _audioSource != null)
                {
                    _audioSource.PlayOneShot(attractSound);
                }
                yield return new WaitForSeconds(2f);
            }
        }
        private void AttractEnemies()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attractionRadius);
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
                if (enemy != null && !_originalTargets.ContainsKey(enemy))
                {
                    if (EnemyInAttractRange(enemy))
                    {
                        _originalTargets[enemy] = GetEnemyTarget(enemy);
                        SetEnemyTarget(enemy, this.transform);
                    }
                }
            }
        }
        private void CleanupEnemies()
        {
            List<EnemyAI> enemiesToRemove = new List<EnemyAI>();
            foreach (var kvp in _originalTargets)
            {
                EnemyAI enemy = kvp.Key;
                if (enemy == null || GetEnemyTarget(enemy) != this.transform)
                {
                    enemiesToRemove.Add(enemy);
                }
            }
            foreach (EnemyAI enemy in enemiesToRemove)
            {
                _originalTargets.Remove(enemy);
                _enemiesThatBit.Remove(enemy);
            }
        }
        private bool EnemyInAttractRange(EnemyAI enemy)
        {
            float distanceToDecoy = Vector3.Distance(enemy.transform.position, transform.position);
            Transform currentTarget = GetEnemyTarget(enemy);
            float distanceToCurrentTarget = Mathf.Infinity;
            if (currentTarget != null)
            {
                distanceToCurrentTarget = Vector3.Distance(enemy.transform.position, currentTarget.position);
            }
            float priorityThreshold = distanceToCurrentTarget * 1.2f; 
            return distanceToDecoy <= priorityThreshold;
        }
        private Transform GetEnemyTarget(EnemyAI enemy)
        {
            System.Reflection.FieldInfo field = typeof(EnemyAI).GetField("HayTarget",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (Transform)field.GetValue(enemy);
            }
            return enemy.transform;
        }
        private void SetEnemyTarget(EnemyAI enemy, Transform target)
        {
            System.Reflection.FieldInfo field = typeof(EnemyAI).GetField("HayTarget",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(enemy, target);
                System.Reflection.FieldInfo targetingField = typeof(EnemyAI).GetField("isTargetingPlayer",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (targetingField != null)
                {
                    targetingField.SetValue(enemy, false);
                }
            }
        }
        public void TakeBite(EnemyAI enemy)
        {
            if (!_isActive || _currentBites >= maxBites) return;
            if (_enemiesThatBit.Contains(enemy)) return;
            _currentBites++;
            _enemiesThatBit.Add(enemy);
            UpdateHaySize();
            Debug.Log($"decoy -1 {_currentBites}/{maxBites}");
            if (_currentBites >= maxBites)
            {
                DestroyDecoy();
            }
        }
        private void UpdateHaySize()
        {
            if (hayVisual != null)
            {
                Vector3 currentScale = hayVisual.transform.localScale;
                Vector3 newScale = currentScale * 0.9f;
                hayVisual.transform.localScale = newScale;
            }
        }
        private void PulseActiveVisual()
        {
            if (_isActive)
            {
                float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                float scale = Mathf.Lerp(0.8f, 1.2f, pulse);
                transform.localScale = Vector3.one * scale;
            }
        }
        private void DeactivateDecoy()
        {
            if (!_isActive) return;
            _isActive = false;
            transform.localScale = Vector3.one;
            ReleaseEnemies();
            Debug.Log("decoy deactivated");
            Destroy(gameObject, 0.05f);
        }
        private void DestroyDecoy()
        {
            if (!_isActive) return;
            _isActive = false;
            ReleaseEnemies();
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            _grabInteractable.enabled = false;
            Destroy(gameObject, 0.1f);
            Debug.Log("decoy destroyed from bites");
        }
        private void ReleaseEnemies()
        {
            foreach (var kvp in _originalTargets)
            {
                EnemyAI enemy = kvp.Key;
                Transform originalTarget = kvp.Value;
                if (enemy != null)
                {
                    SetEnemyTarget(enemy, originalTarget);
                    if (originalTarget != null && originalTarget.CompareTag("Player"))
                    {
                        System.Reflection.FieldInfo targetingField = typeof(EnemyAI).GetField("isTargetingPlayer",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (targetingField != null)
                        {
                            targetingField.SetValue(enemy, true);
                        }
                    }
                }
            }
            _originalTargets.Clear();
            _enemiesThatBit.Clear();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                TakeBite(enemy);
            }
        }
        private void OnDestroy()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.RemoveListener(OnThrown);
                _grabInteractable.selectEntered.RemoveListener(OnPickedUp);
            }
            ReleaseEnemies();
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attractionRadius);
        }
    }
}