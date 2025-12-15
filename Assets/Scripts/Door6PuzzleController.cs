using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Door6PuzzleController : MonoBehaviour
{
    [Header("Kablı Bağlantı Puzzle")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 3f;
    public GameObject puzzleUI;
    public GameObject door6;
    public int requiredConnections = 3; // Kaç kablo bağlanacak

    [Header("Kablo Elemanları")]
    public List<CablePoint> startPoints = new List<CablePoint>(); // Sol taraftaki başlangıç noktaları
    public List<CablePoint> endPoints = new List<CablePoint>();   // Sağ taraftaki bitiş noktaları
    public List<CableLine> cableLines = new List<CableLine>();   // Kablo çizgileri

    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI connectionsText;
    public Button resetButton;
    public Button submitButton;

    [Header("Ünlem İşareti")]
    public GameObject exclamationMark;
    public float blinkSpeed = 0.5f;

    [Header("Görsel Ayarlar")]
    public Color normalColor = Color.gray;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color hoverColor = Color.yellow;
    public float lineWidth = 5f;

    [Header("Debug")]
    public bool showDebug = true;

    [System.Serializable]
    public class CablePoint
    {
        public Image pointImage;
        public int correctPairIndex = -1; // Hangi bitiş noktasıyla eşleşmeli (-1 = eşleşme yok)
        [HideInInspector] public bool isConnected = false;
        [HideInInspector] public int connectionID = -1; // Hangi kabloya bağlı
    }

    [System.Serializable]
    public class CableLine
    {
        public LineRenderer lineRenderer;
        [HideInInspector] public int startPointIndex = -1;
        [HideInInspector] public int endPointIndex = -1;
        [HideInInspector] public bool isConnected = false;
        [HideInInspector] public int lineID;
    }

    private Transform player;
    private bool canInteract = false;
    private bool puzzleCompleted = false;
    private bool isPuzzleUIOpen = false;
    private bool isBlinking = false;
    private float previousTimeScale;

    private int selectedStartPointIndex = -1;
    private int currentConnections = 0;
    private int nextLineID = 0;

    public bool IsPuzzleUIOpen { get; internal set; }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
            StartBlinking();
        }

        InitializeCables();
    }

    void InitializeCables()
    {
        // Her kablo çizgisine ID ata
        for (int i = 0; i < cableLines.Count; i++)
        {
            if (cableLines[i].lineRenderer != null)
            {
                cableLines[i].lineID = i;
                cableLines[i].lineRenderer.enabled = false;
                cableLines[i].lineRenderer.startWidth = lineWidth;
                cableLines[i].lineRenderer.endWidth = lineWidth;
            }
        }

        // Tüm başlangıç noktalarını ayarla
        for (int i = 0; i < startPoints.Count; i++)
        {
            if (startPoints[i].pointImage != null)
            {
                startPoints[i].pointImage.color = normalColor;
                startPoints[i].connectionID = -1;
                AddPointClickHandler(startPoints[i], i, true);
            }
        }

        // Tüm bitiş noktalarını ayarla
        for (int i = 0; i < endPoints.Count; i++)
        {
            if (endPoints[i].pointImage != null)
            {
                endPoints[i].pointImage.color = normalColor;
                endPoints[i].connectionID = -1;
                AddPointClickHandler(endPoints[i], i, false);
            }
        }

        // Buton event'lerini ayarla
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetConnections);

        if (submitButton != null)
            submitButton.onClick.AddListener(CheckSolution);

        if (showDebug) Debug.Log($"Puzzle başlatıldı: {startPoints.Count} başlangıç, {endPoints.Count} bitiş noktası");
    }

    void AddPointClickHandler(CablePoint point, int index, bool isStartPoint)
    {
        // EventTrigger ekle
        var trigger = point.pointImage.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = point.pointImage.gameObject.AddComponent<EventTrigger>();
        }

        // Var olan trigger'ları temizle
        trigger.triggers.Clear();

        // Click event'i ekle
        var clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => {
            if (isStartPoint)
                OnStartPointClicked(index);
            else
                OnEndPointClicked(index);
        });
        trigger.triggers.Add(clickEntry);

        // Hover event'lerini ekle
        var hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => OnPointHover(point, true));
        trigger.triggers.Add(hoverEntry);

        var exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => OnPointHover(point, false));
        trigger.triggers.Add(exitEntry);
    }

    void OnStartPointClicked(int index)
    {
        if (puzzleCompleted || !isPuzzleUIOpen) return;

        var point = startPoints[index];

        if (!point.isConnected)
        {
            // Önceki seçimi temizle
            if (selectedStartPointIndex >= 0 && selectedStartPointIndex < startPoints.Count)
            {
                startPoints[selectedStartPointIndex].pointImage.color = normalColor;
            }

            // Yeni başlangıç noktasını seç
            selectedStartPointIndex = index;
            point.pointImage.color = hoverColor;
            UpdateMessage("Şimdi bir bitiş noktası seçin");

            if (showDebug) Debug.Log($"Başlangıç noktası {index} seçildi");
        }
        else
        {
            // Zaten bağlı bir noktaya tıklandı, bağlantıyı kaldır
            DisconnectCable(index, true);
        }
    }

    void OnEndPointClicked(int index)
    {
        if (puzzleCompleted || !isPuzzleUIOpen) return;
        if (selectedStartPointIndex == -1) return;

        var endPoint = endPoints[index];

        if (!endPoint.isConnected)
        {
            // Bitiş noktası seçildi, bağlantı yap
            ConnectCable(selectedStartPointIndex, index);
            selectedStartPointIndex = -1;
        }
        else
        {
            UpdateMessage("Bu bitiş noktası zaten bağlı!");
        }
    }

    void OnPointHover(CablePoint point, bool isEntering)
    {
        if (puzzleCompleted || !isPuzzleUIOpen) return;

        if (isEntering && !point.isConnected)
        {
            point.pointImage.color = hoverColor;
        }
        else if (!point.isConnected)
        {
            point.pointImage.color = normalColor;
        }
    }

    void ConnectCable(int startIndex, int endIndex)
    {
        var startPoint = startPoints[startIndex];
        var endPoint = endPoints[endIndex];

        if (startPoint.isConnected || endPoint.isConnected)
        {
            UpdateMessage("Bu noktalardan biri zaten bağlı!");
            return;
        }

        // Boş bir kablo çizgisi bul
        CableLine availableLine = null;
        foreach (var line in cableLines)
        {
            if (!line.isConnected)
            {
                availableLine = line;
                break;
            }
        }

        if (availableLine == null)
        {
            UpdateMessage("Bağlantı için kablo kalmadı!");
            return;
        }

        // Bağlantı yap
        startPoint.isConnected = true;
        endPoint.isConnected = true;
        startPoint.connectionID = availableLine.lineID;
        endPoint.connectionID = availableLine.lineID;

        startPoint.pointImage.color = connectedColor;
        endPoint.pointImage.color = connectedColor;

        // Kablo çizgisini ayarla
        availableLine.startPointIndex = startIndex;
        availableLine.endPointIndex = endIndex;
        availableLine.isConnected = true;

        availableLine.lineRenderer.enabled = true;
        availableLine.lineRenderer.startColor = connectedColor;
        availableLine.lineRenderer.endColor = connectedColor;

        // Çizgi pozisyonlarını güncelle
        UpdateLinePosition(availableLine);

        currentConnections++;
        UpdateUI();

        if (currentConnections >= requiredConnections)
        {
            UpdateMessage("Tüm bağlantılar tamamlandı! Kontrol etmek için 'Onayla' butonuna basın.");
        }
        else
        {
            UpdateMessage($"Bağlantı yapıldı! {requiredConnections - currentConnections} bağlantı daha gerekli.");
        }

        if (showDebug) Debug.Log($"Bağlantı: Start[{startIndex}] -> End[{endIndex}] (Line: {availableLine.lineID})");
    }

    void DisconnectCable(int pointIndex, bool isStartPoint)
    {
        CablePoint point;
        int otherPointIndex;
        bool otherIsStartPoint;

        if (isStartPoint)
        {
            point = startPoints[pointIndex];
            otherPointIndex = FindConnectedEndPoint(point.connectionID);
            otherIsStartPoint = false;
        }
        else
        {
            point = endPoints[pointIndex];
            otherPointIndex = FindConnectedStartPoint(point.connectionID);
            otherIsStartPoint = true;
        }

        if (point.connectionID == -1)
        {
            UpdateMessage("Bu nokta bağlı değil!");
            return;
        }

        // Kablo çizgisini bul
        var cableLine = cableLines.Find(line => line.lineID == point.connectionID);
        if (cableLine != null)
        {
            // Bağlantıları kaldır
            point.isConnected = false;
            point.connectionID = -1;
            point.pointImage.color = normalColor;

            // Diğer noktanın bağlantısını kaldır
            if (otherIsStartPoint && otherPointIndex >= 0 && otherPointIndex < startPoints.Count)
            {
                startPoints[otherPointIndex].isConnected = false;
                startPoints[otherPointIndex].connectionID = -1;
                startPoints[otherPointIndex].pointImage.color = normalColor;
            }
            else if (!otherIsStartPoint && otherPointIndex >= 0 && otherPointIndex < endPoints.Count)
            {
                endPoints[otherPointIndex].isConnected = false;
                endPoints[otherPointIndex].connectionID = -1;
                endPoints[otherPointIndex].pointImage.color = normalColor;
            }

            // Çizgiyi gizle
            cableLine.lineRenderer.enabled = false;
            cableLine.isConnected = false;
            cableLine.startPointIndex = -1;
            cableLine.endPointIndex = -1;

            currentConnections--;
            UpdateUI();
            UpdateMessage("Bağlantı kaldırıldı.");

            if (showDebug) Debug.Log($"Bağlantı kaldırıldı: Line {cableLine.lineID}");
        }
    }

    int FindConnectedEndPoint(int lineID)
    {
        for (int i = 0; i < endPoints.Count; i++)
        {
            if (endPoints[i].connectionID == lineID)
                return i;
        }
        return -1;
    }

    int FindConnectedStartPoint(int lineID)
    {
        for (int i = 0; i < startPoints.Count; i++)
        {
            if (startPoints[i].connectionID == lineID)
                return i;
        }
        return -1;
    }

    void UpdateLinePosition(CableLine line)
    {
        if (line.startPointIndex >= 0 && line.startPointIndex < startPoints.Count &&
            line.endPointIndex >= 0 && line.endPointIndex < endPoints.Count)
        {
            var startPoint = startPoints[line.startPointIndex];
            var endPoint = endPoints[line.endPointIndex];

            if (startPoint.pointImage != null && endPoint.pointImage != null)
            {
                Vector3 startPos = startPoint.pointImage.transform.position;
                Vector3 endPos = endPoint.pointImage.transform.position;

                line.lineRenderer.SetPosition(0, startPos);
                line.lineRenderer.SetPosition(1, endPos);
            }
        }
    }

    void CheckSolution()
    {
        if (currentConnections < requiredConnections)
        {
            UpdateMessage($"{requiredConnections} bağlantı gerekli! Şu an: {currentConnections}");
            return;
        }

        int correctCount = 0;

        for (int i = 0; i < startPoints.Count; i++)
        {
            var startPoint = startPoints[i];

            if (startPoint.isConnected)
            {
                // Bu başlangıç noktasına bağlı bitiş noktasını bul
                var cableLine = cableLines.Find(line => line.lineID == startPoint.connectionID);
                if (cableLine != null && cableLine.endPointIndex >= 0)
                {
                    // Doğru eşleşme mi kontrol et
                    if (cableLine.endPointIndex == startPoint.correctPairIndex)
                    {
                        correctCount++;
                        // Doğru bağlantıyı yeşil yap
                        cableLine.lineRenderer.startColor = Color.green;
                        cableLine.lineRenderer.endColor = Color.green;
                    }
                    else
                    {
                        // Yanlış bağlantıyı kırmızı yap
                        cableLine.lineRenderer.startColor = Color.red;
                        cableLine.lineRenderer.endColor = Color.red;
                    }
                }
            }
        }

        if (correctCount >= requiredConnections)
        {
            CompletePuzzle();
        }
        else
        {
            UpdateMessage($"{correctCount}/{requiredConnections} doğru bağlantı. {requiredConnections - correctCount} bağlantıyı düzeltin!");
        }
    }

    void ResetConnections()
    {
        // Tüm bağlantıları sıfırla
        foreach (var point in startPoints)
        {
            point.isConnected = false;
            point.connectionID = -1;
            if (point.pointImage != null)
                point.pointImage.color = normalColor;
        }

        foreach (var point in endPoints)
        {
            point.isConnected = false;
            point.connectionID = -1;
            if (point.pointImage != null)
                point.pointImage.color = normalColor;
        }

        foreach (var line in cableLines)
        {
            line.isConnected = false;
            line.startPointIndex = -1;
            line.endPointIndex = -1;
            if (line.lineRenderer != null)
                line.lineRenderer.enabled = false;
        }

        currentConnections = 0;
        selectedStartPointIndex = -1;
        UpdateMessage("Bağlantılar sıfırlandı. Tekrar deneyin!");
        UpdateUI();

        if (showDebug) Debug.Log("Tüm bağlantılar sıfırlandı");
    }

    void UpdateUI()
    {
        if (connectionsText != null)
            connectionsText.text = $"Bağlantılar: {currentConnections}/{requiredConnections}";
    }

    void UpdateMessage(string text)
    {
        if (messageText != null)
            messageText.text = text;
    }

    void OpenPuzzleUI()
    {
        if (puzzleUI != null && !puzzleCompleted && !isPuzzleUIOpen)
        {
            StopBlinking();

            if (GameTimer.Instance != null)
                GameTimer.Instance.PauseTimer();

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            puzzleUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPuzzleUIOpen = true;

            UpdateMessage($"Kabloları doğru şekilde bağlayın: {requiredConnections} bağlantı gerekli");
            UpdateUI();

            if (showDebug) Debug.Log("Puzzle UI açıldı");
        }
    }

    void CompletePuzzle()
    {
        puzzleCompleted = true;
        UpdateMessage("Başarılı! Tüm kablolar doğru bağlandı! Kapı açılıyor...");

        StartCoroutine(ClosePuzzleAfterDelay(1.5f));

        if (door6 != null)
        {
            door6.SetActive(false);
            if (showDebug) Debug.Log("🚪 Door6 açıldı!");
        }
    }

    IEnumerator ClosePuzzleAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ClosePuzzleUI();
    }

    public void ClosePuzzleUI()
    {
        if (puzzleUI != null)
        {
            if (!puzzleCompleted)
                StartBlinking();

            if (GameTimer.Instance != null)
                GameTimer.Instance.ResumeTimer();

            Time.timeScale = previousTimeScale;
            puzzleUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPuzzleUIOpen = false;

            if (showDebug) Debug.Log("Puzzle UI kapandı");
        }
    }

    void StartBlinking()
    {
        if (exclamationMark == null || puzzleCompleted) return;

        isBlinking = true;
        if (!exclamationMark.activeSelf)
            exclamationMark.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(BlinkExclamation());

        if (showDebug) Debug.Log("🔔 Puzzle ünlem işareti başlatıldı!");
    }

    void StopBlinking()
    {
        if (exclamationMark == null) return;

        isBlinking = false;

        if (showDebug) Debug.Log("🔕 Puzzle ünlem işareti durduruldu!");
    }

    IEnumerator BlinkExclamation()
    {
        while (isBlinking && exclamationMark != null)
        {
            exclamationMark.SetActive(!exclamationMark.activeSelf);
            yield return new WaitForSeconds(blinkSpeed);
        }

        // Blinking durduğunda ünlemi gizle
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
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

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        Gizmos.color = canInteract ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}