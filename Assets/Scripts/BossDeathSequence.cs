using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class BossDeathSequence : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    public float shakeDuration = 3f;
    public float shakeIntensity = 0.03f;
    public float shakeFrequency = 5f;

    [Header("Lovecraft Yazısı Ayarları")]
    public string lovecraftText = "Ph'nglui mglw'nafh Cthulhu R'lyeh wgah'nagl fhtagn";
    public float textAppearDelay = 0.5f;
    public float textRevealDuration = 2f;
    public Color textColor = new Color(0f, 1f, 0.5f, 1f);
    public TMP_FontAsset textFont;
    public float textSize = 48f;

    [Header("Fade Settings")]
    public float fadeDelay = 0f;
    public float fadeDuration = 4f;
    public Color fadeColor = Color.black;

    [Header("Scene Transition")]
    public string nextSceneName = "Level1";
    public float sceneTransitionDelay = 2f;

    [Header("UI Settings")]
    public int fadeSortingOrder = 5;      // Normal UI'ların üstünde ama çok değil
    public int textSortingOrder = 6;      // Fade'in üstünde
    public bool allowUIClicks = true;     // UI tıklamalarına izin ver

    [Header("References")]
    public Camera mainCamera;
    public Transform playerTransform;

    private Vector3 cameraOriginalLocalPos;
    private bool isSequenceActive = false;
    private Image fadeImage;
    private bool wasCameraChild = false;
    private Transform originalCameraParent;
    private TextMeshProUGUI lovecraftTextUI;
    private CanvasGroup textCanvasGroup;
    private GameObject textCanvasObj;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateFadeOverlay();
        CreateLovecraftText();
    }

    void CreateFadeOverlay()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = fadeSortingOrder; // 9999 yerine düşük değer

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // TIKLAMALARI ENGELLEME - GraphicRaycaster ekleme veya disable et
        // GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        // raycaster.enabled = !allowUIClicks; // Eğer UI tıklamalarına izin veriyorsak

        // CanvasGroup ile tıklamaları engelleme
        CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false; // Tıklamalar geçsin
        canvasGroup.interactable = false;

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false; // BU ÇOK ÖNEMLİ! Image tıklamaları engellemesin

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        canvasObj.SetActive(true);
    }

    void CreateLovecraftText()
    {
        // Yazı için Canvas
        textCanvasObj = new GameObject("LovecraftCanvas");
        Canvas textCanvas = textCanvasObj.AddComponent<Canvas>();
        textCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        textCanvas.sortingOrder = textSortingOrder; // 10000 yerine düşük değer

        CanvasScaler textScaler = textCanvasObj.AddComponent<CanvasScaler>();
        textScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        textScaler.referenceResolution = new Vector2(1920, 1080);
        textScaler.matchWidthOrHeight = 0.5f;

        // TIKLAMALARI ENGELLEME
        // GraphicRaycaster textRaycaster = textCanvasObj.AddComponent<GraphicRaycaster>();
        // textRaycaster.enabled = !allowUIClicks;

        // CanvasGroup for fade in/out
        textCanvasGroup = textCanvasObj.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        textCanvasGroup.blocksRaycasts = false; // Tıklamalar geçsin
        textCanvasGroup.interactable = false;

        // TextMeshPro Text
        GameObject textObj = new GameObject("LovecraftText");
        textObj.transform.SetParent(textCanvasObj.transform);

        lovecraftTextUI = textObj.AddComponent<TextMeshProUGUI>();
        lovecraftTextUI.text = lovecraftText;
        lovecraftTextUI.color = textColor;
        lovecraftTextUI.fontSize = textSize;
        lovecraftTextUI.alignment = TextAlignmentOptions.Center;
        lovecraftTextUI.enableWordWrapping = true;
        lovecraftTextUI.overflowMode = TextOverflowModes.Overflow;
        lovecraftTextUI.raycastTarget = false; // Text'e tıklanamaz

        if (textFont != null)
        {
            lovecraftTextUI.font = textFont;
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;

        textCanvasObj.SetActive(true);
    }

    // Diğer metodlar aynı kalacak...
    public void StartDeathSequence()
    {
        if (isSequenceActive) return;

        isSequenceActive = true;
        Debug.Log("🎬 Boss Death Sequence Başladı");

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (mainCamera != null && playerTransform != null)
        {
            if (mainCamera.transform.parent != playerTransform)
            {
                wasCameraChild = false;
                originalCameraParent = mainCamera.transform.parent;
                mainCamera.transform.SetParent(playerTransform);
                cameraOriginalLocalPos = mainCamera.transform.localPosition;
                mainCamera.transform.localRotation = Quaternion.identity;
            }
            else
            {
                wasCameraChild = true;
                cameraOriginalLocalPos = mainCamera.transform.localPosition;
            }
        }

        StartCoroutine(DeathSequenceCoroutine());
    }

    IEnumerator DeathSequenceCoroutine()
    {
        Debug.Log("1️⃣ Camera shake başlıyor...");
        StartCoroutine(CameraShake());

        yield return new WaitForSeconds(shakeDuration);

        if (!wasCameraChild && mainCamera != null)
        {
            mainCamera.transform.SetParent(originalCameraParent);
        }

        Debug.Log("2️⃣ Yazı ve kararma başlıyor...");

        StartCoroutine(ShowLovecraftText());
        StartCoroutine(FadeScreen(0f, 1f, fadeDuration));

        yield return new WaitForSeconds(fadeDuration + sceneTransitionDelay);

        Debug.Log("3️⃣ " + nextSceneName + " sahnesine geçiliyor...");
        LoadNextScene();
    }

    IEnumerator ShowLovecraftText()
    {
        if (textCanvasObj == null || lovecraftTextUI == null) yield break;

        yield return new WaitForSeconds(textAppearDelay);

        Debug.Log("🖋️ Lovecraft yazısı gösteriliyor...");

        string fullText = lovecraftTextUI.text;
        lovecraftTextUI.text = "";
        lovecraftTextUI.maxVisibleCharacters = 0;

        float fadeInTime = textRevealDuration;
        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            textCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);

            int charsToShow = Mathf.FloorToInt((elapsed / fadeInTime) * fullText.Length);
            lovecraftTextUI.maxVisibleCharacters = charsToShow;
            lovecraftTextUI.text = fullText.Substring(0, Mathf.Min(charsToShow, fullText.Length));

            elapsed += Time.deltaTime;
            yield return null;
        }

        lovecraftTextUI.text = fullText;
        lovecraftTextUI.maxVisibleCharacters = fullText.Length;
        textCanvasGroup.alpha = 1f;

        Debug.Log("✅ Yazı tamamen gösterildi");
    }

    IEnumerator CameraShake()
    {
        float elapsed = 0f;
        float currentIntensity = shakeIntensity;

        while (elapsed < shakeDuration && mainCamera != null)
        {
            float x = Mathf.PerlinNoise(Time.time * shakeFrequency, 0) * 2f - 1f;
            float y = Mathf.PerlinNoise(0, Time.time * shakeFrequency) * 2f - 1f;
            float z = Mathf.PerlinNoise(Time.time * shakeFrequency * 0.5f, Time.time * shakeFrequency * 0.5f) * 2f - 1f;

            Vector3 shakeOffset = new Vector3(x, y, z) * currentIntensity;
            mainCamera.transform.localPosition = cameraOriginalLocalPos + shakeOffset;

            elapsed += Time.deltaTime;
            currentIntensity = Mathf.Lerp(shakeIntensity, 0f, elapsed / shakeDuration);

            yield return null;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = cameraOriginalLocalPos;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (fadeImage != null)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
        }

        Debug.Log("🌑 Ekran tamamen karardı");
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
            nextSceneName = "Level1";

        if (IsSceneInBuild(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"❌ {nextSceneName} sahnesi Build Settings'de yok!");
        }
    }

    bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    void OnDestroy()
    {
        if (textCanvasObj != null)
            Destroy(textCanvasObj);

        GameObject fadeCanvas = GameObject.Find("FadeCanvas");
        if (fadeCanvas != null)
            Destroy(fadeCanvas);
    }
}