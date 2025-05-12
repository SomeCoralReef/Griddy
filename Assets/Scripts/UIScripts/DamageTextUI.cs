using UnityEngine;
using TMPro;


public class DamageTextUI : MonoBehaviour
{
    public float floatSpeed = 20f;
    public float fadeDuration = 1f;

    private TextMeshProUGUI text;
    private CanvasGroup canvasGroup;
    private Vector3 moveDirection;
    private float elapsed = 0f;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        moveDirection = new Vector3(0.0f, 1.0f, 0.0f);
    }

    public void Setup(string damageText)
    {
        text.text = damageText;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += moveDirection * floatSpeed * Time.deltaTime;

        canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
        if (canvasGroup.alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
