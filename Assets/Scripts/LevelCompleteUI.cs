using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject levelCompletePanel;
    public TMP_Text completionText;
    public Button nextLevelButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Settings")]
    public string completionMessage = "BÖLÜM TAMAMLANDI!";
    public bool pauseGameOnShow = true;

    private bool isUIVisible = false;

    void Start()
    {
        // Panel başlangıçta kapalı
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        // Buton eventlerini ayarla
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(NextLevel);
        }
        else
        {
            Debug.LogError("❌ NEXT LEVEL BUTON: Referansı eksik!");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("❌ MAIN MENU BUTON: Referansı eksik!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogError("❌ QUIT BUTON: Referansı eksik!");
        }

        Debug.Log("✅ LEVEL COMPLETE UI: Başlatıldı");
    }

    void Update()
    {
        // ESC tuşunu engelle
        if (isUIVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }
    }

    public void ShowLevelComplete()
    {
        if (levelCompletePanel == null)
        {
            Debug.LogError("❌ LEVEL COMPLETE UI: Panel referansı yok!");
            return;
        }

        // UI'yı göster
        levelCompletePanel.SetActive(true);
        isUIVisible = true;

        // Metni güncelle
        if (completionText != null)
        {
            completionText.text = completionMessage;
        }

        // Oyunu duraklat
        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
        }

        // Diğer UI'ları gizle
        HideOtherUIs();

        // Mouse'u göster
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("🎉 LEVEL COMPLETE UI: Gösterildi");
    }

    public void HideLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            isUIVisible = false;

            // Oyunu devam ettir
            Time.timeScale = 1f;

            Debug.Log("📱 LEVEL COMPLETE UI: Gizlendi");
        }
    }

    void HideOtherUIs()
    {
        // HealthBar'ı bul ve gizle
        BossHealth bossHealth = FindObjectOfType<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.HideHealthBar();
        }

        Debug.Log("👻 LEVEL COMPLETE UI: Diğer UI'lar gizlendi");
    }

    void NextLevel()
    {
        Debug.Log("➡️ LEVEL COMPLETE UI: Sonraki Bölüm butonuna tıklandı");

        // Önce UI'yı gizle ve oyunu devam ettir
        HideLevelComplete();
        Time.timeScale = 1f;

        // Sonraki bölümü yükle
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("🎊 OYUN TAMAMLANDI! Ana menüye yönlendiriliyor...");
            SceneManager.LoadScene("MainMenu");
        }
    }

    void GoToMainMenu()
    {
        Debug.Log("🏠 LEVEL COMPLETE UI: Ana Menü butonuna tıklandı");

        // Önce UI'yı gizle ve oyunu devam ettir
        HideLevelComplete();
        Time.timeScale = 1f;

        // Ana menüyü yükle
        SceneManager.LoadScene("MainMenu");
    }

    void QuitGame()
    {
        Debug.Log("🚪 LEVEL COMPLETE UI: Çıkış butonuna tıklandı");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public bool IsUIVisible()
    {
        return isUIVisible;
    }
}