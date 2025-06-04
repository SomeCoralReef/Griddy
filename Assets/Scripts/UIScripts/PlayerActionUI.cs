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
    public SpriteRenderer tileSelectorSpriteRenderer;

    [Header("Element Icon Library")]
    public ElementIconLibrary elementIconLibrary;


    private int currentAttackIndex = 0;
    private bool isSelectingAttack = false;
    private bool isSelectingTile = false;
    public bool hasSelectedAttackAndTile = false;

    [Header("UI Juice")]
    private Vector3 desiredSelectorWorldPos;

    private float selectorLerpSpeed = 10f;
    private bool isSelectorPopping = false;
    private bool hasReachedTile = true;
    private float selectorPopScale = 1.2f; // How much bigger it pops
    private float selectorPopSpeed = 9f;   // How fast it pops


    [SerializeField] private Vector2Int targetTile;
    private GridManager gridManager;

    [Header("UX Clarity")]
    [SerializeField] private Enemy currentlyHoveredEnemy = null;


    private bool searchingForEnemyToHighlight = false;

    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            Debug.Log($"Player reference assigned to: {player.name}");
        }
        attackPanel = attackPanel.GetComponent<RectTransform>();
        float playerX = player.transform.position.x;
        float playerY = player.transform.position.y;
        Vector3 playerWorldPosXY = new Vector3(playerX, playerY, -0.13f);

        tileSelector = Instantiate(tileSelector, playerWorldPosXY, Quaternion.identity);
        tileSelectorSpriteRenderer = tileSelector.GetComponent<SpriteRenderer>();
        targetTile = new Vector2Int(player.gridPos.x, player.gridPos.y);
        gridManager = FindObjectOfType<GridManager>();
        tileSelector.gameObject.SetActive(false);
    }

    public bool HasSelectedAttackAndTile()
    {
        return hasSelectedAttackAndTile;
    }

    void Update()
    {
        if (tileSelector.activeSelf)
        {
            Vector3 targetPosition = new Vector3(desiredSelectorWorldPos.x, desiredSelectorWorldPos.y, tileSelector.transform.position.z);

            tileSelector.transform.position = Vector3.Lerp(
                tileSelector.transform.position,
                targetPosition,
                Time.deltaTime * selectorLerpSpeed
            );

            float distance = Vector3.Distance(new Vector2(tileSelector.transform.position.x, tileSelector.transform.position.y),
                            new Vector2(desiredSelectorWorldPos.x, desiredSelectorWorldPos.y));

            if (distance < 0.5f) // Close enough
            {
                if (!hasReachedTile)
                {
                    hasReachedTile = true;
                    StartSelectorPop();
                }
            }
            else
            {
                hasReachedTile = false;
            }

            if (isSelectorPopping)
            {
                AnimateSelectorPop();
            }
        }

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
                searchingForEnemyToHighlight = true;
                // Start aiming at tile in front of player
                Enemy enemy = FindObjectOfType<Enemy>();
                //I need to make it check if there even is an enemy in front
                if (enemy == null)
                {
                    targetTile = new Vector2Int(player.gridPos.x, player.gridPos.y);
                }
                else
                {
                    targetTile = new Vector2Int(enemy.gridPos.x, enemy.gridPos.y);
                }

                tileSelector.gameObject.SetActive(true);
                tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                tileSelector.transform.localScale = Vector3.one;
                UpdateTileSelectorPosition();
                foreach (Transform child in attackPanel)
                {
                    Destroy(child.gameObject);
                }

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
                tileSelectorSpriteRenderer.color = new Color(1, 0, 0, 1);
                hasSelectedAttackAndTile = true;
                if (currentlyHoveredEnemy != null && currentlyHoveredEnemy.timelineIcon != null)
                {
                    searchingForEnemyToHighlight = false;
                    //currentlyHoveredEnemy.timelineIcon.SetHighlight(false);
                }
                else
                {
                    Debug.Log("No currently hovered enemy to remove highlight from.");
                }
                currentlyHoveredEnemy = null;
                isSelectingTile = false;
            }
            UpdateTileSelectorPosition();
        }
    }




    private void StartSelectorPop()
    {
        isSelectorPopping = true;
        tileSelector.transform.localScale = Vector3.one * selectorPopScale;
    }

    private void AnimateSelectorPop()
    {
        tileSelector.transform.localScale = Vector3.Lerp(
            tileSelector.transform.localScale,
            Vector3.one,
            Time.deltaTime * selectorPopSpeed
        );

        if (Vector3.Distance(tileSelector.transform.localScale, Vector3.one) < 0.01f)
        {
            tileSelector.transform.localScale = Vector3.one;
            isSelectorPopping = false;
        }
    }


    public void BeginActionPhase()
    {
        hasSelectedAttackAndTile = false;
        isSelectingAttack = true;
        currentAttackIndex = 0;
        PopulateAttackList();

        attackPanel.transform.position = player.transform.position + new Vector3(1.0f, 0, 0);
        attackPanel.gameObject.SetActive(true);
        StartCoroutine(ScaleUI(attackPanel, Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), 2f));
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

    public IEnumerator scaleTileSelector(float duration)
    {
        float elapsed = 0f;
        Vector3 initialScale = tileSelector.transform.localScale;
        Vector3 targetScale = Vector3.one * 2.0f;
        float alpha = tileSelectorSpriteRenderer.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            tileSelector.transform.localScale = Vector3.Lerp(initialScale, targetScale, EaseOutBack(t));
            tileSelectorSpriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(alpha, 0, t));
            yield return null;
        }
        tileSelector.transform.localScale = targetScale;
        tileSelector.gameObject.SetActive(false);
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
        foreach (Transform child in attackPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableAttacks.Length; i++)
        {
            GameObject option = Instantiate(attackOptionPrefab, attackPanel);
            option.GetComponentInChildren<TextMeshProUGUI>().text = availableAttacks[i].attackName;
            option.GetComponentInChildren<Image>().sprite = elementIconLibrary.GetIcon(availableAttacks[i].elementType);

        }
        HighlightAttack(currentAttackIndex);
    }

    void HighlightAttack(int index)
    {
        for (int i = 0; i < attackPanel.childCount; i++)
        {
            var text = attackPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            text.color = (i == index) ? Color.yellow : Color.white;
        }
    }

    void UpdateTileSelectorPosition()
    {
        desiredSelectorWorldPos = gridManager.GetWorldPosition(targetTile.x, targetTile.y);

        Enemy hoveredEnemy = null;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (enemy.gridPos == targetTile)
            {
                hoveredEnemy = enemy;
                break;
            }
        }

        if (ReferenceEquals(hoveredEnemy, currentlyHoveredEnemy))
        {
            // Still hovering same enemy, do nothing
            return;
        }

        if (currentlyHoveredEnemy != null && currentlyHoveredEnemy.timelineIcon != null)
        {
            currentlyHoveredEnemy.timelineIcon.SetHighlight(false);
        }

        if (hoveredEnemy != null && hoveredEnemy.timelineIcon != null)
        {
            if (searchingForEnemyToHighlight)
            {
                hoveredEnemy.timelineIcon.SetHighlight(true);
            }   
        }

        currentlyHoveredEnemy = hoveredEnemy;
    }


}
