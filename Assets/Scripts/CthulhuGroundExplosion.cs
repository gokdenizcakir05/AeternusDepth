using UnityEngine;
using System.Collections;

public class CthulhuGroundExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public int damage = 20;
    public float damageRadiusMultiplier = 1f; // Bubble scale'ine göre çarpan
    public float lifetime = 4f;

    [Header("Bubble Visual Settings")]
    public float bubbleDuration = 3f;
    public float bubbleMaxScale = 1.8f; // GÖRSEL boyutu - ISTEDİĞİN DEĞER
    public float pulseSpeed = 2f;
    public float warningIntensity = 1.5f;

    [Header("Visual References")]
    public GameObject bubbleWarning;
    public GameObject explosionEffect;
    public ParticleSystem warningParticles;

    [Header("Collision Timing")]
    public float damageDelay = 0.1f;
    public float activeDamageTime = 0.2f;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private bool hasExploded = false;
    private bool isDamageActive = false;
    private Vector3 originalBubbleScale;
    private float currentBubbleScale = 0f; // Şu anki bubble boyutu
    private float pulseTimer = 0f;
    private Material bubbleMaterial;
    private Color originalBubbleColor;
    private Vector3 originalPosition;
    private SphereCollider damageCollider;
    private bool hasDealtDamage = false;

    void Start()
    {
        originalPosition = transform.position;

        if (bubbleWarning != null)
        {
            originalBubbleScale = bubbleWarning.transform.localScale;
            bubbleWarning.SetActive(false);
            bubbleWarning.transform.localScale = Vector3.zero;

            Renderer renderer = bubbleWarning.GetComponent<Renderer>();
            if (renderer != null)
            {
                bubbleMaterial = renderer.material;
                originalBubbleColor = bubbleMaterial.color;
            }
        }

        if (explosionEffect != null)
        {
            explosionEffect.SetActive(false);
        }

        // Collider'ı oluştur
        damageCollider = gameObject.AddComponent<SphereCollider>();
        damageCollider.isTrigger = true;
        damageCollider.radius = 0.1f; // Başlangıçta çok küçük
        damageCollider.enabled = false;

        StartCoroutine(ExplosionSequence());
    }

    IEnumerator ExplosionSequence()
    {
        // UYARI FAZI - Bubble büyüyor
        if (bubbleWarning != null)
        {
            bubbleWarning.SetActive(true);

            float bubbleTimer = 0f;

            while (bubbleTimer < bubbleDuration)
            {
                bubbleTimer += Time.deltaTime;
                pulseTimer += Time.deltaTime;

                float progress = bubbleTimer / bubbleDuration;

                // 1. Bubble'ın görsel boyutunu güncelle
                currentBubbleScale = Mathf.Lerp(0f, bubbleMaxScale, progress);
                bubbleWarning.transform.localScale = originalBubbleScale * currentBubbleScale;

                // 2. Collider boyutunu da AYNI şekilde güncelle
                if (damageCollider != null)
                {
                    damageCollider.radius = (currentBubbleScale * damageRadiusMultiplier) / 2f;
                    // Not: SphereCollider radius'u, sphere'ın yarıçapıdır.
                    // Bubble'ın scale'i diameter (çap) gibidir, bu yüzden /2 yapıyoruz.
                }

                // 3. Görsel efektler
                float pulsePhase = Mathf.Sin(pulseTimer * pulseSpeed) * 0.5f + 0.5f;
                float pulseIntensity = 0.5f + (pulsePhase * warningIntensity);

                Color warningColor = Color.Lerp(
                    Color.yellow,
                    Color.red,
                    progress
                );

                warningColor.a = Mathf.Lerp(0.3f, 0.9f, progress) * pulseIntensity;

                if (bubbleMaterial != null)
                {
                    bubbleMaterial.color = warningColor;
                    bubbleMaterial.SetColor("_EmissionColor", warningColor * pulseIntensity);
                }

                yield return null;
            }
        }

        // Bubble tam boyuta ulaştı
        currentBubbleScale = bubbleMaxScale;

        // Collider'ı da tam boyuta getir
        if (damageCollider != null)
        {
            damageCollider.radius = (bubbleMaxScale * damageRadiusMultiplier) / 2f;
        }

        // PATLAMA ÖNCESİ SON UYARI
        if (warningParticles != null)
        {
            warningParticles.Play();

            yield return new WaitForSeconds(0.3f);
        }

        // PATLAMA VE HASAR FAZI
        Explode();

        // HASAR COLLIDER'INI AKTİF ET
        StartCoroutine(ActivateDamageCollider());

        Destroy(gameObject, 1f);
    }

    void Explode()
    {
        hasExploded = true;

        if (bubbleWarning != null)
            bubbleWarning.SetActive(false);

        if (explosionEffect != null)
        {
            explosionEffect.SetActive(true);

            ParticleSystem explosionParticles = explosionEffect.GetComponent<ParticleSystem>();
            if (explosionParticles != null)
            {
                explosionParticles.Play();
            }
        }
    }

    IEnumerator ActivateDamageCollider()
    {
        // Kısa bir gecikme
        yield return new WaitForSeconds(damageDelay);

        // Collider'ı aktif et
        isDamageActive = true;
        hasDealtDamage = false;
        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }

        Debug.Log($"💥 Hasar alanı aktif! Radius: {damageCollider.radius}, Bubble Scale: {currentBubbleScale}");

        // Sadece kısa bir süre aktif kalsın
        yield return new WaitForSeconds(activeDamageTime);

        // Collider'ı kapat
        isDamageActive = false;
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // SADECE hasar aktifken ve patlama olduktan sonra
        if (other.CompareTag("Player") && hasExploded && isDamageActive && !hasDealtDamage)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasDealtDamage = true;
                Debug.Log($"💥 Tam isabet! Hasar: {damage}");
            }
        }
    }

    // Scene görünümünde bubble ve collider'ı göster
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (damageCollider != null && damageCollider.enabled)
        {
            // HASAR ALANI - KIRMIZI
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageCollider.radius);
        }

        // BUBBLE GÖRSEL BOYUTU - MAVİ (tahmini)
        if (bubbleWarning != null && bubbleWarning.activeSelf)
        {
            Gizmos.color = Color.blue;
            float visualRadius = (bubbleWarning.transform.lossyScale.x * damageRadiusMultiplier) / 2f;
            Gizmos.DrawWireSphere(transform.position, visualRadius);
        }
        else if (Application.isPlaying)
        {
            // Oyun sırasında bubble boyutunu göster
            Gizmos.color = Color.cyan;
            float visualRadius = (currentBubbleScale * damageRadiusMultiplier) / 2f;
            Gizmos.DrawWireSphere(transform.position, visualRadius);
        }
    }

    public void SetupExplosion(int explosionDamage, float explosionBubbleScale, float explosionLifetime)
    {
        damage = explosionDamage;
        bubbleMaxScale = explosionBubbleScale; // Bubble boyutunu ayarla
        lifetime = explosionLifetime;
    }
}