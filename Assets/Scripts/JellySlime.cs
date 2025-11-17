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

    [Header("Player Trail Mound Settings")]
    public bool enableTrailMounds = true;
    public GameObject moundPrefab;
    public float moundSpawnInterval = 3f;
    public float moundDetectionRadius = 1.5f;
    public float moundLifetime = 5f;
    public int moundDamage = 10;
    public LayerMask groundLayer = 1;

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
    private float lastMoundSpawnTime;
    private Vector3 lastPlayerPosition;
    private float playerMoveThreshold = 0.5f;

    private float lastPlayerSearchTime;
    private float lastDistanceCheckTime;
    private float currentDistanceToPlayer;

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

        FindPlayer();
        lastMoundSpawnTime = Time.time;
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

            if (enableTrailMounds && Time.time - lastMoundSpawnTime >= moundSpawnInterval)
            {
                TrySpawnMoundAtPlayerPosition();
                lastMoundSpawnTime = Time.time;
            }
        }
        else
        {
            NormalAnimation();
        }

        ApplyBounceAnimation();
    }

    // DÜZELTİLDİ: PlayerMovement referansı kaldırıldı
    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            lastPlayerPosition = player.position;
            return;
        }

        // Fallback: Sadece isimle ara (PlayerMovement olmadan)
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

    void TrySpawnMoundAtPlayerPosition()
    {
        if (moundPrefab == null || player == null) return;

        float playerMoveDistance = Vector3.Distance(player.position, lastPlayerPosition);
        if (playerMoveDistance < playerMoveThreshold) return;

        Vector3 spawnPosition = GetPlayerGroundPosition();
        if (!IsValidSpawnPosition(spawnPosition)) return;
        if (IsPositionOccupied(spawnPosition)) return;

        SpawnMoundAtPosition(spawnPosition);
        lastPlayerPosition = player.position;
    }

    Vector3 GetPlayerGroundPosition()
    {
        Vector3 playerPos = player.position;

        RaycastHit hit;
        if (Physics.Raycast(playerPos + Vector3.up * 1f, Vector3.down, out hit, 3f, groundLayer))
        {
            return hit.point + Vector3.up * 0.1f;
        }

        return new Vector3(playerPos.x, playerPos.y - 0.5f, playerPos.z);
    }

    void SpawnMoundAtPosition(Vector3 position)
    {
        GameObject mound = Instantiate(moundPrefab, position, Quaternion.identity);
        DamageMound moundScript = mound.GetComponent<DamageMound>();
        if (moundScript == null)
        {
            moundScript = mound.AddComponent<DamageMound>();
        }
        moundScript.SetupMound(moundDamage, moundLifetime, moundDetectionRadius);
    }

    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.3f);
        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<DamageMound>() != null) return true;
        }
        return false;
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        if (!Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, 1.5f, groundLayer)) return false;
        if (Vector3.Distance(position, transform.position) > detectionRange * 1.5f) return false;
        return true;
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
        if (collision.gameObject.CompareTag("Player"))
        {
            if (showDebug) Debug.Log("Slime: Player'a çarptı!");
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