using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int playerLives = 3;

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

    public void LoseLife()
    {
        playerLives--;
        Debug.Log($"Player lost a life. Lives Left: {playerLives}");
        if (playerLives <= 0)
        {
            // HWandle game over logic here
        }
    }

    public void ResetGame()
    {
        playerLives = 3;
        Debug.Log("Game reset. Player lives restored.");
    }
}
