
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyOrbSpawner : MonoBehaviour
{
    [Header("Orb Settings")]
    public GameObject orbPrefab;
    public Transform[] spawnPoints = new Transform[8];

    [Header("Spawn Rules")]
    public int maxOrbs = 2;
    public float respawnDelay = 60f;

    private readonly List<GameObject> activeOrbs =
        new List<GameObject>();

    private readonly List<int> availablePoints =
        new List<int>();

    private void Start()
    {
        if (orbPrefab == null)
        {
            Debug.LogError("กรุณาใส่ Orb Prefab");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("กรุณากำหนดจุดเกิด");
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                availablePoints.Add(i);
        }

        SpawnUntilFull();
    }

    public void CollectOrb(GameObject orb)
    {
        if (activeOrbs.Remove(orb))
            StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnUntilFull();
    }

    private void SpawnUntilFull()
    {
        activeOrbs.RemoveAll(
            orb => orb == null || !orb.activeInHierarchy
        );

        while (activeOrbs.Count < maxOrbs &&
               availablePoints.Count > 0)
        {
            int randomIndex =
                Random.Range(0, availablePoints.Count);

            int pointIndex = availablePoints[randomIndex];
            availablePoints.RemoveAt(randomIndex);

            Transform point = spawnPoints[pointIndex];

            GameObject orb = Instantiate(
                orbPrefab,
                point.position,
                Quaternion.identity
            );

            activeOrbs.Add(orb);
        }
    }
}
