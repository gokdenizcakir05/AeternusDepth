using UnityEngine;
using System.Collections;

public class DamageMound : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Bubble Animation")]
    public float bubbleDuration = 0.8f;
    public float bubbleMaxScale = 2.8f;

    [Header("Visual References")]
    public GameObject bubbleWarning;
    public ParticleSystem explosionParticles;

    private bool hasDamaged = false;
    private AudioSource audioSource;
    private Vector3 originalBubbleScale;
    private float detectionRadius; // YENÝ: Otomatik hesaplanacak

    void Start()
    {
        if (bubbleWarning != null)
            originalBubbleScale = bubbleWarning.transform.localScale;

        // YENÝ: Detection radius'u bubble boyutuna göre otomatik ayarla
        detectionRadius = (originalBubbleScale.x * bubbleMaxScale) * 0.5f;

        audioSource = gameObject.AddComponent<AudioSource>();

        // Collider ekle (boyutu otomatik ayarlandý)
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = detectionRadius;
        collider.enabled = false;

        if (bubbleWarning != null)
        {
            bubbleWarning.SetActive(true);
            bubbleWarning.transform.localScale = Vector3.zero;
        }

        StartCoroutine(BubbleAnimation());
    }

    public void SetupMound(int moundDamage, float moundLifetime, float moundDetectionRadius)
    {
        damage = moundDamage;
        // Artýk detectionRadius parametresini kullanmýyoruz
    }

    IEnumerator BubbleAnimation()
    {
        // 1. BALONCUK BÜYÜME
        if (bubbleWarning != null)
        {
            float growTimer = 0f;
            float growDuration = bubbleDuration * 0.6f;

            while (growTimer < growDuration)
            {
                growTimer += Time.deltaTime;
                float progress = growTimer / growDuration;

                Vector3 targetScale = originalBubbleScale * bubbleMaxScale;
                bubbleWarning.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

                yield return null;
            }
        }

        // 2. COLLIDER'ý AÇ
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        // 3. PATLAMA
        yield return new WaitForSeconds(0.2f);
        Explode();
    }

    void Explode()
    {
        if (explosionParticles != null)
            explosionParticles.Play();

        if (bubbleWarning != null)
            bubbleWarning.SetActive(false);

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDamaged)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            hasDamaged = true;
            Explode();
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos'da detection radius'u göster
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}