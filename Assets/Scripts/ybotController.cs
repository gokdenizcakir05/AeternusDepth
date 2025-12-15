using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ybotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float backwardSpeed = 1.5f;
    public float rotationSmooth = 10f;
    public float acceleration = 5f;
    public float jumpForce = 5f;

    [Header("Hover Mode Settings")]
    public float hoverHeight = 0.2f;
    public float hoverForce = 10f;
    public float hoverSpeedMultiplier = 1.5f;
    public float hoverBobSpeed = 3f;
    public float hoverBobAmount = 0.05f;
    public float hoverEnergyDrainRate = 5f;
    public float hoverEnergyRegenRate = 3f;
    public float maxHoverEnergy = 100f;

    [Header("Effects")]
    public GameObject hoverBubblePrefab; // ARTIK PREFAB!
    public Transform bubbleSpawnPoint;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = 1;
    public string[] groundTags = { "Ground", "NoSpawnGround", "Platform" };

    private Animator ybotAnim;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpRequested = false;

    // Hover Mode değişkenleri
    private bool isHovering = false;
    private bool hoverRequested = false;
    private float currentHoverEnergy;
    private Vector3 hoverStartPosition;
    private bool canHover = true;
    private float hoverBobTimer = 0f;
    private GameObject currentBubbleInstance; // Sahnedeki particle instance

    // === REWARD SİSTEM İÇİN EKLENEN KOD ===
    private float baseWalkSpeed;
    private float baseRunSpeed;
    private float baseBackwardSpeed;
    // === EKLEME BURADA BİTTİ ===

    private void Start()
    {
        ybotAnim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // === REWARD SİSTEM İÇİN EKLENEN KOD ===
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
        baseBackwardSpeed = backwardSpeed;
        // === EKLEME BURADA BİTTİ ===

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        gameObject.tag = "Player";

        // Hover enerjisini başlat
        currentHoverEnergy = maxHoverEnergy;

        InitializeSceneSettings();
    }

    private void InitializeSceneSettings()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (rb != null) rb.isKinematic = true;
            if (ybotAnim != null) ybotAnim.enabled = false;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (rb != null) rb.isKinematic = false;
            if (ybotAnim != null) ybotAnim.enabled = true;
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            UpdateMovementSpeeds();
            HandleMovementInput();

            // Hover için Shift'e basılı tut
            if (Keyboard.current.leftShiftKey.isPressed && canHover && currentHoverEnergy > 10f && isGrounded && !isHovering)
            {
                hoverRequested = true;
            }

            if (Keyboard.current.leftShiftKey.wasReleasedThisFrame && isHovering)
            {
                StopHover();
            }

            // Hover enerjisi yenileme
            if (!isHovering && currentHoverEnergy < maxHoverEnergy)
            {
                currentHoverEnergy += hoverEnergyRegenRate * Time.deltaTime;
                currentHoverEnergy = Mathf.Clamp(currentHoverEnergy, 0, maxHoverEnergy);
            }

            // Hover bob timer
            if (isHovering)
            {
                hoverBobTimer += Time.deltaTime * hoverBobSpeed;

                // Particle instance'ı spawn point'te tut
                if (currentBubbleInstance != null && bubbleSpawnPoint != null)
                {
                    currentBubbleInstance.transform.position = bubbleSpawnPoint.position;
                    currentBubbleInstance.transform.rotation = bubbleSpawnPoint.rotation;
                }
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping && !isHovering)
            {
                jumpRequested = true;
            }

            if (ybotAnim != null)
            {
                ybotAnim.SetBool("isGrounded", isGrounded);
                ybotAnim.SetFloat("hiz", isHovering ?
                    Mathf.InverseLerp(0, walkSpeed, currentSpeed) :
                    Mathf.InverseLerp(0, runSpeed, currentSpeed));
                ybotAnim.SetBool("isJumping", isJumping);
                ybotAnim.SetBool("isHovering", isHovering);
            }
        }
    }

    private void UpdateMovementSpeeds()
    {
        if (PlayerStats.Instance != null)
        {
            float speedMultiplier = PlayerStats.Instance.GetMovementSpeedMultiplier();
            walkSpeed = baseWalkSpeed * speedMultiplier;
            runSpeed = baseRunSpeed * speedMultiplier;
            backwardSpeed = baseBackwardSpeed * speedMultiplier;
        }
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            CheckGrounded();

            if (hoverRequested && isGrounded && canHover && currentHoverEnergy > 10f)
            {
                StartHover();
                hoverRequested = false;
            }

            if (isHovering)
            {
                HandleHover();

                currentHoverEnergy -= hoverEnergyDrainRate * Time.deltaTime;
                if (currentHoverEnergy <= 0)
                {
                    currentHoverEnergy = 0;
                    StopHover();
                }
            }

            if (jumpRequested && isGrounded && !isJumping && !isHovering)
            {
                Jump();
                jumpRequested = false;
            }

            if (!isHovering)
            {
                HandleMovement();
            }
        }
    }

    private void StartHover()
    {
        isHovering = true;
        isJumping = false;
        hoverStartPosition = transform.position + Vector3.up * hoverHeight;

        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        transform.position = hoverStartPosition;

        // PREFAB'DAN PARTICLE INSTANCE OLUŞTUR
        if (hoverBubblePrefab != null && bubbleSpawnPoint != null)
        {
            // Eski instance varsa temizle
            if (currentBubbleInstance != null)
            {
                Destroy(currentBubbleInstance);
            }

            // Yeni instance oluştur
            currentBubbleInstance = Instantiate(
                hoverBubblePrefab,
                bubbleSpawnPoint.position,
                bubbleSpawnPoint.rotation
            );

            // Particle System component'ini bul ve başlat
            ParticleSystem particleSystem = currentBubbleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            Debug.Log($"Hover başladı! Particle oluşturuldu: {currentBubbleInstance.name}");
        }
        else
        {
            Debug.LogWarning($"HoverBubblePrefab veya SpawnPoint NULL! Prefab: {hoverBubblePrefab != null}, SpawnPoint: {bubbleSpawnPoint != null}");
        }
    }

    private void HandleHover()
    {
        float bobOffset = Mathf.Sin(hoverBobTimer) * hoverBobAmount;
        Vector3 targetPosition = new Vector3(
            transform.position.x,
            hoverStartPosition.y + bobOffset,
            transform.position.z
        );

        Vector3 positionDifference = targetPosition - transform.position;
        rb.AddForce(positionDifference * hoverForce, ForceMode.Acceleration);

        HandleMovement();

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > walkSpeed * hoverSpeedMultiplier)
        {
            horizontalVelocity = horizontalVelocity.normalized * walkSpeed * hoverSpeedMultiplier;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void StopHover()
    {
        isHovering = false;
        rb.useGravity = true;

        // Particle instance'ı temizle
        if (currentBubbleInstance != null)
        {
            // Particle'ı yavaşça durdur
            ParticleSystem particleSystem = currentBubbleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // 2 saniye sonra yok et (particle'ın bitmesini bekle)
            Destroy(currentBubbleInstance, 2f);
            currentBubbleInstance = null;
        }
    }

    private void HandleMovementInput()
    {
        bool forward = Keyboard.current.wKey.isPressed;
        bool backward = Keyboard.current.sKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        Vector3 direction = Vector3.zero;
        if (forward) direction += Vector3.forward;
        if (backward) direction += Vector3.back;
        if (left) direction += Vector3.left;
        if (right) direction += Vector3.right;
        direction.Normalize();

        if (direction.magnitude > 0)
        {
            targetSpeed = isHovering ?
                walkSpeed * hoverSpeedMultiplier :
                (backward ? backwardSpeed : walkSpeed);
        }
        else
        {
            targetSpeed = 0f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
    }

    private void HandleMovement()
    {
        bool forward = Keyboard.current.wKey.isPressed;
        bool backward = Keyboard.current.sKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        Vector3 direction = Vector3.zero;
        if (forward) direction += Vector3.forward;
        if (backward) direction += Vector3.back;
        if (left) direction += Vector3.left;
        if (right) direction += Vector3.right;
        direction.Normalize();

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);
        }

        Vector3 moveDirection = transform.forward * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        if (!isHovering)
        {
            isGrounded = false;

            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
            {
                if (IsValidGroundTag(hit.collider.tag))
                {
                    isGrounded = true;
                }
            }

            if (!isGrounded)
            {
                if (Physics.SphereCast(rayStart, 0.2f, Vector3.down, out hit, groundCheckDistance, groundLayer))
                {
                    if (IsValidGroundTag(hit.collider.tag))
                    {
                        isGrounded = true;
                    }
                }
            }

            if (!isGrounded)
            {
                CheckGroundedByCollision();
            }

            if (isGrounded && !wasGrounded)
            {
                isJumping = false;
            }
        }
        else
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private bool IsValidGroundTag(string tag)
    {
        foreach (string validTag in groundTags)
        {
            if (tag == validTag)
                return true;
        }
        return false;
    }

    private void CheckGroundedByCollision()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + Vector3.down * (groundCheckDistance * 0.5f),
            new Vector3(0.3f, groundCheckDistance * 0.5f, 0.3f),
            Quaternion.identity,
            groundLayer
        );

        foreach (Collider col in colliders)
        {
            if (IsValidGroundTag(col.tag))
            {
                RaycastHit normalHit;
                if (Physics.Raycast(transform.position, Vector3.down, out normalHit, groundCheckDistance * 2f, groundLayer))
                {
                    if (Vector3.Dot(normalHit.normal, Vector3.up) > 0.7f)
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }
        }
    }

    private void Jump()
    {
        isJumping = true;
        isGrounded = false;
        if (ybotAnim != null)
            ybotAnim.SetBool("isJumping", true);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public float GetHoverEnergyPercentage()
    {
        return currentHoverEnergy / maxHoverEnergy;
    }

    public bool IsHovering()
    {
        return isHovering;
    }
}