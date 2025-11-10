using UnityEngine;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Reward Settings")]
    public GameObject[] rewardPrefabs;
    [Range(1, 5)] public int minRewards = 1;
    [Range(1, 5)] public int maxRewards = 3;
    public float rewardSpawnRadius = 1.5f;

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private Animation chestAnimation;
    private bool canInteract = false;
    private bool isOpened = false;
    private bool rewardsGiven = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chestAnimation = GetComponent<Animation>();

        // RIGIDBODY AYARLARI - KESİN ÇÖZÜM
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Fizik etkisiz
        }

        if (showDebug && player == null)
            Debug.LogError("Chest: Player bulunamadı!");
    }

    void Update()
    {
        if (player == null || isOpened) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        canInteract = distanceToPlayer <= interactionRange;

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;

        if (chestAnimation != null)
        {
            foreach (AnimationState state in chestAnimation)
            {
                if (state.name.ToLower().Contains("open"))
                {
                    chestAnimation.Play(state.name);
                    break;
                }
            }
        }

        if (!rewardsGiven)
        {
            GiveRewards();
            rewardsGiven = true;
        }

        if (showDebug) Debug.Log("Chest açıldı!");
    }

    void GiveRewards()
    {
        if (rewardPrefabs == null || rewardPrefabs.Length == 0)
        {
            Debug.LogWarning("Chest: Ödül prefab'ı atanmamış!");
            return;
        }

        int rewardCount = Random.Range(minRewards, maxRewards + 1);

        for (int i = 0; i < rewardCount; i++)
        {
            GameObject rewardPrefab = rewardPrefabs[Random.Range(0, rewardPrefabs.Length)];

            if (rewardPrefab != null)
            {
                Vector3 randomOffset = Random.insideUnitSphere * rewardSpawnRadius;
                randomOffset.y = 0;
                Vector3 spawnPosition = transform.position + randomOffset;

                Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
            }
        }

        if (showDebug) Debug.Log($"Chest ödülü verildi: {rewardCount} adet");
    }

    // GIZMOS'LARI BASİTLEŞTİR
    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = canInteract ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}