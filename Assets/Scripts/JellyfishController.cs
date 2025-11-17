using UnityEngine;
using System.Collections;

public class JellyfishController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float swimSpeed = 3f;
    public float detectionRange = 8f;
    public float rotationLerpSpeed = 5f;

    [Header("Face Direction")]
    public Transform frontPoint;

    [Header("Laser Attack")]
    public GameObject laserPrefab;
    public float attackRange = 6f;
    public float attackCooldown = 3f;
    public int laserDamage = 10;
    public Transform laserSpawnPoint;
    public int lasersPerShot = 2; // 🔫 YENİ: Bir atışta kaç lazer
    public float spreadAngle = 15f; // 🔫 YENİ: Lazerler arası açı

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private bool isFollowing = false;
    private bool canAttack = true;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isFollowing && distanceToPlayer <= detectionRange)
        {
            isFollowing = true;
            if (showDebug) Debug.Log("Jellyfish: Player takip başladı!");
        }

        if (isFollowing)
        {
            FollowPlayer();
        }

        if (isFollowing && distanceToPlayer <= attackRange && canAttack && laserPrefab != null)
        {
            StartCoroutine(PerformAttack());
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FollowPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > 2f)
        {
            Vector3 moveDirection = transform.forward;
            transform.position += moveDirection * swimSpeed * Time.deltaTime;
        }
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;

        if (showDebug) Debug.Log($"Jellyfish: {lasersPerShot} lazer atıyor!");

        // 🔫 ÇİFT LAZER ATIŞI
        ShootMultipleLasers();

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void ShootMultipleLasers()
    {
        if (player == null || laserPrefab == null) return;

        Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.8f;
        if (laserSpawnPoint != null) spawnPos = laserSpawnPoint.position;

        Vector3 playerTarget = player.position + Vector3.up * 1.2f;
        Vector3 baseDirection = (playerTarget - spawnPos).normalized;

        // 🔫 BİRDEN FAZLA LAZER AT
        for (int i = 0; i < lasersPerShot; i++)
        {
            // Her lazer için biraz farklı yön hesapla
            Vector3 direction = CalculateLaserDirection(baseDirection, i);

            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(direction));

            JellyfishLaser laserScript = laser.GetComponent<JellyfishLaser>();
            if (laserScript != null)
            {
                laserScript.damage = laserDamage;
                laserScript.speed = 15f; // İstersen hızı da ayarlayabilirsin
            }

            if (showDebug) Debug.Log($"Lazer {i + 1} atıldı! Yön: {direction}");
        }
    }

    // 🔫 LAZER YÖNÜ HESAPLAMA
    Vector3 CalculateLaserDirection(Vector3 baseDirection, int laserIndex)
    {
        if (lasersPerShot == 1)
        {
            // Tek lazer direkt hedefe
            return baseDirection;
        }
        else
        {
            // Çoklu lazer - spread açısına göre dağıt
            float angleStep = spreadAngle / (lasersPerShot - 1);
            float currentAngle = -spreadAngle / 2f + (angleStep * laserIndex);

            // Açıyı uygula
            Quaternion spreadRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            return spreadRotation * baseDirection;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = isFollowing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);

        // 🔫 LAZER SPREAD GÖSTERGESİ
        if (player != null && lasersPerShot > 1)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 0.8f;
            Vector3 baseDirection = (player.position - spawnPos).normalized;

            Gizmos.color = Color.magenta;
            for (int i = 0; i < lasersPerShot; i++)
            {
                Vector3 dir = CalculateLaserDirection(baseDirection, i);
                Gizmos.DrawRay(spawnPos, dir * 3f);
            }
        }
    }
}