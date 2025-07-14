using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "ItemData", menuName = "Action Data/ItemData")]
public class ItemData : ActionData
{
    public ItemType itemType;
    public bool isConsumable = true;

    public override IEnumerator ExecuteAction(Player player, int targetSlotIndex)
    {
        // Implement item-specific logic here
        // For example, healing or buffing allies
        Enemy enemy = player.EnemyAt(targetSlotIndex);
        if (enemy != null)
        {
            // Apply item effects to the enemy
            Debug.Log("Used item: " + actionName);
            enemy.timelineIcon.SetHighlight(false);
            enemy.timelineIcon.isPulsing = false;
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            timelineManager.isPaused = false;
        }
        yield break;
    }
}




public enum ItemType
{
    Heal,
    Damage,
    Buff,
    Debuff,
    Revive,
    Utility
 }
