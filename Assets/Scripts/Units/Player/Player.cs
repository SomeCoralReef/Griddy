using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AttackData selectedAttack;
    private int aimedSlotIndex;
    private GridManager gridManager;
    public int slotIndex;

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
    

    public void AimAtSlot(int slotIndex)
    {
        aimedSlotIndex = slotIndex;
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

        actionUI.StartCoroutine(actionUI.scaleTileSelector(0.2f));

        foreach (var offset in selectedAttack.patternOffsets)
        {
            int targetSlot = aimedSlotIndex + offset;

            // Grid bounds check
            if (targetSlot < 0 || targetSlot >= gridManager.slots)
            {
                //Debug.Log($"Target {target} out of bounds.");
                continue;
            }
            // Hit check (placeholder logic)
            Enemy enemy = EnemyAt(targetSlot);
            if (enemy != null)
            {
                bool wasBroken = enemy.TakeDamage(selectedAttack.elementType, selectedAttack.power);
                enemy.timelineIcon.SetHighlight(false);
                if (enemy.timelineIcon != null)
                {
                    enemy.timelineIcon.isPulsing = false; // Start pulsing effect
                    Debug.Log("hi");
                    if (wasBroken)
                    {
                        // The enemy was broken → play break effect
                        StartCoroutine(enemy.timelineIcon.PlayBreakEffect(0.0f)); // or targetProgress = 0.0f
                    }
                    else
                    {
                        // Not broken → reset the scale visually
                        if (enemy.timelineIcon != null)
                            StartCoroutine(enemy.timelineIcon.resetScale());
                    }
                }
            }
        }
    }

       


    private Enemy EnemyAt(int slotIndex)
    {
        // Replace with proper tracking system (e.g. EnemyManager or 2D array map)
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            if (e.slotIndex == slotIndex)
                return e;
        }
        return null;
    }
}
