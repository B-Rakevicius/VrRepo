using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject strongEnemyPrefab;
    private const int normalEnemyCost = 1;
    private const int strongEnemyCost = 3;
    public Transform player;
    public float spawnRadius = 50f;
    public float waveInterval = 5f;
    public int maxEnemiesPerWave = 25;
    public LayerMask groundLayer;
    public int totalEnemies = 0;
    private bool DontSpawnNow = false;
    public float gameTimer = 0f;

    public float spawnBuffer = 0f;




    public List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        StartCoroutine(WaveSpawner());
    }
    private void Update()
    {
        if (!DontSpawnNow)
        {
            spawnBuffer += Time.deltaTime*GetSpawnRateMultiplier();
            gameTimer += Time.deltaTime;
        }
    }
    private IEnumerator WaveSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            if (!DontSpawnNow)
            {
                SpawnWave();
            }
        }
    }
    private void SpawnWave()
    {
        float availableBuffer = spawnBuffer;
        int enemiesSpawned = 0;

        while (availableBuffer > 0 && enemiesSpawned < maxEnemiesPerWave && totalEnemies < 100 )
        {
            availableBuffer = spawnBuffer;
            GameObject enemyType;
            int enemyCost;

            if (availableBuffer >= strongEnemyCost && Random.value > 0.7f)
            {
                enemyType = strongEnemyPrefab;
                enemyCost = strongEnemyCost;
                //adjustEnemyType(enemyType, 3);
            }
            else
            {
                enemyType = enemyPrefab;
                enemyCost = normalEnemyCost;
                //adjustEnemyType(enemyType, 1);
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
                    newEnemy.GetComponentInChildren<EnemyAI>().speed += gameTimer * 0.01f;
                    activeEnemies.Add(newEnemy);
                    spawnBuffer -= enemyCost;
                    totalEnemies++;
                    spawned = true;
                    //Debug.Log(enemyType.GetComponentInChildren<EnemyAI>().maxHealth + " <-- spawned enemy who has this much max hp");
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
    /// Method to adjust spawn rates in time, currently returns
    /// 1x amount of enemies between 0-30 seconds of gametime
    /// 2.5x amount of enemies between 30-90 seconds of gametime
    /// 1x amount of enemies between 90-180 seconds of gametime
    /// 6x amount of enemies between 180-forever seconds of gametime
    /// 
    /// This implementation allows for simple scaling with minimal variables
    /// ( but maybe should make this a switch or something, cba right now )
    /// 
    /// should consider changing to wave based if game based on that.
    /// </summary>
    /// <returns></returns>
    private float GetSpawnRateMultiplier()
    {
        if (gameTimer < 30f)
            return 1.0f;
        else if (gameTimer < 90f)
            return 2.5f;
        else if (gameTimer < 180f)
            return 1.0f;
        else
            return 6.0f;
    }
    /// <summary>
    /// Addjusts enemy max hp based on game time, should consider changing to wave based if game based on that.
    /// </summary>
    /// <param name="enemyType"></param>
    /// <param name="mult"></param>
    private void adjustEnemyType(GameObject enemyType, int mult)
    {
        if (gameTimer <= 30f)
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(gameTimer * 0.1));
        else if (gameTimer <= 90f)
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(gameTimer * 0.2));
        else
            enemyType.GetComponentInChildren<EnemyAI>().maxHealth = mult * (5 + (int)(gameTimer * 0.3));
    }
    public void SetSpawningBool(bool booleanMoment)
    {
        if (DontSpawnNow && !booleanMoment)
        {
            //DontSpawnNow = booleanMoment; // in hindsight probably needs this, double check if if breaks with/without
            SpawnWave();
        }
        DontSpawnNow = booleanMoment;
    }
}
