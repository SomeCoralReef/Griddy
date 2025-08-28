// Refactored PlayerActionUI.cs with back button functionality in sub-menu and tile selector
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
    [SerializeField] private Transform tileSelectorPanel;

    [Header("Element Icon Library")]
    public ElementIconLibrary elementIconLibrary;

    private GridManager gridManager;
    private Enemy currentlyHoveredEnemy = null;
    private List<TextMeshProUGUI> mainCommandTexts = new();
    private List<TextMeshProUGUI> subCommandTexts = new();

    private List<GameObject> mainCommandButtons = new();
    private List<GameObject> subCommandButtons = new();

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
    [SerializeField] private bool isSelectingTile = false;
    private bool searchingForEnemyToHighlight = false;

    public bool hasConfirmedActionAndTile = false;

    [Header("Tile Selector UI")]
    public GameObject backSlotButtonPrefab; // Assign in Inspector
    private GameObject backSlotVisual;

    private GameObject tileBackButton;

    private TextMeshProUGUI tileBackButtonText;
    [SerializeField] private bool isTileSelectingPlayers;

    private bool hasShownMainMenuOnce = false;
    private bool comingFromTileSelection = false;

    private int lastTargetSlotIndex = int.MinValue;

    private List<Image> mainCommandImages = new();
    private List<Image> subCommandImages = new();
    private Sprite defaultMainButtonSprite;
    private Sprite highlightedMainButtonSprite;



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
        else if (isSelectingSecondMenu)
        {
            HandleSubMenuInput();
        }
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
        int count = GetSubActionCount() + 1; // +1 for back button
        if (Input.GetKeyDown(KeyCode.W)) currentSecondMenuIndex = Mathf.Max(0, currentSecondMenuIndex - 1);
        if (Input.GetKeyDown(KeyCode.S)) currentSecondMenuIndex = Mathf.Min(count - 1, currentSecondMenuIndex + 1);
        HighlightSelection(subCommandTexts, currentSecondMenuIndex);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentSecondMenuIndex == subCommandTexts.Count - 1)
            {
                ShowMainCommands();
                return;
            }

            ActionData selected = GetCurrentSelectedAction();
            if (selected == null)
            {
                Debug.LogWarning("Selected action is null");
                return;
            }
            player.SelectAction(selected);
            if (!selected.needsTarget)
            {
                player.OnConfirmAction();
                StartCoroutine(FadeTileSelector(0.5f));
            }
            else
            {
                StartTargetSelection(selected);
            }
        }
    }

    void ShowMainCommands()
    {
        isSelectingFirstMenu = true;
        isSelectingSecondMenu = false;
        isSelectingTile = false;
        currentCategory = CommandCategory.Attack;
        currentSecondMenuIndex = 0;

        bool animate = false;

        if (!hasShownMainMenuOnce)
        {
            animate = true;
            hasShownMainMenuOnce = true;
        }

        ClearPanel(mainCommandPanel, mainCommandTexts, mainCommandButtons, animateOut: true);
        ClearPanel(subCommandPanel, subCommandTexts, subCommandButtons, animateOut: true);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainCommandPanel.GetComponent<RectTransform>());

        CreateMainButton("Attack", () => ShowSubPanel(CommandCategory.Attack), animate: animate, 0);
        CreateMainButton("Spells", () => ShowSubPanel(CommandCategory.Spells), animate: animate, 1);
        CreateMainButton("Items", () => ShowSubPanel(CommandCategory.Items), animate: animate, 2);
        CreateMainButton("Defend", () => CloseAllPanels(), animate: animate, 3);
    }

    void ShowSubPanel(CommandCategory category)
    {
        ClearPanel(subCommandPanel, subCommandTexts, subCommandButtons, animateOut: true);
        currentCategory = category;

        List<ActionData> actions = GetActionsByCategory(category);
        bool animate = !comingFromTileSelection; // Only animate if not coming from tile select
        comingFromTileSelection = false; // Reset after use

        for (int i = 0; i < actions.Count; i++)
        {
            int index = i;
            CreateSubButton(actions[i].actionName, () => StartSubAction(index), animate: animate, index);
        }
        CreateSubButton("Back", ShowMainCommands, animate: animate, actions.Count);

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

    void StartTargetSelection(ActionData actionData)
    {
        isSelectingSecondMenu = false;
        isSelectingTile = true;

        tileSelector.SetActive(true);
        tileSelectorSpriteRenderer.color = Color.black;
        tileSelector.transform.localScale = Vector3.one;

        if (actionData is ItemData)
        {
            if (actionData is ItemData itemData && itemData.targetAllies == true)
            {
                //TO DO : Make the target tile go over the player's slot
                Debug.Log("Item targets allies, setting target slot to player's slot.");

            }
        }
        else
        {
            targetSlotIndex = 0;
        }
        UpdateTileSelectorPosition();
        Debug.Log($"Starting target selection for action: {actionData.actionName}");
        CreateBackButton(tileSelectorPanel, () =>
        {
            Debug.Log("Back button pressed in tile selector");
            tileSelector.SetActive(false);
            isSelectingTile = false;
            if (backSlotVisual != null) Destroy(backSlotVisual);
            ShowSubPanel(currentCategory);
        });

        // Instantiate back visual at slot -1
        if (backSlotVisual != null) Destroy(backSlotVisual);
        Vector3 backPos = gridManager.GetWorldPositionForSlot(-1);
    }

    void HandleTileInput()
    {
        int maxSlot = gridManager.slots - 1;

        if (Input.GetKeyDown(KeyCode.S)) targetSlotIndex--;
        if (Input.GetKeyDown(KeyCode.W)) targetSlotIndex++;

        if (targetSlotIndex == -1)
        {
            
            tileSelector.SetActive(false);
        }
        else
        {
            tileSelector.SetActive(true);
        }

        // Clamp to allow -1 for back
        targetSlotIndex = Mathf.Clamp(targetSlotIndex, -1, maxSlot);

        /*
        if (targetSlotIndex == -1)
        {
            tileSelector.SetActive(false);
            isSelectingTile = false;
            if (backSlotVisual != null) Destroy(backSlotVisual);
            ShowSubPanel(currentCategory);
            return;
        }
        */


        UpdateTileSelectorPosition();

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (targetSlotIndex == -1)
            {
                tileSelector.transform.position = tileBackButton.transform.position;
                tileSelectorSpriteRenderer.color = Color.gray;
                tileSelector.SetActive(false);
                

                if (tileBackButton != null) Destroy(tileBackButton);
                tileBackButton = null;

                isSelectingTile = false;
                if (backSlotVisual != null)
                {
                    Destroy(backSlotVisual);
                }

                comingFromTileSelection = true;
                ShowSubPanel(currentCategory);
                return;
            }
            player.AimAtSlot(targetSlotIndex);
            player.OnConfirmAction();
            tileSelectorSpriteRenderer.color = Color.red;
            tileSelector.SetActive(false);
            isSelectingTile = false;
        }
    }

    void CreateMainButton(string label, Action action, bool animate = true, int index = 0)
{
    var btn = Instantiate(commandButtonPrefab, mainCommandPanel);
    var text = btn.GetComponentInChildren<TextMeshProUGUI>();
    var image = btn.GetComponentInChildren<Image>();

    text.text = label;
    btn.GetComponent<Button>().onClick.AddListener(() => action());

    mainCommandTexts.Add(text);
    mainCommandButtons.Add(btn);
    mainCommandImages.Add(image);

    if (index == 0)
    {
            // Only need to do this once
            var button = btn.GetComponent<Button>();
            defaultMainButtonSprite = image.sprite;
            highlightedMainButtonSprite = button.spriteState.highlightedSprite;
    }

    LayoutRebuilder.ForceRebuildLayoutImmediate(mainCommandPanel.GetComponent<RectTransform>());

    if (animate)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchoredPosition -= new Vector2(100f, 0);

        StartCoroutine(DelayPrepareAndAnimate(btn, 0.05f * index));
    }
}


    void CreateSubButton(string label, Action action, bool animate = true, int index = 0)
{
    var btn = Instantiate(commandButtonPrefab, subCommandPanel);
    var text = btn.GetComponentInChildren<TextMeshProUGUI>();
    var image = btn.GetComponentInChildren<Image>();

    text.text = label;
    btn.GetComponent<Button>().onClick.AddListener(() => action());

    subCommandTexts.Add(text);
    subCommandButtons.Add(btn);
    subCommandImages.Add(image);

    LayoutRebuilder.ForceRebuildLayoutImmediate(subCommandPanel.GetComponent<RectTransform>());

    if (animate)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchoredPosition -= new Vector2(100f, 0);

        StartCoroutine(DelayPrepareAndAnimate(btn, 0.05f * index));
    }
}


    void CreateBackButton(Transform panel, Action action)
    {
        if (tileBackButton != null) Destroy(tileBackButton); // Avoid duplicates

        Canvas canvas = GameObject.Find("Attack Panel")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Attack Panel canvas not found!");
            return;
        }


        tileBackButton = Instantiate(backButtonPrefab, tileSelectorPanel);

        Image buttonImage = tileBackButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = tileBackButton.GetComponent<Button>().spriteState.pressedSprite;
        }


        // 🔧 Normalize scale and reset RectTransform
        RectTransform rt = tileBackButton.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;                // Ensure it doesn’t inherit weird scaling
        rt.anchoredPosition3D = Vector3.zero;       // Clear position offset
        rt.localPosition = Vector3.zero;            // Reset local position
        rt.sizeDelta = new Vector2(160, 30);        // You can adjust this to match your other buttons

        // 👇 Position it visually below the lowest enemy slot
        float lowestY = float.MaxValue;
        for (int i = 0; i < gridManager.slots; i++)
        {
            Vector3 slotPos = gridManager.GetWorldPositionForSlot(i);
            if (slotPos.y < lowestY)
                lowestY = slotPos.y;
        }

        Vector3 backPos = new Vector3(player.transform.position.x, lowestY - 1f, 0f);
        tileBackButton.transform.position = backPos;

        // Hook up the button action
        tileBackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
        tileBackButtonText = tileBackButton.GetComponentInChildren<TextMeshProUGUI>();
        tileBackButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            comingFromTileSelection = true;
            action();
        });

        // Optional: match selector color or styling here
    }

    void HighlightSelection(List<TextMeshProUGUI> texts, int selectedIndex)
{
    for (int i = 0; i < texts.Count; i++)
    {
        bool isSelected = (i == selectedIndex);
        texts[i].color = isSelected ? Color.yellow : Color.white;

        if (texts == mainCommandTexts && i < mainCommandImages.Count)
        {
            mainCommandImages[i].sprite = isSelected ? highlightedMainButtonSprite : defaultMainButtonSprite;
        }
        else if (texts == subCommandTexts && i < subCommandImages.Count)
        {
            subCommandImages[i].sprite = isSelected ? highlightedMainButtonSprite : defaultMainButtonSprite;
        }
    }
}


    void ClearPanel(Transform panel, List<TextMeshProUGUI> associatedTextList, List<GameObject> associatedButtonList, bool animateOut = true)
{
    if (animateOut)
    {
        foreach (Transform child in panel)
        {
            StartCoroutine(AnimateUIOutAndDestroy(child.gameObject));
        }
    }
    else
    {
        // Collect list of objects to destroy without interfering with the loop
        List<GameObject> childrenToDestroy = new();
        foreach (Transform child in panel)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        foreach (GameObject obj in childrenToDestroy)
        {
            Destroy(obj);
        }
    }


    foreach (GameObject btn in associatedButtonList)
    {
        Destroy(btn);
    }

    associatedTextList.Clear();

    if (panel == mainCommandPanel) mainCommandImages.Clear();
    else if (panel == subCommandPanel) subCommandImages.Clear();
}


    void ChangeMainCategory(int direction)
    {
        currentCategory = (CommandCategory)(((int)currentCategory + direction + 4) % 4);
    }

    void UpdateTileSelectorPosition()
{
    if (targetSlotIndex == lastTargetSlotIndex) return;

    if (targetSlotIndex == -1)
    {
        if (tileBackButton != null)
        {
            tileSelector.transform.position = tileBackButton.transform.position;
            tileSelectorSpriteRenderer.color = Color.gray;

            Button button = tileBackButton.GetComponent<Button>();
            Image buttonImage = tileBackButton.GetComponentInChildren<Image>();

            if (button != null && buttonImage != null)
            {
                buttonImage.sprite = button.spriteState.highlightedSprite;
            }

            if (tileBackButtonText != null)
            {
                tileBackButtonText.color = Color.yellow;
            }
        }
    }
    else
    {
        desiredSelectorWorldPos = gridManager.GetWorldPositionForSlot(targetSlotIndex);
        tileSelectorSpriteRenderer.color = Color.black;

        if (tileBackButtonText != null)
        {
            tileBackButtonText.color = Color.white;
        }

        if (tileBackButton != null)
        {
            Button button = tileBackButton.GetComponent<Button>();
            Image buttonImage = tileBackButton.GetComponentInChildren<Image>();

            if (button != null && buttonImage != null)
            {
                buttonImage.sprite = button.spriteState.pressedSprite;
            }
        }
    }

    lastTargetSlotIndex = targetSlotIndex;
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
        mainCommandPanel.gameObject.SetActive(true);
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

    public void CloseAllPanels()
    {
        mainCommandPanel.gameObject.SetActive(false);
        subCommandPanel.gameObject.SetActive(false);
    }


    IEnumerator AnimateUIIn(GameObject uiElement, float duration = 0.3f, float delay = 0f)
    {
        yield return new WaitForEndOfFrame(); // Ensure layout is ready

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        RectTransform rt = uiElement.GetComponent<RectTransform>();
        CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();

        Vector2 endPos = rt.anchoredPosition;
        Vector2 startPos = endPos - new Vector2(100f, 0); // Slide from left


        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            cg.alpha = t;
            yield return null;
        }

        cg.alpha = 1f;
        rt.anchoredPosition = endPos;
    }

    IEnumerator AnimateUIOutAndDestroy(GameObject uiElement, float duration = 0.3f)
{
    if (uiElement == null) yield break;

    RectTransform rt = uiElement.GetComponent<RectTransform>();
    CanvasGroup cg = uiElement.GetComponent<CanvasGroup>();

    if (rt == null || cg == null) yield break; // Prevent error if object was destroyed early

    Vector2 startPos = rt.anchoredPosition;
    Vector2 endPos = startPos + new Vector2(-100f, 0); // Slide to left

    float elapsed = 0f;

    while (elapsed < duration)
    {
        if (rt == null || cg == null) yield break;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
        cg.alpha = 1f - t;
        yield return null;
    }

    if (uiElement != null)
        Destroy(uiElement);
}


    void PrepareUIForAnimation(GameObject uiElement)
    {
        var rt = uiElement.GetComponent<RectTransform>();
        var cg = uiElement.GetComponent<CanvasGroup>();

        Vector2 endPos = rt.anchoredPosition;
        Vector2 startPos = endPos - new Vector2(100f, 0); // Slide from left
        rt.anchoredPosition = startPos;

        if (cg != null)
        {
            cg.alpha = 0f;
        }
        else
        {
            cg = uiElement.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }
    }
    
    IEnumerator DelayPrepareAndAnimate(GameObject uiElement, float delay)
    {
        yield return new WaitForEndOfFrame(); // Frame 1 — layout system assigns position
        yield return new WaitForEndOfFrame(); // Frame 2 — layout system finalizes position

        PrepareUIForAnimation(uiElement);
        yield return AnimateUIIn(uiElement, 0.3f, delay);
    }



}
