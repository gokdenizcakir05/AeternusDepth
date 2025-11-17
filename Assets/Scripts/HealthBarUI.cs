using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Bar References")]
    public Image healthFillImage;
    public PlayerHealth playerHealth;

    [Header("Animation")]
    public float animationSpeed = 3f;

    private float targetFill;
    private float currentFill;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // CanvasGroup ekle
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        FindAndConnectPlayerHealth();

        currentFill = 1f;
        targetFill = 1f;

        // YENİ: PlayerHealth death event'ine bağlan
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            Debug.Log("✅ HealthBar: PlayerDeath event'ine bağlandı");
        }
    }

    void Update()
    {
        // Smooth fill animation
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * animationSpeed);

        if (healthFillImage != null)
            healthFillImage.fillAmount = currentFill;

        // ESC MENÜ, DİYALOG VE ÖLÜM KONTROLÜ
        UpdateVisibility();
    }

    // YENİ: GÖRÜNÜRLÜK KONTROLÜ
    void UpdateVisibility()
    {
        bool shouldHide = false;

        // 1. ESC menü kontrolü
        ESCMenu escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu != null && escMenu.isMenuOpen)
        {
            shouldHide = true;
        }

        // 2. Diyalog kontrolü
        SeamanDialogue dialogue = FindObjectOfType<SeamanDialogue>();
        if (dialogue != null && dialogue.isDialogueActive)
        {
            shouldHide = true;
        }

        // 3. YENİ: Ölüm ekranı kontrolü
        DeathScreenUI deathScreen = FindObjectOfType<DeathScreenUI>();
        if (deathScreen != null && deathScreen.deathPanel != null && deathScreen.deathPanel.activeInHierarchy)
        {
            shouldHide = true;
        }

        // 4. YENİ: Puzzle kontrolü - ARTIK ÇALIŞACAK!
        Door6PuzzleController puzzle = FindObjectOfType<Door6PuzzleController>();
        if (puzzle != null && puzzle.IsPuzzleUIOpen)
        {
            shouldHide = true;
        }

        // Görünürlüğü ayarla
        if (canvasGroup != null)
        {
            canvasGroup.alpha = shouldHide ? 0f : 1f;
            canvasGroup.interactable = !shouldHide;
            canvasGroup.blocksRaycasts = !shouldHide;
        }
    }

    // YENİ: PLAYER ÖLDÜĞÜNDE
    void OnPlayerDeath()
    {
        // Can barını hemen gizle
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Debug.Log("💀 HealthBar: Player öldü, can barı gizlendi");
    }

    // YENİ: CAN BARINI MANUEL KONTROL İÇİN METODLAR
    public void ShowHealthBar()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("✅ HealthBar: Can barı gösterildi");
        }
    }

    public void HideHealthBar()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Debug.Log("✅ HealthBar: Can barı gizlendi");
        }
    }

    // PLAYER HEALTH'I BUL VE BAĞLAN
    void FindAndConnectPlayerHealth()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            // YENİ: Death event'ine de bağlan
            playerHealth.OnDeath.AddListener(OnPlayerDeath);

            UpdateHealthBar(playerHealth.currentHealth);
            Debug.Log("✅ HealthBar: PlayerHealth bağlantısı kuruldu!");
        }
        else
        {
            Debug.LogWarning("⚠️ HealthBar: PlayerHealth bulunamadı! 2. deneme yapılıyor...");
            Invoke("RetryConnection", 1f);
        }
    }

    void RetryConnection()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            // YENİ: Death event'ine de bağlan
            playerHealth.OnDeath.AddListener(OnPlayerDeath);

            UpdateHealthBar(playerHealth.currentHealth);
            Debug.Log("✅ HealthBar: 2. denemede PlayerHealth bağlantısı kuruldu!");
        }
        else
        {
            Debug.LogError("❌ HealthBar: PlayerHealth hala bulunamadı!");
        }
    }

    public void UpdateHealthBar(int currentHealth)
    {
        if (playerHealth != null)
        {
            float healthPercent = (float)currentHealth / playerHealth.maxHealth;
            targetFill = healthPercent;
        }
    }

    // SAHNE DEĞİŞİNCE TEKRAR BAĞLAN
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // YENİ: Event bağlantılarını temizle
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // YENİ: CanvasGroup'u kontrol et
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // YENİ: Can barını göster (yeni sahneye geçince)
        ShowHealthBar();

        FindAndConnectPlayerHealth();
    }

    private static HealthBarUI instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}