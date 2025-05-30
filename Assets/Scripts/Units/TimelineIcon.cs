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
    public float pulseScale = 1.3f;
    public float pulseSpeed = 5f;

    private Vector3 defaultScale = Vector3.one;
    [SerializeField] private Vector3 highlightScale = new Vector3(1.5f,1.5f,1.5f);

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

        iconRect.anchoredPosition = new Vector2(t * barWidth, iconRect.anchoredPosition.y); // Set the icon's position based on the timeline progress

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
        Vector3 targetScale = hasEnteredPrepareZone ? baseScale * pulseScale : baseScale;
        iconRect.localScale = Vector3.Lerp(iconRect.localScale, targetScale, Time.deltaTime * pulseSpeed);

        if (iconImage != null)
        {
            iconImage.color = hasEnteredPrepareZone ? Color.yellow : originalColor;
        }
    }

    public IEnumerator PlayBreakEffect(float targetProgress, float duration = 0.8f)
    {
        
        Debug.Log($"Playing break effect for {linkedUnit.name}. In PlayBreakEffect()");
        RectTransform rt = GetComponent<RectTransform>();
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
                unit.timelineProgress = Mathf.Lerp( startProgress, targetProgress,moveT);
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

    public void SetHighlight(bool isHighlighted)
    {
        Debug.Log($"Setting highlight for {linkedUnit.name} to {isHighlighted}");
        StopAllCoroutines();
        Vector3 targetScale = isHighlighted ? highlightScale : defaultScale;
        StartCoroutine(LerpScale(targetScale, 0.2f));
    }

    private IEnumerator LerpScale(Vector3 target, float duration)
    {
        Vector3 initial = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initial, target, elapsed / duration);
            yield return null;
        }

        transform.localScale = target;
    }
}
