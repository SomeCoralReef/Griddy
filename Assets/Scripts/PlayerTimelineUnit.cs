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

        Debug.Log("Game paused for player to select ability.");

        FindObjectOfType<PlayerActionUI>().BeginActionPhase(); // Show action UI
    }

    protected override void OnExecute()
    {
        Debug.Log("Player is executing an action.");
        playerscript.ExecuteAttack();
    }

    void Update()
    {

        if (state == TimelineState.Preparing && actionUI != null && actionUI.HasSelectedAttackAndTile())
        {
            Debug.Log("Player confirmed action. Unpausing timeline.");
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            timelineManager.isPaused = false;

            BeginExecution(); // Player's icon now continues to 100%
        }
    }
}
