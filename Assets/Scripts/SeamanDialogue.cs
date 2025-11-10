using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI; // YENİ: Image için

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
    public Image characterImage; // YENİ: Karakter resmi

    [Header("Character Sprites")]
    public Sprite normalSprite;
    public Sprite talkingSprite; // YENİ: Konuşma sprite'ı

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

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

    void Start()
    {
        // UI'ı başlangıçta kapat
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(false);
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

        // UI'ı aç
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(true);

        // YENİ: Karakter resmini ayarla
        if (characterImage != null && talkingSprite != null)
            characterImage.sprite = talkingSprite;

        // Particle efekti
        if (talkParticles != null && !talkParticles.isPlaying)
            talkParticles.Play();

        // Event tetikle
        onDialogueStart?.Invoke();

        // İlk satırı başlat
        StartTypingLine(dialogueLines[currentLine]);

        if (showDebug) Debug.Log("💬 Diyalog başladı!");
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

        timer += Time.deltaTime;

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

        // UI'ı kapat
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(false);

        // YENİ: Karakter resmini normale döndür
        if (characterImage != null && normalSprite != null)
            characterImage.sprite = normalSprite;

        // Particle efekti durdur
        if (talkParticles != null && talkParticles.isPlaying)
            talkParticles.Stop();

        // Event tetikle
        onDialogueEnd?.Invoke();

        if (showDebug) Debug.Log("💬 Diyalog bitti!");
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