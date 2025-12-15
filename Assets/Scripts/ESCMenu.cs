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
        Debug.Log("🚀 ESCMenu Start executed");
        if (escMenuPanel != null)
        {
            escMenuPanel.SetActive(false);
            Debug.Log("✅ ESCMenu panel hidden at start");
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel reference is empty!");
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
                // PAUSE TIMER
                if (GameTimer.Instance != null)
                    GameTimer.Instance.PauseTimer();

                UpdateRewardsDisplay();
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Debug.Log("⏸️ Game paused, mouse active");
            }
            else
            {
                // RESUME TIMER
                if (GameTimer.Instance != null)
                    GameTimer.Instance.ResumeTimer();

                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("▶️ Game resumed, mouse hidden");
            }
        }
        else
        {
            Debug.LogError("❌ ESCMenu panel reference is empty!");
        }
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
        Debug.Log("🔘 CONTINUE BUTTON WORKED!");
        ToggleMenu();
    }

    public void MainMenu()
    {
        Debug.Log("🔘 MAIN MENU BUTTON WORKED!");

        // STOP TIMER
        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        Time.timeScale = 1f;
        Debug.Log("⏰ Time returned to normal");

        Debug.Log("🏠 Switching to main menu...");
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("🔘 QUIT BUTTON WORKED!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}