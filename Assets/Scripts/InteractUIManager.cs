using UnityEngine;
using TMPro;

public class InteractUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactText;

    // SABÝT METÝN - DEÐÝÞTÝRÝLEMEZ
    private const string INTERACT_TEXT = "Press E";

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
        // BAÞLANGIÇTA METNÝ AYARLA
        if (interactText != null)
        {
            interactText.text = INTERACT_TEXT;
        }
        HideInteractUI();
    }

    public void ShowInteractUI(InteractableObject interactable, string customText = "")
    {
        if (interactable == null) return;

        currentInteractable = interactable;

        if (interactPanel != null)
        {
            interactPanel.SetActive(true);
            isUIVisible = true;

            if (interactText != null)
            {
                // HER ZAMAN AYNI METÝN
                interactText.text = INTERACT_TEXT;
                Debug.Log("Text set to: " + interactText.text);
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
}