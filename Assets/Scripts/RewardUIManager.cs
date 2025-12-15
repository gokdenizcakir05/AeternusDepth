using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rewardPanel;
    public Image leftIcon;
    public Image rightIcon;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI leftDescription;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI rightDescription;
    public Button leftButton;
    public Button rightButton;

    [Header("Reward Settings")]
    public List<PlayerStats.RewardItem> rewardPool = new List<PlayerStats.RewardItem>();

    private PlayerStats.RewardItem leftReward;
    private PlayerStats.RewardItem rightReward;
    private System.Action<PlayerStats.RewardItem> onRewardSelected;
    private bool isRewardPanelOpen = false;

    // Karakter kontrol referansı
    private ybotController playerController;
    private PlayerInput playerInput;

    // ESC menu control
    private ESCMenu escMenu;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;
    private float previousTimeScale;

    void Start()
    {
        Debug.Log("=== REWARD UI MANAGER START ===");

        // Referans kontrolleri
        if (rewardPanel == null) Debug.LogError("RewardPanel reference is not connected!");
        if (leftIcon == null) Debug.LogError("LeftIcon reference is not connected!");
        if (rightIcon == null) Debug.LogError("RightIcon reference is not connected!");
        if (leftText == null) Debug.LogError("LeftText reference is not connected!");
        if (rightText == null) Debug.LogError("RightText reference is not connected!");
        if (leftButton == null) Debug.LogError("LeftButton reference is not connected!");
        if (rightButton == null) Debug.LogError("RightButton reference is not connected!");

        // Buton referanslarını kontrol et
        Debug.Log($"LeftButton reference: {leftButton != null}");
        Debug.Log($"RightButton reference: {rightButton != null}");

        // Karakter kontrollerini bul
        playerController = FindObjectOfType<ybotController>();
        playerInput = FindObjectOfType<PlayerInput>();

        escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu == null) Debug.LogWarning("ESCMenu not found!");

        rewardPanel.SetActive(false);

        // TEST: Buton click listener'larını ekle
        leftButton.onClick.AddListener(() => {
            Debug.Log("=== 🖱️ LEFT BUTTON CLICKED (MOUSE) ===");
            Debug.Log($"LeftReward: {leftReward}, Name: {(leftReward != null ? leftReward.rewardName : "NULL")}");
            SelectReward(leftReward);
        });

        rightButton.onClick.AddListener(() => {
            Debug.Log("=== 🖱️ RIGHT BUTTON CLICKED (MOUSE) ===");
            Debug.Log($"RightReward: {rightReward}, Name: {(rightReward != null ? rightReward.rewardName : "NULL")}");
            SelectReward(rightReward);
        });

        Debug.Log("🎮 TEST: Press 1 for left reward, 2 for right reward (Keyboard backup)");
    }

    void Update()
    {
        // TEST: Klavyeden kontrol (backup sistem)
        if (isRewardPanelOpen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                Debug.Log("=== ⌨️ KEYBOARD 1 PRESSED - Simulating left button click ===");
                if (leftReward != null)
                {
                    SelectReward(leftReward);
                }
                else
                {
                    Debug.LogError("Left reward is null!");
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                Debug.Log("=== ⌨️ KEYBOARD 2 PRESSED - Simulating right button click ===");
                if (rightReward != null)
                {
                    SelectReward(rightReward);
                }
                else
                {
                    Debug.LogError("Right reward is null!");
                }
            }
        }

        if (isRewardPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC: Reward panel is open, ESC menu blocked");
            return;
        }
    }

    public void ShowRewardSelection(System.Action<PlayerStats.RewardItem> callback)
    {
        Debug.Log("=== 🎬 ShowRewardSelection CALLED! ===");

        // Debug için hierarchy bilgisi
        DebugHierarchy();

        if (rewardPool.Count == 0)
        {
            Debug.LogError("RewardPool is empty! Add at least 1 reward.");
            return;
        }

        // 1. Canvas ayarları
        Canvas canvas = rewardPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10000;
            Debug.Log($"✅ Canvas found: {canvas.name}, sortingOrder: {canvas.sortingOrder}");
        }
        else
        {
            Debug.LogError("❌ No Canvas found for reward panel!");
        }

        // 2. Panel'i en üste getir
        rewardPanel.transform.SetAsLastSibling();

        // 3. Event System kontrolü - KRİTİK!
        if (EventSystem.current == null)
        {
            Debug.LogError("❌ NO EVENT SYSTEM IN SCENE! Creating emergency one...");
            GameObject es = new GameObject("EmergencyEventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        else
        {
            Debug.Log($"✅ EventSystem found: {EventSystem.current.gameObject.name}");
            // Odağı butona ver
            EventSystem.current.SetSelectedGameObject(leftButton.gameObject);
        }

        // 4. Butonları AKTİF ET ve RESETLE
        leftButton.interactable = true;
        rightButton.interactable = true;
        leftButton.enabled = true;
        rightButton.enabled = true;

        Debug.Log($"✅ Buttons activated - L: {leftButton.interactable}, R: {rightButton.interactable}");

        // 5. Buton renklerini normal yap
        ColorBlock colors = leftButton.colors;
        colors.normalColor = Color.white;
        leftButton.colors = colors;
        rightButton.colors = colors;

        // 6. PAUSE TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();

        // 7. Karakter kontrollerini durdur
        DisablePlayerControls();

        if (escMenu != null && escMenu.isMenuOpen)
        {
            escMenu.ToggleMenu();
        }

        // 8. Rastgele reward'ları seç
        GetTwoRandomRewards();
        onRewardSelected = callback;
        UpdateRewardUI();

        // 9. Mouse ayarlarını kaydet ve değiştir
        wasCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // 10. Panel'i AÇ
        rewardPanel.SetActive(true);
        isRewardPanelOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log($"✅ ✅ REWARD PANEL OPENED SUCCESSFULLY!");
        Debug.Log($"   Panel Active: {rewardPanel.activeSelf}");
        Debug.Log($"   Left Reward: {(leftReward != null ? leftReward.rewardName : "NULL")}");
        Debug.Log($"   Right Reward: {(rightReward != null ? rightReward.rewardName : "NULL")}");
        Debug.Log($"   🖱️ Click buttons with mouse");
        Debug.Log($"   ⌨️ OR Press 1 for left, 2 for right (keyboard backup)");
    }

    void DisablePlayerControls()
    {
        // ybotController scriptini devre dışı bırak
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("✅ Player controller disabled");
        }
        else
        {
            Debug.LogWarning("⚠️ Player controller not found!");
        }

        // PlayerInput'u devre dışı bırak
        if (playerInput != null)
        {
            playerInput.enabled = false;
            Debug.Log("✅ Player input disabled");
        }

        // Rigidbody'yi dondur
        Rigidbody rb = playerController?.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            Debug.Log("✅ Rigidbody frozen");
        }
    }

    void EnablePlayerControls()
    {
        // ybotController scriptini etkinleştir
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("✅ Player controller enabled");
        }

        // PlayerInput'u etkinleştir
        if (playerInput != null)
        {
            playerInput.enabled = true;
            Debug.Log("✅ Player input enabled");
        }

        // Rigidbody'yi çöz
        Rigidbody rb = playerController?.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Debug.Log("✅ Rigidbody unfrozen");
        }
    }

    void GetTwoRandomRewards()
    {
        if (rewardPool.Count < 2)
        {
            Debug.LogWarning("⚠️ Only 1 reward available, same reward will be shown twice.");
            leftReward = rewardPool[0];
            rightReward = rewardPool[0];
            return;
        }

        int randomIndex1 = Random.Range(0, rewardPool.Count);
        leftReward = rewardPool[randomIndex1];

        int randomIndex2;
        do
        {
            randomIndex2 = Random.Range(0, rewardPool.Count);
        } while (randomIndex2 == randomIndex1 && rewardPool.Count > 1);

        rightReward = rewardPool[randomIndex2];

        Debug.Log($"🎲 Random rewards selected: {leftReward.rewardName} vs {rightReward.rewardName}");
    }

    void UpdateRewardUI()
    {
        if (leftReward == null || rightReward == null)
        {
            Debug.LogError("❌ Rewards are null! Left: " + leftReward + " Right: " + rightReward);
            return;
        }

        // Left reward UI
        if (leftIcon != null)
        {
            leftIcon.sprite = leftReward.icon;
            leftIcon.enabled = (leftReward.icon != null);
        }
        if (leftText != null) leftText.text = leftReward.rewardName;
        if (leftDescription != null) leftDescription.text = GetRewardDescription(leftReward);

        // Right reward UI
        if (rightIcon != null)
        {
            rightIcon.sprite = rightReward.icon;
            rightIcon.enabled = (rightReward.icon != null);
        }
        if (rightText != null) rightText.text = rightReward.rewardName;
        if (rightDescription != null) rightDescription.text = GetRewardDescription(rightReward);

        Debug.Log($"🔄 UI updated: {leftReward.rewardName} - {rightReward.rewardName}");
    }

    string GetRewardDescription(PlayerStats.RewardItem reward)
    {
        if (reward == null) return "NULL REWARD";

        switch (reward.type)
        {
            case PlayerStats.RewardType.Health:
                return $"+{reward.value} Maximum Oxygen";
            case PlayerStats.RewardType.Mana:
                return $"+{reward.value} Maximum Mana";
            case PlayerStats.RewardType.Gold:
                return $"+{reward.value} Gold";
            case PlayerStats.RewardType.Experience:
                return $"+{reward.value} EXP";
            case PlayerStats.RewardType.MovementSpeed:
                return $"+%{reward.floatValue} Movement Speed";
            case PlayerStats.RewardType.AttackSpeed:
                return $"+%{reward.floatValue} Attack Speed";
            case PlayerStats.RewardType.BulletSpeed:
                return $"+%{reward.floatValue} Bullet Speed";
            case PlayerStats.RewardType.Damage:
                return $"+%{reward.floatValue} Damage";
            case PlayerStats.RewardType.SpecialItem:
                return reward.description;
            default:
                return reward.description;
        }
    }

    void SelectReward(PlayerStats.RewardItem selectedReward)
    {
        if (selectedReward == null)
        {
            Debug.LogError("❌ Selected reward is null!");
            return;
        }

        Debug.Log($"🎁 SELECTING REWARD: {selectedReward.rewardName}");

        onRewardSelected?.Invoke(selectedReward);
        CloseRewardPanel();
    }

    void CloseRewardPanel()
    {
        Debug.Log("🔒 Closing reward panel...");

        rewardPanel.SetActive(false);
        isRewardPanelOpen = false;

        // Karakter kontrollerini geri yükle
        EnablePlayerControls();

        // RESUME TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.ResumeTimer();

        Time.timeScale = previousTimeScale;

        Cursor.visible = wasCursorVisible;
        Cursor.lockState = previousCursorLockState;

        Debug.Log("✅ Reward panel closed - Game resumed");
    }

    // DEBUG METODU: Hierarchy'yi kontrol et
    void DebugHierarchy()
    {
        Debug.Log("=== 🔍 UI HIERARCHY DEBUG ===");

        // Reward panel'in parent'larını kontrol et
        Transform current = rewardPanel.transform;
        string hierarchy = "";
        while (current != null)
        {
            hierarchy = current.name + " > " + hierarchy;
            Debug.Log($"   Parent: {current.name}, Active: {current.gameObject.activeSelf}");
            current = current.parent;
        }
        Debug.Log($"   Full Path: {hierarchy}");

        // EventSystem kontrolü
        if (EventSystem.current == null)
        {
            Debug.LogError("   ❌ NO EVENT SYSTEM IN SCENE!");
        }
        else
        {
            Debug.Log($"   ✅ EventSystem: {EventSystem.current.gameObject.name}");
        }

        // GraphicRaycaster kontrolü
        GraphicRaycaster raycaster = rewardPanel.GetComponentInParent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("   ❌ NO GraphicRaycaster in parent!");
        }
        else
        {
            Debug.Log($"   ✅ GraphicRaycaster found");
        }

        // Buton state'leri
        Debug.Log($"   Left Button - active: {leftButton.gameObject.activeSelf}, interactable: {leftButton.interactable}");
        Debug.Log($"   Right Button - active: {rightButton.gameObject.activeSelf}, interactable: {rightButton.interactable}");

        // Raycast Target kontrolleri
        Image[] images = rewardPanel.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.raycastTarget)
            {
                Debug.Log($"   ⚠️ RaycastTarget ON: {img.gameObject.name} (might block clicks)");
            }
        }
    }

    public void HideRewardPanel()
    {
        CloseRewardPanel();
    }

    public bool IsRewardPanelOpen()
    {
        return isRewardPanelOpen;
    }

    // TEST: Manuel olarak panel açmak için (Inspector'dan buton bağla)
    public void TestOpenRewardPanel()
    {
        Debug.Log("=== 🧪 TEST: Manual reward panel open ===");
        ShowRewardSelection((reward) => {
            Debug.Log($"🧪 TEST: Reward selected: {reward.rewardName}");
        });
    }
}