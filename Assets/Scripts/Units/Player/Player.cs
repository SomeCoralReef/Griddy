using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    public ActionData selectedAction;

    private int aimedSlotIndex;
    private GridManager gridManager;
    public int slotIndex;

    public AttackData blastAttack;
    public PlayerActionUI actionUI;

    public bool IsInPreparePhase;
    public GameObject hitVFXPrefab;

    public PlayerAnimationControllerScript playerAnimationController;

    [Header("Player Attacks")]
    public List<AttackData> availableAttacks = new List<AttackData>();
    public List<SpellData> availableSpells = new List<SpellData>();
    public List<ItemData> availableItems = new List<ItemData>();
    
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        actionUI = FindObjectOfType<PlayerActionUI>();
        playerAnimationController = GetComponent<PlayerAnimationControllerScript>();
    }
    void Update()
    {
        playerAnimationController.PlayerPrepare(IsInPreparePhase);
        if (GetComponent<PlayerTimelineUnit>().state == TimelineState.Preparing)
        {
            IsInPreparePhase = true;
        }
        else
        {
            IsInPreparePhase = false;
        }
    }

    public void SelectAction(ActionData action)
    {
        selectedAction = action;
    }



    public void AimAtSlot(int slotIndex)
    {
        aimedSlotIndex = slotIndex;
    }


    public void OnConfirmAction()
    {
        // Resume timeline and begin execution
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
        GetComponent<TimelineUnit>().BeginExecution();
    }

    public void ExecuteAction()
    {
        if (selectedAction == null)
        {
            Debug.LogWarning("No action selected on execution.");
            return;
        }
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = true;

        StartCoroutine(ExecuteActionRoutine(actionUI.currentCategory));
        actionUI.StartCoroutine(actionUI.scaleTileSelector(0.2f));
    }

    private IEnumerator ExecuteActionRoutine(CommandCategory commandCategory)
    {
        if(commandCategory == CommandCategory.Attack && selectedAction != null)
        {
            yield return ExecuteAttackRoutine();
        }
        else if (commandCategory == CommandCategory.Items && selectedAction != null)
        {
            yield return ExecuteItemRoutine();
        }
        else
        {
            Debug.LogWarning("No valid action selected for execution.");
        }
    }

    private IEnumerator ExecuteAttackRoutine()
    {
        Debug.Log("Executing action: " + selectedAction.actionName);
        playerAnimationController.PlayAttack();
        yield return new WaitForSeconds(1.4f);  
        
        foreach (var offset in selectedAction.patternOffsets)
        {
            int targetSlot = aimedSlotIndex + offset;
            if (targetSlot < 0 || targetSlot >= gridManager.slots)
            {
                continue;
            }
            Enemy enemy = EnemyAt(targetSlot);
            if (enemy != null)
            {
                bool wasBroken = enemy.TakeDamage(selectedAction.elementType, selectedAction.powerAmount);
                Vector3 hitposition = enemy.transform.position;

                GameObject vfxInstance = Instantiate(selectedAction.useVFX, new Vector3(hitposition.x, hitposition.y, -1f), Quaternion.identity);

                if (vfxInstance == null)
                {
                    Debug.LogError("VFX prefab not found for attack: " + selectedAction.actionName);
                    continue;
                }
                else
                {

                }
                SpriteVFX vfx = vfxInstance.GetComponent<SpriteVFX>();


                enemy.timelineIcon.SetHighlight(false);
                if (enemy.timelineIcon != null)
                {
                    enemy.timelineIcon.isPulsing = false; // Start pulsing effect
                    if (wasBroken)
                    {
                        // The enemy was broken → play break effect
                        StartCoroutine(enemy.timelineIcon.PlayBreakEffect(0.0f)); // or targetProgress = 0.0f
                    }
                    else
                    {
                        // Not broken → reset the scale visually
                        if (enemy.timelineIcon != null)
                            StartCoroutine(enemy.timelineIcon.resetScale());
                    }
                }
            }
        }
        CameraShake.Instance.Shake(0.2f, 0.2f);
        yield return new WaitForSeconds(1.0f);
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
    }

    private IEnumerator ExecuteItemRoutine()
    {
        Debug.Log("Executing item: " + selectedAction.actionName);
        //playerAnimationController.PlayItemUse();
        yield return new WaitForSeconds(1.4f);
        int targetSlot = aimedSlotIndex;
        Enemy enemy = EnemyAt(targetSlot);
        if (selectedAction.useVFX != null)
        {
            GameObject vfxInstance = Instantiate(selectedAction.useVFX, transform.position, Quaternion.identity);
            if (vfxInstance == null)
            {
                Debug.LogError("VFX prefab not found for item: " + selectedAction.actionName);
            }
            else
            {
                SpriteVFX vfx = vfxInstance.GetComponent<SpriteVFX>();
                //vfx.Play();
            }
        }
        enemy.timelineIcon.SetHighlight(false);
        // Apply item effects here
        // For example, healing or buffing allies
        Debug.Log("Used item: " + selectedAction.actionName);
        enemy.timelineIcon.isPulsing = false;
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
    }

    public void ShowHitEffect(ElementType attackData)
    {
        StartCoroutine(HitFlashRoutine(attackData));

    }

    private IEnumerator HitFlashRoutine(ElementType attackData)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (attackData.Equals(ElementType.Fire))
        {
            spriteRenderer.color = Color.red; // Flash red for fire attacks
        }
        else if (attackData.Equals(ElementType.Water))
        {
            spriteRenderer.color = Color.blue; // Flash blue for water attacks
        }
        else if (attackData.Equals(ElementType.Thunder))
        {
            spriteRenderer.color = Color.yellow; // Flash yellow for thunder attacks
        }
        else if (attackData.Equals(ElementType.Earth))
        {
            spriteRenderer.color = Color.green; // Flash green for earth attacks
        }
        else if (attackData.Equals(ElementType.Light))
        {
            spriteRenderer.color = Color.white; // Flash white for light attacks
        }

        yield return new WaitForSeconds(0.4f);
        Debug.Log("Finished hit flash for " + attackData);
        spriteRenderer.color = Color.white; // Reset to original color after flash
    }

    public Enemy EnemyAt(int slotIndex)
    {
        // Replace with proper tracking system (e.g. EnemyManager or 2D array map)
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            if (e.slotIndex == slotIndex)
                return e;
        }
        return null;
    }

    private void goIntoPreparePhase()
    {
        playerAnimationController.PlayerPrepare(true);
    }

    private void exitPreparePhase()
    {
        playerAnimationController.PlayerPrepare(false);
    }
    
}
