using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class SeamanDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string[] dialogueLines;
    public float dialogueSpeed = 0.05f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI continuePrompt;
    public Image characterImage;

    [Header("Character Sprites")]
    public Sprite normalSprite;
    public Sprite talkingSprite;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    [Header("Other UI Elements to Hide")]
    public HealthBarUI healthBarUI; // Can barı referansı
    public GameTimer gameTimer; // Zaman sayacı referansı

    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    public ParticleSystem talkParticles;

    [Header("Debug")]
    public bool showDebug = true;

    public bool isDialogueActive { get; private set; }
    private bool isInRange = false;
    private int currentLine = 0;
    private string currentText = "";
    private float timer = 0f;
    private bool isTyping = false;

    // YENİ: Oyun durdurma için
    private float previousTimeScale;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;

    void Start()
    {
        // UI'ı başlangıçta kapat
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(false);

        // Referansları otomatik bul
        FindUIReferences();
    }

    void FindUIReferences()
    {
        // HealthBarUI'ı bul
        if (healthBarUI == null)
            healthBarUI = FindObjectOfType<HealthBarUI>();

        // GameTimer'ı bul
        if (gameTimer == null)
            gameTimer = FindObjectOfType<GameTimer>();

        if (showDebug)
        {
            if (healthBarUI != null) Debug.Log("✅ HealthBarUI bulundu!");
            if (gameTimer != null) Debug.Log("✅ GameTimer bulundu!");
        }
    }

    void Update()
    {
        if (isInRange && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }

        if (isDialogueActive)
        {
            HandleDialogueInput();
            UpdateTypingEffect();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        currentLine = 0;

        // YENİ: OYUNU DURDUR
        PauseGame();

        // UI'ı aç
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(true);

        // Karakter resmini ayarla
        if (characterImage != null && talkingSprite != null)
            characterImage.sprite = talkingSprite;

        // Particle efekti
        if (talkParticles != null && !talkParticles.isPlaying)
            talkParticles.Play();

        // CAN BARINI GİZLE
        if (healthBarUI != null)
        {
            CanvasGroup healthBarCanvas = healthBarUI.GetComponent<CanvasGroup>();
            if (healthBarCanvas != null)
            {
                healthBarCanvas.alpha = 0f;
            }
        }

        // ZAMANI DURDUR
        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        // Event tetikle
        onDialogueStart?.Invoke();

        // İlk satırı başlat
        StartTypingLine(dialogueLines[currentLine]);

        if (showDebug) Debug.Log("💬 Diyalog başladı! - Oyun durduruldu");
    }

    // YENİ: OYUNU DURDURMA METODU
    void PauseGame()
    {
        // Zamanı durdur
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Fareyi serbest bırak
        wasCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Player hareketini durdur
        DisablePlayerMovement();
    }

    // YENİ: OYUNU DEVAM ETTİRME METODU
    void ResumeGame()
    {
        // Zamanı normale döndür
        Time.timeScale = previousTimeScale;

        // Fareyi eski haline getir
        Cursor.visible = wasCursorVisible;
        Cursor.lockState = previousCursorLockState;

        // Player hareketini etkinleştir
        EnablePlayerMovement();
    }

    // YENİ: PLAYER HAREKETİNİ DURDUR
    void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // FirstPersonController varsa
            MonoBehaviour fpsController = player.GetComponent<MonoBehaviour>();
            if (fpsController != null && fpsController.GetType().Name.Contains("FirstPersonController"))
            {
                fpsController.enabled = false;
            }

            // CharacterController varsa
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            // Rigidbody varsa
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            if (showDebug) Debug.Log("🚫 Player hareketi durduruldu");
        }
    }

    // YENİ: PLAYER HAREKETİNİ ETKİNLEŞTİR
    void EnablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // FirstPersonController varsa
            MonoBehaviour fpsController = player.GetComponent<MonoBehaviour>();
            if (fpsController != null && fpsController.GetType().Name.Contains("FirstPersonController"))
            {
                fpsController.enabled = true;
            }

            // CharacterController varsa
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            // Rigidbody varsa
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            if (showDebug) Debug.Log("✅ Player hareketi etkinleştirildi");
        }
    }

    void StartTypingLine(string line)
    {
        currentText = "";
        isTyping = true;
        timer = 0f;

        if (dialogueText != null)
            dialogueText.text = "";
    }

    void UpdateTypingEffect()
    {
        if (!isTyping) return;

        timer += Time.unscaledDeltaTime; // TimeScale = 0 olduğu için unscaledDeltaTime kullan

        if (timer >= dialogueSpeed)
        {
            timer = 0f;

            if (currentText.Length < dialogueLines[currentLine].Length)
            {
                currentText += dialogueLines[currentLine][currentText.Length];

                if (dialogueText != null)
                    dialogueText.text = currentText;
            }
            else
            {
                isTyping = false;
                if (showDebug) Debug.Log($"📝 Satır {currentLine + 1} tamamlandı");
            }
        }
    }

    void HandleDialogueInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (isTyping)
            {
                // Yazma animasyonunu atla
                CompleteCurrentLine();
            }
            else
            {
                // Sonraki satıra geç
                NextLine();
            }
        }
    }

    void CompleteCurrentLine()
    {
        isTyping = false;
        currentText = dialogueLines[currentLine];

        if (dialogueText != null)
            dialogueText.text = currentText;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            StartTypingLine(dialogueLines[currentLine]);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        // YENİ: OYUNU DEVAM ETTİR
        ResumeGame();

        // UI'ı kapat
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(false);

        // Karakter resmini normale döndür
        if (characterImage != null && normalSprite != null)
            characterImage.sprite = normalSprite;

        // Particle efekti durdur
        if (talkParticles != null && talkParticles.isPlaying)
            talkParticles.Stop();

        // CAN BARINI GÖSTER
        if (healthBarUI != null)
        {
            CanvasGroup healthBarCanvas = healthBarUI.GetComponent<CanvasGroup>();
            if (healthBarCanvas != null)
            {
                healthBarCanvas.alpha = 1f;
            }
        }

        // ZAMANI DEVAM ETTİR
        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        // Event tetikle
        onDialogueEnd?.Invoke();

        if (showDebug) Debug.Log("💬 Diyalog bitti! - Oyun devam ediyor");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDialogueActive)
        {
            isInRange = true;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            if (showDebug) Debug.Log("🎯 Player etkileşim alanında");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (isDialogueActive)
                EndDialogue();

            if (showDebug) Debug.Log("🎯 Player etkileşim alanından çıktı");
        }
    }

    [ContextMenu("🔊 TEST DIALOGUE")]
    void TestDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            StartDialogue();
            Debug.Log("Test diyalogu başlatıldı!");
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Etkileşim alanını göster
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = isInRange ? Color.green : Color.yellow;
            if (col is BoxCollider boxCol)
            {
                Gizmos.DrawWireCube(transform.position + boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
        }
    }
}