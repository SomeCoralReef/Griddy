using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Data;
using UnityEditor.Build;
using System.Collections.Generic;

public class PlayerActionUI : MonoBehaviour
{
    public Player player;

    [Header("UI References")]
    public GameObject attackOptionPrefab;
    public GameObject tileSelector;
    public SpriteRenderer tileSelectorSpriteRenderer;
    [SerializeField] private Transform mainCommandPanel;
    [SerializeField] private Transform subCommandPanel;
    [SerializeField] private GameObject commandButtonPrefab;
    [SerializeField] private GameObject backButtonPrefab;
    public CommandCategory currentCategory = CommandCategory.Attack;

    private AttackData currentlyHoveredAttack = null;



    [Header("Element Icon Library")]
    public ElementIconLibrary elementIconLibrary;


    private int currentSecondMenuIndex = 0;
    private bool isSelectingSecondMenu = false;
    private bool isSelectingFirstMenu = true;
    private bool isSelectingTile = false;
    public bool hasSelectedAttackAndTile = false;

    [Header("UI Juice")]
    private Vector3 desiredSelectorWorldPos;

    private float selectorLerpSpeed = 10f;
    private bool isSelectorPopping = false;
    private bool hasReachedTile = true;
    private float selectorPopScale = 1.2f; // How much bigger it pops
    private float selectorPopSpeed = 9f;   // How fast it pops


    [SerializeField] private int targetSlotIndex = 0;


    private GridManager gridManager;

    [Header("UX Clarity")]
    [SerializeField] private Enemy currentlyHoveredEnemy = null;

    [Header("List Of Menu Buttons and Sub Menu Buttons")]
    private List<TextMeshProUGUI> mainCommandTexts = new();
    private List<TextMeshProUGUI> subCommandTexts = new();



    private bool searchingForEnemyToHighlight = false;

    void Start()
    {

        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        mainCommandPanel = mainCommandPanel.GetComponent<RectTransform>();
        float playerX = player.transform.position.x;
        float playerY = player.transform.position.y;
        Vector3 playerWorldPosXY = new Vector3(playerX, playerY, -0.13f);

        tileSelector = Instantiate(tileSelector, playerWorldPosXY, Quaternion.identity);
        tileSelectorSpriteRenderer = tileSelector.GetComponent<SpriteRenderer>();
        targetSlotIndex = player.slotIndex; // Assuming player.slotIndex is an int representing the slot
        gridManager = FindObjectOfType<GridManager>();
        tileSelector.gameObject.SetActive(false);

        ShowMainCommands(player);
    }

    void ShowMainCommands(Player p)
    {
        player = p;
        currentCategory = CommandCategory.Attack; // Default to Attack category
        ClearPanel(mainCommandPanel);
        Debug.Log("Showing main commands for player: " + player.name);
        CreateMainButton("Attack", () => ShowSubPanel(CommandCategory.Attack));
        CreateMainButton("Spells", () => ShowSubPanel(CommandCategory.Spells));
        CreateMainButton("Items", () => ShowSubPanel(CommandCategory.Items));
        CreateMainButton("Defend", () =>
        {
            CloseAllPanels();
        });
    }

    void ShowSubPanel(CommandCategory category)
    {
        Debug.Log("Showing sub panel for category: " + category);
        ClearPanel(subCommandPanel);
        currentCategory = category;
        switch (category)
        {
            case CommandCategory.Attack:
                for (int i = 0; i < player.availableAttacks.Count; i++)
                {
                    Debug.Log("Creating sub button for attack: " + player.availableAttacks[i].actionName);
                    int index = i;
                    CreateSubButton(player.availableAttacks[i].actionName, () => StartAttackSelection(index));
                }
                break;

            case CommandCategory.Spells:
                for (int i = 0; i < player.availableSpells.Count; i++)
                {
                    Debug.Log("Creating sub button for spell: " + player.availableSpells[i].actionName);
                    int index = i;
                    CreateSubButton(player.availableSpells[i].actionName, () => Debug.Log("Spell selected: " + index));
                }
                break;

            case CommandCategory.Items:
                for (int i = 0; i < player.availableItems.Count; i++)
                {
                    int index = i;
                    CreateSubButton(player.availableItems[i].actionName, () => Debug.Log("Item used: " + index));
                }
                break;
        }

        CreateSubButton("Back", () => ShowMainCommands(player));
        subCommandPanel.gameObject.SetActive(true);
    }

