using UnityEngine;

public class SimpleSlimeController : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public float moveSpeed = 6f;
    public float rotationLerpSpeed = 8f; // JELLYSLIME GİBİ
    public float stopDistance = 0.5f;

    [Header("Patlama Ayarları")]
    public int explosionDamage = 20;
    public float explosionTriggerDistance = 0.8f;
    public GameObject explosionEffect;

    [Header("Face Direction - JELLYSLIME GİBİ")]
    public Transform frontPoint; // BUNU EKLEDİM!

    private Transform player;
    private Rigidbody rb;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // FRONTPOINT KONTROLÜ - JELLYSLIME GİBİ
        if (frontPoint == null)
        {
            // JellySlime'daki frontPoint'i ara
            frontPoint = transform.Find("FrontPoint");
            if (frontPoint == null)
            {
                // Yeni frontPoint oluştur (JellySlime ile aynı)
                GameObject frontPointObj = new GameObject("FrontPoint");
                frontPoint = frontPointObj.transform;
                frontPoint.SetParent(transform);
                frontPoint.localPosition = new Vector3(0, 0, 0.5f);
            }
        }

        FindPlayer();
        Debug.Log("🎯 Mini Slime JellySlime gibi ayarlandı!");
    }

    void Update()
    {
        if (hasExploded) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        // JELLYSLIME GİBİ TAKİP ET
        FollowPlayerJellyStyle();
        CheckExplosion();
    }

    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // JELLYSLIME'DAKİ FollowPlayer METODUNUN AYNISI
    void FollowPlayerJellyStyle()
    {
        if (player == null || rb == null) return;

        Vector3 slimeForward = GetSlimeForward();
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(slimeForward, directionToPlayer, Vector3.up);
            float rotationAmount = Mathf.Clamp(angle * rotationLerpSpeed * Time.deltaTime, -180f, 180f);
            transform.Rotate(0f, rotationAmount, 0f, Space.World);
        }

        Vector3 moveDirection = GetSlimeForward();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            Vector3 targetVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 5f);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // JELLYSLIME'DAKİ GetSlimeForward METODUNUN AYNISI
    Vector3 GetSlimeForward()
    {
        if (frontPoint != null) return (frontPoint.position - transform.position).normalized;
        return transform.forward;
    }

    void CheckExplosion()
    {
        if (player == null || hasExploded) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= explosionTriggerDistance)
        {
            Explode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (rb != null) rb.linearVelocity = Vector3.zero;
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(explosionDamage);
        }

        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmos()
    {
        // JELLYSLIME GİBİ GIZMOS
        if (frontPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, frontPoint.position);
        }

        // Yön çizgisi
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, GetSlimeForward() * 2f);

        // Patlama menzili
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionTriggerDistance);
    }
}