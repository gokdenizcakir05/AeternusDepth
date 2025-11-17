using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button settingsButton;
    public Button creditsButton; // YENİ: Credits butonu
    public Button exitButton;

    [Header("Credits Panel")]
    public GameObject creditsPanel; // YENİ: Credits paneli
    public TextMeshProUGUI creditsText; // YENİ: Credits metni
    public Button closeCreditsButton; // YENİ: Credits kapatma butonu
    public float scrollSpeed = 30f; // YENİ: Kaydırma hızı

    [Header("Scene Names")]
    public string gameSceneName = "SampleScene";

    [Header("Audio")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    // YENİ: Credits coroutine referansı
    private Coroutine creditsCoroutine;

    void Start()
    {
        // AudioSource'u ayarla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Buton event'lerini bağla
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            AddButtonSound(startButton);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
            AddButtonSound(settingsButton);
        }

        if (creditsButton != null) // YENİ: Credits butonu
        {
            creditsButton.onClick.AddListener(OpenCredits);
            AddButtonSound(creditsButton);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
            AddButtonSound(exitButton);
        }

        // YENİ: Close credits butonu
        if (closeCreditsButton != null)
        {
            closeCreditsButton.onClick.AddListener(CloseCredits);
            AddButtonSound(closeCreditsButton);
        }

        // YENİ: Credits panelini başlangıçta gizle
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        // Menüde cursor gözüksün
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("🏠 Ana menü yüklendi");
    }

    void AddButtonSound(Button button)
    {
        // Buton ses efekti ekle
        button.onClick.AddListener(PlayButtonSound);
    }

    void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void StartGame()
    {
        Debug.Log("🎮 Oyun başlatılıyor: " + gameSceneName);

        // Önceki oyundan kalan persistent objeleri temizle
        CleanupPreviousGame();

        // Oyun sahnesine geç
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("⚙️ Ayarlar açılıyor...");
        // Burayı settings paneli ile dolduracaksın
    }

    // YENİ: CREDITS PANELİNİ AÇ
    public void OpenCredits()
    {
        Debug.Log("🎬 Emeği Geçenler açılıyor...");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);

            // Credits kaydırmayı başlat
            if (creditsText != null)
            {
                creditsCoroutine = StartCoroutine(ScrollCredits());
            }
        }
    }

    // YENİ: CREDITS PANELİNİ KAPAT
    public void CloseCredits()
    {
        Debug.Log("🎬 Emeği Geçenler kapatılıyor...");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);

            // Credits kaydırmayı durdur
            if (creditsCoroutine != null)
            {
                StopCoroutine(creditsCoroutine);
                creditsCoroutine = null;
            }
        }
    }

    // YENİ: CREDITS KAYDIRMA ANİMASYONU
    IEnumerator ScrollCredits()
    {
        // Metnin başlangıç pozisyonunu ayarla
        RectTransform textTransform = creditsText.GetComponent<RectTransform>();
        Vector2 startPosition = textTransform.anchoredPosition;
        startPosition.y = -creditsText.preferredHeight - 100f;
        textTransform.anchoredPosition = startPosition;

        // Kaydırmayı başlat
        while (textTransform.anchoredPosition.y < creditsText.preferredHeight + 100f)
        {
            textTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        // Kaydırma bittiğinde otomatik kapat
        CloseCredits();
    }

    public void ExitGame()
    {
        Debug.Log("👋 Oyun kapatılıyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        // YENİ: ESC tuşuyla credits'i kapat
        if (creditsPanel != null && creditsPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCredits();
        }
    }

    void CleanupPreviousGame()
    {
        Debug.Log("🧹 Önceki oyun temizleniyor...");

        // Player'ı temizle
        ybotController player = FindObjectOfType<ybotController>();
        if (player != null)
        {
            Destroy(player.gameObject);
            Debug.Log("✅ Player temizlendi");
        }

        // Kamerayı temizle
        IsometricCameraController camera = FindObjectOfType<IsometricCameraController>();
        if (camera != null)
        {
            Destroy(camera.gameObject);
            Debug.Log("✅ Kamera temizlendi");
        }

        // HealthBar'ı temizle
        HealthBarUI healthBar = FindObjectOfType<HealthBarUI>();
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
            Debug.Log("✅ HealthBar temizlendi");
        }

        // SpawnManager'ı temizle
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            Destroy(spawnManager.gameObject);
            Debug.Log("✅ SpawnManager temizlendi");
        }

        // PlayerHealth'i temizle (player ile birlikte gidecek ama yine de kontrol et)
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null && playerHealth.gameObject != player?.gameObject)
        {
            Destroy(playerHealth.gameObject);
            Debug.Log("✅ PlayerHealth temizlendi");
        }
    }

    // Oyun içinden menüye dönmek için (boss sonrası vs.)
    public static void ReturnToMainMenu()
    {
        // Mevcut sahneyi al
        Scene currentScene = SceneManager.GetActiveScene();

        // Eğer zaten main menu'de değilsek
        if (currentScene.name != "MainMenu")
        {
            // Tüm persistent objeleri temizle
            GameObject[] persistentObjects = GameObject.FindGameObjectsWithTag("Persistent");
            foreach (GameObject obj in persistentObjects)
            {
                Destroy(obj);
            }

            // Main menu'ye dön
            SceneManager.LoadScene("MainMenu");
        }
    }
}