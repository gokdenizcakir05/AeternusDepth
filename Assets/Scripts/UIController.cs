using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Hover Energy UI")]
    public Slider hoverEnergySlider;
    public Image hoverEnergyFill;
    public GameObject hoverEnergyPanel;
    public Text hoverEnergyText;

    [Header("Colors")]
    public Color fullEnergyColor = Color.green;
    public Color mediumEnergyColor = Color.yellow;
    public Color lowEnergyColor = Color.red;

    [Header("References")]
    public ybotController playerController;

    private void Start()
    {
        if (hoverEnergyPanel != null)
            hoverEnergyPanel.SetActive(true);

        if (playerController == null)
            playerController = FindObjectOfType<ybotController>();
    }

    private void Update()
    {
        if (playerController != null)
        {
            UpdateHoverEnergyUI();
        }
    }

    private void UpdateHoverEnergyUI()
    {
        // Enerji yüzdesini al
        float energyPercentage = playerController.GetHoverEnergyPercentage();
        bool isHovering = playerController.IsHovering();

        // Slider'ý güncelle
        if (hoverEnergySlider != null)
        {
            hoverEnergySlider.value = energyPercentage;

            // Renk güncelle
            if (hoverEnergyFill != null)
            {
                if (energyPercentage > 0.5f)
                    hoverEnergyFill.color = fullEnergyColor;
                else if (energyPercentage > 0.2f)
                    hoverEnergyFill.color = mediumEnergyColor;
                else
                    hoverEnergyFill.color = lowEnergyColor;
            }
        }

        // Text'i güncelle
        if (hoverEnergyText != null)
        {
            hoverEnergyText.text = $"Hover: {Mathf.RoundToInt(energyPercentage * 100)}%";

            // Hover modunda deðilken gri yap
            if (!isHovering && energyPercentage < 100)
            {
                hoverEnergyText.color = Color.gray;
            }
            else if (isHovering)
            {
                hoverEnergyText.color = Color.cyan;
            }
            else
            {
                hoverEnergyText.color = Color.white;
            }
        }

        // Hover modunda iken UI'ý daha görünür yap
        if (hoverEnergyPanel != null && hoverEnergyFill != null)
        {
            if (isHovering)
            {
                hoverEnergyFill.GetComponent<Image>().color = new Color(
                    hoverEnergyFill.color.r,
                    hoverEnergyFill.color.g,
                    hoverEnergyFill.color.b,
                    1f
                );
            }
            else
            {
                hoverEnergyFill.GetComponent<Image>().color = new Color(
                    hoverEnergyFill.color.r,
                    hoverEnergyFill.color.g,
                    hoverEnergyFill.color.b,
                    0.7f
                );
            }
        }
    }

    // Enerji yenilenirken yanýp sönme efekti
    public void FlashEnergyBar()
    {
        if (hoverEnergyPanel != null)
        {
            // Animasyon eklemek için burayý geniþletebilirsin
            // Örneðin: StartCoroutine(FlashCoroutine());
        }
    }
}