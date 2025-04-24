using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    protected float moveCooldown;
    public Vector2Int gridPos;
    protected float timerProgress = 0f;
    protected bool isStunned = false;
    protected int stunnedTurnCounter = 0;


    public virtual void Initialize(EnemyData newData, Vector2Int spawnPos)
    {
        data = newData;
        gridPos = spawnPos;
        transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);
        moveCooldown = data.speed;
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
            Debug.Log($"{data.enemyName} hit the player! Dealing {data.damage} damage.");
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
        Debug.Log($"{data.enemyName} took {amount} {type} damage!");

        // Check if the element matches any weakness
        for (int i = 0; i < data.weaknesses.Length; i++)
        {
            if (data.weaknesses[i] == type)
            {
                data.weaknesses[i] = ElementType.None;
                Debug.Log($"{data.enemyName}'s weakness broken: {type}");
            }
        }

    // TODO: Check if all 3 weaknesses are broken → apply stun
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
