using UnityEngine;

public class EnemyStraight : Enemy
{
    public override void Initialize(EnemyData newData, Vector2Int spawnPos)
    {
        health = 3;
        base.Initialize(newData, spawnPos);
        // Additional initialization for straight-moving enemies if needed
    }
    protected override void Move()
    {
        if (gridPos.x < 7)
        {
            gridPos.x += 1;
            transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);
        }
        else
        {
            Debug.Log($"{data.enemyName} (Straight) reached the player! Dealing {data.damage} damage.");
        }
    }
}
