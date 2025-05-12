using UnityEngine;
using UnityEngine.UI;

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
    public float pulseSpeed =5f;


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
        if(linkedUnit == null || barRect == null)
        {
            Destroy(gameObject); // Icon auto-destroys when linked unit is dead
            return;
        }


        float t = Mathf.Clamp01(linkedUnit.timelineProgress); // Normalize to 0-1 range for the bar
        float barWidth = barRect.rect.width; // Get the width of the bar

        iconRect.anchoredPosition = new Vector2(t* barWidth, iconRect.anchoredPosition.y); // Set the icon's position based on the timeline progress
        
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
}
