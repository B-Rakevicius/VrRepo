using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ObjectReset : MonoBehaviour
{
    public float positionCheckInterval = 5f;
    public float movementThreshold = 1.0f;
    public float maxDistanceFromOrigin = 2f;
    public float resetDelay = 10f;
    public AudioClip resetAudioClip;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Coroutine checkCoroutine;
    private float lastMovementTime;
    private bool isMoving, showGizmos=true;
    private AudioSource _audioSource;
    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        lastPosition = originalPosition;
        lastMovementTime = -resetDelay;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f; // for 3d sound
        }
    }
    private void Start()
    {
        checkCoroutine = StartCoroutine(PositionCheckRoutine());
    }
    private void Update()
    {
        if (isMoving && Time.time - lastMovementTime >= resetDelay)
        {
            ResetToOriginal();
            isMoving = false;
        }
    }
    private IEnumerator PositionCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(positionCheckInterval);

        while (true)
        {
            yield return wait;
            CheckForMovement();
            CheckDistanceFromOrigin();
        }
    }
    private void CheckForMovement()
    {
        if (Vector3.Distance(transform.position, lastPosition) > movementThreshold)
        {
            lastPosition = transform.position;
            RegisterMovement();
        }
    }
    private void CheckDistanceFromOrigin()
    {
        if (Vector3.Distance(transform.position, originalPosition) > maxDistanceFromOrigin)
        {
            Debug.Log($"{gameObject.name} exceeded max distance, resetting");
            ResetToOriginal();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
        {
            RegisterMovement();
        }
    }
    private void RegisterMovement()
    {
        lastMovementTime = Time.time;
        isMoving = true;
    }
    public void ResetToOriginal()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        lastPosition = originalPosition;
        isMoving = false;

        // Play reset audio
        if (resetAudioClip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(resetAudioClip);
        }

        Debug.Log($"{gameObject.name} reset to original position");
    }
    private void OnDestroy()
    {
        if (checkCoroutine != null)
            StopCoroutine(checkCoroutine);
    }
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? originalPosition : transform.position, 0.1f);

        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, originalPosition);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? originalPosition : transform.position, maxDistanceFromOrigin);
    }
}