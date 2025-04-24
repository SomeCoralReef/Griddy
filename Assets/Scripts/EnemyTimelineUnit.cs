using UnityEngine;

public class EnemyTimelineUnit : TimelineUnit
{
    private Enemy enemyScript;

    private void Start()
    {
        enemyScript = GetComponent<Enemy>();
    }

    protected override void OnPrepare()
    {
        Debug.Log($"{enemyScript.data.enemyName} preparing action...");

        // Optionally calculate target tile or action logic here
        // No player input — it auto-prepares

        BeginExecution(); // continue to move toward end of timeline
    }

    protected override void OnExecute()
    {
        enemyScript.PerformAction(); // move or attack
    }
}