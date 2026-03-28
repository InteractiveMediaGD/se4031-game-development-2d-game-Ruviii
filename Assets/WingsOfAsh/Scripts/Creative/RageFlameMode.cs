using UnityEngine;
using UnityEngine.UI;

public class RageFlameMode : MonoBehaviour
{
    [Header("Health bar fill (assign Slider Fill Rect Image)")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Color healthyGold = new Color(1f, 0.82f, 0.35f, 1f);
    [SerializeField] private Color criticalRed = new Color(0.95f, 0.15f, 0.12f, 1f);

    [Header("Low health vignette (full-screen UI Image, dark red)")]
    [SerializeField] private Image lowHealthVignette;
    [SerializeField] private float maxVignetteAlpha = 0.55f;

    [Header("Background ruin glow (optional SpriteRenderers)")]
    [SerializeField] private SpriteRenderer[] backgroundLayers;
    [SerializeField] private Color ruinsDim = new Color(0.45f, 0.45f, 0.55f, 1f);
    [SerializeField] private Color ruinsBright = new Color(1f, 0.55f, 0.2f, 1f);

    public void Refresh(float currentHealth, int maxHealth)
    {
        float ratio = maxHealth <= 0 ? 0f : Mathf.Clamp01(currentHealth / maxHealth);

        if (healthFillImage != null)
        {
            healthFillImage.color = Color.Lerp(criticalRed, healthyGold, ratio);
        }

        if (lowHealthVignette != null)
        {
            Color c = lowHealthVignette.color;
            c.a = (1f - ratio) * maxVignetteAlpha;
            lowHealthVignette.color = c;
        }

        if (backgroundLayers != null && backgroundLayers.Length > 0)
        {
            Color target = Color.Lerp(ruinsBright, ruinsDim, ratio);
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                if (backgroundLayers[i] != null)
                {
                    backgroundLayers[i].color = target;
                }
            }
        }
    }
}
