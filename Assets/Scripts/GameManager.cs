using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int numberOfPlayersInParty = 2;

    [Header("Player Stats")]
    public int playerLives = 88;
    public int maxHealth = 88;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoseLife(int amount)
    {
        playerLives -= amount;
        Debug.Log($"Player lost {amount} lives. Lives Left: {playerLives}");
        if (playerLives <= 0)
        {
            // Handle game over logic here
        }
    }

    public void ResetGame()
    {
        playerLives = 88;
        Debug.Log("Game reset. Player lives restored.");
    }
}
