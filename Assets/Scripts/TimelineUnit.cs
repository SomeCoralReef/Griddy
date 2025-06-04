using UnityEngine;

public class TimelineUnit : MonoBehaviour
{
    public float speed = 0.2f; // units per second along timeline
    public float timelineProgress = 0f; // 0 to 1
    public TimelineState state = TimelineState.Idle;
    protected bool isPlayerControlled = false;
    public bool DebugMode = false; // Set to true for debugging purposes
    private bool hasTriggeredPrepare = false;

    [SerializeField] protected float prepareThreshold = 0.65f;
    public float PrepareThreshold => prepareThreshold;

    public bool isBeingBroken = false;



    public virtual void UpdateTimeline()
    {
        if (TimelineManagerIsPaused() || isBeingBroken) return;
        switch (state)
        {
            case TimelineState.Idle:
                timelineProgress += speed * Time.deltaTime;

                if (timelineProgress >= PrepareThreshold)
                {
                    timelineProgress = PrepareThreshold;
                    state = TimelineState.Preparing;
                    OnPrepare();
                }
                break;

            case TimelineState.Paused:
                // Pause logic
                break;

            case TimelineState.Preparing:
                // Wait for player input or auto-execute logic
                break;

            case TimelineState.Executing:
                timelineProgress += speed * Time.deltaTime;
                if (timelineProgress >= 1.0f)
                {
                    OnExecute();
                    ResetTimeline();
                }
                break;
        }
    }


    private bool TimelineManagerIsPaused()
    {
        return FindObjectOfType<TimelineManager>().isPaused;
    }
    protected virtual void OnPrepare()
    {
        Debug.Log("Virtual Prepare called");

    }

    protected virtual void OnExecute()
    {
        // Final action when timeline completes
    }

    protected void ResetTimeline()
    {
        timelineProgress = 0f;
        state = TimelineState.Idle;
        hasTriggeredPrepare = false;

        TimelineIcon icon = FindObjectOfType<TimelineManager>().GetIconForUnit(this);
        icon.SnapToTarget();
    }

    public void BeginExecution()
    {
        state = TimelineState.Executing;
    }
}
