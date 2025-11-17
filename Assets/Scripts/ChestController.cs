using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public float chestOpenDelay = 0.5f;

    [Header("UI Reward System")]
    public bool useUIRewardSystem = true;

    [Header("Door Settings")]
    public float doorAnimationTime = 1.5f;

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private Animation chestAnimation;
    private bool canInteract = false;
    private bool isOpened = false;
    private bool rewardsGiven = false;
    private List<GameObject> dungeonDoors = new List<GameObject>();
    private int roomID;
    private RewardUIManager rewardUIManager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        chestAnimation = GetComponent<Animation>();

        if (useUIRewardSystem)
        {
            rewardUIManager = FindObjectOfType<RewardUIManager>();
            if (showDebug && rewardUIManager == null)
                Debug.LogWarning("RewardUIManager bulunamadı!");
        }

        FindRoomID();

        // Oda 6 hariç tüm odalar için kapıları bul
        if (roomID != 6)
        {
            FindDoorsByRoomID();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (showDebug && player == null)
            Debug.LogError("Chest: Player bulunamadı!");
    }

    void FindRoomID()
    {
        RoomManager[] allRooms = FindObjectsOfType<RoomManager>();
        float closestDistance = Mathf.Infinity;
        RoomManager closestRoom = null;

        foreach (RoomManager room in allRooms)
        {
            float distance = Vector3.Distance(transform.position, room.transform.position);
            if (distance < closestDistance && distance <= room.roomRadius)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }

        if (closestRoom != null)
        {
            string roomName = closestRoom.roomName;
            if (roomName.ToLower().Contains("oda"))
            {
                string numberPart = roomName.Replace("Oda", "").Replace("oda", "").Trim();
                if (int.TryParse(numberPart, out int id))
                {
                    roomID = id;
                    if (showDebug) Debug.Log($"Chest {roomID}. odada bulundu: {roomName}");
                }
            }
        }

        if (roomID == 0) roomID = 1;
    }

    void FindDoorsByRoomID()
    {
        dungeonDoors.Clear();

        string doorTag = "Door" + roomID;
        GameObject[] foundDoors = GameObject.FindGameObjectsWithTag(doorTag);

        foreach (GameObject door in foundDoors)
        {
            dungeonDoors.Add(door);
        }

        if (dungeonDoors.Count > 0)
        {
            if (showDebug) Debug.Log($"{dungeonDoors.Count} adet kapı bulundu! Room: {roomID}");
        }
        else
        {
            if (showDebug) Debug.LogWarning("Hiç kapı bulunamadı! RoomID: " + roomID);
        }
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

        StartCoroutine(DelayedChestOpen());
        if (showDebug) Debug.Log("Chest açılıyor... Room: " + roomID);
    }

    IEnumerator DelayedChestOpen()
    {
        yield return new WaitForSeconds(chestOpenDelay);

        if (!rewardsGiven)
        {
            if (useUIRewardSystem && rewardUIManager != null)
            {
                ShowRewardSelection();
            }
            else
            {
                Debug.LogError("Reward sistemi çalışmıyor!");
            }
            rewardsGiven = true;
        }

        // Oda 6 ise kapıları açma, diğer odalarda kapıları aç
        if (roomID != 6 && dungeonDoors.Count > 0)
        {
            StartCoroutine(AnimateDoorsOpen());
        }
        else if (roomID == 6)
        {
            if (showDebug) Debug.Log("🎮 Oda 6 chest'i - Kapılar açılmadı, puzzle için hazır!");
        }

        if (showDebug) Debug.Log("Chest tamamen açıldı! Room: " + roomID);
    }

    IEnumerator AnimateDoorsOpen()
    {
        foreach (GameObject door in dungeonDoors)
        {
            if (door != null)
            {
                StartCoroutine(AnimateSingleDoor(door));
            }
        }

        yield return new WaitForSeconds(doorAnimationTime);

        if (showDebug) Debug.Log($"Tüm kapılar açıldı! Toplam: {dungeonDoors.Count} adet");
    }

    IEnumerator AnimateSingleDoor(GameObject door)
    {
        Vector3 originalPosition = door.transform.position;
        Vector3 targetPosition = originalPosition + Vector3.down * 10f;

        float elapsedTime = 0f;

        while (elapsedTime < doorAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / doorAnimationTime;

            door.transform.position = Vector3.Lerp(originalPosition, targetPosition, progress);

            yield return null;
        }

        door.SetActive(false);
    }

    void ShowRewardSelection()
    {
        if (rewardUIManager != null)
        {
            rewardUIManager.ShowRewardSelection(OnRewardSelected);
        }
        else
        {
            Debug.LogError("RewardUIManager bulunamadı!");
        }
    }

    void OnRewardSelected(PlayerStats.RewardItem selectedReward)
    {
        ApplyReward(selectedReward);

        if (showDebug) Debug.Log($"Ödül uygulandı: {selectedReward.rewardName}");
    }

    void ApplyReward(PlayerStats.RewardItem reward)
    {
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            playerStats.ApplyReward(reward);
        }
        else
        {
            Debug.LogError("PlayerStats bulunamadı!");
        }

        if (reward.type == PlayerStats.RewardType.SpecialItem && reward.physicalPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(reward.physicalPrefab, spawnPos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = canInteract ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}