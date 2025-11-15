using UnityEngine;
public class BalloonComponent : MonoBehaviour, IDamageable
{
    public GameObject balloonObject;
    public float popForce = 2f;
    public float startingHeight = 9f;
    public float descentSpeed = 0.7f;
    private EnemyAI enemyAI;
    private bool isPopped = false;
    private Rigidbody enemyRigidbody;
    private CharacterController controller;
    private float currentHeight;
    private bool hasLanded = false;
    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        controller = GetComponent<CharacterController>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }
        Vector3 startPos = transform.position;
        currentHeight = startingHeight;
        transform.position = new Vector3(startPos.x, startingHeight, startPos.z);
        SetupBalloonPhysics();
        SetupBalloonCollider();
    }
    private void SetupBalloonPhysics()
    {
        enemyRigidbody = gameObject.AddComponent<Rigidbody>();
        enemyRigidbody.useGravity = false;
        enemyRigidbody.linearDamping = 0.5f;
        enemyRigidbody.angularDamping = 0.5f;
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
    private void SetupBalloonCollider()
    {
        if (balloonObject != null)
        {
            Collider balloonCollider = balloonObject.GetComponent<Collider>();
            if (balloonCollider == null)
            {
                balloonCollider = balloonObject.AddComponent<BoxCollider>();
            }
            balloonCollider.isTrigger = false;

            BalloonDamageHandler damageHandler = balloonObject.GetComponent<BalloonDamageHandler>();
            if (damageHandler == null)
            {
                damageHandler = balloonObject.AddComponent<BalloonDamageHandler>();
            }
            damageHandler.Initialize(this);
        }
        else
        {
            Debug.LogWarning("BalloonObject not assigned in BalloonComponent!");
        }
    }
    private void Update()
    {
        if (!isPopped && !hasLanded)
        {
            FlyingMovement();
            ApplyDescent();
            CheckForGroundCollision();
        }
    }
    private void FlyingMovement()
    {
        if (enemyAI == null) return;
        Transform hayTarget = GetHayTarget();
        if (hayTarget == null) return;
        Vector3 direction = (hayTarget.position - transform.position).normalized;
        direction.y = 0; // move horizontally
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, enemyAI.rotationSpeed * Time.deltaTime);
        }
        float distanceToTarget = Vector3.Distance(transform.position, hayTarget.position);
        if (distanceToTarget > enemyAI.stoppingDistance)
        {
            Vector3 horizontalMovement = direction * enemyAI.speed * Time.deltaTime;
            transform.position += horizontalMovement;
        }
    }
    private Transform GetHayTarget()
    {
        GameObject[] hayTargets = GameObject.FindGameObjectsWithTag("Hay");
        Transform closestHay = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject hay in hayTargets)
        {
            if (hay == null) continue;
            float distance = Vector3.Distance(hay.transform.position, currentPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHay = hay.transform;
            }
        }
        if (closestHay != null)
        {
            return closestHay;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform : null;
        }
    }
    private void ApplyDescent()
    {
        if (currentHeight > 0f)
        {
            currentHeight -= descentSpeed * Time.deltaTime;
            currentHeight = Mathf.Max(0f, currentHeight);
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(currentPos.x, currentHeight, currentPos.z);
            if (currentHeight <= 0f && !hasLanded)
            {
                OnLand();
            }
        }
    }
    private void CheckForGroundCollision()
    {
        float raycastDistance = 1.5f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            if (hit.collider.gameObject != gameObject && hit.collider.gameObject != balloonObject)
            {
                Debug.Log($"Balloon detected ground collision with: {hit.collider.gameObject.name}");
                PopBalloon(Vector3.up * 0.5f);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isPopped && !hasLanded)
        {
            if (collision.gameObject == balloonObject) return;
            if (collision.relativeVelocity.magnitude > 0.5f)
            {
                Debug.Log($"Balloon enemy collided with: {collision.gameObject.name} (velocity: {collision.relativeVelocity.magnitude})");
                Vector3 hitDirection = collision.relativeVelocity.normalized;
                PopBalloon(hitDirection);
            }
        }
    }
    private void OnLand()
    {
        hasLanded = true;
        if (enemyRigidbody != null)
        {
            Destroy(enemyRigidbody);
        }
        if (controller != null)
        {
            controller.enabled = true;
        }
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
        if (balloonObject != null)
        {
            Destroy(balloonObject);
        }

        Destroy(this);
    }
    public void TakeDamage(float damage, Vector3 hitDirection, float knockbackStrength = 0f, string source = "")
    {
        if (!isPopped)
        {
            PopBalloon(hitDirection);
        }
    }
    public void OnBalloonHit(Vector3 hitDirection, float hitForce = 1f)
    {
        if (!isPopped)
        {
            PopBalloon(hitDirection * hitForce);
        }
    }
    private void PopBalloon(Vector3 hitDirection)
    {
        if (isPopped) return;
        isPopped = true;
        PlayPopEffects();
        if (enemyRigidbody != null)
        {
            enemyRigidbody.AddForce(hitDirection * popForce + Vector3.down * 2f, ForceMode.Impulse);
        }
        if (enemyRigidbody != null)
        {
            enemyRigidbody.useGravity = true;
        }
        Invoke(nameof(ReleaseEnemy), 0.5f);
    }
    private void ReleaseEnemy()
    {
        if (enemyRigidbody != null)
        {
            Destroy(enemyRigidbody);
        }
        if (controller != null)
        {
            controller.enabled = true;
        }
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
        if (balloonObject != null)
        {
            Destroy(balloonObject);
        }
        Destroy(this);
    }
    private void PlayPopEffects()
    {
        Debug.Log("Balloon popped!");
        if (balloonObject != null)
        {
            ParticleSystem popParticles = balloonObject.GetComponent<ParticleSystem>();
            if (popParticles != null)
            {
                popParticles.Play();
            }
            Renderer balloonRenderer = balloonObject.GetComponent<Renderer>();
            if (balloonRenderer != null)
            {
                balloonRenderer.enabled = false;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (!isPopped && !hasLanded)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.down * 1.5f);
        }
    }
}
public class BalloonDamageHandler : MonoBehaviour, IDamageable
{
    private BalloonComponent balloonComponent;
    public void Initialize(BalloonComponent balloonComp)
    {
        balloonComponent = balloonComp;
    }
    public void TakeDamage(float damage, Vector3 hitDirection, float knockbackStrength = 0f, string source = "")
    {
        if (balloonComponent != null)
        {
            balloonComponent.OnBalloonHit(hitDirection, knockbackStrength);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile") || collision.gameObject.CompareTag("Weapon"))
        {
            Vector3 hitDirection = collision.relativeVelocity.normalized;
            if (balloonComponent != null)
            {
                balloonComponent.OnBalloonHit(hitDirection, 1f);
            }
        }
    }
}