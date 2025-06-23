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

        GameObject icon = Instantiate(timelineIconPrefab, timelineBar);
        TimelineIcon iconScript = icon.GetComponent<TimelineIcon>();
        enemy.timelineIcon = iconScript; // Link the icon to the enemy
        iconScript.linkedUnit = unit;
        iconScript.barRect = timelineBar;

        SpriteRenderer iconSpriteRenderer = icon.GetComponentInChildren<SpriteRenderer>();
        if (unit is PlayerTimelineUnit)
            iconSpriteRenderer.color = Color.blue;
        else if (unit is EnemyTimelineUnit)
            iconSpriteRenderer.color = Color.red;

        timelineIcons.Add(iconScript);    

        //Debug.Log($"Manually registered unit {unit.name} on timeline.");
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

        SpriteRenderer iconSpriteRenderer = icon.GetComponentInChildren<SpriteRenderer>();
        iconSpriteRenderer.color = Color.blue; // Player icons are blue

        timelineIcons.Add(iconScript);
    }


    void Update()
    {
        foreach (TimelineUnit unit in new List<TimelineUnit>(units))
        {
            if (unit != null)
            {
                unit.UpdateTimeline();
                UpdateTimelineIconPositions();
            }
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
