using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "AttackData", menuName = "Action Data/AttackData")]
public class AttackData : ActionData
{
    public ElementType elementType;
    public int[] patternOffsets;

    public override IEnumerator ExecuteAction(Player player, int targetSlotIndex)
    {
        player.playerAnimationController.PlayAttack();
        yield return new WaitForSeconds(1.4f); // Wait for the attack animation to finish

        GridManager grid = GameObject.FindObjectOfType<GridManager>();
        foreach (int offset in patternOffsets)
        {
            int targetSlot = targetSlotIndex + offset;
            if (targetSlot < 0 || targetSlot >= grid.slots) continue;

            Enemy enemy = player.EnemyAt(targetSlot);
            if (enemy == null) continue;

            bool wasBroken = enemy.TakeDamage(elementType, powerAmount);
            GameObject vfxInstance = GameObject.Instantiate(useVFX, enemy.transform.position + new Vector3(0, 0, -1f), Quaternion.identity);

            if (enemy.timelineIcon != null)
            {
                enemy.timelineIcon.SetHighlight(false);
                enemy.timelineIcon.isPulsing = false;
                if (wasBroken)
                {
                    player.StartCoroutine(enemy.timelineIcon.PlayBreakEffect(0.0f));
                }
                else
                {
                    player.StartCoroutine(enemy.timelineIcon.resetScale());
                }
            }

            CameraShake.Instance.Shake(0.2f, 0.2f);
            yield return new WaitForSeconds(1.0f);
            TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
            if (timelineManager != null)
            {
                timelineManager.isPaused = false;
            }
        }
    }
}
