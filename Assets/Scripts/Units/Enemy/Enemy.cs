using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

public class Enemy : MonoBehaviour
{

    [Header("General Enemy Data")]
    public EnemyData data;
    protected float moveCooldown;
    protected float timerProgress = 0f;
    
    [Header("Enemy Stats (Override in child class)")]
    [SerializeField] protected int health;
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;


    [Header("Broken Properties")]
    public bool isBroken = false;
    protected float BrokennedTurnCounter = 0;

    private Coroutine blinkCoroutine;
    [SerializeField] private GameObject BrokenEffectPrefab;
    [SerializeField] private Transform effectAnchor; // Empty transform above enemy



    [Header("Floating Damage Properties")]
    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private Canvas damageSpaceCanvas;

    [SerializeField] private Slider healthBar;

    [Header("Enemy Weaknesses")]
    [SerializeField] private Transform weaknessPanel;
    [SerializeField] private GameObject weaknessIconPrefab;
    [SerializeField] private Sprite redIcon;
    [SerializeField] private Sprite blueIcon;
    [SerializeField] private Sprite yellowIcon;
    [SerializeField] private Sprite greenIcon;
    [SerializeField] private Sprite orangeIcon;
    [SerializeField] private Sprite purpleIcon;


    [Header("Timeline Icon")]
    [SerializeField] public Sprite timelineIconSprite; // For orange weakness
    private List<Image> weaknessIcons = new List<Image>();
    protected ElementType[] runtimeWeaknesses;
    public TimelineIcon timelineIcon;


    [Header("RPG Slot System")]
    public int slotIndex;
    [SerializeField] private SpriteRenderer spriteRenderer;


    [Header("Enemy Attacks")]
    [SerializeField] private EnemyAttackData[] attacks;


    public virtual void Initialize(EnemyData newData, int spawnPos)
    {
        data = newData;
        slotIndex = spawnPos;

        transform.position = FindAnyObjectByType<GridManager>().GetWorldPositionForSlot(slotIndex);

        health = data.health;
        damage = data.damage;
        moveCooldown = data.speed;
        damageSpaceCanvas = GameObject.Find("DamageNumbersCanvas").GetComponent<Canvas>();

        healthBar.maxValue = data.health;
        healthBar.value = health;

        for (int i = 0; i < data.weaknesses.Length; i++)
        {
            ElementType type = data.weaknesses[i];
            GameObject iconObj = Instantiate(weaknessIconPrefab, weaknessPanel);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetElementSprite(type);
            weaknessIcons.Add(img);
        }
        runtimeWeaknesses = (ElementType[])data.weaknesses.Clone();
    }

    private void Awake()
    {
        // Make sure we have a valid renderer even if not wired in the Inspector
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    protected virtual void Update()
    {
        if (isBroken) return;

        timerProgress += Time.deltaTime;
    }


    public virtual void OnExecute()
    {
        Debug.Log($"{data.enemyName} executed action with {health} HP remaining.");
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = true;
        if (attacks.Length > 0)
        {
            EnemyAttackData chosenAttack = attacks[Random.Range(0, attacks.Length)];
            StartCoroutine(ExecuteActionRoutine(chosenAttack));

        }
        else
        {
            Debug.LogWarning($"{data.enemyName} has no attacks defined!");
        }
    }

    private IEnumerator ExecuteActionRoutine(EnemyAttackData chosenAttack)
    {
        Debug.Log($"{data.enemyName} is preparing to attack with {chosenAttack.attackName} ({chosenAttack.elementType})");
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(1.4f); // Simulate preparation time
        PerformAttack(chosenAttack);
        Player player = FindObjectOfType<Player>();
        
        if (player != null)
        {

            player.ShowHitEffect(chosenAttack.elementType);
        }
        GameObject vfxInstance = Instantiate(chosenAttack.vfxPrefab, new Vector3 (player.transform.position.x, player.transform.position.y, -1f), Quaternion.identity);
        CameraShake.Instance.Shake(0.2f, 0.2f);
        yield return new WaitForSeconds(0.5f);
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        timelineManager.isPaused = false;
    }


    protected virtual void PerformAttack(EnemyAttackData Enemyattack)
    {
        Debug.Log("Performing attack: " + Enemyattack.attackName);
        GameManager.Instance.LoseLife(Enemyattack.power);

        //TO DO: Add additional attack logic here like animations, and vfx and all that bullshit please. (Prefable make a pause to do these animations that will run about the length of the animation time);
    }



    private Sprite GetElementSprite(ElementType type)
    {
        switch (type)
        {
            //TODO: search all elements and change icon to them.
            case ElementType.Red: return redIcon;
            case ElementType.Blue: return blueIcon;
            case ElementType.Purple: return purpleIcon;
            case ElementType.Green: return greenIcon;
            case ElementType.Yellow: return yellowIcon;
            case ElementType.Orange: return orangeIcon; // Orange uses light icon
            default: return null;
        }
    }

    public void BreakGuard()
    {
        if (isBroken) return;
        isBroken = true;

        var timelineManager = FindObjectOfType<TimelineManager>();
        var timeline = GetComponent<TimelineUnit>();
        var icon = timelineManager != null ? timelineManager.GetIconForUnit(timeline) : null;

        float targetProgress = (timeline.timelineProgress >= timeline.PrepareThreshold)
            ? 0.0f
            : timeline.timelineProgress - 0.3f;

        if (icon != null)
            StartCoroutine(icon.PlayBreakEffect(targetProgress));

        // Only start blink if we have a valid spriteRenderer
        if (blinkCoroutine == null && spriteRenderer != null)
            blinkCoroutine = StartCoroutine(BlinkWhileBroken());
    }

    public void EndBreak()
    {
        if (!isBroken) return;

        isBroken = false;
        ResetWeaknesses();

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            spriteRenderer.color = Color.white;
            blinkCoroutine = null;
        }

        Debug.Log($"{data.enemyName} is no longer broken. Weaknesses reset.");
    }


