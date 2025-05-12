using UnityEngine;

public class Player : MonoBehaviour
{
    public AttackData selectedAttack;
    private Vector2Int aimedTile;
    private GridManager gridManager;
    public Vector2Int gridPos;

    public AttackData blastAttack;
    public PlayerActionUI actionUI;

    public bool IsInPreparePhase;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        actionUI = FindObjectOfType<PlayerActionUI>();
    }
    void Update()
    {
        if (GetComponent<PlayerTimelineUnit>().state == TimelineState.Preparing)
        {
            IsInPreparePhase = true;
        }
        else
        {
            IsInPreparePhase = false;
        }
    }
    public void SelectAttack(AttackData attack)
    {
        selectedAttack = attack;
    }
    

    public void AimAtTile(Vector2Int tilePos)
    {
        aimedTile = tilePos;
    }


    public void OnConfirmAction()
    {
        // Resume timeline
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
        GetComponent<TimelineUnit>().BeginExecution();
    }

    public void ExecuteAttack()
    {
        if (selectedAttack == null)
        {
            Debug.LogWarning("No attack selected on execution.");
            return;
        }

        actionUI.StartCoroutine(actionUI.scaleTileSelector(0.5f));

        foreach (var offset in selectedAttack.patternOffsets)
        {
            Vector2Int target = aimedTile + offset;

            // Grid bounds check
            if (target.x < 0 || target.x >= gridManager.columns ||
                target.y < 0 || target.y >= gridManager.rows)
            {
                //Debug.Log($"Target {target} out of bounds.");
                continue;
            }
            // Hit check (placeholder logic)
            Enemy enemy = EnemyAt(target);
            if (enemy != null)
            {
                enemy.TakeDamage(selectedAttack.elementType, selectedAttack.power);
            }
        }

       
    }

    private Enemy EnemyAt(Vector2Int pos)
    {
        // Replace with proper tracking system (e.g. EnemyManager or 2D array map)
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            if (e.gridPos == pos)
                return e;
        }
        return null;
    }
}
