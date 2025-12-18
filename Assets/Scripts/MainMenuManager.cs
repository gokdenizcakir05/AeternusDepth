using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button exitButton;
    public TextMeshProUGUI gameTitleText;

    [Header("Credits Panel")]
    public GameObject creditsPanel;
    public TextMeshProUGUI creditsText;
    public Button closeCreditsButton;
    public float scrollSpeed = 30f;

    [Header("Scene Names")]
    public string gameSceneName = "SampleScene";

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound; // YENİ: Üzerine gelme sesi
    private AudioSource audioSource;

    [Header("Button Animation Settings")]
    public float hoverScaleAmount = 1.1f;
    public float animationSpeed = 15f;

    [Header("Title Animation")]
    public float titleFloatSpeed = 1.5f;
    public float titleFloatAmount = 20f;

    private Coroutine creditsCoroutine;
    private Dictionary<Button, Vector3> buttonOriginalScales = new Dictionary<Button, Vector3>();
    private HashSet<Button> hoveredButtons = new HashSet<Button>(); // YENİ: Hover kontrolü

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

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

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OpenCredits);
            AddButtonSound(creditsButton);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
            AddButtonSound(exitButton);
        }

        if (closeCreditsButton != null)
        {
            closeCreditsButton.onClick.AddListener(CloseCredits);
            AddButtonSound(closeCreditsButton);
        }

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        InitializeButtonAnimations();
        StartTitleAnimation();

        Debug.Log("🏠 Ana menü yüklendi");
    }

    void InitializeButtonAnimations()
    {
        Button[] allButtons = { startButton, settingsButton, creditsButton, exitButton };
        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                buttonOriginalScales[btn] = btn.transform.localScale;
                AddButtonHoverEffects(btn);
            }
        }
    }

    void AddButtonHoverEffects(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHoverEnter(button); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnButtonHoverExit(button); });
        trigger.triggers.Add(entryExit);
    }

    void OnButtonHoverEnter(Button button)
    {
        if (button != null && buttonOriginalScales.ContainsKey(button))
        {
            // YENİ: Hover sesi çal (eğer daha önce hover edilmemişse)
            if (!hoveredButtons.Contains(button))
            {
                PlayHoverSound();
                hoveredButtons.Add(button);
            }

            StopAllCoroutines();
            StartCoroutine(AnimateButtonScale(button, buttonOriginalScales[button] * hoverScaleAmount));
        }
    }

    void OnButtonHoverExit(Button button)
    {
        if (button != null && buttonOriginalScales.ContainsKey(button))
        {
            // YENİ: Butondan çıkınca hover listesinden çıkar
            hoveredButtons.Remove(button);

            StopAllCoroutines();
            StartCoroutine(AnimateButtonScale(button, buttonOriginalScales[button]));
        }
    }

    void PlayHoverSound()
    {
        if (buttonHoverSound != null && audioSource != null)
        {
            // YENİ: Hover sesini çal
            audioSource.PlayOneShot(buttonHoverSound);
            Debug.Log("🔊 Buton hover sesi çalındı");
        }
    }

    IEnumerator AnimateButtonScale(Button button, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startScale = button.transform.localScale;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime);
            yield return null;
        }

        button.transform.localScale = targetScale;
    }

    void StartTitleAnimation()
    {
        if (gameTitleText != null)
        {
            StartCoroutine(AnimateTitle());
        }
    }

    IEnumerator AnimateTitle()
    {
        Vector3 originalPosition = gameTitleText.transform.localPosition;

        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * titleFloatSpeed) * titleFloatAmount;
            gameTitleText.transform.localPosition = originalPosition + new Vector3(0, yOffset, 0);
            yield return null;
        }
    }

    void AddButtonSound(Button button)
    {
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

        if (startButton != null)
        {
            StartCoroutine(ButtonClickEffect(startButton));
        }

        StartCoroutine(LoadGameScene());
    }

    IEnumerator ButtonClickEffect(Button button)
    {
        Vector3 originalScale = button.transform.localScale;

        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime * 10f;
            button.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.8f, elapsed);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime * 10f;
            button.transform.localScale = Vector3.Lerp(originalScale * 0.8f, originalScale, elapsed);
            yield return null;
        }

        button.transform.localScale = originalScale;
    }

    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(0.2f);
        CleanupPreviousGame();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("⚙️ Ayarlar açılıyor...");

        if (settingsButton != null)
        {
            StartCoroutine(ButtonClickEffect(settingsButton));
        }
    }

    public void OpenCredits()
    {
        Debug.Log("🎬 Emeği Geçenler açılıyor...");

        if (creditsButton != null)
        {
            StartCoroutine(ButtonClickEffect(creditsButton));
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);

            if (creditsText != null)
            {
                creditsCoroutine = StartCoroutine(ScrollCredits());
            }
        }
    }

    public void CloseCredits()
    {
        Debug.Log("🎬 Emeği Geçenler kapatılıyor...");

        if (closeCreditsButton != null)
        {
            StartCoroutine(ButtonClickEffect(closeCreditsButton));
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);

            if (creditsCoroutine != null)
            {
                StopCoroutine(creditsCoroutine);
                creditsCoroutine = null;
            }
        }
    }

    IEnumerator ScrollCredits()
    {
        RectTransform textTransform = creditsText.GetComponent<RectTransform>();
        Vector2 startPosition = textTransform.anchoredPosition;
        startPosition.y = -creditsText.preferredHeight - 100f;
        textTransform.anchoredPosition = startPosition;

        while (textTransform.anchoredPosition.y < creditsText.preferredHeight + 100f)
        {
            textTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        CloseCredits();
    }

    public void ExitGame()
    {
        Debug.Log("👋 Oyun kapatılıyor...");

        if (exitButton != null)
        {
            StartCoroutine(ButtonClickEffect(exitButton));
        }

        StartCoroutine(ExitGameDelayed());
    }

    IEnumerator ExitGameDelayed()
    {
        yield return new WaitForSeconds(0.2f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (creditsPanel != null && creditsPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCredits();
        }
    }

    void CleanupPreviousGame()
    {
        Debug.Log("🧹 Önceki oyun temizleniyor...");

        ybotController player = FindObjectOfType<ybotController>();
        if (player != null)
        {
            Destroy(player.gameObject);
            Debug.Log("✅ Player temizlendi");
        }

        IsometricCameraController camera = FindObjectOfType<IsometricCameraController>();
        if (camera != null)
        {
            Destroy(camera.gameObject);
            Debug.Log("✅ Kamera temizlendi");
        }

        HealthBarUI healthBar = FindObjectOfType<HealthBarUI>();
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
            Debug.Log("✅ HealthBar temizlendi");
        }

        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            Destroy(spawnManager.gameObject);
            Debug.Log("✅ SpawnManager temizlendi");
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null && playerHealth.gameObject != player?.gameObject)
        {
            Destroy(playerHealth.gameObject);
            Debug.Log("✅ PlayerHealth temizlendi");
        }
    }

    public static void ReturnToMainMenu()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "MainMenu")
        {
            GameObject[] persistentObjects = GameObject.FindGameObjectsWithTag("Persistent");
            foreach (GameObject obj in persistentObjects)
            {
                Destroy(obj);
            }

            SceneManager.LoadScene("MainMenu");
        }
    }
}