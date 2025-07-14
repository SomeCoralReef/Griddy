using UnityEngine;

public class PlayerTimelineUnit : TimelineUnit
{
    private Player playerscript;
    private PlayerActionUI actionUI;

    private void Start()
    {
        isPlayerControlled = true;
        state = TimelineState.Idle;
        timelineProgress = 0f;
        playerscript = GetComponent<Player>();
        actionUI = FindObjectOfType<PlayerActionUI>();
    }

    protected override void OnPrepare()
    {
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = true;
        PlayerActionUI playerActionUI = FindObjectOfType<PlayerActionUI>(); // Show action UI
        if (playerActionUI != null)
        {
            playerActionUI.BeginActionPhase();
        }
        else
        {
            Debug.Log("PlayerActionUI not found in the scene.");
        }
        
    }

    protected override void OnExecute()
    {
        playerscript.ExecuteAction();
    }

    void Update()
    {
        if (state == TimelineState.Preparing && actionUI != null && actionUI.HasSelectedAttackAndTile())
        {
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            timelineManager.isPaused = false;

            BeginExecution(); // Player's icon now continues to 100%
        }
    }
}
