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
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite thunderIcon;
    [SerializeField] private Sprite earthIcon;
    [SerializeField] private Sprite lightIcon;

    private List<Image> weaknessIcons = new List<Image>();
    private ElementType[] runtimeWeaknesses;

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

    protected virtual void Update()
    {
        if (isBroken) return;

        timerProgress += Time.deltaTime;
    }


    protected virtual void OnExecute()
    {
        Debug.Log($"{data.enemyName} executed action with {health} HP remaining.");
        if (attacks.Length > 0)
        {
            EnemyAttackData chosenAttack = attacks[Random.Range(0, attacks.Length)];
            PerformAttack(chosenAttack);
        }
        else
        {
            Debug.LogWarning($"{data.enemyName} has no attacks defined!");
        }

        
    }

    protected virtual void PerformAttack(EnemyAttackData Enemyattack)
    {
        Debug.Log("Performing attack: " + Enemyattack.attackName);
        GameManager.Instance.LoseLife();
        //TO DO: Add additional attack logic here like animations, and vfx and all that bullshit please. (Prefable make a pause to do these animations that will run about the length of the animation time);
    }


    private Sprite GetElementSprite(ElementType type)
    {
        Debug.Log($"Getting sprite for element type: {type}");
        switch (type)
        {
            case ElementType.Fire: return fireIcon;
            case ElementType.Water: return waterIcon;
            case ElementType.Thunder: return thunderIcon;
            case ElementType.Earth: return earthIcon;
            case ElementType.Light: return lightIcon;
            default: return null;
        }
    }

    public void BreakGuard()
    {
        if (isBroken) return; // prevent repeat Broken triggering
        isBroken = true;
        
        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
        TimelineUnit timeline = GetComponent<TimelineUnit>();
        TimelineIcon icon = timelineManager.GetIconForUnit(timeline);
        float targetProgress;

        if (timeline.timelineProgress >= timeline.PrepareThreshold)
        {
            targetProgress = 0.0f;
            //timeline.timelineProgress = 0f;
            Debug.Log($"{data.enemyName} was Brokenned during prepare. Reset to 0.");
        }
        else
        {
            targetProgress = timeline.timelineProgress - 0.3f;
            //timeline.timelineProgress = Mathf.Max(0f, timeline.timelineProgress - 0.4f);
            Debug.Log($"{data.enemyName} was Brokenned. Timeline moved back 40%.");
        }

        if (icon != null)
        {
            StartCoroutine(icon.PlayBreakEffect(targetProgress));
        }

        
        // ✅ Reset their state so they keep moving forward

        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkWhileBroken());
        }
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
        Color normalColor = spriteRenderer.color;
        Color blinkColor = new Color(1f, 0f, 0f, 1f); // Red color

        while (isBroken)
        {
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = normalColor;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = normalColor;
        blinkCoroutine = null;
    }

    public bool TakeDamage(ElementType type, float amount)
    {
        bool isCrit = isBroken;
        bool isBrokenNow = false;

        if (isCrit)
        {
            amount *= 2.0f;
        }
        health -= Mathf.RoundToInt(amount);
        healthBar.value = health;

        Debug.Log($"{data.enemyName} took {amount} damage. Remaining HP: {health}");

        ShowFloatingDamage(amount);

        // Check weaknesses if needed
        for (int i = 0; i < data.weaknesses.Length; i++)
        {
            if (runtimeWeaknesses[i] == type)
            {
                runtimeWeaknesses[i] = ElementType.None;

                if (i < weaknessIcons.Count)
                {
                    StartCoroutine(WeaknessIconJuiceWhenDestroyed(weaknessIcons[i]));
                }

                if (AllWeaknessesBroken())
                {
                    BreakGuard(); // ✅ Broken enemy
                    isBrokenNow = true;
                    StartCoroutine(PlayBrokenEffect()); // ✅ visual Broken feedback
                }

                Debug.Log($"{data.enemyName}'s weakness broken: {type}");
            }
        }

        if (health <= 0)
        {
            Die();
        }
        return isBrokenNow;
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

        TimelineManager timelineManager = FindObjectOfType<TimelineManager>();
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
