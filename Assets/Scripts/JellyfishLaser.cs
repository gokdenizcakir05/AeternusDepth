using UnityEngine;

public class JellyfishLaser : MonoBehaviour
{
    public int damage = 10;
    public float speed = 15f;

    void Start()
    {
        // 3 saniye sonra otomatik yok ol
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        // İleri doğru hareket et
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Player'a çarptığında
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // Duvara çarptığında
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}