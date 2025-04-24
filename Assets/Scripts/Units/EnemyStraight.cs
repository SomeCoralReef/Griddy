using UnityEngine;

public class EnemyStraight : Enemy
{
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
