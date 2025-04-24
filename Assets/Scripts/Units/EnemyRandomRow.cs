using UnityEngine;

public class EnemyRandomRow : Enemy
{
    protected override void Move()
    {
         if (gridPos.x < 7)
        {
            gridPos.x += 1;
            gridPos.y = Random.Range(0, 4); // assumes 4 rows

            transform.position = FindObjectOfType<GridManager>().GetWorldPosition(gridPos.x, gridPos.y);
        }
        else
        {
            Debug.Log($"{data.enemyName} (RandomRow) hit the player for {data.damage}!");
        }
    }
}
