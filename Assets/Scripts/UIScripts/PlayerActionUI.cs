// Refactored PlayerActionUI.cs
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Element Icon Library")]
    public ElementIconLibrary elementIconLibrary;

    private GridManager gridManager;
    private Enemy currentlyHoveredEnemy = null;
    private List<TextMeshProUGUI> mainCommandTexts = new();
    private List<TextMeshProUGUI> subCommandTexts = new();

    private Vector3 desiredSelectorWorldPos;
    private int targetSlotIndex = 0;
    private int currentSecondMenuIndex = 0;

    private bool isSelectorPopping = false;
    private float selectorPopScale = 1.2f;
    private float selectorPopSpeed = 9f;
    private float selectorLerpSpeed = 10f;

    private CommandCategory currentCategory = CommandCategory.Attack;
    private bool isSelectingFirstMenu = true;
    private bool isSelectingSecondMenu = false;
    private bool isSelectingTile = false;
    private bool searchingForEnemyToHighlight = false;

    public bool hasConfirmedActionAndTile = false;

    void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        gridManager = FindObjectOfType<GridManager>();
        tileSelector = Instantiate(tileSelector, player.transform.position, Quaternion.identity);
        tileSelectorSpriteRenderer = tileSelector.GetComponent<SpriteRenderer>();
        tileSelector.SetActive(false);
        targetSlotIndex = player.slotIndex;

    }

    void Update()
    {
        HandleTileSelector();
        if (isSelectingFirstMenu) HandleMainMenuInput();
        else if (isSelectingSecondMenu) HandleSubMenuInput();
        else if (isSelectingTile) HandleTileInput();
    }

    void HandleTileSelector()
    {
        if (!tileSelector.activeSelf) return;

        tileSelector.transform.position = Vector3.Lerp(tileSelector.transform.position,
            new Vector3(desiredSelectorWorldPos.x, desiredSelectorWorldPos.y, tileSelector.transform.position.z),
            Time.deltaTime * selectorLerpSpeed);

        if (Vector3.Distance(tileSelector.transform.position, desiredSelectorWorldPos) < 0.5f)
            StartSelectorPop();

        if (isSelectorPopping)
        {
            tileSelector.transform.localScale = Vector3.Lerp(tileSelector.transform.localScale, Vector3.one, Time.deltaTime * selectorPopSpeed);
            if (Vector3.Distance(tileSelector.transform.localScale, Vector3.one) < 0.01f)
            {
                tileSelector.transform.localScale = Vector3.one;
                isSelectorPopping = false;
            }
        }
    }

    void HandleMainMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) ChangeMainCategory(-1);
        if (Input.GetKeyDown(KeyCode.S)) ChangeMainCategory(1);
        HighlightSelection(mainCommandTexts, (int)currentCategory);
        if (Input.GetKeyDown(KeyCode.R)) ShowSubPanel(currentCategory);
    }

    void HandleSubMenuInput()
    {
        int count = GetSubActionCount();
        if (Input.GetKeyDown(KeyCode.W)) currentSecondMenuIndex = Mathf.Max(0, currentSecondMenuIndex - 1);
        if (Input.GetKeyDown(KeyCode.S)) currentSecondMenuIndex = Mathf.Min(count - 1, currentSecondMenuIndex + 1);
        HighlightSelection(subCommandTexts, currentSecondMenuIndex);

        if (Input.GetKeyDown(KeyCode.R))
        {
            ActionData selected = GetCurrentSelectedAction();
            if (selected == null) return;

            player.SelectAction(selected);
            if (!selected.needsTarget)
            {
                player.OnConfirmAction();
                StartCoroutine(FadeTileSelector(0.5f));
            }
            else StartTargetSelection();
        }
    }

    void HandleTileInput()
    {
        bool targetingEnemies = !player.selectedAction.targetAllies;
        int maxSlot = gridManager.slots - 1;

        if (Input.GetKeyDown(KeyCode.S)) targetSlotIndex = Mathf.Max(0, targetSlotIndex - 1);
        if (Input.GetKeyDown(KeyCode.W)) targetSlotIndex = Mathf.Min(maxSlot, targetSlotIndex + 1);

        UpdateTileSelectorPosition();

        if (Input.GetKeyDown(KeyCode.R))
        {
            player.AimAtSlot(targetSlotIndex);
            player.OnConfirmAction();
            tileSelectorSpriteRenderer.color = Color.red;
            tileSelector.SetActive(false);
            isSelectingTile = false;
        }
    }

    void ShowMainCommands()
    {
        ClearPanel(mainCommandPanel);
        CreateMainButton("Attack", () => ShowSubPanel(CommandCategory.Attack));
        CreateMainButton("Spells", () => ShowSubPanel(CommandCategory.Spells));
        CreateMainButton("Items", () => ShowSubPanel(CommandCategory.Items));
        CreateMainButton("Defend", () => CloseAllPanels());
    }

    void ShowSubPanel(CommandCategory category)
    {
        ClearPanel(subCommandPanel);
        currentCategory = category;
        List<ActionData> actions = GetActionsByCategory(category);

        for (int i = 0; i < actions.Count; i++)
        {
            int index = i;
            CreateSubButton(actions[i].actionName, () => StartSubAction(index));
        }
        CreateSubButton("Back", ShowMainCommands);

        subCommandPanel.gameObject.SetActive(true);
        isSelectingSecondMenu = true;
        isSelectingFirstMenu = false;
        currentSecondMenuIndex = 0;
    }

    void StartSubAction(int index)
    {
        currentSecondMenuIndex = index;
        HandleSubMenuInput();
    }

    void StartTargetSelection()
    {
        isSelectingTile = true;
        tileSelector.SetActive(true);
        tileSelectorSpriteRenderer.color = Color.black;
        tileSelector.transform.localScale = Vector3.one;
        targetSlotIndex = player.slotIndex + 1;
        UpdateTileSelectorPosition();
    }

    void CreateMainButton(string label, Action action)
    {
        Debug.Log($"Creating main button: {label}" + $" with action {action.Method.Name}");
        var btn = Instantiate(commandButtonPrefab, mainCommandPanel);
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        btn.GetComponent<Button>().onClick.AddListener(() => action());
        mainCommandTexts.Add(text);
    }

    void CreateSubButton(string label, Action action)
    {
        var btn = Instantiate(commandButtonPrefab, subCommandPanel);
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        btn.GetComponent<Button>().onClick.AddListener(() => action());
        subCommandTexts.Add(text);
    }

    void HighlightSelection(List<TextMeshProUGUI> texts, int selectedIndex)
    {
        for (int i = 0; i < texts.Count; i++)
            texts[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel) Destroy(child.gameObject);
    }

    void ChangeMainCategory(int direction)
    {
        currentCategory = (CommandCategory)(((int)currentCategory + direction + 4) % 4);
    }

    void UpdateTileSelectorPosition()
    {
        desiredSelectorWorldPos = gridManager.GetWorldPositionForSlot(targetSlotIndex);
    }

    void StartSelectorPop() => isSelectorPopping = true;

    IEnumerator FadeTileSelector(float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = tileSelector.transform.localScale;
        Vector3 endScale = Vector3.one * 2f;
        float startAlpha = tileSelectorSpriteRenderer.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            tileSelector.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            tileSelectorSpriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(startAlpha, 0, t));
            yield return null;
        }

        tileSelector.SetActive(false);
    }

    public void StartActionSelection()
{
    hasConfirmedActionAndTile = false;
    isSelectingFirstMenu = true;
    ShowMainCommands();
}


    List<ActionData> GetActionsByCategory(CommandCategory category)
    {
        return category switch
        {
            CommandCategory.Attack => new List<ActionData>(player.availableAttacks),
            CommandCategory.Spells => new List<ActionData>(player.availableSpells),
            CommandCategory.Items => new List<ActionData>(player.availableItems),
            _ => new List<ActionData>()
        };
    }

    ActionData GetCurrentSelectedAction()
    {
        return currentCategory switch
        {
            CommandCategory.Attack => player.availableAttacks[currentSecondMenuIndex],
            CommandCategory.Spells => player.availableSpells[currentSecondMenuIndex],
            CommandCategory.Items => player.availableItems[currentSecondMenuIndex],
            _ => null
        };
    }

    int GetSubActionCount()
    {
        return currentCategory switch
        {
            CommandCategory.Attack => player.availableAttacks.Count,
            CommandCategory.Spells => player.availableSpells.Count,
            CommandCategory.Items => player.availableItems.Count,
            _ => 0
        };
    }

    void CloseAllPanels()
    {
        mainCommandPanel.gameObject.SetActive(false);
        subCommandPanel.gameObject.SetActive(false);
    }
}
