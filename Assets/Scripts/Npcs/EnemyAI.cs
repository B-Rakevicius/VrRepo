using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VRTemplate;

public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    public float speed = 1.25f;
    public float stoppingDistance = 0.75f;
    public float rotationSpeed = 5f;
    public float gravity = 10f;
    public LayerMask groundLayer;

    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;
    private bool isDying = false;

    [Header("Damage Feedback")]
    public Renderer enemyRenderer;
    public float hitFlashDuration = 0.2f;

    [Header("Knockback Settings")]
    public float knockbackResistance = 0.5f; // 0 = no resistance, 1 = full resistance
    private Vector3 knockbackForce;
    private Transform HayTarget;
    private Vector3 velocity;
    private CharacterController controller;

    [Header("Eating Settings")]
    public float eatingRange = 1.5f;
    private float lastEatTime;
    private float eatCooldown = 1.0f;
    private bool isEating = false;
    private GameObject currentHayTarget;
    private bool isTargetingPlayer = false;
    private void Start()
    {
        FindTarget();
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        if (controller == null)
        {
            Debug.LogError("CharacterController is missing on enemy!");
        }
    }
    private void FindTarget()
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
            HayTarget = closestHay;
            isTargetingPlayer = false;
        }
        else
        {
            // If no hay found, target the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                HayTarget = player.transform;
                isTargetingPlayer = true;
                Debug.Log($"{this.name} is now targeting the player.");
            }
            else
            {
                HayTarget = null;
                Debug.LogWarning("No hay and no player found.");
            }
        }
    }
    private void TryEatHay()
    {
        if (isEating) return;
        if (HayTarget == null)
        {
            FindTarget();
            return;
        }
        if (Time.time - lastEatTime < eatCooldown) return;

        float distanceToTarget = Vector3.Distance(transform.position, HayTarget.position);
        if (distanceToTarget <= eatingRange)
        {
            StartCoroutine(EatWithDelay(HayTarget.gameObject));
        }
    }
    private IEnumerator EatWithDelay(GameObject target)
    {
        isEating = true;
        currentHayTarget = target;
        // Play preparation animations/sounds immediately
        PlayEatingAnimation();
        PlayEatingSound();
        PlayEatingVFX();
        yield return new WaitForSeconds(0.5f);
        if (target != null)
        {
            if (!isTargetingPlayer)
            {
                HayScript hayScript = target.GetComponent<HayScript>();
                if (hayScript != null)
                {
                    hayScript.TakeBite();
                    Debug.Log($"{this.name} took a bite of hay. Remaining health: " + hayScript.currentHealth);
                    if (hayScript.IsDestroyed())
                    {
                        Debug.Log("Hay eaten, switching targets.");
                        FindTarget();
                    }
                }
                else
                {
                    Debug.LogWarning("No HayScript found, destroying impostor hay immediately.");
                    Destroy(target);
                    FindTarget();
                }
            }
            else
            {
                Debug.Log($"{this.name} has eaten the player. gg ez no re.");
                EndGame();
            }
        }
        lastEatTime = Time.time;
        isEating = false;
        currentHayTarget = null;
    }
    private void EndGame()
    {
        GameManager.Instance.GameOver();
        // Destroy(HayTarget.gameObject);
    }
    private void Update()
    {
        TryEatHay();
        if (HayTarget == null || isEating) return;
        Vector3 direction = (HayTarget.position - transform.position).normalized;
        direction.y = 0;
        if (knockbackForce.magnitude > 0.1f)
        {
            controller.Move(knockbackForce * Time.deltaTime);
            knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, Time.deltaTime * 5f);
        }
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        if (Vector3.Distance(transform.position, HayTarget.position) > stoppingDistance &&
            Vector3.Distance(transform.position, HayTarget.position) > eatingRange/2)
        {
            controller.Move(direction * speed * Time.deltaTime);
        }
        if (!IsGrounded())
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0;
        }
        controller.Move(velocity * Time.deltaTime);
    }
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }
    public void TakeDamage(float damage, Vector3 hitDirection, float knockbackStrength = 0f, string source = "")
    {
        int intDamage = Mathf.RoundToInt(damage);
        currentHealth -= intDamage;
        if (!string.IsNullOrEmpty(source))
            Debug.Log($"Hit by {source} for {damage} damage!");
        if (knockbackStrength > 0)
        {
            ApplyKnockback(hitDirection, knockbackStrength);
        }
        if (currentHealth <= 0)
        {
            if (!isDying)
                Die();
        }
        else
        {
            Debug.Log("baa fix hit damage color showing");
            //StartCoroutine(ApplyShadeAfterDelay(this.gameObject.transform, 0.5f));
        }
    }
    private void ApplyKnockback(Vector3 hitDirection, float knockbackStrength)
    {
        Vector3 knockback = hitDirection.normalized * knockbackStrength * (1 - knockbackResistance);
        knockback.y = 0;
        knockbackForce = knockback;
    }
    private void Die()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyDestroyed(gameObject);
        }
        isDying = true;
        LootManager.Instance.TryDropLoot(transform.position);
        Destroy(this);
        //PointManager.Instance.addSlain();
    }
    private IEnumerator ApplyShadeAfterDelay(Transform enemy, float delay)
    {
        foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials.ElementAt(i).shader = ShaderManager.instance.damageShader;
            }
        }
        yield return new WaitForSeconds(hitFlashDuration);
        foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials.ElementAt(i).shader = ShaderManager.instance.normalShader;
            }
        }
    }
    private void PlayEatingAnimation()
    {

    }
    private void PlayEatingSound()
    {

    }

    private void PlayEatingVFX()
    {

    }
}
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitDirection, float knockbackStrength = 0f, string source = "");
}