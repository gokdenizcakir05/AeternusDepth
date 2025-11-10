using UnityEngine;
using UnityEngine.InputSystem;

public class ybotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float backwardSpeed = 1.5f;
    public float rotationSmooth = 10f;
    public float acceleration = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = 1;
    public string[] groundTags = { "Ground", "NoSpawnGround", "Platform" }; // Tüm ground tag'leri

    [Header("Debug")]
    public bool showGroundDebug = false;

    private Animator ybotAnim;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpRequested = false;

    private void Start()
    {
        ybotAnim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Rigidbody ayarları
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Karakteri "Player" tag'i ile işaretle
        gameObject.tag = "Player";

        if (showGroundDebug)
            Debug.Log("ybotController: Ground detection aktif!");
    }

    private void Update()
    {
        HandleMovement();

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping)
        {
            jumpRequested = true;
        }

        ybotAnim.SetBool("isGrounded", isGrounded);
        ybotAnim.SetFloat("hiz", Mathf.InverseLerp(0, runSpeed, currentSpeed));
        ybotAnim.SetBool("isJumping", isJumping);
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (jumpRequested && isGrounded && !isJumping)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // 1. Raycast ile ground kontrolü (mevcut sistem)
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // Tag kontrolü - tüm ground tag'lerini kabul et
            if (IsValidGroundTag(hit.collider.tag))
            {
                isGrounded = true;
            }
        }

        // 2. SphereCast ile daha geniş alan kontrolü (backup)
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

        // 3. Collision-based ground kontrolü (ekstra güvenlik)
        if (!isGrounded)
        {
            CheckGroundedByCollision();
        }

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            if (showGroundDebug) Debug.Log("ybot: Grounded!");
        }
        else if (!isGrounded && wasGrounded)
        {
            if (showGroundDebug) Debug.Log("ybot: In Air!");
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
        // Collision tabanlı ground kontrolü (physics-based)
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
                // Normal kontrolü - yukarı doğru yüzey mi?
                RaycastHit normalHit;
                if (Physics.Raycast(transform.position, Vector3.down, out normalHit, groundCheckDistance * 2f, groundLayer))
                {
                    if (Vector3.Dot(normalHit.normal, Vector3.up) > 0.7f) // 45 dereceden düz yüzey
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }
        }
    }

    private void HandleMovement()
    {
        bool forward = Keyboard.current.wKey.isPressed;
        bool backward = Keyboard.current.sKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;
        bool run = Keyboard.current.leftShiftKey.isPressed;

        Vector3 direction = Vector3.zero;
        if (forward) direction += Vector3.forward;
        if (backward) direction += Vector3.back;
        if (left) direction += Vector3.left;
        if (right) direction += Vector3.right;
        direction.Normalize();

        if (direction.magnitude > 0)
        {
            if (run)
                targetSpeed = runSpeed;
            else if (backward)
                targetSpeed = backwardSpeed;
            else
                targetSpeed = walkSpeed;
        }
        else
            targetSpeed = 0f;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);
        }

        Vector3 moveDirection = transform.forward * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    private void Jump()
    {
        isJumping = true;
        isGrounded = false;
        ybotAnim.SetBool("isJumping", true);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (showGroundDebug) Debug.Log("ybot: Jump!");
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGroundDebug) return;

        // Ground check görselleştirme
        Gizmos.color = isGrounded ? Color.green : Color.red;

        // Raycast çizgisi
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayStart, Vector3.down * groundCheckDistance);

        // SphereCast alanı
        Gizmos.color = isGrounded ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(rayStart + Vector3.down * groundCheckDistance, 0.2f);

        // OverlapBox alanı
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(
            transform.position + Vector3.down * (groundCheckDistance * 0.5f),
            new Vector3(0.6f, groundCheckDistance, 0.6f)
        );
    }
}