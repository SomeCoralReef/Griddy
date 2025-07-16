using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "ItemData", menuName = "Action Data/ItemData")]
public class ItemData : ActionData
{
    public ItemType itemType;
    public bool isConsumable = true;

    public override IEnumerator ExecuteAction(Player user, int targetSlotIndex)
    {
        Debug.Log($"Using item: {actionName} on slot {targetSlotIndex}");

        // VFX
        if (useVFX != null)
        {
            GameObject.Instantiate(useVFX, user.transform.position, Quaternion.identity);
        }

        if (targetAllies)
        {
            Player targetPlayer = user.PlayerAt(targetSlotIndex);
            if (targetPlayer != null)
            {
                ApplyItemEffectToPlayer(targetPlayer);
            }
        }
        else
        {
            Enemy targetEnemy = user.EnemyAt(targetSlotIndex);
            if (targetEnemy != null)
            {
                ApplyItemEffectToEnemy(targetEnemy);
            }
        }

        yield return new WaitForSeconds(0.5f);
        TimelineManager timelineManager = GameObject.FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
    }

    private void ApplyItemEffectToEnemy(Enemy enemy)
    {
        // Apply item effect (customize this logic)
        Debug.Log($"Item {actionName} applied to enemy at slot {enemy.slotIndex}");
        if (enemy.timelineIcon != null)
        {
            enemy.timelineIcon.SetHighlight(false);
            enemy.timelineIcon.isPulsing = false;
        }
    }

    private void ApplyItemEffectToPlayer(Player player)
    {
        // Example logic — you can add healing, buffs, etc. here
        Debug.Log($"Item {actionName} applied to player at slot {player.slotIndex}");
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
