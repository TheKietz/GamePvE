using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Cài đặt Endless Mode")]
    public List<GameObject> enemyPrefabs; // Kéo Grunt, Tank, Assassin vào đây
    public Transform[] spawnPoints;       // Kéo các điểm Spawn vào đây

    [Header("Cân bằng Game")]
    public int maxEnemiesOnScreen = 8;    // Giới hạn số quái tối đa 1 wave
    public float spawnInterval = 3f;      // Mấy giây ra 1 con?

    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime = 0;

    void Update()
    {
        // 1. Dọn dẹp danh sách quái chết
        activeEnemies.RemoveAll(x => x == null || !x.activeInHierarchy || x.tag != "Enemy");

        // 2. Logic Spawn liên tục
        if (activeEnemies.Count < maxEnemiesOnScreen && Time.time >= nextSpawnTime)
        {
            SpawnRandomEnemy();

            // Đặt thời gian cho lần spawn tiếp theo
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0) return;

        // Random quái và vị trí
        int randomEnemy = Random.Range(0, enemyPrefabs.Count);
        int randomPoint = Random.Range(0, spawnPoints.Length);

        GameObject newEnemy = Instantiate(enemyPrefabs[randomEnemy], spawnPoints[randomPoint].position, spawnPoints[randomPoint].rotation);
        activeEnemies.Add(newEnemy);
    }
}