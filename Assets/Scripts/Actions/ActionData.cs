using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ActionData", menuName = "Scriptable Objects/ActionData")]
public class ActionData : ScriptableObject
{
    public string actionName;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject useVFX;

    public int powerAmount;
    public bool needsTarget;
    public bool targetAllies;

    public virtual IEnumerator ExecuteAction(Player player, int targetSlotIndex)
    {
        yield break;
    }
}
