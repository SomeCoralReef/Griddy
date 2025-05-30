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

    private List<Vector2Int> occupiedPositions = new List<Vector2Int>();



    void Start()
    {
        SpawnEnemies(enemyCount);
    }

    // Update is called once per frame
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2Int spawnPos = GetRandomEmptySpawnPosition();
            if (spawnPos == Vector2Int.zero && occupiedPositions.Count >= 4)
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
            timelineManager.RegisterUnit(enemyGO.GetComponent<Enemy>(), enemyGO.GetComponent<TimelineUnit>());

            Enemy enemyScript = enemyGO.GetComponent<Enemy>();
            enemyScript.Initialize(data, spawnPos);
            //Debug.Log($"Spawned {data.enemyName} at {spawnPos}");

            occupiedPositions.Add(spawnPos);
        }
    }

    private Vector2Int GetRandomEmptySpawnPosition()
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();

        // All positions on the leftmost column (x = 0)
        for (int y = 0; y < gridManager.rows; y++)
        {
            Vector2Int pos = new Vector2Int(0, y);
            if (!occupiedPositions.Contains(pos))
                possiblePositions.Add(pos);
        }

        if (possiblePositions.Count == 0)
            return Vector2Int.zero;

        return possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];
    }
}
