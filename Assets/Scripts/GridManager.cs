using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("New RPG Slot Grid")]
    public int slots = 4;
    public float slotSpacing = 2f;

    public float slotX = 5f;

    public GameObject slotPrefab;

    public Transform slotsParent;

    [HideInInspector] public Vector3[] slotPositions;


    private void Awake()
    {
        CreateSlots();
    }

    private void CreateSlots()
    {
        slotPositions = new Vector3[slots];
        for (int i = 0; i < slots; i++)
        {
            Vector3 pos = new Vector3(i * slotSpacing, 0, 0);
            slotPositions[i] = pos;

            if (slotPrefab != null)
            {
                Instantiate(slotPrefab, pos, Quaternion.identity, slotsParent);
            }
            else
            {
                Debug.LogWarning("Slot prefab is not assigned in GridManager.");
            }
        }
    }
    public Vector3 GetWorldPositionForSlot(int slotIndex)
    {
        float enemySideX = 8f; // right side
        float baseY = 0f;
        float verticalSpacing = 1f;
        float y = baseY + slotIndex * verticalSpacing;
        return new Vector3(enemySideX, y, 0);
    }

    public Vector3 GetPlayerSpawnPosition(int playerIndex)
    {
        float playerSideX = -3f; // left side of screen
        float baseY = -8f;
        float verticalSpacing = 2f;
        float y = baseY + playerIndex * verticalSpacing;
        return new Vector3(playerSideX, y, 0);
    }
}
