using UnityEngine;

public class AutoBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;
    public float bulletSpeed = 25f;
    public float bulletLifetime = 2f;
    public GameObject hitEffect;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f;

    private Vector3 direction;
    private Rigidbody rb;
    private bool hasHit = false;
    private bool useRigidbody = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // RIGIDBODY KONTROLÜ - SADECE BİR TANESİNİ KULLAN
        if (rb != null)
        {
            useRigidbody = true;
            rb.linearVelocity = direction * bulletSpeed;
            Debug.Log($"🚀 Rigidbody Hızı: {rb.linearVelocity.magnitude}");
        }
        else
        {
            useRigidbody = false;
            Debug.Log($"🚀 Transform Hareketi: {bulletSpeed}");
        }

        Destroy(gameObject, bulletLifetime);
    }

    public void Initialize(Vector3 dir, float speed, float bulletDamage)
    {
        direction = dir.normalized;
        bulletSpeed = speed;
        damage = bulletDamage;
        transform.forward = direction;

        Debug.Log($"🎯 Mermi Başlatıldı: Speed={bulletSpeed}");
    }

    void Update()
    {
        // RIGIDBODY YOKSA VEYA KİNETİK DEĞİLSE MANUEL HAREKET
        if (!useRigidbody && !hasHit)
        {
            transform.position += direction * bulletSpeed * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // RIGIDBODY VARKEN VE KİNETİKSE BU KISMI KULLAN
        if (useRigidbody && rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit || !other.CompareTag("Enemy")) return;

        hasHit = true;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, direction);
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // TEST İÇİN: Inspector'da hızı değiştirince anında güncelle
    void OnValidate()
    {
        if (Application.isPlaying && rb != null && useRigidbody)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }
}