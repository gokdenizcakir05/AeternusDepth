using UnityEngine;
using System.Collections;

public class CthulhuGroundExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public int damage = 20;
    public float radius = 3f;
    public float lifetime = 4f;

    [Header("Animation Settings")]
    public float bubbleDuration = 1f;
    public float bubbleMaxScale = 2f;

    [Header("Visual References")]
    public GameObject bubbleWarning;
    public GameObject explosionEffect;
    public ParticleSystem warningParticles;

    private bool hasExploded = false;
    private Vector3 originalBubbleScale;

    void Start()
    {
        if (bubbleWarning != null)
        {
            originalBubbleScale = bubbleWarning.transform.localScale;
            bubbleWarning.SetActive(false);
            bubbleWarning.transform.localScale = Vector3.zero;
        }

        if (explosionEffect != null)
        {
            explosionEffect.SetActive(false);
        }

        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = radius;
        collider.enabled = false;

        StartCoroutine(ExplosionSequence());
    }

    public void SetupExplosion(int explosionDamage, float explosionRadius, float explosionLifetime)
    {
        damage = explosionDamage;
        radius = explosionRadius;
        lifetime = explosionLifetime;
    }

    IEnumerator ExplosionSequence()
    {
        if (bubbleWarning != null)
        {
            bubbleWarning.SetActive(true);

            float bubbleTimer = 0f;

            while (bubbleTimer < bubbleDuration)
            {
                bubbleTimer += Time.deltaTime;
                float progress = bubbleTimer / bubbleDuration;

                Vector3 targetScale = originalBubbleScale * bubbleMaxScale;
                bubbleWarning.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);

                Renderer renderer = bubbleWarning.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color bubbleColor = Color.Lerp(
                        new Color(1f, 0.3f, 0.3f, 0.5f),
                        new Color(1f, 0f, 0f, 0.8f),
                        progress
                    );
                    renderer.material.color = bubbleColor;
                }

                yield return null;
            }
        }

        if (warningParticles != null)
            warningParticles.Play();

        yield return new WaitForSeconds(0.3f);

        Explode();

        Destroy(gameObject, 1f);
    }

    void Explode()
    {
        hasExploded = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        if (bubbleWarning != null)
            bubbleWarning.SetActive(false);

        if (explosionEffect != null)
        {
            explosionEffect.SetActive(true);
        }

        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider player in hitPlayers)
        {
            if (player.CompareTag("Player"))
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        Invoke("DisableCollider", 0.2f);
    }

    void DisableCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && hasExploded)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}