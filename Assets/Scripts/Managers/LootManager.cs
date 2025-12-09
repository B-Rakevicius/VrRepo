using UnityEngine;
using System.Collections.Generic;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;
    public GameObject SheepWool;
    public float playerDirectionBias = 0.7f;
    public float minGroundDistance = 0.3f;
    public LayerMask groundLayer;
    private Transform playerTransform;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        GameObject dropDirectionTarget = GameObject.FindGameObjectWithTag("WoolDropDirection");
        if (dropDirectionTarget != null)
        {
            playerTransform = dropDirectionTarget.transform;
        }
    }

    /// <summary>
    /// Determines if an item should drop and spawns it at a given location.
    /// </summary>
    public void TryDropLoot(Vector3 position)
    {
        if (SheepWool != null)
        {
            // Spawn at a height above the ground
            Vector3 spawnPosition = position;

            GameObject spawned = Instantiate(SheepWool, spawnPosition, Quaternion.identity);
            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = spawned.AddComponent<Rigidbody>();
            }

            // Calculate direction toward player with some randomness
            Vector3 directionToPlayer = Vector3.zero;
            if (playerTransform != null)
            {
                directionToPlayer = (playerTransform.position - position).normalized;
                // Remove vertical component so it goes horizontally toward player
                directionToPlayer.y = 0;
            }
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            Vector3 horizontalDirection = Vector3.Lerp(randomDirection, directionToPlayer, playerDirectionBias).normalized;
            Vector3 finalDirection = (horizontalDirection + Vector3.up).normalized;
            float popForce = Random.Range(5f, 8f);
            rb.AddForce(finalDirection * popForce, ForceMode.Impulse);
            // Add component to keep it above ground
            GroundHover hoverComponent = spawned.GetComponent<GroundHover>();
            if (hoverComponent == null)
            {
                hoverComponent = spawned.AddComponent<GroundHover>();
            }
            hoverComponent.minHeight = minGroundDistance;
            hoverComponent.groundLayer = groundLayer;
        }
    }
}