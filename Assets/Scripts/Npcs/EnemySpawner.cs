using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public int cost = 1;
    public int minWave = 1;
    [Tooltip("Weight for random selection (higher = more likely to spawn)")]
    public float spawnWeight = 1f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Configuration")]
    public List<EnemySpawnConfig> enemyConfigs = new List<EnemySpawnConfig>();

    [Header("Spawn Settings")]
    public Transform player;
    public float spawnRadius = 50f, waveInterval = 5f, spawnBuffer = 0f;
    public int maxEnemiesPerWave = 25, totalEnemies = 0;
    public LayerMask groundLayer;
    public bool isPaused = false;

    [Header("Spawn Bounds")]
    public List<Bounds> spawnBounds = new List<Bounds>();
    public bool useSpawnBounds = true;

    private float timeTotal = 0f, timeDuringWaves = 0f;
    private Coroutine waveCoroutine;
    public List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Validate enemy configs
        ValidateEnemyConfigs();

        isPaused = true;
        waveCoroutine = StartCoroutine(WaveSpawner());
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += OnRoundStarted;
        }
    }

    private void ValidateEnemyConfigs()
    {
        if (enemyConfigs.Count == 0)
        {
            Debug.LogWarning("No enemy configurations found! Please add at least one enemy to the enemyConfigs list.");
        }

        // Remove any null prefabs
        enemyConfigs.RemoveAll(config => config.enemyPrefab == null);

        // Ensure spawn weights are positive
        foreach (var config in enemyConfigs)
        {
            if (config.spawnWeight <= 0)
            {
                config.spawnWeight = 1f;
                Debug.LogWarning($"Fixed spawn weight for enemy {config.enemyPrefab.name} - must be positive.");
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted -= OnRoundStarted;
        }
    }

    private void OnRoundStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.currentWave % 5 == 0)
        {
            PauseSpawning();
            Debug.Log($"Spawning paused at round {GameManager.Instance.currentWave}");
            CheckAndUpdateInbetweenWaves();
        }
        else
        {
            ResumeSpawning();
        }
    }

    private void Update()
    {
        if (!isPaused)
        {
            spawnBuffer += Time.deltaTime * GetSpawnRateMultiplier();
            timeDuringWaves += Time.deltaTime;
        }
        timeTotal += Time.deltaTime;
    }

    private IEnumerator WaveSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            if (!isPaused)
            {
                SpawnWave();
            }
        }
    }

    private void ResetBuffer()
    {
        spawnBuffer = 0f;
        isPaused = true;
    }

    private void SpawnWave()
    {
        float availableBuffer = spawnBuffer;
        int enemiesSpawned = 0;
        int currentWave = GameManager.Instance.currentWave;

        while (availableBuffer > 0 && enemiesSpawned < maxEnemiesPerWave && totalEnemies < 100)
        {
            availableBuffer = spawnBuffer;

            // Get available enemies for current wave
            List<EnemySpawnConfig> availableEnemies = GetAvailableEnemies(currentWave);
            if (availableEnemies.Count == 0)
            {
                Debug.LogWarning("No enemies available to spawn at wave " + currentWave);
                break;
            }

            // Select random enemy based on weights
            EnemySpawnConfig selectedConfig = SelectRandomEnemy(availableEnemies);

            if (selectedConfig != null && availableBuffer >= selectedConfig.cost)
            {
                SpawnEnemy(selectedConfig);
                enemiesSpawned++;
            }
            else
            {
                // Try to find a cheaper enemy if available
                EnemySpawnConfig cheaperEnemy = FindCheapestAvailableEnemy(availableEnemies);
                if (cheaperEnemy != null && availableBuffer >= cheaperEnemy.cost)
                {
                    SpawnEnemy(cheaperEnemy);
                    enemiesSpawned++;
                }
                else
                {
                    break;
                }
            }
        }
        GameManager.Instance.StartRound();
    }

    private List<EnemySpawnConfig> GetAvailableEnemies(int currentWave)
    {
        List<EnemySpawnConfig> available = new List<EnemySpawnConfig>();

        foreach (var config in enemyConfigs)
        {
            if (config.enemyPrefab != null && currentWave >= config.minWave)
            {
                available.Add(config);
            }
        }

        return available;
    }

    private EnemySpawnConfig SelectRandomEnemy(List<EnemySpawnConfig> availableEnemies)
    {
        if (availableEnemies.Count == 0) return null;

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var enemy in availableEnemies)
        {
            totalWeight += enemy.spawnWeight;
        }

        // Random selection with weights
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var enemy in availableEnemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemy;
            }
        }

        // Fallback to first available
        return availableEnemies[0];
    }

    private EnemySpawnConfig FindCheapestAvailableEnemy(List<EnemySpawnConfig> availableEnemies)
    {
        if (availableEnemies.Count == 0) return null;

        EnemySpawnConfig cheapest = availableEnemies[0];
        foreach (var enemy in availableEnemies)
        {
            if (enemy.cost < cheapest.cost)
            {
                cheapest = enemy;
            }
        }
        return cheapest;
    }

    private void SpawnEnemy(EnemySpawnConfig config)
    {
        int attempts = 0;
        bool spawned = false;

        while (attempts < 10 && !spawned)
        {
            Vector3 spawnPosition = GetSpawnPosition();

            if (spawnPosition != Vector3.zero)
            {
                // Check if position is clear of other enemies
                float enemyRadius = 1.5f;
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, enemyRadius);
                bool positionClear = true;
                foreach (Collider col in colliders)
                {
                    if (col.CompareTag("Enemy"))
                    {
                        positionClear = false;
                        break;
                    }
                }

                if (positionClear)
                {
                    GameObject newEnemy = Instantiate(config.enemyPrefab, spawnPosition, Quaternion.identity);

                    // Apply enemy scaling based on wave
                    EnemyAI enemyAI = newEnemy.GetComponentInChildren<EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.speed += timeTotal * 0.001f;
                        AdjustEnemyStats(enemyAI, config);
                    }

                    activeEnemies.Add(newEnemy);
                    spawnBuffer -= config.cost;
                    totalEnemies++;
                    spawned = true;

                    Debug.Log($"Spawned {config.enemyPrefab.name} (Cost: {config.cost}, Wave: {GameManager.Instance.currentWave})");
                }
            }
            attempts++;
        }

        if (!spawned)
        {
            Debug.LogWarning($"Failed to find clear space to spawn {config.enemyPrefab.name} after multiple attempts.");
        }
    }

    private void AdjustEnemyStats(EnemyAI enemyAI, EnemySpawnConfig config)
    {
        int currentWave = GameManager.Instance.currentWave;
        int baseMultiplier = Mathf.Max(1, config.cost); // cost as base multiplier indicator

        if (currentWave <= 10)
            enemyAI.maxHealth = baseMultiplier * (5 + (int)(currentWave * 0.2));
        else if (currentWave <= 20)
            enemyAI.maxHealth = baseMultiplier * (5 + (int)(currentWave * 0.3));
        else
            enemyAI.maxHealth = baseMultiplier * (5 + (int)(currentWave * 0.4));

        enemyAI.currentHealth = enemyAI.maxHealth;
    }

    private Vector3 GetSpawnPosition()
    {
        // Try using spawn bounds first if enabled and available
        if (useSpawnBounds && spawnBounds.Count > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                Bounds randomBounds = spawnBounds[Random.Range(0, spawnBounds.Count)];
                Vector3 boundsPosition = GetRandomPositionInBounds(randomBounds);

                if (boundsPosition != Vector3.zero && IsPositionValid(boundsPosition))
                {
                    return boundsPosition;
                }
            }
        }

        // Fall back to random spawn around player
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 10f;
            Vector3 spawnPosition = player.position + randomOffset;

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                Vector3 groundPosition = hit.point;
                if (IsPositionValid(groundPosition))
                {
                    return groundPosition;
                }
            }
        }

        return Vector3.zero;
    }

    private Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y + 10f,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 30f, groundLayer))
        {
            Vector3 groundPosition = hit.point;

            if (bounds.Contains(new Vector3(groundPosition.x, bounds.center.y, groundPosition.z)))
            {
                return groundPosition;
            }
        }

        return Vector3.zero;
    }

    private bool IsPositionValid(Vector3 position)
    {
        if (!Physics.Raycast(position + Vector3.up * 2f, Vector3.down, 5f, groundLayer))
        {
            return false;
        }

        if (Vector3.Distance(position, player.position) < 5f)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Method to adjust spawn rates based on wave
    /// </summary>
    private float GetSpawnRateMultiplier()
    {
        int currentWave = GameManager.Instance.currentWave;

        if (currentWave < 5)
            return 1.0f;
        else if (currentWave < 10)
            return 2.5f;
        else if (currentWave < 20)
            return 1.0f;
        else
            return 6.0f;
    }

    /// <summary>
    /// Pauses the enemy spawner between waves
    /// </summary>
    public void PauseSpawning()
    {
        if (!isPaused)
        {
            isPaused = true;
            Debug.Log("Enemy spawning paused");
        }
    }

    /// <summary>
    /// Resumes the enemy spawner
    /// </summary>
    public void ResumeSpawning()
    {
        if (isPaused)
        {
            isPaused = false;
            Debug.Log("Enemy spawning resumed");
        }
    }

    /// <summary>
    /// Toggles the pause state of the enemy spawner
    /// </summary>
    public void TogglePauseSpawning()
    {
        isPaused = !isPaused;
        Debug.Log($"Enemy spawning {(isPaused ? "paused" : "resumed")}");
    }

    /// <summary>
    /// Sets the pause state directly
    /// </summary>
    public void SetSpawningPaused(bool pause)
    {
        if (isPaused && !pause)
        {
            isPaused = false;
            SpawnWave();
            Debug.Log("Enemy spawning resumed and wave spawned");
        }
        else if (!isPaused && pause)
        {
            isPaused = true;
            Debug.Log("Enemy spawning paused");
        }
    }

    /// <summary>
    /// Gets the current pause state
    /// </summary>
    public bool IsSpawningPaused()
    {
        return isPaused;
    }

    private void CheckAndUpdateInbetweenWaves()
    {
        if (GameManager.Instance == null) return;

        bool shouldShowInbetween = IsSpawningPaused() && activeEnemies.Count == 0;
        GameManager.Instance.SetInbetweenWavesState(shouldShowInbetween);
    }

    public void OnEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        CheckAndUpdateInbetweenWaves();
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        if (useSpawnBounds)
        {
            Gizmos.color = Color.green;
            foreach (Bounds bounds in spawnBounds)
            {
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player != null ? player.position : transform.position, spawnRadius);
    }

    // Public methods for managing enemy configs
    public void AddEnemyConfig(GameObject prefab, int cost = 1, int minWave = 1, float weight = 1f)
    {
        enemyConfigs.Add(new EnemySpawnConfig
        {
            enemyPrefab = prefab,
            cost = cost,
            minWave = minWave,
            spawnWeight = weight
        });
    }

    public void RemoveEnemyConfig(GameObject prefab)
    {
        enemyConfigs.RemoveAll(config => config.enemyPrefab == prefab);
    }

    public void ClearEnemyConfigs()
    {
        enemyConfigs.Clear();
    }
}