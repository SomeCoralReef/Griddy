using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour
{
    public GameObject timelineIconPrefab;       // Drag your UnitIcon prefab here
    public RectTransform timelineBar;           // Drag the TimelineBar UI RectTransform

    public bool isPaused = false;

    private List<TimelineUnit> units = new List<TimelineUnit>();
    public List<TimelineIcon> timelineIcons = new List<TimelineIcon>();

    public void RegisterEnemyUnit(Enemy enemy, TimelineUnit unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Tried to register a null TimelineUnit!");
            return;
        }

        units.Add(unit);

        //TO DO: Replace with with it's own icon prefab based on enemy type
        GameObject icon = Instantiate(timelineIconPrefab, timelineBar);
        TimelineIcon iconScript = icon.GetComponent<TimelineIcon>();
        enemy.timelineIcon = iconScript; // Link the icon to the enemy
        iconScript.linkedUnit = unit;
        iconScript.barRect = timelineBar;
        iconScript.isFriendly = false;

        Image iconSprite = icon.GetComponent<Image>();
        iconSprite.sprite = enemy.timelineIconSprite;

        timelineIcons.Add(iconScript);
    }

    public void RegisterPlayerUnit(TimelineUnit unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Tried to register a null TimelineUnit!");
            return;
        }

        units.Add(unit);

        GameObject icon = Instantiate(timelineIconPrefab, timelineBar);
        TimelineIcon iconScript = icon.GetComponent<TimelineIcon>();
        iconScript.linkedUnit = unit;
        iconScript.barRect = timelineBar;
        iconScript.isFriendly = true;

        Image iconSpriteRenderer = icon.GetComponent<Image>();

        

        timelineIcons.Add(iconScript);
    }


    void Update()
    {
        float playerSpeedMultiplier = 1f;

        if (Input.GetKey(KeyCode.E))
        {
            playerSpeedMultiplier = 1.2f;
            TimelineIcon[] timelineIcons = FindObjectsOfType<TimelineIcon>();
            foreach (var timelineIcon in timelineIcons)
            {
                if (timelineIcon.isFriendly)
                {
                    GlowVFXTimeline glowVFX = timelineIcon.GetComponent<GlowVFXTimeline>();
                    if (glowVFX != null)
                    {
                        glowVFX.ShowFastGlow();
                    }
                    else
                    {
                        Debug.Log("GlowVFXTimeline not found on friendly icon!");
                    }
                }
            }
        }

        else if (Input.GetKey(KeyCode.Q))
        {
            playerSpeedMultiplier = 0.8f;
            TimelineIcon[] timelineIcons = FindObjectsOfType<TimelineIcon>();
            foreach (var timelineIcon in timelineIcons)
            {
                if (timelineIcon.isFriendly)
                {
                    GlowVFXTimeline glowVFX = timelineIcon.GetComponent<GlowVFXTimeline>();
                    if (glowVFX != null)
                    {
                        glowVFX.ShowSlowGlow();
                    }
                    else
                    {
                        Debug.Log("GlowVFXTimeline not found on friendly icon!");
                    }
                }
            }
        }
        else
        {
            playerSpeedMultiplier = 1f; // Reset to normal speed
            TimelineIcon[] timelineIcons = FindObjectsOfType<TimelineIcon>();
            foreach (var timelineIcon in timelineIcons)
            {
                if (timelineIcon.isFriendly)
                {
                    GlowVFXTimeline glowVFX = timelineIcon.GetComponent<GlowVFXTimeline>();
                    if (glowVFX != null)
                    {
                        glowVFX.ShowNormalGlow();
                    }
                }
            }
        }

        foreach (TimelineUnit unit in new List<TimelineUnit>(units))
            {
                if (unit == null) continue;

                // Apply modifier for Player units
                if (unit is PlayerTimelineUnit)
                    unit.UpdateTimeline(playerSpeedMultiplier);
                else
                    unit.UpdateTimeline(); // Normal speed for enemies

                UpdateTimelineIconPositions();
            }
    }

    public void UpdateTimelineIconPositions()
    {
        Dictionary<float, List<TimelineIcon>> iconsByProgress = new Dictionary<float, List<TimelineIcon>>();

        foreach (TimelineIcon icon in timelineIcons)
        {
            if (icon == null || icon.linkedUnit == null)
            {
                continue;
            }

            float roundedProgress = Mathf.Round(icon.linkedUnit.timelineProgress * 100f) / 100f;

            if (!iconsByProgress.ContainsKey(roundedProgress))
            {
                iconsByProgress[roundedProgress] = new List<TimelineIcon>();
            }
            iconsByProgress[roundedProgress].Add(icon);
        }

        foreach (var kvp in iconsByProgress)
        {
            List<TimelineIcon> group = kvp.Value;
            int count = group.Count;

            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) / 2.0f) * 50f;
                group[i].SetHorizontalOffset(offset);
            }
        }
    }

    public void UnregisterUnit(TimelineUnit unit)
    {
        if (units.Contains(unit))
        {
            Debug.Log($"Unregistered {unit.name} from timeline.");
            units.Remove(unit);
        }
    }
    
    public TimelineIcon GetIconForUnit(TimelineUnit unit)
    {
        foreach (TimelineIcon icon in FindObjectsOfType<TimelineIcon>())
        {
            if (icon.linkedUnit == unit)
                return icon;
        }
        return null;
    }
}
