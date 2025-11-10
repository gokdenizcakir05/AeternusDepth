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
    public float minBubbleSpeed = 0.3f;

    [Header("Ground Settings")]
    public LayerMask groundLayer = 1;
    public float groundOffset = 0.5f;

    [Header("Knockback Settings")]
    public float knockbackStrength = 3f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool alwaysShowGizmos = true;

    private Transform player;
    private bool isFollowing = false;
    private bool isMoving = false;
    private ParticleSystem bubbleParticleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private float characterHeight;
    private Collider seahorseCollider; // Sadece referans

    // KNOCKBACK SİSTEMİ
    private Vector3 knockbackVelocity;
    private float knockbackDecay = 4f;

    void Start()
    {
        // Collider referansını al (zaten inspector'da var)
        seahorseCollider = GetComponent<Collider>();

        CalculateCharacterHeight();
        FixGroundPosition(); // BU METODU GÜNCELLEDİM
        FindPlayer();
        InitializeBubbleSystem();

        if (showDebug && player == null)
            Debug.LogError("Seahorse: Player bulunamadı!");
    }

    void CalculateCharacterHeight()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            characterHeight = renderer.bounds.size.y;
            if (showDebug) Debug.Log($"Denizatı yüksekliği: {characterHeight}");
        }
        else
        {
            characterHeight = 1f;
            if (showDebug) Debug.Log("Renderer bulunamadı, varsayılan yükseklik kullanılıyor");
        }
    }

    void FixGroundPosition()
    {
        if (seahorseCollider == null)
        {
            if (showDebug) Debug.LogError("Collider bulunamadı!");
            return;
        }

        // Collider'ın bounds'ını al
        Bounds colliderBounds = seahorseCollider.bounds;
        float colliderBottomY = colliderBounds.min.y;
        float colliderHeight = colliderBounds.size.y;

        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f, groundLayer))
        {
            // Collider'ın en altı zemine değecek şekilde ayarla
            float targetY = hit.point.y - colliderBottomY + transform.position.y + groundOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

            if (showDebug)
            {
                Debug.Log($"✅ Denizatı zemine yerleştirildi!");
                Debug.Log($"- Collider Bottom: {colliderBottomY}");
                Debug.Log($"- Hit Point: {hit.point.y}");
                Debug.Log($"- Target Y: {targetY}");
            }
        }
        else
        {
            if (showDebug)
                Debug.LogWarning("Denizatı: Ground bulunamadı!");

            // Collider'ın altını zemin seviyesine getir
            transform.position = new Vector3(transform.position.x, -colliderBottomY + groundOffset, transform.position.z);
        }
    }

    void InitializeBubbleSystem()
    {
        if (bubbleParticlePrefab == null)
        {
            Debug.LogError("❌ BUBBLE PARTICLE PREFAB ATANMAMIŞ!");
            return;
        }

        GameObject bubbleInstance = Instantiate(bubbleParticlePrefab, transform.position, Quaternion.identity);
        bubbleInstance.transform.SetParent(transform);
        bubbleInstance.name = "BubbleParticles";

        bubbleParticleSystem = bubbleInstance.GetComponent<ParticleSystem>();

        if (bubbleParticleSystem == null)
        {
            Debug.LogError("❌ PREFAB'DA PARTICLE SYSTEM COMPONENT'I YOK!");
            return;
        }

        emissionModule = bubbleParticleSystem.emission;
        emissionModule.rateOverTime = 0;

        bubbleParticleSystem.Play();

        if (showDebug)
        {
            Debug.Log($"✅ Bubble Particle System oluşturuldu!");
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

        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Seahorse: Player takip başladı!");
        }

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
            if (showDebug) Debug.Log($"Player bulundu: {player.name}");
        }
        else
        {
            if (showDebug) Debug.LogWarning("Player objesi bulunamadı!");
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

        Vector3 moveDirection = GetSeahorseForward();
        moveDirection.y = 0f;
        moveDirection.Normalize();

        // HAREKET + KNOCKBACK
        Vector3 movement = moveDirection * swimSpeed * Time.deltaTime;
        Vector3 knockbackMovement = knockbackVelocity * Time.deltaTime;

        transform.position += movement + knockbackMovement;

        if (showDebug && Time.frameCount % 90 == 0)
        {
            Debug.Log($"🎯 Movement: {movement.magnitude:F2}, Knockback: {knockbackVelocity.magnitude:F2}");
        }

        MaintainGroundHeight();
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
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 2f);
        }
    }

    void CheckMovementAndControlBubbles()
    {
        bool wasMoving = isMoving;

        isMoving = isFollowing || knockbackVelocity.magnitude > 0.1f;

        if (wasMoving != isMoving && showDebug)
        {
            Debug.Log($"🚀 Hareket durumu: {wasMoving} -> {isMoving}");
        }

        if (bubbleParticleSystem != null)
        {
            bubbleParticleSystem.transform.position = transform.position;

            if (isMoving)
            {
                emissionModule.rateOverTime = bubbleEmissionRate;
                if (!bubbleParticleSystem.isPlaying)
                {
                    bubbleParticleSystem.Play();
                }
            }
            else
            {
                emissionModule.rateOverTime = 0;
                if (bubbleParticleSystem.isPlaying)
                {
                    bubbleParticleSystem.Stop();
                }
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

        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    // TRIGGER ILE Mermi yakalama
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") || other.CompareTag("PlayerBullet"))
        {
            Vector3 bulletDirection = (transform.position - other.transform.position).normalized;
            bulletDirection.y = 0f;
            ApplyKnockback(bulletDirection);
        }
    }

    // Knockback uygulama
    void ApplyKnockback(Vector3 direction)
    {
        knockbackVelocity = direction.normalized * knockbackStrength;
        if (showDebug) Debug.Log($"💥 Knockback uygulandı!");
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        // Collider'ı göster
        if (seahorseCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(seahorseCollider.bounds.center, seahorseCollider.bounds.size);
        }

        // Knockback vektörünü göster
        if (knockbackVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, knockbackVelocity * 0.3f);
        }

        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}