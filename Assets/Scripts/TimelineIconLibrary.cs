using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TimelineIconLibrary", menuName = "Scriptable Objects/TimelineIconLibrary")]
public class TimelineIconLibrary : ScriptableObject
{
    [System.Serializable]
    public class IconEntry
    {
        public string unitName;
        public Sprite iconSprite;
    }

    public List<IconEntry> icons;

    public Sprite GetIcon(string unitName)
    {
        foreach (var entry in icons)
        {
            string targetName = entry.unitName + "(Clone)";
            if(targetName == unitName)
            {
                Debug.Log($"Icon found for unit: {unitName}");
                return entry.iconSprite;
            }
        }
        return null; // Return null if no icon is found
    }
}
