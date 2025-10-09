using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VRTemplate;
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3.5f;
    public float stoppingDistance = 1.5f;
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
    private Transform player;
    private Vector3 velocity;
    private CharacterController controller;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        if (controller == null)
        {
            Debug.LogError("CharacterController is missing on enemy!");
        }
    }
    private void Update()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (knockbackForce.magnitude > 0.1f)
        {
            controller.Move(knockbackForce * Time.deltaTime);
            knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, Time.deltaTime * 5f); // Dampen over time
        }
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        if (Vector3.Distance(transform.position, player.position) > stoppingDistance)
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
    public void TakeDamage(int damage, Vector3 hitDirection, float knockbackStrength = 0f)
    {
        //Debug.Log("taking damage here " + (damage + (0.01f*PointManager.Instance.dmgUp)));

        // currentHealth -= (int)(damage + (0.01f * PointManager.Instance.dmgUp));  
        // old damage calculation with leveling system in use.
        currentHealth -= damage;
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
            StartCoroutine(ApplyShadeAfterDelay(this.gameObject.transform, 0.5f));
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
        isDying = true;
        LootManager.Instance.TryDropLoot(transform.position);

        Destroy(gameObject.transform.parent.gameObject);

        PointManager.Instance.addSlain();
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
}
