using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ESCMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escMenuPanel;
    public Transform rewardsContent;
    public GameObject rewardItemPrefab;
    public TextMeshProUGUI totalStatsText;
    public TextMeshProUGUI rewardsText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Escape;
    public bool isMenuOpen = false;

    void Start()
    {
        Debug.Log("🚀 ESCMenu Start çalıştı");
        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(false);
            Debug.Log("✅ ESCMenu panel başlangıçta gizlendi");
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel referansı boş!");
        }
    }

    void Update()
    {
        RewardUIManager rewardManager = FindObjectOfType<RewardUIManager>();
        if (rewardManager != null && rewardManager.IsRewardPanelOpen())
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("⌨️ ESC tuşuna basıldı");
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        Debug.Log("🔄 ToggleMenu çağrıldı, önceki durum: " + isMenuOpen);
        isMenuOpen = !isMenuOpen;

        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(isMenuOpen);

            if (isMenuOpen)
            {
                // TIMER'I DURDUR
                if (GameTimer.Instance != null)
                    GameTimer.Instance.PauseTimer();

                UpdateRewardsDisplay();
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Debug.Log("⏸️ Oyun duraklatıldı, fare aktif");
            }
            else
            {
                // TIMER'I DEVAM ETTIR
                if (GameTimer.Instance != null)
                    GameTimer.Instance.ResumeTimer();

                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("▶️ Oyun devam ediyor, fare gizlendi");
            }
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel referansı boş!");
        }
    }

    void UpdateRewardsDisplay()
    {
        Debug.Log("📊 UpdateRewardsDisplay çağrıldı");

        if (rewardsText != null)
        {
            rewardsText.text = "KAZANILAN GÜÇLENDİRMELER\n\n";

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
                    rewardsText.text += "• Henüz güçlendirme kazanılmadı\n";
                }
            }
            else
            {
                rewardsText.text += "• PlayerStats bulunamadı\n";
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
                totalStatsText.text = "İstatistikler yükleniyor...";
            }
        }

        Debug.Log("✅ Ödül görüntüsü güncellendi");
    }

    public void ResumeGame()
    {
        Debug.Log("🔘 DEVAM ET BUTONU ÇALIŞTI!");
        ToggleMenu();
    }

    public void MainMenu()
    {
        Debug.Log("🔘 ANA MENÜ BUTONU ÇALIŞTI!");

        // TIMER'I DURDUR
        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        Time.timeScale = 1f;
        Debug.Log("⏰ Zaman normale döndürüldü");

        Debug.Log("🏠 Ana menüye geçiliyor...");
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("🔘 ÇIKIŞ BUTONU ÇALIŞTI!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}