using UnityEngine;

public class JellySlime : MonoBehaviour
{
    [Header("Movement Settings")]
    public float bounceSpeed = 4f;
    public float bounceAmount = 0.2f;
    public float jumpHeight = 0.5f;
    public float jumpFrequency = 1f;
    public float detectionRange = 3f;
    public float moveSpeed = 2f;
    public float rotationLerpSpeed = 5f;

    [Header("Face Direction")]
    public Transform frontPoint;

    [Header("Combat Settings")]
    public int bodySlamDamage = 15;
    public float attackCooldown = 2f;

    [Header("Split Settings")]
    public bool enableSplit = true;
    public GameObject miniSlimePrefab;
    public int splitCount = 2;
    public float miniSlimeMoveSpeed = 6f;
    public int miniSlimeExplosionDamage = 20;

    [Header("Optimization Settings")]
    public float playerSearchInterval = 0.3f;
    public float distanceCheckInterval = 0.1f;

    [Header("Debug")]
    public bool showDebug = false;
    public bool alwaysShowGizmos = true;

    private Vector3 originalScale;
    private Vector3 startPosition;
    private float timeOffset;
    private Transform player;
    private bool isFollowing = false;
    private Rigidbody rb;
    private Vector3 lastPlayerPosition;
    private float playerMoveThreshold = 0.5f;

    private float lastPlayerSearchTime;
    private float lastDistanceCheckTime;
    private float currentDistanceToPlayer;
    private float lastAttackTime;
    private EnemyHealth enemyHealth;

    void Start()
    {
        originalScale = transform.localScale;
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f);

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;

        // EnemyHealth component'ini al
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }

        FindPlayer();
        lastPlayerSearchTime = Time.time;
        lastDistanceCheckTime = Time.time;

        if (player != null)
        {
            lastPlayerPosition = player.position;
            currentDistanceToPlayer = Vector3.Distance(transform.position, player.position);
        }
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead()) return;

        if (Time.time - lastPlayerSearchTime >= playerSearchInterval)
        {
            if (player == null)
            {
                FindPlayer();
                if (player == null)
                {
                    NormalAnimation();
                    lastPlayerSearchTime = Time.time;
                    return;
                }
                else
                {
                    lastPlayerPosition = player.position;
                    currentDistanceToPlayer = Vector3.Distance(transform.position, player.position);
                }
            }
            lastPlayerSearchTime = Time.time;
        }

        if (player != null && Time.time - lastDistanceCheckTime >= distanceCheckInterval)
        {
            currentDistanceToPlayer = Vector3.Distance(transform.position, player.position);
            lastDistanceCheckTime = Time.time;
        }

        if (!isFollowing && currentDistanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Slime: Player takip başladı!");
        }

        if (isFollowing)
        {
            FollowPlayer();
        }
        else
        {
            NormalAnimation();
        }

        ApplyBounceAnimation();
    }

    void OnEnemyDeath(GameObject deadEnemy)
    {
        if (showDebug) Debug.Log("🔪 SLIME: EnemyHealth öldü! Bölünme kontrolü...");

        if (enableSplit && miniSlimePrefab != null)
        {
            SplitIntoMiniSlimes();
        }
    }

    void SplitIntoMiniSlimes()
    {
        for (int i = 0; i < splitCount; i++)
        {
            float angle = i * (360f / splitCount);
            Vector3 spawnDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPos = transform.position + spawnDir * 1f;

            GameObject miniSlime = Instantiate(miniSlimePrefab, spawnPos, Quaternion.identity);

            SetupMiniSlime(miniSlime);

            if (showDebug) Debug.Log($"🔪 MINI SLIME {i + 1} oluşturuldu!");
        }
    }

    void SetupMiniSlime(GameObject miniSlimeObj)
    {
        // SimpleSlimeController ekle
        SimpleSlimeController controller = miniSlimeObj.GetComponent<SimpleSlimeController>();
        if (controller == null)
        {
            controller = miniSlimeObj.AddComponent<SimpleSlimeController>();
        }

        // CONTROLLER AYARLARI - JELLYSLIME İLE AYNI
        controller.moveSpeed = miniSlimeMoveSpeed;
        controller.rotationLerpSpeed = 8f; // JELLYSLIME GİBİ
        controller.stopDistance = 0.5f;
        controller.explosionDamage = miniSlimeExplosionDamage;
        controller.explosionTriggerDistance = 0.8f;

        // FRONTPOINT'İ AYARLA - ÖNEMLİ!
        if (controller.frontPoint == null)
        {
            // Mini slime'daki frontPoint'i bul
            Transform miniFrontPoint = miniSlimeObj.transform.Find("FrontPoint");
            if (miniFrontPoint != null)
            {
                controller.frontPoint = miniFrontPoint;
            }
        }

        // JellySlime'ı kapat
        JellySlime jellyScript = miniSlimeObj.GetComponent<JellySlime>();
        if (jellyScript != null)
        {
            jellyScript.enabled = false;
        }

        // Görsel ayarlar
        miniSlimeObj.transform.localScale = transform.localScale * 0.6f;

        // EnemyHealth ayarı
        EnemyHealth miniHealth = miniSlimeObj.GetComponent<EnemyHealth>();
        if (miniHealth != null)
        {
            miniHealth.maxHealth = 5f;
        }

        Debug.Log("✅ Mini slime JellySlime gibi ayarlandı!");
    }

    void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            lastPlayerPosition = player.position;
            return;
        }

        if (player == null)
        {
            playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                lastPlayerPosition = player.position;
            }
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;

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

        if (rb != null)
        {
            Vector3 targetVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 5f);
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    void NormalAnimation()
    {
        float time = Time.time + timeOffset;
        float jump = Mathf.Abs(Mathf.Sin(time * jumpFrequency)) * jumpHeight;
        Vector3 newPosition = new Vector3(transform.position.x, startPosition.y + jump, transform.position.z);

        if (rb != null) rb.MovePosition(newPosition);
        else transform.position = newPosition;
    }

    Vector3 GetSlimeForward()
    {
        if (frontPoint != null) return (frontPoint.position - transform.position).normalized;
        return transform.forward;
    }

    void ApplyBounceAnimation()
    {
        float time = Time.time + timeOffset;
        float bounce = Mathf.Sin(time * bounceSpeed) * bounceAmount;
        transform.localScale = new Vector3(originalScale.x - bounce * 0.5f, originalScale.y + bounce, originalScale.z - bounce * 0.5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (enemyHealth != null && enemyHealth.IsDead()) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(bodySlamDamage);
                    if (showDebug) Debug.Log($"💥 SLIME: Çarpma saldırısı! {bodySlamDamage} hasar!");
                }
                lastAttackTime = Time.time;
            }

            isFollowing = false;
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 faceDirection = GetSlimeForward();
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + faceDirection * 2f);

        if (frontPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(frontPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, frontPoint.position);
        }

        if (isFollowing && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}