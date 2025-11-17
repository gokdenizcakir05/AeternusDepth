using UnityEngine;

public class BossSpike : MonoBehaviour
{
    private int damage;
    private float speed;
    private float lifetime;
    private Vector3 direction;

    public void Launch(Vector3 launchDirection, float spikeSpeed, int spikeDamage, float spikeLifetime)
    {
        direction = launchDirection;
        speed = spikeSpeed;
        damage = spikeDamage;
        lifetime = spikeLifetime;

        // Yönü ayarla - yatay olacak
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Yatay hareket
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}