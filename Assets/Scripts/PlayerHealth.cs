using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityTime = 1f;

    [Header("Damage Flash Effect")]
    public SkinnedMeshRenderer characterRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;
    public int flashCount = 2;

    [Header("Events")]
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDeath;
    public UnityEvent<int> OnHealthChanged;

    [Header("Debug")]
    public bool showDebug = true;

    private float lastDamageTime;
    private Material[] originalMaterials;
    private Coroutine flashCoroutine;
    private static PlayerHealth instance;

    

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);

        if (characterRenderer != null)
        {
            originalMaterials = characterRenderer.materials;
        }

        // BONUS CANLARI UYGULA
        ApplyHealthBonuses();

        if (showDebug) Debug.Log("PlayerHealth: Health sistemi ba�lat�ld�. Can: " + currentHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || currentHealth <= 0)
            return;

        if (Time.time - lastDamageTime < invincibilityTime)
            return;

        currentHealth -= damageAmount;
        lastDamageTime = Time.time;

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke();

        StartFlashEffect();

        if (showDebug) Debug.Log("PlayerHealth: " + damageAmount + " hasar ald�! Kalan can: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void StartFlashEffect()
    {
        if (characterRenderer == null)
        {
            if (showDebug) Debug.LogWarning("PlayerHealth: Character renderer atanmam��!");
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        Material[] flashMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            flashMaterials[i] = new Material(originalMaterials[i]);
            flashMaterials[i].color = flashColor;

            if (flashMaterials[i].HasProperty("_EmissionColor"))
            {
                flashMaterials[i].SetColor("_EmissionColor", flashColor * 2f);
                flashMaterials[i].EnableKeyword("_EMISSION");
            }
        }

        for (int flashIndex = 0; flashIndex < flashCount; flashIndex++)
        {
            characterRenderer.materials = flashMaterials;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            characterRenderer.materials = originalMaterials;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }

        characterRenderer.materials = originalMaterials;

        foreach (var mat in flashMaterials)
        {
            DestroyImmediate(mat);
        }
    }

    public void Heal(int healAmount)
    {
        int actualMaxHealth = maxHealth;

        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            actualMaxHealth += playerStats.maxHealthBonus;
        }

        currentHealth = Mathf.Min(currentHealth + healAmount, actualMaxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (showDebug) Debug.Log($"PlayerHealth: {healAmount} can yenilendi! Mevcut can: {currentHealth}/{actualMaxHealth}");
    }

    public void RestoreFullHealth()
    {
        int actualMaxHealth = maxHealth;

        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            actualMaxHealth += playerStats.maxHealthBonus;
        }

        currentHealth = actualMaxHealth;
        OnHealthChanged?.Invoke(currentHealth);

        if (showDebug) Debug.Log("PlayerHealth: Can tamamen yenilendi!");
    }

    // YEN�: Bonus can ekleme metodu
    public void AddMaxHealth(int bonusHealth)
    {
        int oldMaxHealth = maxHealth;
        maxHealth += bonusHealth;
        currentHealth += bonusHealth;

        OnHealthChanged?.Invoke(currentHealth);

        if (showDebug) Debug.Log($"PlayerHealth: Maksimum Can {oldMaxHealth} -> {maxHealth} (+{bonusHealth})");
    }

    // YEN�: Mevcut bonuslar� uygula
    public void ApplyHealthBonuses()
    {
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null && playerStats.maxHealthBonus > 0)
        {
            AddMaxHealth(playerStats.maxHealthBonus);
        }
    }

    private void Die()
    {
        if (showDebug) Debug.Log("PlayerHealth: Player �ld�!");

        OnDeath?.Invoke();

        if (characterRenderer != null)
        {
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                originalMaterials[i].color = Color.red;
            }
        }

        // Karakter kontrol�n� devre d��� b�rak
        ybotController controller = GetComponent<ybotController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Rigidbody'yi dondur
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;

        if (showDebug) Debug.Log("PlayerHealth: Invincibility s�resi bitti.");
    }

    public float GetHealthPercentage()
    {
        int actualMaxHealth = maxHealth;
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            actualMaxHealth += playerStats.maxHealthBonus;
        }
        return (float)currentHealth / actualMaxHealth;
    }

    public bool IsFullHealth()
    {
        int actualMaxHealth = maxHealth;
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            actualMaxHealth += playerStats.maxHealthBonus;
        }
        return currentHealth >= actualMaxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}