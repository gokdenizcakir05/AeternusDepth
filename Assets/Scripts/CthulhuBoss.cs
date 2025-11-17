using UnityEngine;
using System.Collections;

public class CthulhuBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 15f;
    public float rotationLerpSpeed = 4f;
    public float minDistanceToPlayer = 3f;
    public float maxDistanceToPlayer = 8f;

    [Header("Face Direction")]
    public Transform frontPoint;

    [Header("Attack Settings")]
    public GameObject laserPrefab;
    public float laserAttackCooldown = 3f;
    public int laserDamage = 15;
    public Transform laserSpawnPoint;
    public int lasersPerShot = 3;
    public float laserSpreadAngle = 20f;
    public float laserSpeed = 15f;

    [Header("Laser Burst Settings")]
    public int laserBurstCount = 2;
    public float timeBetweenBursts = 0.8f;

    [Header("Ground Explosion Settings")]
    public GameObject groundExplosionPrefab;
    public float explosionAttackCooldown = 5f;
    public int explosionsPerAttack = 3;
    public int explosionDamage = 20;
    public float explosionRadius = 3f;
    public float explosionLifetime = 4f;

    [Header("Attack Sequence Settings")]
    public float timeBetweenAttacks = 0.5f;
    public bool useAttackSequence = true;

    [Header("Aggression Settings")]
    public float playerSearchRate = 0.1f;
    public float attackResponseTime = 0.3f;

    [Header("Collision Settings")]
    public LayerMask obstacleLayers = 1;
    public float obstacleCheckDistance = 2f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool alwaysShowGizmos = true;

    // Referanslar
    public BossHealth bossHealth;

    private Transform player;
    private Rigidbody rb;
    private bool isInCombat = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector3 movementVelocity;
    private float movementSmoothTime = 0.2f;
    private float lastPlayerSearchTime;

    void Start()
    {
        // BossHealth kontrolü
        if (bossHealth == null)
        {
            bossHealth = GetComponent<BossHealth>();
            if (bossHealth == null)
            {
                Debug.LogError("❌ BOSS: BossHealth componenti bulunamadı!");
                return;
            }
        }

        // BossHealth event'lerini bağla
        bossHealth.OnPhaseChanged += OnPhaseChanged;
        bossHealth.OnDeath += OnBossDeath;

        // Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 1.5f;

        FindPlayer();

        if (frontPoint == null) CreateFrontPoint();
        if (laserSpawnPoint == null) CreateLaserSpawnPoint();

        Debug.Log("✅ BOSS: Başlangıç tamamlandı");
    }

    void Update()
    {
        if (!bossHealth.IsAlive()) return;

        if (Time.time - lastPlayerSearchTime >= playerSearchRate)
        {
            if (player == null) FindPlayer();
            lastPlayerSearchTime = Time.time;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isInCombat && distanceToPlayer <= detectionRange)
        {
            isInCombat = true;
            bossHealth.ShowHealthBar();
            if (showDebug) Debug.Log("Cthulhu Boss: Savaş başladı!");
        }

        if (isInCombat && bossHealth.IsAlive())
        {
            LookAtPlayer();
            MoveTowardsPlayer();

            if (canAttack && distanceToPlayer <= maxDistanceToPlayer && !isAttacking)
            {
                StartCoroutine(AttackSequence());
            }
        }
    }

    void CreateFrontPoint()
    {
        GameObject frontObj = new GameObject("FrontPoint");
        frontObj.transform.SetParent(transform);
        frontObj.transform.localPosition = new Vector3(0, 1.5f, 2f);
        frontPoint = frontObj.transform;

        if (showDebug) Debug.Log("✅ BOSS: FrontPoint oluşturuldu");
    }

    void CreateLaserSpawnPoint()
    {
        GameObject laserObj = new GameObject("LaserSpawnPoint");
        laserObj.transform.SetParent(transform);
        laserObj.transform.localPosition = new Vector3(0, 2.5f, 1.5f);
        laserSpawnPoint = laserObj.transform;

        if (showDebug) Debug.Log("✅ BOSS: LaserSpawnPoint oluşturuldu");
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }

    IEnumerator AttackSequence()
    {
        canAttack = false;
        isAttacking = true;

        if (showDebug) Debug.Log("Cthulhu: Saldırı dizisi başlıyor!");
        yield return new WaitForSeconds(attackResponseTime);

        // Lazer atışları
        for (int burst = 0; burst < laserBurstCount; burst++)
        {
            ShootMultipleLasers();

            if (showDebug) Debug.Log($"Cthulhu: {burst + 1}. lazer patlaması!");

            if (burst < laserBurstCount - 1)
            {
                float burstTimer = 0f;
                while (burstTimer < timeBetweenBursts)
                {
                    burstTimer += Time.deltaTime;
                    LookAtPlayer();
                    yield return null;
                }
            }
        }

        // Patlama saldırıları
        if (useAttackSequence)
        {
            yield return new WaitForSeconds(timeBetweenAttacks);

            if (showDebug) Debug.Log("Cthulhu: Patlama saldırısı!");

            for (int i = 0; i < explosionsPerAttack; i++)
            {
                CreateGroundExplosion();

                float betweenExplosionTimer = 0f;
                while (betweenExplosionTimer < 0.3f)
                {
                    betweenExplosionTimer += Time.deltaTime;
                    LookAtPlayer();
                    yield return null;
                }
            }
        }

        // Bekleme süresi
        float totalCooldown = useAttackSequence ?
            (laserAttackCooldown + explosionAttackCooldown) / 2f : laserAttackCooldown;

        float cooldownTimer = 0f;
        while (cooldownTimer < totalCooldown)
        {
            cooldownTimer += Time.deltaTime;
            LookAtPlayer();
            yield return null;
        }

        isAttacking = false;
        canAttack = true;

        if (showDebug) Debug.Log("Cthulhu: Saldırı dizisi bitti!");
    }

    void MoveTowardsPlayer()
    {
        if (player == null) return;

        float currentDistance = Vector3.Distance(transform.position, player.position);
        Vector3 moveDirection = Vector3.zero;

        if (currentDistance > maxDistanceToPlayer)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0f;
            moveDirection = directionToPlayer;
        }
        else if (currentDistance < minDistanceToPlayer)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0f;
            moveDirection = -directionToPlayer;
        }

        if (moveDirection != Vector3.zero && IsPathBlocked(moveDirection))
        {
            moveDirection = Vector3.zero;
        }

        Vector3 targetVelocity = moveDirection * moveSpeed;
        movementVelocity = Vector3.Lerp(movementVelocity, targetVelocity, movementSmoothTime);

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(movementVelocity.x, rb.linearVelocity.y, movementVelocity.z);
        }
        else
        {
            transform.position += movementVelocity * Time.deltaTime;
        }
    }

    void ShootMultipleLasers()
    {
        if (player == null || laserPrefab == null) return;

        Vector3 spawnPos = laserSpawnPoint != null ? laserSpawnPoint.position : transform.position + Vector3.up * 2.5f;
        Vector3 playerTarget = player.position + Vector3.up * 1.5f;
        Vector3 baseDirection = (playerTarget - spawnPos).normalized;

        for (int i = 0; i < lasersPerShot; i++)
        {
            Vector3 direction = CalculateLaserDirection(baseDirection, i);
            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(direction));

            CthulhuLaser laserScript = laser.GetComponent<CthulhuLaser>();
            if (laserScript != null)
            {
                laserScript.damage = laserDamage;
                laserScript.speed = laserSpeed;
            }
        }
    }

    Vector3 CalculateLaserDirection(Vector3 baseDirection, int laserIndex)
    {
        if (lasersPerShot == 1)
        {
            return baseDirection;
        }
        else
        {
            float angleStep = laserSpreadAngle / (lasersPerShot - 1);
            float currentAngle = -laserSpreadAngle / 2f + (angleStep * laserIndex);
            Quaternion spreadRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            return spreadRotation * baseDirection;
        }
    }

    void CreateGroundExplosion()
    {
        if (groundExplosionPrefab == null || player == null) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-4f, 4f),
            0f,
            Random.Range(-4f, 4f)
        );

        Vector3 spawnPosition = player.position + randomOffset;

        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            spawnPosition = hit.point + Vector3.up * 0.1f;
        }

        GameObject explosion = Instantiate(groundExplosionPrefab, spawnPosition, Quaternion.identity);

        CthulhuGroundExplosion explosionScript = explosion.GetComponent<CthulhuGroundExplosion>();
        if (explosionScript != null)
        {
            explosionScript.SetupExplosion(explosionDamage, explosionRadius, explosionLifetime);
        }
    }

    void OnPhaseChanged(int newPhase)
    {
        if (showDebug) Debug.Log($"Cthulhu: {newPhase}. faza geçildi!");

        switch (newPhase)
        {
            case 2:
                laserAttackCooldown *= 0.6f;
                explosionsPerAttack++;
                lasersPerShot++;
                laserBurstCount++;
                timeBetweenBursts *= 0.8f;
                moveSpeed *= 1.2f;
                break;
            case 3:
                laserAttackCooldown *= 0.4f;
                explosionsPerAttack += 2;
                lasersPerShot += 2;
                laserBurstCount += 2;
                timeBetweenBursts *= 0.6f;
                moveSpeed *= 1.4f;
                break;
        }
    }

    void OnBossDeath()
    {
        if (showDebug) Debug.Log("Cthulhu Boss yenildi!");
        Destroy(gameObject, 2f);
    }

    bool IsPathBlocked(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, direction, out hit, obstacleCheckDistance, obstacleLayers))
        {
            if (hit.collider.CompareTag("Player")) return false;
            return true;
        }
        return false;
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"✅ BOSS: Player bulundu - {player.name}");
        }
        else
        {
            Debug.LogError("❌ BOSS: Player bulunamadı!");
        }
    }

    // BASİTLEŞTİRİLMİŞ Hasar alma - Mermi ile çarpışma
    void OnTriggerEnter(Collider other)
    {
        if (!bossHealth.IsAlive()) return;

        Debug.Log($"🔵 BOSS TRIGGER: {other.name} (Tag: {other.tag})");

        // SADECE "Bullet" tag'ini kontrol et
        if (other.CompareTag("Bullet"))
        {
            AutoBullet bullet = other.GetComponent<AutoBullet>();
            if (bullet != null)
            {
                Debug.Log($"🎯 BOSS MERMİ HASARI: {bullet.damage}");
                bossHealth.TakeDamage(bullet.damage);
            }
        }
    }

    void OnDestroy()
    {
        // Event bağlantılarını temizle
        if (bossHealth != null)
        {
            bossHealth.OnPhaseChanged -= OnPhaseChanged;
            bossHealth.OnDeath -= OnBossDeath;
        }
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos && !showDebug) return;

        Gizmos.color = isAttacking ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);
        Gizmos.DrawWireSphere(transform.position, maxDistanceToPlayer);

        if (frontPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, frontPoint.position);
            Gizmos.DrawWireSphere(frontPoint.position, 0.3f);
        }
    }
}