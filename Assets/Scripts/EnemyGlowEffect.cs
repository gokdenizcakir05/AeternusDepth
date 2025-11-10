using UnityEngine;
using System.Collections;

public class EnemyGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.red;
    public float glowIntensity = 2f;
    public float pulseSpeed = 2f;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;

    private Renderer enemyRenderer;
    private Material glowMaterial;
    private bool isPulsing = true;

    void Start()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            // Mevcut materyali kopyala ve glow ekle
            glowMaterial = new Material(enemyRenderer.material);
            enemyRenderer.material = glowMaterial;

            // Glow özelliklerini ayarla
            SetupGlowMaterial();

            StartCoroutine(PulseGlow());
        }
    }

    void SetupGlowMaterial()
    {
        // Standard Shader için
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
        glowMaterial.EnableKeyword("_EMISSION");

        // Outline shader kullanýyorsan
        if (glowMaterial.HasProperty("_OutlineColor"))
        {
            glowMaterial.SetColor("_OutlineColor", glowColor);
            glowMaterial.SetFloat("_OutlineWidth", 0.1f);
        }
    }

    IEnumerator PulseGlow()
    {
        while (isPulsing)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

            glowMaterial.SetColor("_EmissionColor", glowColor * currentIntensity);

            yield return null;
        }
    }

    public void SetGlowColor(Color newColor)
    {
        glowColor = newColor;
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }

    void OnDestroy()
    {
        isPulsing = false;
        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}