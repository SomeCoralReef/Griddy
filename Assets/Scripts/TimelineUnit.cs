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
    public virtual void UpdateTimeline()
    {
        if (TimelineManagerIsPaused()) return;
        
        if(DebugMode){Debug.Log($"{name} | State: {state} | Progress: {timelineProgress:F2}");}
        // ✅ Do nothing if we're in Preparing — timeline shouldn't move
        if (state == TimelineState.Preparing)
        {
            if(DebugMode){Debug.Log($"{name} is in Preparing phase. Skipping timeline update.");}
            return;
        }

        switch (state)
        {
            case TimelineState.Idle:
                timelineProgress += speed * Time.deltaTime;
                if (timelineProgress >= PrepareThreshold)
                {
                    timelineProgress = prepareThreshold;
                    state = TimelineState.Preparing;
                    hasTriggeredPrepare = true;
        
                    OnPrepare(); // This sets isPaused = true for the first time
                }
                break;

            case TimelineState.Executing:
                timelineProgress += speed * Time.deltaTime;
                if (timelineProgress >= 1.0f)
                {
                    timelineProgress = 1.0f;
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
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            timelineManager.isPaused = true;

            FindObjectOfType<PlayerActionUI>().BeginActionPhase(); // Show action UI
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
    }

    public void BeginExecution()
    {
        state = TimelineState.Executing;
    }
}
