using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    protected float moveCooldown;
    public Vector2Int gridPos;
    protected float timerProgress = 0f;
    protected bool isStunned = false;
    protected int stunnedTurnCounter = 0;

    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;

    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private Canvas damageSpaceCanvas;

    [SerializeField] private Slider healthBar;


    public virtual void Initialize(EnemyData newData, Vector2Int spawnPos)
    {
        data = newData;
        gridPos = spawnPos;
        transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);

        health = data.health;
        damage = data.damage;
        moveCooldown = data.speed;
        damageSpaceCanvas = GameObject.Find("DamageNumbersCanvas").GetComponent<Canvas>();

        healthBar.maxValue = data.health;
        healthBar.value = health;
    }

    protected virtual void Update()
    {
        if(isStunned) return;

        timerProgress += Time.deltaTime;

        /*if(timerProgress >= moveCooldown)
        {
            Move();
            timerProgress = 0f;
        }*/

    }

    protected virtual void Move()
    {
        TryMoveRight();
    }

    protected void TryMoveRight()
    {
        if (gridPos.x < 7)
        {
            gridPos.x += 1;
            transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);
        }
        else
        {
            //Debug.Log($"{data.enemyName} hit the player! Dealing {data.damage} damage.");
        }
    }

    public void Stun()
    {
        isStunned = true;
        stunnedTurnCounter = 1;
        timerProgress = 0f;
    }

    public void ResolveStun()
    {
        stunnedTurnCounter--;
        if (stunnedTurnCounter <= 0)
            isStunned = false;
    }

    public void TakeDamage(ElementType type, float amount)
    {
        health -= Mathf.RoundToInt(amount);
        healthBar.value = health;

        Debug.Log($"{data.enemyName} took {amount} damage. Remaining HP: {health}");

        ShowFloatingDamage(amount);

        // Check weaknesses if needed
        for (int i = 0; i < data.weaknesses.Length; i++)
        {
            if (data.weaknesses[i] == type)
            {
                data.weaknesses[i] = ElementType.None;
                Debug.Log($"{data.enemyName}'s weakness broken: {type}");
            }
        }

        if (health <= 0)
        {
            Die();
        }

    // TODO: Check if all 3 weaknesses are broken → apply stun
    }

    private void ShowFloatingDamage(float amount)
    {
        if (floatingDamagePrefab == null || damageSpaceCanvas == null) return;

        Vector3 spawnPos = transform.position; // Lift text slightly
        Debug.Log($"Spawning floating damage text at {spawnPos}");
        GameObject dmgObj = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity, damageSpaceCanvas.transform);
        if(dmgObj == null)
        {
            Debug.LogError("Floating damage prefab is null.");
            return;
        }
        DamageTextUI dmgText = dmgObj.GetComponent<DamageTextUI>();
        if (dmgText != null)
        {
            Debug.Log("DamageTextUI component found.");
            dmgText.Setup(amount.ToString());
        } else
        {
            Debug.LogError("DamageTextUI component not found on the prefab.");
        }
    }

    private void Die()
    {
        Debug.Log($"{data.enemyName} died!");

        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.UnregisterUnit(GetComponent<TimelineUnit>());

        Destroy(gameObject);
    }

    public virtual void PerformAction()
    {
        if (isStunned)
        {
            Debug.Log($"{data.enemyName} is stunned and skips turn.");
            ResolveStun();
            return;
        }

        //Debug.Log($"{data.enemyName} is performing their action (default: Move).");
        Move();
    }
}
