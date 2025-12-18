using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Door6PuzzleController : MonoBehaviour
{
    [Header("References")]
    public GameObject puzzleUI;
    public Button hiddenButton;
    public GameObject mossContainer;
    public GameObject door6;
    public TextMeshProUGUI cleanText;
    public TextMeshProUGUI messageText;

    [Header("Moss Settings")]
    public GameObject mossPrefab;
    public int mossCount = 18;
    public float mossCleanDistance = 300f; // 600x600 için

    [Header("UI Settings")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Ünlem İşareti")]
    public GameObject exclamationMark;
    public float blinkSpeed = 0.5f;

    private List<GameObject> mossList = new List<GameObject>();
    private int cleanedMossCount = 0;
    private bool puzzleCompleted = false;
    private bool isPuzzleUIOpen = false;
    private bool isBlinking = false;
    private Transform player;
    private float previousTimeScale;

    public bool IsPuzzleUIOpen { get { return isPuzzleUIOpen; } }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (puzzleUI) puzzleUI.SetActive(false);

        if (hiddenButton)
        {
            hiddenButton.onClick.AddListener(OnButtonClick);
        }

        StartBlinking();
        CreateMossObjects();
    }

    void Update()
    {
        if (player == null || puzzleCompleted || isPuzzleUIOpen) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool canInteract = distance <= interactionRange;

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            OpenPuzzleUI();
        }
    }

    void StartBlinking()
    {
        if (exclamationMark == null) return;
        isBlinking = true;
        exclamationMark.SetActive(true);
        StartCoroutine(BlinkExclamation());
    }

    void StopBlinking()
    {
        if (exclamationMark == null) return;
        isBlinking = false;
        exclamationMark.SetActive(false);
        StopAllCoroutines();
    }

    IEnumerator BlinkExclamation()
    {
        while (isBlinking)
        {
            exclamationMark.SetActive(!exclamationMark.activeSelf);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    void CreateMossObjects()
    {
        if (mossContainer == null || mossPrefab == null) return;

        // Eski yosunları temizle
        foreach (Transform child in mossContainer.transform)
        {
            Destroy(child.gameObject);
        }
        mossList.Clear();
        cleanedMossCount = 0;

        // 600x600 İÇİN AYARLAR
        float maxPosition = 250f;   // -250 ile +250 arası
        float minScale = 1.2f;      // Minimum boyut
        float maxScale = 2.0f;      // Maksimum boyut

        for (int i = 0; i < mossCount; i++)
        {
            GameObject moss = Instantiate(mossPrefab, mossContainer.transform);

            RectTransform rt = moss.GetComponent<RectTransform>();

            // Rastgele pozisyon (600x600 alan içinde)
            rt.anchoredPosition = new Vector2(
                Random.Range(-maxPosition, maxPosition),
                Random.Range(-maxPosition, maxPosition)
            );

            // Rastgele boyut (1.2x - 2.0x)
            float scale = Random.Range(minScale, maxScale);
            rt.localScale = new Vector3(scale, scale, 1f);

            // Rastgele dönüş (-60 ile +60 derece)
            rt.rotation = Quaternion.Euler(0, 0, Random.Range(-60f, 60f));

            // Draggable script bağla
            MossDraggable draggable = moss.GetComponent<MossDraggable>();
            if (draggable != null)
            {
                draggable.Initialize(this, moss);
            }

            mossList.Add(moss);
        }
    }

    public void OnMossDragged(GameObject moss, Vector2 dragAmount)
    {
        if (puzzleCompleted) return;

        RectTransform rt = moss.GetComponent<RectTransform>();
        rt.anchoredPosition += dragAmount;

        // Düğme merkezinden uzaklık
        float distanceFromCenter = rt.anchoredPosition.magnitude;

        // Uzaklaştırınca temizle
        if (distanceFromCenter > mossCleanDistance)
        {
            RemoveMoss(moss);
        }
    }

    void RemoveMoss(GameObject moss)
    {
        if (mossList.Contains(moss))
        {
            mossList.Remove(moss);
            StartCoroutine(FadeOutMoss(moss));
            cleanedMossCount++;
            UpdateProgress();
        }
    }

    IEnumerator FadeOutMoss(GameObject moss)
    {
        Image mossImage = moss.GetComponent<Image>();
        float fadeTime = 0.3f;
        float elapsed = 0f;
        Color startColor = mossImage.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            mossImage.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        Destroy(moss);
    }

    void UpdateProgress()
    {
        float percent = (float)cleanedMossCount / mossCount * 100f;

        if (cleanText)
            cleanText.text = $"Moss Cleaned: %{(int)percent}"; // İNGİLİZCE

        if (cleanedMossCount >= mossCount)
        {
            if (messageText)
                messageText.text = "BUTTON CLEANED! CLICK NOW!"; // İNGİLİZCE
        }
        else if (messageText)
        {
            messageText.text = "Drag moss away from button!"; // İNGİLİZCE
        }
    }

    public void OnButtonClick()
    {
        if (puzzleCompleted) return;

        // Tüm yosunlar temizlendi mi kontrol et
        if (cleanedMossCount < mossCount)
        {
            if (messageText)
                messageText.text = "Clean all moss first!"; // İNGİLİZCE
            return;
        }

        Debug.Log("🎯 Button clicked!");
        puzzleCompleted = true;

        // Kapıyı aç
        if (door6)
        {
            door6.SetActive(false);
            Debug.Log("🚪 Door opened!");
        }

        if (messageText)
            messageText.text = "SUCCESS! DOOR OPENED!"; // İNGİLİZCE

        StartCoroutine(CloseAfterDelay(2f));
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ClosePuzzleUI();
    }

    void OpenPuzzleUI()
    {
        if (puzzleUI != null)
        {
            StopBlinking();

            // Timer'ı durdur
            if (GameTimer.Instance != null)
                GameTimer.Instance.PauseTimer();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            puzzleUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPuzzleUIOpen = true;

            UpdateProgress();
        }
    }

    public void ClosePuzzleUI()
    {
        if (puzzleUI != null)
        {
            // Puzzle tamamlanmadıysa ünlemi tekrar başlat
            if (!puzzleCompleted)
                StartBlinking();

            // Timer'ı devam ettir
            if (GameTimer.Instance != null)
                GameTimer.Instance.ResumeTimer();

            Time.timeScale = previousTimeScale;

            puzzleUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPuzzleUIOpen = false;
        }
    }
}