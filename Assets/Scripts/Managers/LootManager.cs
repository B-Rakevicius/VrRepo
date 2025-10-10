using UnityEngine;
using System.Collections.Generic;
//using Unity.Android.Gradle.Manifest;
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

        Debug.Log($"Enemy is supposed to drop loot, implement later :]");
        if (SheepWool != null)
        {
            GameObject spawned = Instantiate(SheepWool, position, Quaternion.identity);
        }
    }
}