    void CreateMainButton(string label, UnityEngine.Events.UnityAction action)
    {   
        var btn = Instantiate(commandButtonPrefab, mainCommandPanel);
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        btn.GetComponentInChildren<TextMeshProUGUI>().text = label;
        btn.GetComponent<Button>().onClick.AddListener(action);
        mainCommandTexts.Add(text);
    }

    void CreateSubButton(string label, UnityEngine.Events.UnityAction action)
    {
        var btn = Instantiate(commandButtonPrefab, subCommandPanel);
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        btn.GetComponent<Button>().onClick.AddListener(action);
        subCommandTexts.Add(text);
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

       void CloseAllPanels()
    {
        mainCommandPanel.gameObject.SetActive(false);
        subCommandPanel.gameObject.SetActive(false);
    }


    void StartAttackSelection(int index)
    {
        currentSecondMenuIndex = index;
        isSelectingSecondMenu = true;
        isSelectingTile = false;
        hasSelectedAttackAndTile = false;
        mainCommandPanel.gameObject.SetActive(false);
        subCommandPanel.gameObject.SetActive(false);
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

        if (isSelectingFirstMenu)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ChangeSelection(-1);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeSelection(1);
            }
            HighlightSelection(mainCommandTexts, (int)currentCategory);

            if (Input.GetKeyDown(KeyCode.R))
            {
                ShowSubPanel(currentCategory);
                isSelectingFirstMenu = false;
                isSelectingSecondMenu = true;
                searchingForEnemyToHighlight = true;
                // Start aiming at tile in front of player
                Enemy enemy = FindObjectOfType<Enemy>();
                //I need to make it check if there even is an enemy in front
                if (enemy == null)
                {
                    targetSlotIndex = player.slotIndex + 1; // Default to the next slot
                }
                else
                {
                    targetSlotIndex = enemy.slotIndex;
                }

                tileSelector.gameObject.SetActive(true);
                tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                tileSelector.transform.localScale = Vector3.one;
                UpdateTileSelectorPosition();
                foreach (Transform child in mainCommandPanel)
                {
                    Destroy(child.gameObject);
                }

            }
        }
        else if (isSelectingSecondMenu)
        {
            switch (currentCategory)
            {
                case CommandCategory.Attack:
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        currentSecondMenuIndex = Mathf.Max(0, currentSecondMenuIndex - 1);
                    }

                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        currentSecondMenuIndex = Mathf.Min(player.availableAttacks.Count - 1, currentSecondMenuIndex + 1);
                    }
                    HighlightSelection(subCommandTexts, currentSecondMenuIndex);
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        player.SelectAction(player.availableAttacks[currentSecondMenuIndex]);
                        Debug.Log("Selected attack: " + player.availableAttacks[currentSecondMenuIndex].actionName);
                        isSelectingTile = true;
                        isSelectingSecondMenu = false;
                        isSelectingFirstMenu = false;
                        hasSelectedAttackAndTile = false;
                        tileSelector.gameObject.SetActive(true);
                        tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                        tileSelector.transform.localScale = Vector3.one;
                        UpdateTileSelectorPosition();
                    }
                    break;
                case CommandCategory.Spells:
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        currentSecondMenuIndex = Mathf.Max(0, currentSecondMenuIndex - 1);
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        currentSecondMenuIndex = Mathf.Min(player.availableSpells.Count - 1, currentSecondMenuIndex + 1);
                    }
                    HighlightSelection(subCommandTexts, currentSecondMenuIndex);
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        player.SelectAction(player.availableSpells[currentSecondMenuIndex]);
                        Debug.Log("Selected spell: " + player.availableSpells[currentSecondMenuIndex].actionName);
                        isSelectingTile = true;
                        isSelectingSecondMenu = false;
                        isSelectingFirstMenu = false;
                        hasSelectedAttackAndTile = false;
                        tileSelector.gameObject.SetActive(true);
                        tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                        tileSelector.transform.localScale = Vector3.one;
                        UpdateTileSelectorPosition();
                    }
                    
                    break;
                case CommandCategory.Items:
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        currentSecondMenuIndex = Mathf.Max(0, currentSecondMenuIndex - 1);
                    }
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        currentSecondMenuIndex = Mathf.Min(player.availableItems.Count - 1, currentSecondMenuIndex + 1);
                    }
                    HighlightSelection(subCommandTexts, currentSecondMenuIndex);
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        player.SelectAction(player.availableItems[currentSecondMenuIndex]);
                        Debug.Log("Selected item: " + player.availableItems[currentSecondMenuIndex].actionName);
                        if(player.availableItems[currentSecondMenuIndex].needsTarget)
                        {
                            player.SelectAction(player.availableItems[currentSecondMenuIndex]);
                            isSelectingTile = true;
                            isSelectingSecondMenu = false;
                            isSelectingFirstMenu = false;
                            hasSelectedAttackAndTile = false;
                            tileSelector.gameObject.SetActive(true);
                            tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                            tileSelector.transform.localScale = Vector3.one;
                            UpdateTileSelectorPosition();
                        }
                        else
                        {
                            player.OnConfirmAction();
                            StartCoroutine(scaleTileSelector(0.5f));
                        }
                    }
                    break;
            }
        }
        else if (isSelectingTile)
        {
            bool selectingEnemySide;
            if(player.selectedAction.targetAllies == false)
            {
                selectingEnemySide = true;
            } else 
            {
                selectingEnemySide = false;
            }

            if (selectingEnemySide)
            {
                // EnemySide Selection
                if (Input.GetKeyDown(KeyCode.S)) { targetSlotIndex = Mathf.Max(0, targetSlotIndex - 1); }
                if (Input.GetKeyDown(KeyCode.W)) { targetSlotIndex = Mathf.Min(gridManager.slots - 1, targetSlotIndex + 1); }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    player.AimAtSlot(targetSlotIndex);
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
                CloseAllPanels();
            } else 
            {
                // PlayerSide Selection
                if (Input.GetKeyDown(KeyCode.S)) { targetSlotIndex = Mathf.Max(0, targetSlotIndex - 1); }
                if (Input.GetKeyDown(KeyCode.W)) { targetSlotIndex = Mathf.Min(gridManager.slots - 1, targetSlotIndex + 1); }
            }
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
        isSelectingFirstMenu = true;
        currentSecondMenuIndex = 0;
        //PopulateAttackList(player);

        mainCommandPanel.transform.position = player.transform.position + new Vector3(1.0f, 0, 0);
        mainCommandPanel.gameObject.SetActive(true);
        StartCoroutine(ScaleUI(mainCommandPanel, Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), 2f));
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

    void ChangeSelection(int direction)
    {
        currentCategory = (CommandCategory) ((int) currentCategory + direction);
    }

    void PopulateAttackList(Player player)
    {
        foreach (Transform child in mainCommandPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player.availableAttacks.Count; i++)
        {
            GameObject option = Instantiate(attackOptionPrefab, mainCommandPanel);
            option.GetComponentInChildren<TextMeshProUGUI>().text = player.availableAttacks[i].actionName;
            option.GetComponentInChildren<Image>().sprite = elementIconLibrary.GetIcon(player.availableAttacks[i].elementType);

        }
        //TextMeshProUGUI text = player.availableAttacks[currentAttackIndex].GetComponentInChildren<TextMeshProUGUI>();
        HighlightSelection(mainCommandTexts, (int)currentCategory);
    }

    void HighlightSelection(List<TextMeshProUGUI> texts, int selectedIndex)
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    void UpdateTileSelectorPosition()
    {
        desiredSelectorWorldPos = gridManager.GetWorldPositionForSlot(targetSlotIndex);

        Enemy hoveredEnemy = null;
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (enemy.slotIndex == targetSlotIndex)
            {
                hoveredEnemy = enemy;
                break;
            }
        }

        if (ReferenceEquals(hoveredEnemy, currentlyHoveredEnemy))
        {
            // If we already have the hovered enemy, no need to continue searching
            return;
        }
            
        if (currentlyHoveredEnemy != null && currentlyHoveredEnemy.timelineIcon != null)
            currentlyHoveredEnemy.timelineIcon.SetHighlight(false);

        if (hoveredEnemy != null && hoveredEnemy.timelineIcon != null && searchingForEnemyToHighlight)
        {
            hoveredEnemy.timelineIcon.SetHighlight(true);
        }
        currentlyHoveredEnemy = hoveredEnemy;
    }


}
