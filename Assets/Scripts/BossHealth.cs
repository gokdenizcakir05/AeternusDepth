using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1000;
    public int currentHealth;

    [Header("Health Bar UI")]
    public Image healthBarFillImage;
    public TMP_Text healthBarText;
    public GameObject healthBarPanel;

    [Header("Phase Settings")]
    public float phase2Threshold = 0.5f;
    public float phase3Threshold = 0.2f;
    public Color phase1Color = Color.red;
    public Color phase2Color = Color.yellow;
    public Color phase3Color = Color.magenta;

    [Header("Death Settings")]
    public GameObject deathEffectPrefab;
    public float deathEffectDuration = 2f;
    public bool destroyOnDeath = true;
    public float destroyDelay = 0.1f;

    [Header("Debug")]
    public bool showDebug = true;

    // Events
    public System.Action<int> OnDamageTaken;
    public System.Action<int> OnPhaseChanged;
    public System.Action OnDeath;

    private int currentPhase = 1;
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;
    private float healthBarAnimationSpeed = 3f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        InitializeHealthBar();
        Debug.Log($"✅ BOSS HEALTH: Başlatıldı - {maxHealth} HP");
    }

    void Update()
    {
        UpdateHealthBarAnimation();
    }

    void InitializeHealthBar()
    {
        if (healthBarFillImage != null)
        {
            healthBarFillImage.type = Image.Type.Filled;
            healthBarFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthBarFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthBarFillImage.fillAmount = 1f;
            healthBarFillImage.color = phase1Color;
        }

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }

        targetFillAmount = 1f;
        currentFillAmount = 1f;
        UpdateHealthBarUI();
    }

    void UpdateHealthBarAnimation()
    {
        if (healthBarFillImage != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * healthBarAnimationSpeed);
            healthBarFillImage.fillAmount = currentFillAmount;
        }
    }

    void UpdateHealthBarUI()
    {
        targetFillAmount = (float)currentHealth / maxHealth;

        if (healthBarText != null)
        {
            healthBarText.text = $"{currentHealth} / {maxHealth}";
        }

        if (healthBarFillImage != null)
        {
            switch (currentPhase)
            {
                case 1: healthBarFillImage.color = phase1Color; break;
                case 2: healthBarFillImage.color = phase2Color; break;
                case 3: healthBarFillImage.color = phase3Color; break;
            }
        }
    }

    void CheckPhase()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        int newPhase = 1;

        if (healthPercent <= phase3Threshold) newPhase = 3;
        else if (healthPercent <= phase2Threshold) newPhase = 2;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            if (showDebug) Debug.Log($"🔁 BOSS HEALTH: {currentPhase}. faza geçildi!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isDead) return;

        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (showDebug)
        {
            Debug.Log($"💥 BOSS HEALTH: {damage} hasar aldı! {previousHealth} -> {currentHealth} HP");
        }

        UpdateHealthBarUI();
        CheckPhase();
        OnDamageTaken?.Invoke(damage);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        int intDamage = Mathf.RoundToInt(damageAmount);
        TakeDamage(intDamage);
        if (showDebug) Debug.Log($"🔢 BOSS HEALTH: Float damage {damageAmount} -> int {intDamage}");
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        if (showDebug)
        {
            Debug.Log($"❤️ BOSS HEALTH: {healAmount} can yenilendi! {previousHealth} -> {currentHealth} HP");
        }

        UpdateHealthBarUI();
        CheckPhase();
    }

    void Die()
    {
        isDead = true;

        if (showDebug) Debug.Log("💀 BOSS HEALTH: Boss öldü!");

        // Ölüm efekti
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, deathEffectDuration);
        }

        // Event tetikle
        OnDeath?.Invoke();

        // Health bar'ı gizle
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }

        // BossDeathSequence'i başlat
        BossDeathSequence deathSequence = FindObjectOfType<BossDeathSequence>();
        if (deathSequence != null)
        {
            deathSequence.StartDeathSequence();
        }

        // Boss'u yok et
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    public void ShowHealthBar()
    {
        if (healthBarPanel != null && !isDead)
        {
            healthBarPanel.SetActive(true);
            Debug.Log("📊 BOSS HEALTH: Health bar gösterildi");
        }
    }

    public void HideHealthBar()
    {
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0 && !isDead;
    }

    public int GetCurrentPhase()
    {
        return currentPhase;
    }
}