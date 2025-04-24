using UnityEngine;

public class EnemyZigZag : Enemy
{
    private int yDirection = 1;

    protected override void Move()
    {
        if(gridPos.x < 7)
        {
            gridPos.x += 1;

            gridPos.y +=  yDirection;
            if(gridPos.y >= 3 || gridPos.y <= 0)
            {
                yDirection *= -1; // Reverse direction
            }

            transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);
        }
        else 
        {
            Debug.Log($"{data.enemyName} hit the player! Dealing {data.damage} damage.");
        }
    }
}
