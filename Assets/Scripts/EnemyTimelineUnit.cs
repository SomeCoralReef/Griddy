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
        BeginExecution(); // continue to move toward end of timeline
    }

    protected override void OnExecute()
    {
        enemyScript.PerformAction();
        if (enemyScript.isBroken)
        {
            enemyScript.EndBreak();
        }     // decrement Broken duration if Brokenned
    }

    public override void UpdateTimeline()
    {
        base.UpdateTimeline();
    }
}