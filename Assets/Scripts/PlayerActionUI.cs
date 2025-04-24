using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerActionUI : MonoBehaviour
{
    public Player player;
    public AttackData[] availableAttacks;


    
    [Header("UI References")]
    public RectTransform attackPanel;
    public GameObject attackOptionPrefab;
    public GameObject tileSelector;


    private int currentAttackIndex = 0;
    private bool isSelectingAttack = false;
    private bool isSelectingTile = false;
    public bool hasSelectedAttackAndTile = false;

    [SerializeField]private Vector2Int targetTile = new Vector2Int(0, 0);
    private GridManager gridManager;

    void Start()
    {
        attackPanel = attackPanel.GetComponent<RectTransform>();
        Vector3 playerWorldPos = player.transform.position;
        tileSelector = Instantiate(tileSelector, transform);
        gridManager = FindObjectOfType<GridManager>();
        tileSelector.gameObject.SetActive(false);
    }

    public bool HasSelectedAttackAndTile()
    {
        return hasSelectedAttackAndTile;
    }

    void Update()
    {
        //if(player.IsInPreparePhase == false) return;
        if (isSelectingAttack)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ChangeAttack(-1);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeAttack(1);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                player.SelectAttack(availableAttacks[currentAttackIndex]);
                isSelectingAttack = false;
                isSelectingTile = true;

                // Start aiming at tile in front of player
                targetTile = new Vector2Int(player.gridPos.x - 1, player.gridPos.y);
                tileSelector.gameObject.SetActive(true);
                UpdateTileSelectorPosition();
            }
        }
        else if (isSelectingTile)
        {
            if (Input.GetKeyDown(KeyCode.W)) { targetTile.y = Mathf.Min(gridManager.rows - 1, targetTile.y + 1); }
            if (Input.GetKeyDown(KeyCode.S)) { targetTile.y = Mathf.Max(0, targetTile.y - 1); }
            if (Input.GetKeyDown(KeyCode.A)) { targetTile.x = Mathf.Max(0, targetTile.x - 1); }
            if (Input.GetKeyDown(KeyCode.D)) { targetTile.x = Mathf.Min(gridManager.columns - 1, targetTile.x + 1); }

            if (Input.GetKeyDown(KeyCode.R))
            {
                player.AimAtTile(targetTile);
                player.OnConfirmAction();
                tileSelector.gameObject.SetActive(false);
                isSelectingTile = false;
                hasSelectedAttackAndTile = true;
                //Debug.Log($"Aimed at tile: {targetTile}");
            }
            UpdateTileSelectorPosition();
        }
    }

    public void BeginActionPhase()
    {
        hasSelectedAttackAndTile = false;
        isSelectingAttack = true;
        currentAttackIndex = 0;
        PopulateAttackList();

        Debug.Log(player.transform.position);
        attackPanel.transform.position = player.transform.position + new Vector3(9f, 0, 0);
        attackPanel.gameObject.SetActive(true);
        StartCoroutine(ScaleUI(attackPanel, Vector3.zero, new Vector3(0.1f,0.1f,0.1f), 2f));

        Debug.Log("Moved attack panel to " + attackPanel.transform.position);
    }
    public IEnumerator ScaleUI(Transform target, Vector3 fromScale, Vector3 toScale, float duration)
    {
        
        float elapsed = 0f;
        target.localScale = fromScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = Vector3.Lerp(fromScale, toScale, EaseOutBack(t));
                yield return null;
            }

            target.localScale = toScale;
        }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    void ChangeAttack(int direction)
    {   
        currentAttackIndex = (currentAttackIndex + direction + availableAttacks.Length) % availableAttacks.Length;
        HighlightAttack(currentAttackIndex);
    }

    void PopulateAttackList()
    {
        foreach(Transform child in attackPanel)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < availableAttacks.Length; i++)
        {
            GameObject option = Instantiate(attackOptionPrefab, attackPanel);
            option.GetComponentInChildren<TextMeshProUGUI>().text = availableAttacks[i].attackName;
        }

        HighlightAttack(currentAttackIndex);
    }

    void HighlightAttack(int index)
    {
        for(int i = 0; i< attackPanel.childCount; i++)
        {
            var text = attackPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            text.color = (i == index) ? Color.yellow : Color.white;
        }
    }

    void UpdateTileSelectorPosition()
    {
        tileSelector.transform.position = gridManager.GetWorldPosition(targetTile.x, targetTile.y);
        //Debug.Log("Moved tile selector to " + tileSelector.transform.position);
    }
}
