
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawnManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public float activationRange = 15f;

    [Header("Spawn Points - ใส่ 12 จุด")]
    public Transform[] spawnPoints = new Transform[12];

    [Header("Zombie Prefabs")]
    public GameObject[] zombiePrefabs;

    [Header("Spawn Settings")]
    public int minZombiesPerSpawn = 1;
    public int maxZombiesPerSpawn = 4;
    public float respawnCooldown = 15f;
    public int maxAliveZombies = 32;

    [Header("NavMesh")]
    public float navMeshSearchRadius = 3f;

    private float[] nextSpawnTime;
    private readonly List<GameObject> aliveZombies =
        new List<GameObject>();

    private void Start()
    {
        nextSpawnTime = new float[spawnPoints.Length];

        for (int i = 0; i < nextSpawnTime.Length; i++)
            nextSpawnTime[i] = 0f;

        FindPlayer();

        Debug.Log(
            "ZombieSpawnManager started. " +
            "Spawn points: " + spawnPoints.Length +
            ", Zombie prefabs: " +
            (zombiePrefabs == null ? 0 : zombiePrefabs.Length),
            this
        );

        if (player == null)
        {
            Debug.LogError(
                "ไม่พบ Player! ตรวจ Tag ของตัวละครให้เป็น Player",
                this
            );
        }

        if (zombiePrefabs == null || zombiePrefabs.Length == 0)
        {
            Debug.LogError(
                "ยังไม่ได้ใส่ Zombie Prefabs ใน Inspector",
                this
            );
        }

        bool hasSpawnPoint = false;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                hasSpawnPoint = true;
                break;
            }
        }

        if (!hasSpawnPoint)
        {
            Debug.LogError(
                "ยังไม่ได้ใส่ Spawn Points ใน Inspector",
                this
            );
        }
    }

    private void Update()
    {
        RemoveDeadReferences();

        if (player == null)
        {
            FindPlayer();
            if (player == null)
                return;
        }

        if (zombiePrefabs == null || zombiePrefabs.Length == 0)
            return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];

            if (point == null)
                continue;

            float distance = Vector3.Distance(
                player.position,
                point.position
            );

            if (distance > activationRange)
                continue;

            if (Time.time < nextSpawnTime[i])
                continue;

            int availableSlots =
                maxAliveZombies - aliveZombies.Count;

            if (availableSlots <= 0)
                return;

            SpawnZombieGroup(point, i, availableSlots);
        }
    }

    private void FindPlayer()
    {
        GameObject foundPlayer =
            GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
            Debug.Log("พบ Player: " + foundPlayer.name, this);
        }
    }

    private void SpawnZombieGroup(
        Transform point,
        int pointIndex,
        int availableSlots)
    {
        if (!NavMesh.SamplePosition(
            point.position,
            out NavMeshHit hit,
            navMeshSearchRadius,
            NavMesh.AllAreas))
        {
            Debug.LogWarning(
                "ไม่พบ NavMesh ใกล้จุดเกิด: " + point.name +
                " ลองตรวจ NavMesh และตำแหน่ง Spawn Point",
                point
            );
            return;
        }

        List<GameObject> choices = new List<GameObject>();

        foreach (GameObject prefab in zombiePrefabs)
        {
            if (prefab != null)
                choices.Add(prefab);
        }

        if (choices.Count == 0)
        {
            Debug.LogError("Zombie Prefabs ทุกช่องว่างเปล่า!", this);
            return;
        }

        int minCount = Mathf.Max(1, minZombiesPerSpawn);
        int maxCount = Mathf.Max(minCount, maxZombiesPerSpawn);

        int requestedCount = Random.Range(
            minCount,
            maxCount + 1
        );

        int spawnCount = Mathf.Min(
            requestedCount,
            availableSlots
        );

        int spawnedCount = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab =
                choices[Random.Range(0, choices.Count)];

            GameObject zombie = Instantiate(
                prefab,
                hit.position,
                point.rotation
            );

            NavMeshAgent agent =
                zombie.GetComponent<NavMeshAgent>();

            if (agent != null && !agent.isOnNavMesh)
            {
                if (!agent.Warp(hit.position))
                {
                    Debug.LogWarning(
                        "ซอมบี้ไม่สามารถวางบน NavMesh ได้: " +
                        zombie.name,
                        zombie
                    );

                    Destroy(zombie);
                    continue;
                }
            }

            aliveZombies.Add(zombie);
            spawnedCount++;
        }

        if (spawnedCount > 0)
        {
            nextSpawnTime[pointIndex] =
                Time.time + respawnCooldown;

            Debug.Log(
                point.name + " เกิดซอมบี้ " + spawnedCount +
                " ตัว | รวม " + aliveZombies.Count +
                "/" + maxAliveZombies,
                this
            );
        }
    }

    private void RemoveDeadReferences()
    {
        aliveZombies.RemoveAll(zombie => zombie == null);
    }

    public int GetAliveZombieCount()
    {
        RemoveDeadReferences();
        return aliveZombies.Count;
    }
}
