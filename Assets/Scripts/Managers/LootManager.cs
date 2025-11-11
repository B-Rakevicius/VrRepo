using UnityEngine;
using System.Collections.Generic;
public class LootManager : MonoBehaviour
{
    public static LootManager Instance;
    public GameObject SheepWool;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    /// <summary>
    /// Determines if an item should drop and spawns it at a given location.
    /// </summary>
    public void TryDropLoot(Vector3 position)
    {
        if (SheepWool != null)
        {
            GameObject spawned = Instantiate(SheepWool, position, Quaternion.identity);
            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = spawned.AddComponent<Rigidbody>();
            }
            // upward
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                1f,
                Random.Range(-1f, 1f)
            ).normalized;
            float popForce = Random.Range(5f, 8f);
            rb.AddForce(randomDirection * popForce, ForceMode.Impulse);
            // spin
            Vector3 randomTorque = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f)
            );
            rb.AddTorque(randomTorque);
        }
    }
}