using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rewardPanel;
    public Image leftIcon;
    public Image rightIcon;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI leftDescription;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI rightDescription;
    public Button leftButton;
    public Button rightButton;

    [Header("Reward Settings")]
    public List<PlayerStats.RewardItem> rewardPool = new List<PlayerStats.RewardItem>();

    private PlayerStats.RewardItem leftReward;
    private PlayerStats.RewardItem rightReward;
    private System.Action<PlayerStats.RewardItem> onRewardSelected;
    private bool isRewardPanelOpen = false;

    // ESC menü kontrolü için
    private ESCMenu escMenu;
    private bool wasCursorVisible;
    private CursorLockMode previousCursorLockState;
    private float previousTimeScale;

    void Start()
    {
        if (rewardPanel == null) Debug.LogError("RewardPanel referansı bağlanmamış!");
        if (leftIcon == null) Debug.LogError("LeftIcon referansı bağlanmamış!");
        if (rightIcon == null) Debug.LogError("RightIcon referansı bağlanmamış!");
        if (leftText == null) Debug.LogError("LeftText referansı bağlanmamış!");
        if (rightText == null) Debug.LogError("RightText referansı bağlanmamış!");
        if (leftButton == null) Debug.LogError("LeftButton referansı bağlanmamış!");
        if (rightButton == null) Debug.LogError("RightButton referansı bağlanmamış!");

        escMenu = FindObjectOfType<ESCMenu>();
        if (escMenu == null) Debug.LogWarning("ESCMenu bulunamadı!");

        rewardPanel.SetActive(false);
        leftButton.onClick.AddListener(() => SelectReward(leftReward));
        rightButton.onClick.AddListener(() => SelectReward(rightReward));
    }

    void Update()
    {
        if (isRewardPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC: Reward paneli açık, ESC menü engellendi");
            return;
        }
    }

    public void ShowRewardSelection(System.Action<PlayerStats.RewardItem> callback)
    {
        if (rewardPool.Count == 0)
        {
            Debug.LogError("RewardPool boş! En az 1 ödül ekleyin.");
            return;
        }

        // TIMER'I DURDUR
        if (GameTimer.Instance != null)
            GameTimer.Instance.PauseTimer();

        if (escMenu != null && escMenu.isMenuOpen)
        {
            escMenu.ToggleMenu();
        }

        GetTwoRandomRewards();
        onRewardSelected = callback;
        UpdateRewardUI();

        wasCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        rewardPanel.SetActive(true);
        isRewardPanelOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("🎁 Reward paneli açıldı - Zaman durduruldu");
    }

    void GetTwoRandomRewards()
    {
        if (rewardPool.Count < 2)
        {
            Debug.LogWarning("Sadece 1 ödül var, aynı ödül iki kere gösterilecek.");
            leftReward = rewardPool[0];
            rightReward = rewardPool[0];
            return;
        }

        int randomIndex1 = Random.Range(0, rewardPool.Count);
        leftReward = rewardPool[randomIndex1];

        int randomIndex2;
        do
        {
            randomIndex2 = Random.Range(0, rewardPool.Count);
        } while (randomIndex2 == randomIndex1 && rewardPool.Count > 1);

        rightReward = rewardPool[randomIndex2];
    }

    void UpdateRewardUI()
    {
        if (leftReward == null || rightReward == null)
        {
            Debug.LogError("Ödüller null! Left: " + leftReward + " Right: " + rightReward);
            return;
        }

        if (leftIcon != null) leftIcon.sprite = leftReward.icon;
        if (leftText != null) leftText.text = leftReward.rewardName;
        if (leftDescription != null) leftDescription.text = GetRewardDescription(leftReward);

        if (rightIcon != null) rightIcon.sprite = rightReward.icon;
        if (rightText != null) rightText.text = rightReward.rewardName;
        if (rightDescription != null) rightDescription.text = GetRewardDescription(rightReward);

        Debug.Log("UI güncellendi: " + leftReward.rewardName + " - " + rightReward.rewardName);
    }

    string GetRewardDescription(PlayerStats.RewardItem reward)
    {
        if (reward == null) return "NULL ÖDÜL";

        switch (reward.type)
        {
            case PlayerStats.RewardType.Health:
                return $"+{reward.value} Maksimum Oksijen";
            case PlayerStats.RewardType.Mana:
                return $"+{reward.value} Maksimum Mana";
            case PlayerStats.RewardType.Gold:
                return $"+{reward.value} Altın";
            case PlayerStats.RewardType.Experience:
                return $"+{reward.value} EXP";
            case PlayerStats.RewardType.MovementSpeed:
                return $"+%{reward.floatValue} Hareket Hızı";
            case PlayerStats.RewardType.AttackSpeed:
                return $"+%{reward.floatValue} Saldırı Hızı";
            case PlayerStats.RewardType.BulletSpeed:
                return $"+%{reward.floatValue} Mermi Hızı";
            case PlayerStats.RewardType.Damage:
                return $"+%{reward.floatValue} Hasar";
            case PlayerStats.RewardType.SpecialItem:
                return reward.description;
            default:
                return reward.description;
        }
    }

    void SelectReward(PlayerStats.RewardItem selectedReward)
    {
        if (selectedReward == null)
        {
            Debug.LogError("Seçilen ödül null!");
            return;
        }

        onRewardSelected?.Invoke(selectedReward);
        CloseRewardPanel();
        Debug.Log($"🎁 Ödül seçildi: {selectedReward.rewardName}");
    }

    void CloseRewardPanel()
    {
        rewardPanel.SetActive(false);
        isRewardPanelOpen = false;

        // TIMER'I DEVAM ETTIR
        if (GameTimer.Instance != null)
            GameTimer.Instance.ResumeTimer();

        Time.timeScale = previousTimeScale;

        Cursor.visible = wasCursorVisible;
        Cursor.lockState = previousCursorLockState;

        Debug.Log("🎁 Reward paneli kapandı - Zaman normale döndü");
    }

    public void HideRewardPanel()
    {
        CloseRewardPanel();
    }

    public bool IsRewardPanelOpen()
    {
        return isRewardPanelOpen;
    }
}