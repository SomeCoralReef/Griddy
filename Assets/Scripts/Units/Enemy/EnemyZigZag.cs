using UnityEngine;

public class EnemyZigZag : Enemy
{
    private int yDirection = 1;

    public override void Initialize(EnemyData newData, int spawnPos)
    {
        health = 2;
        base.Initialize(newData, spawnPos);

        // Maybe zig-zag enemies are faster!
        speed *= 1.2f;
    }
}
