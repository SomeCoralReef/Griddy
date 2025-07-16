using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "SpellData", menuName = "Action Data/Spells")]
public class SpellData : ActionData
{
    public int manaCost;
    public string spellDescription;
    public ElementType elementType;


    public override IEnumerator ExecuteAction(Player user, int targetSlotIndex)
    {
        Debug.Log($"Casting spell: {actionName}");

        if (useVFX != null)
        {
            GameObject.Instantiate(useVFX, user.transform.position, Quaternion.identity);
        }

        if (targetAllies)
        {
            Player targetPlayer = user.PlayerAt(targetSlotIndex);
            if (targetPlayer != null)
            {
                ApplySpellToPlayer(targetPlayer);
            }
        }
        else
        {
            Enemy targetEnemy = user.EnemyAt(targetSlotIndex);
            if (targetEnemy != null)
            {
                ApplySpellToEnemy(targetEnemy);
            }
        }

        yield return new WaitForSeconds(0.5f);
        TimelineManager timelineManager = GameObject.FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
    }


       private void ApplySpellToEnemy(Enemy enemy)
    {
        // Damage or debuff
        bool wasBroken = enemy.TakeDamage(elementType, powerAmount);
        if (enemy.timelineIcon != null)
        {
            enemy.timelineIcon.SetHighlight(false);
            enemy.timelineIcon.isPulsing = false;
        }
        Debug.Log($"Spell {actionName} hit enemy at slot {enemy.slotIndex}, broken: {wasBroken}");
    }

    private void ApplySpellToPlayer(Player player)
    {
        // Heal or buff
        Debug.Log($"Spell {actionName} applied to player at slot {player.slotIndex}");
    }
}
