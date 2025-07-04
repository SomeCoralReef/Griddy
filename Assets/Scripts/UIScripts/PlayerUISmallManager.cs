using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUISmallManager : MonoBehaviour
{
    public static PlayerUISmallManager Instance { get; private set; }
    public GameObject playerUISmallPrefab;
    private float playerUISpacingBetween = -125f;
    public GameManager gameManager;
    public GameObject CanvasParent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < gameManager.numberOfPlayersInParty; i++)
        {
            Vector3 position = this.transform.position; // Adjust position as needed
            GameObject playerUI = Instantiate(playerUISmallPrefab, position + new Vector3(0, i * playerUISpacingBetween, 0), Quaternion.identity, CanvasParent.transform);
            playerUI.GetComponentInChildren<Scrollbar>().value = Normalize(GameManager.Instance.playerLives, 0f, GameManager.Instance.maxHealth);
            playerUI.name = "PlayerUISmall" + (i + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var playerUI in CanvasParent.GetComponentsInChildren<Scrollbar>())
        {
            if (playerUI != null)
            {
                playerUI.value = Normalize(GameManager.Instance.playerLives, 0f, GameManager.Instance.maxHealth);
            }
        }
    }

    float Normalize(float value, float min, float max)
    {
        // Prevent division by zero
        if (Mathf.Approximately(max - min, 0f))
        {
            Debug.LogWarning("Normalize: min and max are equal; returning 0.");
            return 0f;
        }
        return Mathf.Clamp01((value - min) / (max - min));
    }
}
