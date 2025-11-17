using UnityEngine;

public class CthulhuLaser : MonoBehaviour
{
    public int damage = 15;
    public float speed = 15f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers = (1 << 8) | (1 << 0) | (1 << 10);

    void Start()
    {
        Destroy(gameObject, 3f);

        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
            collider.height = 1.5f;
            collider.direction = 2;
        }
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        int otherLayer = other.gameObject.layer;
        if (((1 << otherLayer) & collisionLayers) != 0)
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
            else if (otherLayer == LayerMask.NameToLayer("Obstacle") ||
                     otherLayer == LayerMask.NameToLayer("Default"))
            {
                Destroy(gameObject);
            }
        }
    }
}