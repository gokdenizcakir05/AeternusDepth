using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Hover Energy UI")]
    public Slider hoverEnergySlider;
    public Image hoverEnergyFill;
    public GameObject hoverEnergyPanel;
    public Text hoverEnergyText;

    [Header("Colors")]
    public Color fullEnergyColor = Color.green;
    public Color mediumEnergyColor = Color.yellow;
    public Color lowEnergyColor = Color.red;

    [Header("References")]
    public ybotController playerController;

    [Header("UI Visibility")]
    public bool autoHideUI = true; // Otomatik gizleme açık

    private CanvasGroup hoverCanvasGroup;

    private void Start()
    {
        // CanvasGroup oluştur veya bul
        if (hoverEnergyPanel != null)
        {
            hoverCanvasGroup = hoverEnergyPanel.GetComponent<CanvasGroup>();
            if (hoverCanvasGroup == null)
            {
                hoverCanvasGroup = hoverEnergyPanel.AddComponent<CanvasGroup>();
            }

            hoverEnergyPanel.SetActive(true);
            hoverCanvasGroup.alpha = 1f;
        }

        if (playerController == null)
            playerController = FindObjectOfType<ybotController>();
    }

    private void Update()
    {
        if (playerController != null)
        {
            UpdateHoverEnergyUI();
        }

        // Görünürlük kontrolü
        if (autoHideUI)
        {
            UpdateHoverUIVisibility();
        }
    }

    // HealthBarUI'dakiyle AYNI görünürlük kontrolü
    private void UpdateHoverUIVisibility()
    {
        if (hoverCanvasGroup == null) return;

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

        // 3. Ölüm ekranı kontrolü
        DeathScreenUI deathScreen = FindObjectOfType<DeathScreenUI>();
        if (deathScreen != null && deathScreen.deathPanel != null && deathScreen.deathPanel.activeInHierarchy)
        {
            shouldHide = true;
        }

        // 4. Puzzle kontrolü
        Door6PuzzleController puzzle = FindObjectOfType<Door6PuzzleController>();
        if (puzzle != null && puzzle.IsPuzzleUIOpen)
        {
            shouldHide = true;
        }

        // Görünürlüğü ayarla (HealthBarUI ile AYNI mantık)
        hoverCanvasGroup.alpha = shouldHide ? 0f : 1f;
        hoverCanvasGroup.interactable = !shouldHide;
        hoverCanvasGroup.blocksRaycasts = !shouldHide;

        // DEBUG: Kontrol etmek için
        if (shouldHide && Time.frameCount % 120 == 0)
        {
            Debug.Log("⚠️ HoverUI gizlendi - Neden: " + GetHideReason());
        }
    }

    // DEBUG: Neden gizlendiğini görmek için
    private string GetHideReason()
    {
        ESCMenu escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu != null && escMenu.isMenuOpen) return "ESC Menu";

        SeamanDialogue dialogue = FindObjectOfType<SeamanDialogue>();
        if (dialogue != null && dialogue.isDialogueActive) return "Seaman Dialogue";

        DeathScreenUI deathScreen = FindObjectOfType<DeathScreenUI>();
        if (deathScreen != null && deathScreen.deathPanel != null && deathScreen.deathPanel.activeInHierarchy) return "Death Screen";

        Door6PuzzleController puzzle = FindObjectOfType<Door6PuzzleController>();
        if (puzzle != null && puzzle.IsPuzzleUIOpen) return "Puzzle UI";

        return "Unknown";
    }

    private void UpdateHoverEnergyUI()
    {
        // Eğer UI gizliyse, güncelleme yapma
        if (hoverCanvasGroup != null && hoverCanvasGroup.alpha <= 0.1f)
            return;

        // Enerji yüzdesini al
        float energyPercentage = playerController.GetHoverEnergyPercentage();
        bool isHovering = playerController.IsHovering();

        // Slider'ı güncelle
        if (hoverEnergySlider != null)
        {
            hoverEnergySlider.value = energyPercentage;

            // Renk güncelle
            if (hoverEnergyFill != null)
            {
                if (energyPercentage > 0.5f)
                    hoverEnergyFill.color = fullEnergyColor;
                else if (energyPercentage > 0.2f)
                    hoverEnergyFill.color = mediumEnergyColor;
                else
                    hoverEnergyFill.color = lowEnergyColor;
            }
        }

        // Text'i güncelle
        if (hoverEnergyText != null)
        {
            hoverEnergyText.text = $"Hover: {Mathf.RoundToInt(energyPercentage * 100)}%";

            // Hover modunda değilken gri yap
            if (!isHovering && energyPercentage < 100)
            {
                hoverEnergyText.color = Color.gray;
            }
            else if (isHovering)
            {
                hoverEnergyText.color = Color.cyan;
            }
            else
            {
                hoverEnergyText.color = Color.white;
            }
        }

        // Hover modunda iken UI'ı daha görünür yap
        if (hoverEnergyPanel != null && hoverEnergyFill != null)
        {
            if (isHovering)
            {
                hoverEnergyFill.GetComponent<Image>().color = new Color(
                    hoverEnergyFill.color.r,
                    hoverEnergyFill.color.g,
                    hoverEnergyFill.color.b,
                    1f
                );
            }
            else
            {
                hoverEnergyFill.GetComponent<Image>().color = new Color(
                    hoverEnergyFill.color.r,
                    hoverEnergyFill.color.g,
                    hoverEnergyFill.color.b,
                    0.7f
                );
            }
        }
    }

    // Manuel kontrol için metodlar
    public void ShowHoverUI()
    {
        if (hoverCanvasGroup != null)
        {
            hoverCanvasGroup.alpha = 1f;
            hoverCanvasGroup.interactable = true;
            hoverCanvasGroup.blocksRaycasts = true;
        }
    }

    public void HideHoverUI()
    {
        if (hoverCanvasGroup != null)
        {
            hoverCanvasGroup.alpha = 0f;
            hoverCanvasGroup.interactable = false;
            hoverCanvasGroup.blocksRaycasts = false;
        }
    }

    // Enerji yenilenirken yanıp sönme efekti
    public void FlashEnergyBar()
    {
        // Animasyon eklemek için burayı genişletebilirsin
        // Örneğin: StartCoroutine(FlashCoroutine());
    }

    // Sahne değişiminde DontDestroyOnLoad için
    private static UIController instance;
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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Yeni sahneye geçince playerController'ı tekrar bul
        if (playerController == null)
            playerController = FindObjectOfType<ybotController>();

        // UI'ı göster
        ShowHoverUI();
    }
}

// SceneManager için using eklemeyi unutma!
// Eğer yoksa en üste ekle: using UnityEngine.SceneManagement;