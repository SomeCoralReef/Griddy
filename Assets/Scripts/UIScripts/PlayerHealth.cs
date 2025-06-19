using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public GameManager gameManager;
    public TextMeshProUGUI healthText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.Instance;


    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = $" {gameManager.playerLives} /  {gameManager.maxHealth}";
    }
}
