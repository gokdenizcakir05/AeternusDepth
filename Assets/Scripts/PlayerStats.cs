using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [System.Serializable]
    public class RewardItem
    {
        public string rewardName;
        public string description;
        public Sprite icon;
        public RewardType type;
        public int value;
        public float floatValue;
        public GameObject physicalPrefab;
    }

    public enum RewardType
    {
        Health,
        Mana,
        Gold,
        Experience,
        MovementSpeed,
        AttackSpeed,
        BulletSpeed,
        Damage,
        SpecialItem
    }

    [Header("Stat Bonuses")]
    public float movementSpeedBonus = 0f;
    public float attackSpeedBonus = 0f;
    public float bulletSpeedBonus = 0f;
    public float damageBonus = 0f;
    public int maxHealthBonus = 0;

    [Header("Current Bonuses (Readonly)")]
    [SerializeField] private float _currentMovementSpeedBonus = 0f;
    [SerializeField] private float _currentAttackSpeedBonus = 0f;
    [SerializeField] private float _currentBulletSpeedBonus = 0f;
    [SerializeField] private float _currentDamageBonus = 0f;

    [Header("Acquired Rewards")]
    [SerializeField] private List<string> acquiredRewards = new List<string>();

    public static PlayerStats Instance;

    public float CurrentMovementSpeedBonus => _currentMovementSpeedBonus;
    public float CurrentAttackSpeedBonus => _currentAttackSpeedBonus;
    public float CurrentBulletSpeedBonus => _currentBulletSpeedBonus;
    public float CurrentDamageBonus => _currentDamageBonus;

    // AWAKE METODU EKSİK! EKLEYELİM:
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad YOK - sadece Instance ataması
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log("✅ PlayerStats Awake çalıştı - Instance atandı");
    }

    void Start()
    {
        Debug.Log("🎯 PlayerStats Start çalıştı - Bonuslar sıfırlandı");
    }

    public void ApplyReward(RewardItem reward)
    {
        Debug.Log($"🔍 Ödül uygulanıyor: {reward.rewardName} - Value: {reward.value} - FloatValue: {reward.floatValue}");

        // Ödülü kazanılanlar listesine ekle
        string rewardDisplay = $"{reward.rewardName}";
        if (reward.value > 0) rewardDisplay += $" +{reward.value}";
        if (reward.floatValue > 0) rewardDisplay += $" +%{reward.floatValue}";

        acquiredRewards.Add(rewardDisplay);

        switch (reward.type)
        {
            case RewardType.Health:
                maxHealthBonus += reward.value;
                UpdatePlayerHealth(reward.value);
                Debug.Log($"💧 Maksimum Oksijen +{reward.value} (Toplam Bonus: {maxHealthBonus})");
                break;

            case RewardType.MovementSpeed:
                movementSpeedBonus += reward.floatValue;
                _currentMovementSpeedBonus = movementSpeedBonus;
                Debug.Log($"🏃 Hareket Hızı +%{reward.floatValue} (Toplam: %{movementSpeedBonus})");
                break;

            case RewardType.AttackSpeed:
                attackSpeedBonus += reward.floatValue;
                _currentAttackSpeedBonus = attackSpeedBonus;
                Debug.Log($"⚡ Saldırı Hızı +%{reward.floatValue} (Toplam: %{attackSpeedBonus})");
                break;

            case RewardType.BulletSpeed:
                bulletSpeedBonus += reward.floatValue;
                _currentBulletSpeedBonus = bulletSpeedBonus;
                Debug.Log($"💨 Mermi Hızı +%{reward.floatValue} (Toplam: %{bulletSpeedBonus})");
                break;

            case RewardType.Damage:
                damageBonus += reward.floatValue;
                _currentDamageBonus = damageBonus;
                Debug.Log($"💥 Hasar +%{reward.floatValue} (Toplam: %{damageBonus}, Çarpan: {GetDamageMultiplier()})");
                break;
        }

        DebugStats();
    }

    public void UpdatePlayerHealth(int newBonus)
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.AddMaxHealth(newBonus);
            Debug.Log($"❤️ PlayerHealth'e +{newBonus} bonus can eklendi");
        }
        else
        {
            Debug.LogWarning("❌ PlayerHealth bulunamadı!");
        }
    }

    public float GetMovementSpeedMultiplier()
    {
        return 1 + movementSpeedBonus / 100f;
    }

    public float GetAttackSpeedMultiplier()
    {
        return 1 + attackSpeedBonus / 100f;
    }

    public float GetBulletSpeedMultiplier()
    {
        return 1 + bulletSpeedBonus / 100f;
    }

    public float GetDamageMultiplier()
    {
        return 1 + damageBonus / 100f;
    }

    public int GetTotalMaxHealth()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            return playerHealth.maxHealth + maxHealthBonus;
        }
        return 100 + maxHealthBonus;
    }

    // ESC Menüsü için metodlar
    public List<string> GetAllAcquiredRewards()
    {
        if (acquiredRewards == null)
        {
            acquiredRewards = new List<string>();
        }
        return acquiredRewards;
    }

    public string GetTotalStatsSummary()
    {
        string summary = "TOPLAM İSTATİSTİKLER\n\n";
        summary += $"🏃 Hareket Hızı: +%{movementSpeedBonus:F1}\n\n";
        summary += $"⚡ Saldırı Hızı: +%{attackSpeedBonus:F1}\n\n";
        summary += $"💨 Mermi Hızı: +%{bulletSpeedBonus:F1}\n\n";
        summary += $"💥 Hasar: +%{damageBonus:F1}\n\n";
        summary += $"💧 Oksijen Bonusu: +{maxHealthBonus}";

        return summary;
    }

    public void DebugStats()
    {
        Debug.Log($"🎯 PLAYER STATS DEBUG:");
        Debug.Log($"💥 Hasar Bonusu: %{damageBonus} (Çarpan: {GetDamageMultiplier()})");
        Debug.Log($"💨 Mermi Hızı Bonusu: %{bulletSpeedBonus} (Çarpan: {GetBulletSpeedMultiplier()})");
        Debug.Log($"⚡ Saldırı Hızı Bonusu: %{attackSpeedBonus} (Çarpan: {GetAttackSpeedMultiplier()})");
        Debug.Log($"🏃 Hareket Hızı Bonusu: %{movementSpeedBonus} (Çarpan: {GetMovementSpeedMultiplier()})");
        Debug.Log($"💧 Oksijen Bonusu: +{maxHealthBonus}");
        Debug.Log($"📊 Toplam Kazanılan Ödül: {acquiredRewards.Count}");
    }

    // Reset metodu - restart için
    public void ResetStats()
    {
        movementSpeedBonus = 0f;
        attackSpeedBonus = 0f;
        bulletSpeedBonus = 0f;
        damageBonus = 0f;
        maxHealthBonus = 0;
        acquiredRewards.Clear();

        Debug.Log("🔄 PlayerStats resetlendi");
    }
}