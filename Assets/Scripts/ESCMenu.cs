using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class ESCMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escMenuPanel;
    public Transform rewardsContent;
    public GameObject rewardItemPrefab;
    public TextMeshProUGUI totalStatsText;
    public TextMeshProUGUI rewardsText;

    [Header("Hover UI Reference")]
    public GameObject hoverEnergyUI; // Inspector'dan hover UI'ını sürükle

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool isMenuOpen = false;

    // Canvas reference
    private Canvas escCanvas;
    private GraphicRaycaster raycaster;

    // Hover UI kaydı
    private bool wasHoverUIActive = true;

    void Start()
    {
        Debug.Log("🚀 ESCMenu Start executed");

        // Canvas ve raycaster'ı bul
        escCanvas = escMenuPanel.GetComponentInParent<Canvas>();
        if (escCanvas != null)
        {
            raycaster = escCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = escCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ GraphicRaycaster eklendi");
            }

            // Canvas'ı TimeScale'den bağımsız yap
            escCanvas.pixelPerfect = false;
        }

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(false);
            Debug.Log("✅ ESCMenu panel hidden at start");
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel reference is empty!");
        }

        // EventSystem kontrolü
        EnsureEventSystemExists();

        // Raycast Target'ları düzelt
        FixESCMenuRaycastTargets();
    }

    void EnsureEventSystemExists()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("❌ NO EVENT SYSTEM! Creating one...");
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void FixESCMenuRaycastTargets()
    {
        if (escMenuPanel == null) return;

        Debug.Log("🔧 Fixing ESC Menu Raycast Targets...");

        // Tüm butonları bul
        Button[] buttons = escMenuPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            // Buton'u etkinleştir
            button.interactable = true;

            // Buton Image'ının Raycast Target'ını aç
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
                Debug.Log($"✅ Button Raycast Target açık: {button.gameObject.name}");
            }

            // Buton'un içindeki Text'lerin Raycast Target'ını kapat
            TMPro.TextMeshProUGUI[] texts = button.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (TMPro.TextMeshProUGUI text in texts)
            {
                text.raycastTarget = false;
            }
        }

        // Diğer Image'ların Raycast Target'ını kapat
        Image[] allImages = escMenuPanel.GetComponentsInChildren<Image>(true);
        foreach (Image image in allImages)
        {
            if (image.GetComponent<Button>() == null)
            {
                image.raycastTarget = false;
            }
        }

        Debug.Log("✅ ESC Menu Raycast Target'lar düzeltildi!");
    }

    void Update()
    {
        // Reward panel açıksa ESC'yi engelle
        RewardUIManager rewardManager = FindObjectOfType<RewardUIManager>();
        if (rewardManager != null && rewardManager.IsRewardPanelOpen())
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("⌨️ ESC key pressed");
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        Debug.Log("🔄 ToggleMenu called, previous state: " + isMenuOpen);
        isMenuOpen = !isMenuOpen;

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(isMenuOpen);

            if (isMenuOpen)
            {
                OpenESCMenu();
            }
            else
            {
                CloseESCMenu();
            }
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel reference is empty!");
        }
    }

    void OpenESCMenu()
    {
        Debug.Log("🟢 OPENING ESC MENU");

        // Hover UI'ını kaydet ve gizle
        if (hoverEnergyUI != null)
        {
            wasHoverUIActive = hoverEnergyUI.activeSelf;
            hoverEnergyUI.SetActive(false);
            Debug.Log("✅ Hover Energy UI hidden");
        }

        // PAUSE TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();

        // TimeScale'i değiştir (OYUN DURSUN)
        Time.timeScale = 0f;

        // Input modülünü kontrol et
        if (EventSystem.current != null)
        {
            StandaloneInputModule inputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            if (inputModule != null)
            {
                // Input modülünü yeniden başlat
                inputModule.enabled = false;
                inputModule.enabled = true;
            }

            // Focus'u sıfırla
            EventSystem.current.SetSelectedGameObject(null);

            // İlk butonu seç
            Button firstButton = escMenuPanel.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
                Debug.Log($"✅ Focus set to: {firstButton.gameObject.name}");
            }
        }

        UpdateRewardsDisplay();

        // Mouse'u göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Butonları etkinleştir (1 frame sonra)
        StartCoroutine(EnableButtonsAfterFrame());

        Debug.Log("⏸️ Game paused, ESC Menu open");
    }

    IEnumerator EnableButtonsAfterFrame()
    {
        yield return null; // Bir frame bekle

        // Tüm butonları etkinleştir
        Button[] buttons = escMenuPanel.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }

        Debug.Log($"✅ All buttons enabled ({buttons.Length} buttons)");
    }

    void CloseESCMenu()
    {
        Debug.Log("🔴 CLOSING ESC MENU");

        // Hover UI'ını geri göster
        if (hoverEnergyUI != null && wasHoverUIActive)
        {
            hoverEnergyUI.SetActive(true);
            Debug.Log("✅ Hover Energy UI restored");
        }

        // RESUME TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.ResumeTimer();

        // TimeScale'i normale çevir
        Time.timeScale = 1f;

        // Mouse'u gizle
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("▶️ Game resumed, ESC Menu closed");
    }

    void UpdateRewardsDisplay()
    {
        Debug.Log("📊 UpdateRewardsDisplay called");

        if (rewardsText != null)
        {
            rewardsText.text = "ACQUIRED UPGRADES\n\n";

            if (PlayerStats.Instance != null)
            {
                List<string> rewards = PlayerStats.Instance.GetAllAcquiredRewards();

                if (rewards.Count > 0)
                {
                    foreach (string reward in rewards)
                    {
                        rewardsText.text += $"• {reward}\n\n";
                    }
                }
                else
                {
                    rewardsText.text += "• No upgrades acquired yet\n";
                }
            }
            else
            {
                rewardsText.text += "• PlayerStats not found\n";
            }
        }

        if (totalStatsText != null)
        {
            if (PlayerStats.Instance != null)
            {
                totalStatsText.text = PlayerStats.Instance.GetTotalStatsSummary();
            }
            else
            {
                totalStatsText.text = "Loading statistics...";
            }
        }

        Debug.Log("✅ Rewards display updated");
    }

    public void ResumeGame()
    {
        Debug.Log("🔘 CONTINUE BUTTON CLICKED!");
        ToggleMenu();
    }

    public void MainMenu()
    {
        Debug.Log("🔘 MAIN MENU BUTTON CLICKED!");

        // STOP TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        // TimeScale'i normale çevir
        Time.timeScale = 1f;

        // Hover UI'ı geri göster (scene değişeceği için)
        if (hoverEnergyUI != null)
        {
            hoverEnergyUI.SetActive(true);
        }

        Debug.Log("⏰ Time returned to normal");

        Debug.Log("🏠 Switching to main menu...");
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("🔘 QUIT BUTTON CLICKED!");

        // TimeScale'i normale çevir
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // DEBUG: Inspector'dan butonları test et
    public void TestAllButtons()
    {
        Debug.Log("🧪 TESTING ALL BUTTONS...");

        Button[] buttons = escMenuPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            Debug.Log($"Button: {button.gameObject.name}");
            Debug.Log($"  - Interactable: {button.interactable}");
            Debug.Log($"  - Enabled: {button.enabled}");
            Debug.Log($"  - Active: {button.gameObject.activeInHierarchy}");

            // Image kontrolü
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Debug.Log($"  - Image Raycast Target: {image.raycastTarget}");
            }
        }
    }
}