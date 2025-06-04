using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimelineIcon : MonoBehaviour
{
    public TimelineUnit linkedUnit;
    public RectTransform barRect;

    private RectTransform iconRect;

    private Image iconImage;


    private bool hasEnteredPrepareZone = false;
    private Vector3 baseScale;
    private Color originalColor;

    [Header("Pulse Effect Settings")]
    public bool isPulsing = false;
    private float pulseTimer = 0f;
    [SerializeField] private float pulseDuration = 1f; // Duration of one pulse cycle
    [SerializeField] private float pulseMagnitude = 0.1f; // Magnitude of the pulse effect

    public float pulseScale = 1.3f;
    public float pulseSpeed = 5f;

    [SerializeField] private Image glowImage; // Assign in Inspector
    private Color originalGlowColor;

    private Vector3 defaultScale = Vector3.one;
    [SerializeField] private Vector3 highlightScale = new Vector3(1.5f,1.5f,1.5f);

    private bool isHighlighted = false;

    private float horizontalOffset = 0f;
    public bool snapToTargetPosition = false;

    void Awake()
    {
        if (glowImage != null)
        {
            originalGlowColor = glowImage.color;
            glowImage.enabled = false;
        }
    }

    void Start()
    {
        iconRect = GetComponent<RectTransform>();
        iconImage = GetComponent<Image>();
        baseScale = iconRect.localScale;

        if (iconImage != null)
            originalColor = iconImage.color;
    }

    void Update()
    {
        if (linkedUnit == null || barRect == null)
        {
            Destroy(gameObject); // Icon auto-destroys when linked unit is dead
            return;
        }
        float t = Mathf.Clamp01(linkedUnit.timelineProgress); // Normalize to 0-1 range for the bar
        float barWidth = barRect.rect.width; // Get the width of the bar

        Vector2 targetPos = new Vector2(t * barWidth + horizontalOffset, iconRect.anchoredPosition.y);
        if (snapToTargetPosition)
        {
            // Instantly move to the target position
            iconRect.anchoredPosition = targetPos;
            snapToTargetPosition = false; // Reset for future updates
        }
        else
        {
            // Smoothly animate to the target position
            iconRect.anchoredPosition = Vector2.Lerp(iconRect.anchoredPosition, targetPos, Time.deltaTime * 10f);
        }

        bool isInPrepareZone = linkedUnit != null && t >= linkedUnit.PrepareThreshold && t < 1.0f;

        if (!hasEnteredPrepareZone && isInPrepareZone)
        {
            hasEnteredPrepareZone = true;
        }
        else if (hasEnteredPrepareZone && !isInPrepareZone)
        {
            hasEnteredPrepareZone = false;
        }

        // Animate scaling


        if (iconImage != null)
        {
            iconImage.color = hasEnteredPrepareZone ? Color.yellow : originalColor;
        }

        if (isPulsing)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scaleOffset = Mathf.Sin(pulseTimer) * pulseMagnitude;
            iconRect.localScale = baseScale * (1.0f + scaleOffset);
        }

        if (isPulsing)
        {
            pulseTimer += Time.deltaTime * pulseDuration;
            float scaleOffset = Mathf.Sin(pulseTimer) * pulseMagnitude;
            iconRect.localScale = baseScale * (1.0f + scaleOffset);
        }
    }

    public void SnapToTarget()
    {
        snapToTargetPosition = true;
    }
    
    public void SetHorizontalOffset(float offset)
    {
        horizontalOffset = offset;
    }


    public IEnumerator PlayBreakEffect(float targetProgress, float duration = 0.8f)
    {

        Debug.Log($"Playing break effect for {linkedUnit.name}. In PlayBreakEffect()");
        RectTransform rt = GetComponent<RectTransform>();
        rt.localScale = defaultScale; // Reset to default scale before starting the effect
        Vector3 originalScale = rt.localScale;
        Vector3 enlargedScale = originalScale * 1.5f;

        float t = 0f;
        float moveStartTime = 0.3f;
        float moveEndTime = duration;

        TimelineUnit unit = linkedUnit;
        unit.isBeingBroken = true; // Set the unit to being broken
        float startProgress = unit.timelineProgress;

        // Scale up immediately
        rt.localScale = enlargedScale;

        while (t < duration)
        {
            t += Time.deltaTime
;
            // Timeline movement back starts at moveStartTime
            if (t >= moveStartTime && t <= moveEndTime)
            {
                float moveT = Mathf.InverseLerp(moveStartTime, moveEndTime, t);
                unit.timelineProgress = Mathf.Lerp(startProgress, targetProgress, moveT);
            }

            // Smoothly scale back to normal
            rt.localScale = Vector3.Lerp(enlargedScale, originalScale, t / duration);

            yield return null;
        }
        //unit.timelineProgress = targetProgress; // Set the final progress
        rt.localScale = originalScale;
        unit.isBeingBroken = false; // Reset the unit's state
        unit.state = TimelineState.Idle; // Reset state to Idle after the effect
        //unit.timelineProgress = targetProgress;
    }

    public IEnumerator resetScale( float duration = 0.3f)
    {
        Debug.Log($"Resetting scale for {linkedUnit.name}. In resetScale()");
        RectTransform rt = GetComponent<RectTransform>();
        Vector3 initialScale = rt.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(initialScale, defaultScale, elapsed / duration);
            yield return null;
        }

        // Ensure the final scale is set correctly
        rt.localScale = defaultScale;
    }

    public void SetHighlight(bool highlight)
    {
        
        if (highlight == isPulsing)
            return;

        isPulsing = highlight;
        if (!highlight)
        {
            iconRect.localScale = baseScale; // Reset to normal size
        }

        if (glowImage != null)
        {
            Debug.Log($"Setting highlight for {linkedUnit.name} to {highlight}");
            glowImage.enabled = highlight;
        }
    }

    /*
    public void SetHighlight(bool highlight)
    {
        if (highlight == isHighlighted)
        {
            Debug.LogWarning("TimelineIcon is already in the requested highlight state.");
            return; // Skip if already in that state
        }
        isHighlighted = highlight;
        StopAllCoroutines();
        Vector3 target = defaultScale; // Default scale is the normal size
        if (highlight)
        {
            target = highlightScale;
        }
        else
        {
            target = defaultScale; // Reset to default scale
        }

        RectTransform rt = GetComponent<RectTransform>();
        //rt.localScale = target; // Immediately set the scale to the target
        StartCoroutine(LerpScale(target));
    }
    */


    private IEnumerator LerpScale(Vector3 target, float duration = 0.3f)
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector3 initial = rt.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(initial, target, elapsed / duration);
            yield return null;
        }

        // After the loop finishes, set the final target scale ONCE
        rt.localScale = target;
    }
}
