using UnityEngine;

public class SeamanController : MonoBehaviour
{
    [Header("Seaman Settings")]
    public float rotationSpeed = 2f;
    public float detectionRange = 3f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Debug")]
    public bool showDebug = true;

    private SeamanDialogue dialogue;
    private bool isFacingPlayer = false;

    void Start()
    {
        dialogue = GetComponent<SeamanDialogue>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player != null && !dialogue.isDialogueActive)
        {
            FacePlayer();
            CheckPlayerDistance();
        }

        UpdateAnimation();
    }

    void FacePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isFacingPlayer = distance <= detectionRange;

        if (showDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎯 Player mesafesi: {distance:F1}m");
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsTalking", dialogue.isDialogueActive);
            animator.SetBool("PlayerNear", isFacingPlayer);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Detection range
        Gizmos.color = isFacingPlayer ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Bakış yönü
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}