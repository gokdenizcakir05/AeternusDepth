using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Door6PuzzleController : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 3f;
    public GameObject puzzleUI;
    public Button[] puzzlePieces;
    public GameObject door6;

    [Header("Pattern Settings")]
    public float startDelay = 2f;
    public float patternShowTime = 1f;
    public float betweenShowTime = 0.5f;
    public float stageTransitionDelay = 1f;
    public int totalStages = 3;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color playerColor = Color.blue;
    public Color correctColor = Color.green;

    [Header("UI References")]
    public TextMeshProUGUI messageText;

    [Header("Ünlem İşareti")]
    public GameObject exclamationMark; // Ünlem işareti GameObject'i
    public float blinkSpeed = 0.5f; // Yanıp sönme hızı

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private bool canInteract = false;
    private bool puzzleCompleted = false;
    private bool isShowingPattern = false;
    private bool isPlayerTurn = false;
    private bool isPuzzleUIOpen = false;
    private float previousTimeScale;
    private bool isBlinking = false;

    private List<int> pattern = new List<int>();
    private List<int> playerInput = new List<int>();
    private int currentStage = 1;

    // YENİ: SADECE BU SATIRI EKLE!
    public bool IsPuzzleUIOpen { get { return isPuzzleUIOpen; } }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        // Ünlem işaretini başlangıçta gizle ve yanıp sönmeyi başlat
        if (exclamationMark != null)
            exclamationMark.SetActive(false);

        StartBlinking();

        SetupPuzzleButtons();
        GeneratePattern();
    }

    void Update()
    {
        if (player == null || puzzleCompleted || isPuzzleUIOpen) return;

        float distance = Vector3.Distance(transform.position, player.position);
        canInteract = distance <= interactionRange;

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            OpenPuzzleUI();
        }
    }

    // YENİ: ÜNLEM YANIP SÖNME METODLARI
    void StartBlinking()
    {
        if (exclamationMark == null) return;

        isBlinking = true;
        exclamationMark.SetActive(true);
        StartCoroutine(BlinkExclamation());

        if (showDebug) Debug.Log("🔔 Puzzle ünlem işareti başlatıldı!");
    }

    void StopBlinking()
    {
        if (exclamationMark == null) return;

        isBlinking = false;
        exclamationMark.SetActive(false);
        StopAllCoroutines();

        if (showDebug) Debug.Log("🔕 Puzzle ünlem işareti durduruldu!");
    }

    IEnumerator BlinkExclamation()
    {
        while (isBlinking)
        {
            if (exclamationMark != null)
            {
                exclamationMark.SetActive(!exclamationMark.activeSelf);
            }
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    void OpenPuzzleUI()
    {
        if (puzzleUI != null)
        {
            // ÜNLEMİ DURDUR
            StopBlinking();

            // TIMER'I DURDUR
            if (GameTimer.Instance != null)
                GameTimer.Instance.PauseTimer();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            puzzleUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPuzzleUIOpen = true;

            StartGame();

            if (showDebug) Debug.Log("Puzzle UI açıldı - Zaman durduruldu, ünlem gizlendi!");
        }
    }

    void SetupPuzzleButtons()
    {
        for (int i = 0; i < puzzlePieces.Length; i++)
        {
            int pieceIndex = i;
            puzzlePieces[i].onClick.AddListener(() => OnPuzzlePieceClicked(pieceIndex));
        }
    }

    Image GetSymbolImage(Button button)
    {
        if (button == null) return null;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null && buttonImage.sprite != null)
        {
            return buttonImage;
        }

        Image childImage = button.GetComponentInChildren<Image>();
        if (childImage != null)
        {
            return childImage;
        }

        return null;
    }

    void SetSymbolColor(Button button, Color color)
    {
        Image symbolImage = GetSymbolImage(button);
        if (symbolImage != null)
        {
            symbolImage.color = color;
        }
    }

    void GeneratePattern()
    {
        pattern.Clear();
        for (int i = 0; i < totalStages; i++)
        {
            pattern.Add(Random.Range(0, puzzlePieces.Length));
        }

        Debug.Log($"🎯 Oluşturulan pattern: {string.Join(", ", pattern)}");
    }

    void StartGame()
    {
        currentStage = 1;
        playerInput.Clear();

        foreach (Button btn in puzzlePieces)
        {
            SetSymbolColor(btn, normalColor);
        }

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        UpdateMessage("Hazır ol...");
        yield return new WaitForSecondsRealtime(1f);

        UpdateMessage("Deseni izle!");
        yield return new WaitForSecondsRealtime(startDelay - 1f);

        StartCoroutine(ShowPattern());
    }

    IEnumerator ShowPattern()
    {
        isShowingPattern = true;
        isPlayerTurn = false;

        SetButtonsInteractable(false);

        for (int i = 0; i < currentStage; i++)
        {
            int symbolIndex = pattern[i];
            Button currentButton = puzzlePieces[symbolIndex];

            SetSymbolColor(currentButton, highlightColor);
            yield return new WaitForSecondsRealtime(patternShowTime);

            SetSymbolColor(currentButton, normalColor);

            if (i < currentStage - 1)
            {
                yield return new WaitForSecondsRealtime(betweenShowTime);
            }
        }

        isShowingPattern = false;
        isPlayerTurn = true;
        UpdateMessage($"Sıra sende! {currentStage} sembolü tekrarla");
        SetButtonsInteractable(true);
    }

    void OnPuzzlePieceClicked(int pieceIndex)
    {
        if (!isPlayerTurn || isShowingPattern || puzzleCompleted) return;

        SetSymbolColor(puzzlePieces[pieceIndex], playerColor);
        StartCoroutine(ResetSymbolColor(puzzlePieces[pieceIndex], 0.3f));

        playerInput.Add(pieceIndex);
        CheckCurrentInput();
    }

    void CheckCurrentInput()
    {
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != pattern[i])
            {
                StartCoroutine(RestartStage());
                return;
            }
        }

        if (playerInput.Count == currentStage)
        {
            if (currentStage == totalStages)
            {
                CompletePuzzle();
            }
            else
            {
                StartCoroutine(NextStageWithDelay());
            }
        }
    }

    IEnumerator NextStageWithDelay()
    {
        isPlayerTurn = false;
        SetButtonsInteractable(false);

        UpdateMessage($"Aşama {currentStage} tamamlandı!");

        yield return new WaitForSecondsRealtime(stageTransitionDelay);

        currentStage++;
        playerInput.Clear();
        UpdateMessage($"Aşama {currentStage} için hazır ol!");

        yield return new WaitForSecondsRealtime(0.5f);

        UpdateMessage("Deseni izle!");
        StartCoroutine(ShowPattern());
    }

    IEnumerator ResetSymbolColor(Button button, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (!puzzleCompleted)
            SetSymbolColor(button, normalColor);
    }

    IEnumerator RestartStage()
    {
        isPlayerTurn = false;
        SetButtonsInteractable(false);

        UpdateMessage("Yanlış! Tekrar deneyelim");

        foreach (Button btn in puzzlePieces)
        {
            SetSymbolColor(btn, Color.red);
        }

        yield return new WaitForSecondsRealtime(1f);

        foreach (Button btn in puzzlePieces)
        {
            SetSymbolColor(btn, normalColor);
        }

        playerInput.Clear();
        currentStage = 1;
        UpdateMessage("Yeniden başlıyor...");

        yield return new WaitForSecondsRealtime(0.5f);

        StartCoroutine(ShowPattern());
    }

    void CompletePuzzle()
    {
        puzzleCompleted = true;

        foreach (Button btn in puzzlePieces)
        {
            SetSymbolColor(btn, correctColor);
        }

        UpdateMessage("Tebrikler! Puzzle tamamlandı");
        StartCoroutine(ClosePuzzleAfterDelay(2f));

        if (door6 != null)
        {
            door6.SetActive(false);
            Debug.Log("🚪 Door6 açıldı!");
        }
    }

    IEnumerator ClosePuzzleAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ClosePuzzleUI();
    }

    void SetButtonsInteractable(bool interactable)
    {
        foreach (Button btn in puzzlePieces)
        {
            btn.interactable = interactable;
        }
    }

    void UpdateMessage(string text)
    {
        if (messageText != null)
            messageText.text = text;
    }

    public void ClosePuzzleUI()
    {
        if (puzzleUI != null)
        {
            // EĞER PUZZLE TAMAMLANMADIYSA ÜNLEMİ TEKRAR BAŞLAT
            if (!puzzleCompleted)
            {
                StartBlinking();
            }

            // TIMER'I DEVAM ETTIR
            if (GameTimer.Instance != null)
                GameTimer.Instance.ResumeTimer();

            Time.timeScale = previousTimeScale;

            puzzleUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPuzzleUIOpen = false;

            if (showDebug) Debug.Log("Puzzle UI kapandı - Zaman normale döndü!" + (puzzleCompleted ? " Puzzle tamamlandı!" : " Ünlem tekrar başlatıldı!"));
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = canInteract ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}