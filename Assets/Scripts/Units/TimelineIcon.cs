using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimelineIcon : MonoBehaviour
{
    public TimelineUnit linkedUnit;
    public RectTransform barRect;

    private RectTransform iconRect;

    public SpriteRenderer iconSpriteRenderer;

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

    [SerializeField] private TimelineIconLibrary iconLibrary;


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
        iconSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        baseScale = iconRect.localScale;

        if (iconSpriteRenderer != null)
            originalColor = iconSpriteRenderer.color;


        if (linkedUnit != null && iconLibrary != null)
        {
            Debug.Log("Setting icon for linked unit: " + linkedUnit.name);
            string key = linkedUnit.name; // OR linkedUnit.data.enemyName if more robust
            Sprite iconSprite = iconLibrary.GetIcon(key);
            if (iconSprite != null)
            {
                iconSpriteRenderer.sprite = iconSprite;
            }
        }
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


        if (iconSpriteRenderer != null)
        {
            iconSpriteRenderer.color = hasEnteredPrepareZone ? Color.yellow : originalColor;
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
        if (this == null || linkedUnit == null)
        {
            yield break; // Exit if the icon or linked unit is not set
        }
        RectTransform rt = GetComponent<RectTransform>();
        if(rt == null || linkedUnit == null)
        {
            yield break; // Bail if already destroyed
        }
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
            if(this == null || linkedUnit == null || rt == null)
            {
                yield break; // Exit if the icon or linked unit is not set
            }
            t += Time.deltaTime;
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

        if (rt != null)
        {
            rt.localScale = originalScale;
        }

        if (linkedUnit != null)
        {
            linkedUnit.isBeingBroken = false; // Reset the unit's state
            linkedUnit.state = TimelineState.Idle; // Reset state to Idle after the effect
        }

    }

    public IEnumerator resetScale( float duration = 0.3f)
    {
        if(this == null || linkedUnit == null)
        {
            yield break; // Exit if the icon or linked unit is not set
        }
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) yield break; // Bail if already destroyed

        Vector3 initialScale = rt.localScale;
        float elapsed = 0f;

        
        while (elapsed < duration)
        {
             if (this == null || rt == null) yield break;
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(initialScale, defaultScale, elapsed / duration);
            yield return null;
        }
        
        if( rt!= null)
        {
            rt.localScale = defaultScale;
        }
        // Ensure the final scale is set correctly

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
            glowImage.enabled = highlight;
        }
    }



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
