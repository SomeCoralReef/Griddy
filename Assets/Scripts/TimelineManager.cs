using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour
{
    public GameObject timelineIconPrefab;       // Drag your UnitIcon prefab here
    public RectTransform timelineBar;           // Drag the TimelineBar UI RectTransform

    public bool isPaused = false;

    private List<TimelineUnit> units = new List<TimelineUnit>();

    public void RegisterUnit(TimelineUnit unit)
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

        Image iconImage = icon.GetComponent<Image>();
        if (unit is PlayerTimelineUnit)
            iconImage.color = Color.blue;
        else if (unit is EnemyTimelineUnit)
            iconImage.color = Color.red;

        //Debug.Log($"Manually registered unit {unit.name} on timeline.");
    }


    void Update()
    {
        foreach (TimelineUnit unit in new List<TimelineUnit>(units))
        {
            if (unit != null)
            {
                unit.UpdateTimeline();    
            }
        }
    }
    
    public void UnregisterUnit(TimelineUnit unit)
    {
        if (units.Contains(unit))
        {
            units.Remove(unit);
            Debug.Log($"Unregistered {unit.name} from timeline.");
        }
    }
}
