using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;
    public float spawnRadius = 15f;
    public float minSpawnDistance = 5f;
    public float spawnInterval = 5f;
    public float raycastHeightOffset = 5f;

    [Header("Wave Settings")]
    public int minEnemiesPerWave = 1;
    public int maxEnemiesPerWave = 3;

    [Header("Spawn Control")]
    public Camera mainCamera;
    public bool useRandomEnemy = true;
    public string specificEnemyID = "Zombie";

    [Header("Layers")]
    public LayerMask groundLayer;

    private Coroutine _spawnRoutine;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        StartSpawning();
    }

    public void StartSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
    }

    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }

    public void SetWaveSize(int newMin, int newMax)
    {
        minEnemiesPerWave = newMin;
        maxEnemiesPerWave = newMax;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWave();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnWave()
    {
        if (player == null) return;

        int enemiesToSpawn = Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);
        int attempts = 0;
        int spawned = 0;

        while (spawned < enemiesToSpawn && attempts < enemiesToSpawn * 10)
        {
            attempts++;

            bool tryFrontOfCamera = Random.value < 0.5f; // 50% chance to try spawning in front

            Vector3 candidatePos;

            if (tryFrontOfCamera)
            {
                // Pick a point in front of the camera within the spawnRadius
                Vector3 forward = mainCamera.transform.forward;
                Vector3 randomOffset = (Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * forward) * Random.Range(minSpawnDistance, spawnRadius);
                candidatePos = player.position + randomOffset + Vector3.up * raycastHeightOffset;
            }
            else
            {
                // Original random circle around player
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                candidatePos = player.position + new Vector3(randomCircle.x, raycastHeightOffset, randomCircle.y);
            }

            // Too close to player
            if (Vector3.Distance(candidatePos, player.position) < minSpawnDistance)
                continue;

            // Raycast to ground
            if (Physics.Raycast(candidatePos, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 spawnPos = hit.point;

                bool canSpawn = false;

                if (!tryFrontOfCamera)
                {
                    // Spawn behind/side, ensure not visible
                    canSpawn = !IsVisibleFromCamera(spawnPos);
                }
                else
                {
                    // Spawn in front, but ensure blocked
                    if (IsInFrontOfCamera(spawnPos) && IsBlockedFromCamera(spawnPos))
                        canSpawn = true;
                }

                // On NavMesh and allowed by visibility rules
                if (canSpawn && NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                {
                    Vector3 finalPos = navHit.position + Vector3.up * 0.05f;

                    if (useRandomEnemy)
                        EnemyManager.Instance.SpawnRandom(finalPos, Quaternion.identity);
                    else
                        EnemyManager.Instance.Spawn(specificEnemyID, finalPos, Quaternion.identity);

                    spawned++;
                }
            }
        }
    }

    private bool IsInFrontOfCamera(Vector3 worldPos)
    {
        Vector3 dirToPos = (worldPos - mainCamera.transform.position).normalized;
        return Vector3.Dot(mainCamera.transform.forward, dirToPos) > 0; // > 0 means in front
    }

    private bool IsBlockedFromCamera(Vector3 worldPos)
    {
        if (Physics.Raycast(mainCamera.transform.position,
                            (worldPos - mainCamera.transform.position).normalized,
                            out RaycastHit hit))
        {
            // If the first thing hit isn't the spawn point itself, it's blocked
            return Vector3.Distance(hit.point, worldPos) > 0.5f;
        }
        return false;
    }


    private bool IsVisibleFromCamera(Vector3 worldPos)
    {
        if (mainCamera == null) return false;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);
        bool inView = viewportPos.z > 0 &&
                      viewportPos.x > 0 && viewportPos.x < 1 &&
                      viewportPos.y > 0 && viewportPos.y < 1;

        if (!inView) return false;

        if (Physics.Raycast(mainCamera.transform.position,
                            (worldPos - mainCamera.transform.position).normalized,
                            out RaycastHit hit))
        {
            if (hit.collider != null && Vector3.Distance(hit.point, worldPos) < 0.5f)
                return true;
        }

        return false;
    }

}
