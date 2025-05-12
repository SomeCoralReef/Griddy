using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject playerPrefab;
    public TimelineManager timelineManager;
    public GridManager gridManager;

    [Header("Party Setup")]
    public int playerCount = 1; // We'll support up to 3 later

    void Start()
    {
        SpawnPlayers(playerCount);
    }

    void SpawnPlayers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Right now, spawn just off-grid (x = 8), place on row i
            Vector2Int spawnPos = new Vector2Int(8, i);
            GameObject playerGO = Instantiate(playerPrefab);

            Player playerScript = playerGO.GetComponent<Player>();
            playerScript.gridPos = spawnPos;

            playerGO.transform.position = gridManager.GetWorldPosition(spawnPos.x, spawnPos.y);

            // Register to timeline
            timelineManager.RegisterUnit(playerGO.GetComponent<TimelineUnit>());

            //Debug.Log($"Spawned player {i + 1} at grid {spawnPos}");
        }
    }
}
