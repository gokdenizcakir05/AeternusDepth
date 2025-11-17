using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public string interactText = "E Basınız";
    public bool showDebug = true;

    private Transform player;
    private bool isPlayerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null && showDebug)
            Debug.LogError($"❌ {gameObject.name}: Player bulunamadı!");
    }

    void Update()
    {
        if (player == null)
        {
            if (showDebug) Debug.LogWarning($"⚠️ {gameObject.name}: Player null!");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool nowInRange = distance <= interactionRange;

        // Menzil değişikliği
        if (nowInRange != isPlayerInRange)
        {
            isPlayerInRange = nowInRange;

            if (isPlayerInRange)
            {
                // UI göster
                if (InteractUIManager.Instance != null)
                {
                    InteractUIManager.Instance.ShowInteractUI(this, interactText);
                    if (showDebug) Debug.Log($"✅ {gameObject.name}: UI GÖSTERİLDİ (Menzil: {distance:F1}m)");
                }
            }
            else
            {
                // UI gizle
                if (InteractUIManager.Instance != null)
                {
                    InteractUIManager.Instance.HideInteractUIForObject(this);
                    if (showDebug) Debug.Log($"❌ {gameObject.name}: UI GİZLENDİ (Menzil: {distance:F1}m)");
                }
            }
        }
    }

    public virtual void OnInteract()
    {
        Debug.Log($"🎯 Etkileşim: {gameObject.name}");

        // Etkileşimden sonra UI'ı gizle
        if (InteractUIManager.Instance != null)
        {
            InteractUIManager.Instance.HideInteractUI();
        }
    }

    void OnDestroy()
    {
        // Obje yok olursa UI'ı temizle
        if (isPlayerInRange && InteractUIManager.Instance != null)
        {
            InteractUIManager.Instance.HideInteractUIForObject(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}