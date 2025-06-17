using UnityEngine;

public class EnemyStraight : Enemy
{
    public override void Initialize(EnemyData newData, int spawnPos)
    {
        health = 3;
        base.Initialize(newData, spawnPos);
        // Additional initialization for straight-moving enemies if needed
    }
}
