using UnityEngine;
using TMPro;

public class InteractUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactText;

    [Header("Settings")]
    public string defaultText = "E Basýnýz";

    private InteractableObject currentInteractable;
    private bool isUIVisible = false;

    public static InteractUIManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideInteractUI();
    }

    public void ShowInteractUI(InteractableObject interactable, string customText = "")
    {
        // ÖNCE HER ZAMAN GÝZLE
        HideInteractUI();

        if (interactable == null) return;

        currentInteractable = interactable;

        if (interactPanel != null)
        {
            interactPanel.SetActive(true);
            isUIVisible = true;

            if (interactText != null)
            {
                interactText.text = string.IsNullOrEmpty(customText) ? defaultText : customText;
            }
        }
    }

    public void HideInteractUI()
    {
        if (interactPanel != null && interactPanel.activeSelf)
        {
            interactPanel.SetActive(false);
            isUIVisible = false;
            currentInteractable = null;
        }
    }

    // BU METODU EKLE!
    public void HideInteractUIForObject(InteractableObject interactable)
    {
        if (currentInteractable == interactable)
        {
            HideInteractUI();
        }
    }

    void Update()
    {
        if (isUIVisible && Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.OnInteract();
            HideInteractUI();
        }
    }

    public bool IsUIVisible()
    {
        return isUIVisible;
    }

    public InteractableObject GetCurrentInteractable()
    {
        return currentInteractable;
    }
}