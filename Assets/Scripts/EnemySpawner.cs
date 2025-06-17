using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{

    [Header("References")]
    public GridManager gridManager;

    [Header("Enemy Setup")]
    public List<GameObject> enemyPrefabs;
    public List<EnemyData> enemyDatas;
    public int enemyCount = 3;

    private List<int> occupiedSlots = new List<int>();



    void Start()
    {
        SpawnEnemies(enemyCount);
    }

    // Update is called once per frame
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int spawnSlot = GetRandomEmptySpawnPosition();
            if (spawnSlot == -1 && occupiedSlots.Count >= 4)
            {
                Debug.LogWarning("No more spawn positions available.");
                return;
            }

            // Random enemy type
            int randomIndex = UnityEngine.Random.Range(0, enemyPrefabs.Count);
            GameObject prefab = enemyPrefabs[randomIndex];
            EnemyData data = enemyDatas[randomIndex];

            GameObject enemyGO = Instantiate(prefab);
            
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            timelineManager.RegisterEnemyUnit(enemyGO.GetComponent<Enemy>(), enemyGO.GetComponent<TimelineUnit>());

            Enemy enemyScript = enemyGO.GetComponent<Enemy>();
            enemyScript.Initialize(data, spawnSlot);
            //Debug.Log($"Spawned {data.enemyName} at {spawnPos}");

            occupiedSlots.Add(spawnSlot);
        }
    }

    private int GetRandomEmptySpawnPosition()
    {
        List<int> possiblePositions = new List<int>();

        // All positions on the leftmost column (x = 0)
        for (int slot = 0; slot < gridManager.slots; slot++)
        {
            if (!occupiedSlots.Contains(slot))
                possiblePositions.Add(slot);
        }

        if (possiblePositions.Count == 0)
            return -1;

        return possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];
    }
}
