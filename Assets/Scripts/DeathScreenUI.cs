using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathReasonText;
    public TextMeshProUGUI statsSummaryText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Death Messages")]
    public string defaultDeathMessage = "OKSİJEN TÜKENDİ";
    public string enemyDeathMessage = "DÜŞMAN TARAFINDAN YOK EDİLDİN";

    private PlayerHealth playerHealth;

    void Start()
    {
        // Başlangıçta death paneli gizle
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // PlayerHealth'i bul
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Ölüm event'ine bağla
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }

        // Buton event'lerini MANUEL bağla
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        Debug.Log("💀 DeathScreenUI başlatıldı - Buton event'leri bağlandı");
    }

    void OnPlayerDeath()
    {
        ShowDeathScreen();
    }

    public void ShowDeathScreen(string deathReason = "")
    {
        Debug.Log("💀 Death screen gösteriliyor");

        // Zamanı durdur
        Time.timeScale = 0f;

        // Fareyi göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Death paneli göster
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // Ölüm sebebini ayarla
        if (deathReasonText != null)
        {
            string message = GetDeathMessage(deathReason);
            deathReasonText.text = message;
        }

        // İstatistikleri göster
        if (statsSummaryText != null)
        {
            statsSummaryText.text = GetStatsSummary();
        }

        // ESC menüyü kapat (açıksa)
        ESCMenu escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu != null && escMenu.isMenuOpen)
        {
            escMenu.ToggleMenu();
        }
    }

    string GetDeathMessage(string reason)
    {
        switch (reason.ToLower())
        {
            case "enemy":
            case "düşman":
                return enemyDeathMessage;
            default:
                return defaultDeathMessage;
        }
    }

    string GetStatsSummary()
    {
        string summary = "SON İSTATİSTİKLERİN\n\n";

        if (PlayerStats.Instance != null)
        {
            // "TOPLAM İSTATİSTİKLER" başlığını kaldır, sadece istatistikleri al
            string stats = PlayerStats.Instance.GetTotalStatsSummary();
            // "TOPLAM İSTATİSTİKLER" yazısını sil
            stats = stats.Replace("TOPLAM İSTATİSTİKLER\n\n", "");
            summary += stats;
        }
        else
        {
            summary += "İstatistikler yüklenemedi";
        }

        // Player health bilgisi
        if (playerHealth != null)
        {
            summary += $"\n\n💧 Son Oksijen: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
        }

        return summary;
    }

    public void RestartGame()
    {
        Debug.Log("🔁 Oyun yeniden başlatılıyor...");

        // Zamanı normale döndür
        Time.timeScale = 1f;

        // Mevcut sahneyi yeniden yükle
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("🏠 Ana menüye dönülüyor...");

        // Zamanı normale döndür
        Time.timeScale = 1f;

        // Ana menüye git
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("🔴 Oyun kapatılıyor...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Event bağlantılarını temizle
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        }

        // Buton event'lerini temizle
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }
}