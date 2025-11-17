using UnityEngine;

public class BabySeahorse : MonoBehaviour
{
    [Header("Baby Settings")]
    public int damage = 5;
    public float speed = 8f;
    public float lifetime = 4f;
    public GameObject hitEffect;

    private Vector3 moveDirection;
    private float timer;
    private bool isLaunched = false;

    void Start()
    {
        // Rigidbody ayarlarını garantile
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!isLaunched) return;

        // Hareket
        transform.position += moveDirection * speed * Time.deltaTime;

        // Ömür kontrolü
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector3 direction, float launchSpeed, int launchDamage, float launchLifetime)
    {
        moveDirection = direction;
        speed = launchSpeed;
        damage = launchDamage;
        lifetime = launchLifetime;
        isLaunched = true;

        // Fırlatma yönüne doğru dön
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLaunched) return;

        // Player'a hasar ver
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        // Herhangi bir şeye çarpınca yok ol
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}