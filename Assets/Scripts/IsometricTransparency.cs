using UnityEngine;
using System.Collections;

public class IsometricTransparency : MonoBehaviour
{
    [Header("Transparency Settings")]
    [Range(0.1f, 1f)] public float transparencyAlpha = 0.3f; // 1'e kadar ayarlanabilir
    public float checkInterval = 0.1f;

    [Header("Debug")]
    public bool showDebug = true;

    private Transform player;
    private Camera mainCamera;
    private Renderer objectRenderer;
    private bool isTransparent = false;

    void Start()
    {
        // Player'ı bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Camera'yı bul
        mainCamera = Camera.main;

        // Renderer'ı bul
        objectRenderer = GetComponent<Renderer>();

        // Kontrolü başlat
        if (player != null && mainCamera != null && objectRenderer != null)
        {
            StartCoroutine(TransparencyCheck());
            Debug.Log("✅ Transparency sistemi başlatıldı!");
        }
    }

    IEnumerator TransparencyCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckTransparency();
        }
    }

    void CheckTransparency()
    {
        if (player == null || mainCamera == null || objectRenderer == null) return;

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 playerPos = player.position;
        Vector3 direction = (playerPos - cameraPos).normalized;
        float distance = Vector3.Distance(cameraPos, playerPos);

        // RAYCAST YAP
        RaycastHit[] hits = Physics.RaycastAll(cameraPos, direction, distance);

        bool shouldBeTransparent = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject || IsInChildren(hit.collider.transform))
            {
                shouldBeTransparent = true;
                if (showDebug) Debug.Log($"🟢 {gameObject.name} HIT EDİLDİ! Transparent olacak.");
                break;
            }
        }

        // Transparency durumunu güncelle
        if (shouldBeTransparent && !isTransparent)
        {
            SetTransparency(true);
        }
        else if (!shouldBeTransparent && isTransparent)
        {
            SetTransparency(false);
        }

        // Debug çizgisi
        if (showDebug)
        {
            Debug.DrawRay(cameraPos, direction * distance, shouldBeTransparent ? Color.red : Color.green, checkInterval);
        }
    }

    bool IsInChildren(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.gameObject == gameObject)
                return true;
            current = current.parent;
        }
        return false;
    }

    void SetTransparency(bool transparent)
    {
        if (isTransparent == transparent) return;

        isTransparent = transparent;

        if (transparent)
        {
            // MATERIAL'IN SHADER'INI DEĞİŞTİR
            MakeMaterialTransparent();

            if (showDebug) Debug.Log($"🔵 {gameObject.name} TRANSPARANT YAPILDI (Alpha: {transparencyAlpha})");
        }
        else
        {
            // MATERIAL'I RESETLE
            ResetMaterial();

            if (showDebug) Debug.Log($"⚪ {gameObject.name} NORMALE DÖNDÜ");
        }
    }

    void MakeMaterialTransparent()
    {
        if (objectRenderer == null) return;

        // MEVCUT MATERIAL'ı AL
        Material mat = objectRenderer.material;

        // SHADER MODE'UNU TRANSPARENT YAP
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 2); // Fade mode = 2
        }

        // RENDERING MODE'U DEĞİŞTİR
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        // KEYWORD'LERİ AYARLA
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        // RENDER QUEUE'YU AYARLA
        mat.renderQueue = 3000;

        // ALPHA DEĞERİNİ AYARLA (İSTEDİĞİN SAYIYI KULLAN)
        if (mat.HasProperty("_Color"))
        {
            Color color = mat.color;
            color.a = transparencyAlpha; // BU DEĞERİ DEĞİŞTİREBİLİRSİN
            mat.color = color;
        }

        // MATERIAL'ı GÜNCELLE
        objectRenderer.material = mat;
    }

    void ResetMaterial()
    {
        if (objectRenderer == null) return;

        Material mat = objectRenderer.material;

        // OPAQUE MODA DÖN
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 0); // Opaque mode = 0
        }

        // BLEND MODE'U RESETLE
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);

        // KEYWORD'LERİ RESETLE
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        // RENDER QUEUE'YU RESETLE
        mat.renderQueue = -1;

        // ALPHA'YI 1 YAP
        if (mat.HasProperty("_Color"))
        {
            Color color = mat.color;
            color.a = 1f;
            mat.color = color;
        }

        objectRenderer.material = mat;
    }

    void OnDestroy()
    {
        // Material'ı orijinal haline döndür
        if (!isTransparent)
        {
            ResetMaterial();
        }
    }
}