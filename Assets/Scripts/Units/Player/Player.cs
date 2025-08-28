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
        IsInPreparePhase = GetComponent<PlayerTimelineUnit>().state == TimelineState.Preparing;
        playerAnimationController.PlayerPrepare(IsInPreparePhase);
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
        FindObjectOfType<TimelineManager>().isPaused = false;
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
        if (selectedAction is AttackData)
        {
            playerAnimationController.PlayAttack();
        }

        StartCoroutine(selectedAction.ExecuteAction(this, aimedSlotIndex));
        actionUI.CloseAllPanels();
        
    }
    

    public void ShowHitEffect(ElementType attackData)
    {
        StartCoroutine(HitFlashRoutine(attackData));

    }

    private IEnumerator HitFlashRoutine(ElementType attackData)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (attackData.Equals(ElementType.Red))
        {
            spriteRenderer.color = Color.red; // Flash red for red attacks
        }
        else if (attackData.Equals(ElementType.Blue))
        {
            spriteRenderer.color = Color.blue; // Flash blue for blue attacks
        }
        else if (attackData.Equals(ElementType.Yellow))
        {
            spriteRenderer.color = Color.yellow; // Flash yellow for yellow attacks
        }
        else if (attackData.Equals(ElementType.Green))
        {
            spriteRenderer.color = Color.green; // Flash green for green attacks
        }
        else if (attackData.Equals(ElementType.Purple))
        {
            spriteRenderer.color = Color.white; // Flash white for purple attacks
        }
        else if (attackData.Equals(ElementType.Orange))
        {
            spriteRenderer.color = Color.cyan; // Flash cyan for orange attacks
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

    public Player PlayerAt(int slotIndex)
{
    foreach (Player p in FindObjectsOfType<Player>())
    {
        if (p.slotIndex == slotIndex)
            return p;
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
