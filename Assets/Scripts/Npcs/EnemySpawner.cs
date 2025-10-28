using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab, strongEnemyPrefab;
    private const int normalEnemyCost = 1, strongEnemyCost = 3;
    public Transform player;
    public float spawnRadius = 50f, waveInterval = 5f, spawnBuffer = 0f;
    public int maxEnemiesPerWave = 25, totalEnemies = 0;
    public LayerMask groundLayer;
    private bool isPaused = false;
    private float timeTotal = 0f, timeDuringWaves = 0f;
    private Coroutine waveCoroutine;
    public List<GameObject> activeEnemies = new List<GameObject>();
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        waveCoroutine = StartCoroutine(WaveSpawner());
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundStarted += OnRoundStarted;
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
        DifficultyLogging();
        CheckAndUpdateInbetweenWaves();
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
        while (availableBuffer > 0 && enemiesSpawned < maxEnemiesPerWave && totalEnemies < 100)
        {
            availableBuffer = spawnBuffer;
            GameObject enemyType;
            int enemyCost;
            if (availableBuffer >= strongEnemyCost && Random.value > 0.7f)
            {
                enemyType = strongEnemyPrefab;
                enemyCost = strongEnemyCost;
            }
            else
            {
                enemyType = enemyPrefab;
                enemyCost = normalEnemyCost;
            }
            if (availableBuffer >= enemyCost)
            {
                SpawnEnemy(enemyType, enemyCost);
                enemiesSpawned++;
            }
            else
            {
                break;
            }
        }
        GameManager.Instance.StartRound();
    }
    private void SpawnEnemy(GameObject enemyType, int enemyCost)
    {
        int attempts = 0;
        bool spawned = false;
        while (attempts < 10 && !spawned)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 10f;
            Vector3 spawnPosition = player.position + randomOffset;
            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                spawnPosition = hit.point;
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
                    GameObject newEnemy = Instantiate(enemyType, spawnPosition, Quaternion.identity);
                    newEnemy.GetComponentInChildren<EnemyAI>().speed += timeTotal * 0.001f;
                    int mult = enemyType == strongEnemyPrefab ? 3 : 1;
                    AdjustEnemyType(newEnemy, mult);

                    activeEnemies.Add(newEnemy);
                    spawnBuffer -= enemyCost;
                    totalEnemies++;
                    spawned = true;
                }
            }
            attempts++;
        }
        if (!spawned)
        {
            Debug.LogWarning("Failed to find clear space to spawn enemy after multiple attempts.");
        }
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
    /// Adjusts enemy max hp based on current wave
    /// </summary>
    private void AdjustEnemyType(GameObject enemyType, int mult)
    {
        int currentWave = GameManager.Instance.currentWave;

        if (currentWave <= 10)
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(currentWave * 2));
        else if (currentWave <= 20)
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(currentWave * 3));
        else
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(currentWave * 4));
    }

    /// <summary>
    /// Checks and logs when enemies get stronger based on wave thresholds
    /// </summary>
    private void DifficultyLogging()
    {
        int currentWave = GameManager.Instance.currentWave;

        switch (currentWave)
        {
            case 10:
                Debug.Log("Enemies growing stronger: Now +3 HP per wave with 2.5x spawn rate");
                break;
            case 20:
                Debug.Log("Enemies evolving: Now +4 HP per wave with 6x spawn rate");
                break;
            case 5:
                Debug.Log("Enemies getting tougher: Now +2 HP per wave with 1x spawn rate");
                break;
        }
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
    /// Sets the pause state directly (replaces SetSpawningBool)
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
}