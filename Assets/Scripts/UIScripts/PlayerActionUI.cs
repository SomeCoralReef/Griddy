using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Data;
using UnityEditor.Build;

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
    private CommandCategory currentCategory = CommandCategory.None;

    private AttackData currentlyHoveredAttack = null;



    [Header("Element Icon Library")]
    public ElementIconLibrary elementIconLibrary;


    private int currentAttackIndex = 0;
    private bool isSelectingAttack = false;
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
        currentCategory = CommandCategory.None;
        ClearPanel(mainCommandPanel);
        Debug.Log("Showing main commands for player: " + player.name);
        CreateMainButton("Attack", () => ShowSubPanel(CommandCategory.Attack));
        CreateMainButton("Spells", () => ShowSubPanel(CommandCategory.Spells));
        CreateMainButton("Items", () => ShowSubPanel(CommandCategory.Items));
        CreateMainButton("Defend", () =>
        {
            //p.Defend();
            CloseAllPanels();
        });
    }

    void ShowSubPanel(CommandCategory category)
    {
        ClearPanel(subCommandPanel);
        currentCategory = category;
        switch (category)
        {
            case CommandCategory.Attack:
                for (int i = 0; i < player.availableAttacks.Count; i++)
                {
                    int index = i;
                    CreateSubButton(player.availableAttacks[i].attackName, () => StartAttackSelection(index));
                    Debug.Log("Creating sub button for attack: " + player.availableAttacks[i].attackName);
                }
                break;

            case CommandCategory.Spells:
                for (int i = 0; i < player.availableSpells.Count; i++)
                {
                    int index = i;
                    CreateSubButton(player.availableSpells[i].attackName, () => Debug.Log("Spell selected: " + index));
                }
                break;

            case CommandCategory.Items:
                for (int i = 0; i < player.availableItems.Count; i++)
                {
                    int index = i;
                    CreateSubButton(player.availableItems[i].itemName, () => Debug.Log("Item used: " + index));
                }
                break;
        }

        CreateSubButton("Back", () => ShowMainCommands(player));
        subCommandPanel.gameObject.SetActive(true);
    }

    void CreateMainButton(string label, UnityEngine.Events.UnityAction action)
    {
        Debug.Log("Creating main button: " + label);
        var btn = Instantiate(commandButtonPrefab, mainCommandPanel);
        btn.GetComponentInChildren<TextMeshProUGUI>().text = label;
        btn.GetComponent<Button>().onClick.AddListener(action);
    }

    void CreateSubButton(string label, UnityEngine.Events.UnityAction action)
    {
        var btn = Instantiate(commandButtonPrefab, subCommandPanel);
        btn.GetComponentInChildren<TextMeshProUGUI>().text = label;
        btn.GetComponent<Button>().onClick.AddListener(action);
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
        currentAttackIndex = index;
        isSelectingAttack = true;
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

            if (Input.GetKeyDown(KeyCode.R))
            {
                ShowSubPanel(currentCategory);
                isSelectingFirstMenu = false;
                isSelectingAttack = true;
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
        else if (isSelectingAttack)
        {
            Debug.Log(currentAttackIndex);
            if (Input.GetKeyDown(KeyCode.W))
            {
                currentAttackIndex = Mathf.Max(0, currentAttackIndex - 1);
                HighlightSelection(currentAttackIndex);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                currentAttackIndex = Mathf.Min(player.availableAttacks.Count - 1, currentAttackIndex + 1);
                HighlightSelection(currentAttackIndex);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                player.SelectAttack(player.availableAttacks[currentAttackIndex]);
                isSelectingTile = true;
                isSelectingAttack = false;
                isSelectingFirstMenu = false;
                hasSelectedAttackAndTile = false;
                tileSelector.gameObject.SetActive(true);
                tileSelectorSpriteRenderer.color = new Color(0, 0, 0, 1);
                tileSelector.transform.localScale = Vector3.one;
                UpdateTileSelectorPosition();
            }
        }
        else if (isSelectingTile)
        {
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
        HighlightSelection(currentAttackIndex);
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
            option.GetComponentInChildren<TextMeshProUGUI>().text = player.availableAttacks[i].attackName;
            option.GetComponentInChildren<Image>().sprite = elementIconLibrary.GetIcon(player.availableAttacks[i].elementType);

        }
        HighlightSelection(currentAttackIndex);
    }

    void HighlightSelection(int index)
    {
        for (int i = 0; i < mainCommandPanel.childCount; i++)
        {
            var text = mainCommandPanel.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            text.color = (i == index) ? Color.yellow : Color.white;
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
