using UnityEngine;
using System.Collections;

public class SeahorseController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimSpeed = 2f;
    public float detectionRange = 5f;
    public float rotationLerpSpeed = 3f;

    [Header("Face Direction")]
    public Transform frontPoint;

    [Header("Bubble Effects")]
    public GameObject bubbleParticlePrefab;
    public float bubbleEmissionRate = 10f;

    [Header("Ground Settings")]
    public LayerMask groundLayer = 1;
    public float groundOffset = 0.5f;

    [Header("Knockback Settings")]
    public float knockbackStrength = 3f;

    [Header("Baby Seahorse Attack")]
    public GameObject babySeahorsePrefab;
    public float attackRange = 4f;
    public float attackCooldown = 3f;
    public int babiesPerAttack = 3;
    public float babySpeed = 8f;
    public int babyDamage = 5;
    public float babyLifetime = 4f;

    [Header("Collision Settings")]
    public LayerMask obstacleLayers = 1;
    public float obstacleCheckDistance = 1f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool alwaysShowGizmos = true;

    private Transform player;
    private bool isFollowing = false;
    private bool isMoving = false;
    private ParticleSystem bubbleParticleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private Collider seahorseCollider;
    private Rigidbody rb;

    // KNOCKBACK SİSTEMİ
    private Vector3 knockbackVelocity;
    private float knockbackDecay = 4f;

    // SALDIRI SİSTEMİ
    private bool canAttack = true;
    private bool isInAttackRange = false;

    void Start()
    {
        seahorseCollider = GetComponent<Collider>();

        // DÜZELTME: Dungeon için gravity AÇIK
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Dungeon ayarları
        rb.useGravity = true; // ✅ DUNGEON İÇİN GRAVITY AÇIK
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;

        FixGroundPosition();
        FindPlayer();
        InitializeBubbleSystem();

        if (showDebug)
        {
            if (player == null) Debug.LogError("Seahorse: Player bulunamadı!");
            if (babySeahorsePrefab == null) Debug.LogError("Seahorse: BabySeahorsePrefab atanmamış!");
        }
    }

    void FixGroundPosition()
    {
        if (seahorseCollider == null) return;

        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f, groundLayer))
        {
            Bounds colliderBounds = seahorseCollider.bounds;
            float colliderBottomY = colliderBounds.min.y;
            float targetY = hit.point.y - colliderBottomY + transform.position.y + groundOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
    }

    void InitializeBubbleSystem()
    {
        if (bubbleParticlePrefab == null) return;

        GameObject bubbleInstance = Instantiate(bubbleParticlePrefab, transform.position, Quaternion.identity);
        bubbleInstance.transform.SetParent(transform);
        bubbleParticleSystem = bubbleInstance.GetComponent<ParticleSystem>();

        if (bubbleParticleSystem != null)
        {
            emissionModule = bubbleParticleSystem.emission;
            emissionModule.rateOverTime = 0;
            bubbleParticleSystem.Play();
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // KNOCKBACK DECAY
        if (knockbackVelocity.magnitude > 0.1f)
        {
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }
        else
        {
            knockbackVelocity = Vector3.zero;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // TAKİP KONTROLÜ
        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Seahorse: Player takip başladı!");
        }

        // SALDIRI MENZİLİ KONTROLÜ
        isInAttackRange = distanceToPlayer <= attackRange;

        // SALDIRI KONTROLÜ
        if (isInAttackRange && canAttack && babySeahorsePrefab != null)
        {
            StartCoroutine(PerformAttack());
        }

        // HAREKET KONTROLÜ
        if (isFollowing)
        {
            FollowPlayer();
        }

        CheckMovementAndControlBubbles();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

        Vector3 seahorseForward = GetSeahorseForward();
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(seahorseForward, directionToPlayer, Vector3.up);
            transform.Rotate(0f, angle * rotationLerpSpeed * Time.deltaTime, 0f, Space.World);
        }

        // ENGEL KONTROLÜ
        Vector3 moveDirection = GetSeahorseForward();
        moveDirection.y = 0f;
        moveDirection.Normalize();

        if (IsPathBlocked(moveDirection))
        {
            if (showDebug) Debug.Log("Engel tespit edildi! Hareket engellendi.");
            return;
        }

        // DÜZELTME: Transform ile hareket (Rigidbody velocity yerine)
        Vector3 movement = moveDirection * swimSpeed * Time.deltaTime;
        Vector3 knockbackMovement = knockbackVelocity * Time.deltaTime;

        // Transform pozisyonunu direkt değiştir
        transform.position += movement + knockbackMovement;

        // Zemin yüksekliğini koru
        MaintainGroundHeight();
    }

    bool IsPathBlocked(Vector3 direction)
    {
        if (seahorseCollider == null) return false;

        // Önünde engel var mı kontrol et
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        float rayLength = obstacleCheckDistance;

        if (Physics.Raycast(rayStart, direction, out hit, rayLength, obstacleLayers))
        {
            // Player'ı engelleme
            if (hit.collider.CompareTag("Player")) return false;

            if (showDebug) Debug.Log($"Engel tespit edildi: {hit.collider.gameObject.name}");
            return true;
        }

        // Yanlarda da engel kontrolü
        Vector3[] sideDirections = new Vector3[]
        {
            Quaternion.Euler(0, 30, 0) * direction,
            Quaternion.Euler(0, -30, 0) * direction
        };

        foreach (Vector3 sideDir in sideDirections)
        {
            if (Physics.Raycast(rayStart, sideDir, out hit, rayLength * 0.7f, obstacleLayers))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void MaintainGroundHeight()
    {
        if (seahorseCollider == null) return;

        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 5f, groundLayer))
        {
            Bounds colliderBounds = seahorseCollider.bounds;
            float colliderBottomY = colliderBounds.min.y;
            float targetY = hit.point.y - colliderBottomY + transform.position.y + groundOffset;

            Vector3 newPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = newPosition; // ✅ Direkt pozisyon ata
        }
    }

    void CheckMovementAndControlBubbles()
    {
        isMoving = isFollowing || knockbackVelocity.magnitude > 0.1f;

        if (bubbleParticleSystem != null)
        {
            bubbleParticleSystem.transform.position = transform.position;

            if (isMoving)
            {
                emissionModule.rateOverTime = bubbleEmissionRate;
                if (!bubbleParticleSystem.isPlaying) bubbleParticleSystem.Play();
            }
            else
            {
                emissionModule.rateOverTime = 0;
                if (bubbleParticleSystem.isPlaying) bubbleParticleSystem.Stop();
            }
        }
    }

    Vector3 GetSeahorseForward()
    {
        if (frontPoint != null)
        {
            Vector3 direction = (frontPoint.position - transform.position).normalized;
            direction.y = 0f;
            return direction;
        }
        return transform.forward;
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;

        if (showDebug) Debug.Log($"🔥 Saldırı başlatılıyor! {babiesPerAttack} bebek fırlatılacak.");

        for (int i = 0; i < babiesPerAttack; i++)
        {
            LaunchBabySeahorse();
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void LaunchBabySeahorse()
    {
        if (babySeahorsePrefab == null || player == null) return;

        Vector3 spawnPosition = transform.position + GetSeahorseForward() * 1f + Vector3.up * 0.5f;
        GameObject baby = Instantiate(babySeahorsePrefab, spawnPosition, Quaternion.identity);

        Vector3 playerTarget = player.position + Vector3.up * 1f;
        Vector3 directionToPlayer = (playerTarget - spawnPosition).normalized;

        Vector3 randomOffset = new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.2f, 0.2f)
        );

        Vector3 finalDirection = (directionToPlayer + randomOffset).normalized;

        BabySeahorse babyScript = baby.GetComponent<BabySeahorse>();
        if (babyScript != null)
        {
            babyScript.Launch(finalDirection, babySpeed, babyDamage, babyLifetime);
        }
        else
        {
            Rigidbody rb = baby.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = finalDirection * babySpeed;
        }

        if (showDebug) Debug.Log($"👶 Küçük denizatı fırlatıldı!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Vector3 bulletDirection = (transform.position - other.transform.position).normalized;
            bulletDirection.y = 0f;
            ApplyKnockback(bulletDirection);
        }
    }

    void ApplyKnockback(Vector3 direction)
    {
        knockbackVelocity = direction.normalized * knockbackStrength;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Duvarlarla çarpışma
        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            if (showDebug) Debug.Log($"Duvar ile çarpışma: {collision.gameObject.name}");

            // Çarpışma anında knockback'i sıfırla
            knockbackVelocity = Vector3.zero;
        }
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        // Takip menzili
        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Saldırı menzili
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Engel kontrol çizgisi
        Gizmos.color = Color.cyan;
        Vector3 direction = GetSeahorseForward();
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, direction * obstacleCheckDistance);

        // Yan kontrol çizgileri
        Vector3[] sideDirections = new Vector3[]
        {
            Quaternion.Euler(0, 30, 0) * direction,
            Quaternion.Euler(0, -30, 0) * direction
        };

        Gizmos.color = Color.blue;
        foreach (Vector3 sideDir in sideDirections)
        {
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, sideDir * obstacleCheckDistance * 0.7f);
        }
    }
}
