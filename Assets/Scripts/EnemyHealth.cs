using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    public GameObject deathEffect;

    [Header("Chest Drop Settings")]
    public GameObject chestPrefab;
    [Range(0f, 1f)] public float chestDropChance = 0.0f; // 0 YAP - chest düşmesin
    public bool guaranteedDrop = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.3f;

    [Header("Debug")]
    public bool showDebug = true;

    // EVENT - RoomManager için gerekli
    public static event Action<GameObject> OnEnemyDeath;
    public event Action<GameObject> OnDeath;

    private float currentHealth;
    private bool isDead = false;
    private Rigidbody rb;
    private Collider enemyCollider;
    private Vector3 lastHitDirection;
    private float knockbackTimer = 0f;
    private bool isKnockback = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();

        // TAG KONTROLÜ - ÇOK ÖNEMLİ!
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
            if (showDebug) Debug.Log($"🔧 {gameObject.name} tag'i 'Enemy' yapıldı");
        }

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (showDebug) Debug.Log($"🟢 {gameObject.name} canı: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isKnockback && rb != null)
        {
            rb.AddForce(lastHitDirection * knockbackForce * 0.5f, ForceMode.Force);
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (showDebug)
        {
            Debug.Log($"💥 {gameObject.name} {damage} hasar aldı! Kalan can: {currentHealth}");
        }

        ApplyKnockback(hitDirection);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ApplyKnockback(Vector3 direction)
    {
        if (rb == null) return;

        lastHitDirection = direction.normalized;
        isKnockback = true;
        knockbackTimer = knockbackDuration;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(lastHitDirection * knockbackForce, ForceMode.Impulse);

        if (showDebug) Debug.Log($"🔴 {gameObject.name} geri tepme: {lastHitDirection * knockbackForce}");
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebug) Debug.Log($"💀 {gameObject.name} ANINDA ÖLDÜ!");

        // ROOM MANAGER'A HABER VER - BU ÇOK ÖNEMLİ!
        OnEnemyDeath?.Invoke(gameObject);
        OnDeath?.Invoke(gameObject);

        // Ölüm efekti
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Tüm script'leri devre dışı bırak
        DisableAllScripts();

        // Collider'ı devre dışı bırak
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Rigidbody'yi devre dışı bırak
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // OBJE YOK ET
        Destroy(gameObject);
    }

    void DisableAllScripts()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this && script.enabled)
            {
                script.enabled = false;
            }
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsInKnockback()
    {
        return isKnockback;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        // Can barını göster
        Vector3 worldPos = transform.position + Vector3.up * 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldPos, new Vector3(1f, 0.2f, 0f));

        if (!isDead)
        {
            Gizmos.color = Color.green;
            float healthPercent = currentHealth / maxHealth;
            Gizmos.DrawCube(worldPos, new Vector3(healthPercent, 0.15f, 0f));
        }
    }
}