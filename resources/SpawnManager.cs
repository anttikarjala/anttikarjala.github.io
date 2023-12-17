using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private List<GameObject> enemyPrefabs;
    [SerializeField]
    private GameObject bossPrefab;
    [SerializeField]
    private List<GameObject> pillarPrefabs;
    
    // For boss round intro animation
    private readonly float playerSpeed = 100.0f;
    private readonly int bossRound = 3;
    
    // For enemy spawn range
    private readonly float radius = 20.0f;
    private readonly float minRadius = 47.0f;
    private readonly float maxRadius = 52.0f;

    private PlayerController playerControllerScript;
    private GameManager gameManagerScript;
    private FollowPlayer followPlayerScript;
    private float spawnRadius;
    private int enemyCount;
    private int bossCount;

    private int waveNumber;

    public bool bossLevel;
    public float enemySpeed;

    private void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        gameManagerScript = GameObject.Find("Game Manager").GetComponent<GameManager>();
        followPlayerScript = GameObject.Find("Main Camera").GetComponent<FollowPlayer>();
    }

    public void StartSpawning()
    {
        waveNumber = 1;
        enemySpeed = 5.0f;
        SpawnEnemyWave(waveNumber);
        StartCoroutine(SpawnWallPrefab());
        StartCoroutine(SpawnSingleEnemy());
    }

    void Update()
    {
        if (gameManagerScript.gameIsActive)
        {
            enemyCount = FindObjectsOfType<EnemyBehaviour>().Length;
            bossCount = GameObject.FindGameObjectsWithTag("Boss").Length;

            if (enemyCount <= 5 && bossCount == 0 && !bossLevel)
            {
                waveNumber++;
                enemySpeed = enemySpeed + 0.5f;
                
                if (waveNumber % bossRound == 0)
                {
                    StartCoroutine(SpawnBossWave());
                }
                else
                {
                    SpawnEnemyWave(waveNumber);
                }
            }
        }
    }

    private IEnumerator SpawnSingleEnemy()
    {
        yield return new WaitForSeconds(2.0f);

        while (true)
        {
            int enemyCount = Random.Range(1, 3);
            for (int i = 1; i < enemyCount; i++)
            {
                int enemyIndex = Random.Range(0, enemyPrefabs.Count);

                Instantiate(enemyPrefabs[enemyIndex], GetRandomSpawnPos(), enemyPrefabs[enemyIndex].transform.rotation);
                Instantiate(enemyPrefabs[enemyIndex], GetParaElfSpawnPos(), enemyPrefabs[enemyIndex].transform.rotation);
            }

            int spawnInterval = Random.Range(1, 3);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int enemyIndex = Random.Range(0, enemyPrefabs.Count);

            Instantiate(enemyPrefabs[enemyIndex], GetRandomSpawnPos(), enemyPrefabs[enemyIndex].transform.rotation);
            Instantiate(enemyPrefabs[enemyIndex], GetParaElfSpawnPos(), enemyPrefabs[enemyIndex].transform.rotation);
        }
    }

    private IEnumerator SpawnBossWave()
    {
        bossLevel = true;

        Vector3 bossSpawnPos = new(0, 10, 0);
        Vector3 playerBossPos = new(0, 1.0f, -28);

        followPlayerScript.isShaking = true;
        
        /*playerControllerScript.enabled = false;

        // Rotate player towards boss while moving and shaking
        while (player.transform.position != playerBossPos)
        {
            player.transform.LookAt(Vector3.zero);
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerBossPos, playerSpeed * Time.deltaTime);

            yield return null;
        }*/

        followPlayerScript.isShaking = false;

        GameObject tmpBoss = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);
        
        yield return new WaitForSeconds(0.5f);

        // Enable boss and player scripts after intro
        tmpBoss.GetComponent<BossBehaviour>().enabled = true;
        // playerControllerScript.enabled = true;
    }

    private IEnumerator SpawnWallPrefab()
    {
        yield return new WaitForSeconds(5.0f);

        while (true)
        {
            int pillarsToSpawn = Random.Range(1, 8);

            for (int i = 1; i < pillarsToSpawn; i++)
            {
                int pillarIndex = Random.Range(0, pillarPrefabs.Count);

                float angle = Random.Range(0.0f, 360.0f);

                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                Vector3 spawnPos = new Vector3(x, pillarPrefabs[pillarIndex].transform.position.y, z);
                Quaternion rotation = Quaternion.LookRotation(Vector3.zero - spawnPos);

                GameObject tmpWall = Instantiate(pillarPrefabs[pillarIndex], spawnPos, pillarPrefabs[pillarIndex].transform.rotation);

                yield return new WaitForSeconds(2.0f);
            }
        }
    }
    private Vector3 GetRandomSpawnPos()
    {
        spawnRadius = Random.Range(minRadius, maxRadius);
        Vector3 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = new Vector3(randomPoint.x, 0.75f, randomPoint.y);

        return spawnPos;
    }

    private Vector3 GetParaElfSpawnPos()
    {
        spawnRadius = Random.Range(minRadius, maxRadius);
        Vector3 randomSkyPoint = Random.insideUnitSphere.normalized * spawnRadius;
        Vector3 skySpawnPos;

        if (randomSkyPoint.y < 0)
        {
            skySpawnPos = new Vector3(randomSkyPoint.x, -randomSkyPoint.y, randomSkyPoint.z);
        }

        else
        {
            skySpawnPos = randomSkyPoint;
        }

        return skySpawnPos;
    }
}
