
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject playerPrefab;
    public TimelineManager timelineManager;
    public GridManager gridManager;

    [Header("Party Setup")]
    public int playerCount = 1; // We'll support up to 3 later
    

    public int playerSlotSpacing = 10;

    void Start()
    {
        SpawnPlayers(playerCount);
    }

    void SpawnPlayers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int spawnSlot = gridManager.slots;
            GameObject playerGO = Instantiate(playerPrefab, gridManager.GetPlayerSpawnPosition(spawnSlot), Quaternion.identity);

            Player playerScript = playerGO.GetComponent<Player>();
            playerScript.slotIndex = spawnSlot;

            playerGO.transform.position = gridManager.GetPlayerSpawnPosition(spawnSlot);

            timelineManager.RegisterPlayerUnit(playerGO.GetComponent<TimelineUnit>());

            Debug.Log($"Spawned player at slot {spawnSlot} at position {playerGO.transform.position}");
        }
    }
}