    private void ResetWeaknesses()
    {
        runtimeWeaknesses = (ElementType[])data.weaknesses.Clone();

        // Reset icons if needed
        for (int i = 0; i < runtimeWeaknesses.Length; i++)
        {
            if (i < weaknessIcons.Count)
            {
                weaknessIcons[i].sprite = GetElementSprite(runtimeWeaknesses[i]);
                weaknessIcons[i].color = Color.white;
            }
        }
    }


    private IEnumerator BlinkWhileBroken()
    {
        // If there's still no renderer, just bail out
        if (spriteRenderer == null)
            yield break;

        // Cache initial color safely
        Color normalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Color blinkColor  = new Color(1f, 0f, 0f, 1f);

        while (isBroken)
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(0.1f);

            if (spriteRenderer == null) yield break;

            spriteRenderer.color = normalColor;
            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        blinkCoroutine = null;
    }

    public virtual void CheckLayers(ElementType type)
    {
        // Default implementation does nothing, can be overridden by subclasses
    }

    public bool TakeDamage(ElementType type, float amount)
    {
        bool isCrit = isBroken;
        if (isCrit) amount *= 2f;

        health -= Mathf.RoundToInt(amount);
        healthBar.value = health;

        ShowFloatingDamage(amount, isCrit);

        if (health <= 0)
        {
            Die();
            return false; // don't start break stuff if dead
        }

        // Weakness logic after we know it survived
        for (int i = 0; i < data.weaknesses.Length; i++)
        {
            CheckLayers(type);
            if (runtimeWeaknesses[i] == type)
            {
                runtimeWeaknesses[i] = ElementType.None;
                if (i < weaknessIcons.Count)
                    StartCoroutine(WeaknessIconJuiceWhenDestroyed(weaknessIcons[i]));

                if (AllWeaknessesBroken())
                {
                    BreakGuard();
                    StartCoroutine(PlayBrokenEffect());
                    return true;
                }
            }
        }

        return isBroken; // or false if you prefer “just broke this hit” strictly
    }



    private IEnumerator PlayBrokenEffect()
    {
        GameObject effect = Instantiate(BrokenEffectPrefab, effectAnchor.position, Quaternion.identity, transform);
        yield return new WaitForSeconds(1f); // Show for 1 sec
        Destroy(effect);
    }

    private void ShowFloatingDamage(float amount, bool isCrit = false)
    {
        if (floatingDamagePrefab == null || damageSpaceCanvas == null) return;

        Vector3 spawnPos = transform.position; // Lift text slightly
        GameObject dmgObj = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity, damageSpaceCanvas.transform);
        if (dmgObj == null)
        {
            return;
        }
        DamageTextUI dmgText = dmgObj.GetComponent<DamageTextUI>();
        if (dmgText != null)
        {
            dmgText.Setup(amount.ToString(), isCrit);
        }
    }

    private IEnumerator WeaknessIconJuiceWhenDestroyed(Image icon)
    {
        RectTransform rt = icon.GetComponent<RectTransform>();

        Vector3 originalScale = rt.localScale;
        Vector3 poppedScale = originalScale * 1.3f;
        Vector3 originalPos = rt.localPosition;

        float shakeIntensity = 5f;
        float duration = 0.3f;
        float t = 0f;

        rt.localScale = poppedScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float shakeAmount = Mathf.Sin(t * 50f) * shakeIntensity;
            rt.localPosition = originalPos + new Vector3(shakeAmount, 0f, 0f);
            yield return null;
        }

        // Reset position and scale
        rt.localPosition = originalPos;
        rt.localScale = originalScale;

        // Fade to gray
        icon.color = Color.gray;
    }

    private void Die()
    {
            Debug.Log($"{data.enemyName} died!");

    // Stop blinking safely and restore color if possible
    if (blinkCoroutine != null)
    {
        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
    }
    if (spriteRenderer != null)
        spriteRenderer.color = Color.white;

    // Unregister from timeline before destroy
    TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
    if (timelineManager != null)
        timelineManager.UnregisterUnit(GetComponent<TimelineUnit>());

    Destroy(gameObject);
    }


    private bool AllWeaknessesBroken()
    {
        foreach (ElementType type in runtimeWeaknesses)
        {
            if (type != ElementType.None)
                return false;
        }
        return true;
    }
}
