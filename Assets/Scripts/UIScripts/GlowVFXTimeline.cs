using UnityEngine;
using UnityEngine.UI;

public class GlowVFXTimeline : MonoBehaviour
{
      [SerializeField] private Image iconImage; // assign the Image on the prefab
    private Material runtimeMat;

    // optional: defaults
    public Color slowColor = new Color(0.2f, 0.6f, 1f, 1f); // blue
    public Color fastColor = new Color(1f, 0.25f, 0.25f, 1f); // red
    public Color normalColor = new Color(1f, 1f, 1f, 1f); // white
    public Color prepColor = new Color(1f, 1f, 0.25f, 1f); // yellow
    public float glowSize = 4f;
    public float glowIntensity = 3f;


    void Awake()
    {
        if (!iconImage) iconImage = GetComponent<Image>();
        // IMPORTANT: instantiate a material per-instance so we don't edit the shared one
        runtimeMat = new Material(iconImage.material);
        iconImage.material = runtimeMat;

        // baseline: no glow
        SetGlowEnabled(false);
    }

    public void SetGlowEnabled(bool enabled)
    {
        runtimeMat.SetFloat("_GlowIntensity", enabled ? glowIntensity : 0f);
        runtimeMat.SetFloat("_GlowSize", glowSize);
    }

    public void SetGlowColor(Color c)
    {
        runtimeMat.SetColor("_GlowColor", c);
    }
    

    // Call these from your timeline speed logic:
    public void ShowFastGlow() { SetGlowColor(fastColor); SetGlowEnabled(true); }
    public void ShowSlowGlow()  { SetGlowColor(slowColor);  SetGlowEnabled(true); }

    public void ShowNormalGlow() { SetGlowColor(normalColor); SetGlowEnabled(false); }
    public void ShowPrepGlow() { SetGlowColor(prepColor); SetGlowEnabled(true); }
    public void ClearGlow() { SetGlowEnabled(false); }
}
