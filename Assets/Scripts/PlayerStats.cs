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

    // AWAKE METHOD WAS MISSING! LET'S ADD IT:
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad - only Instance assignment
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log("✅ PlayerStats Awake executed - Instance assigned");
    }

    void Start()
    {
        Debug.Log("🎯 PlayerStats Start executed - Bonuses reset to zero");
    }

    public void ApplyReward(RewardItem reward)
    {
        Debug.Log($"🔍 Applying reward: {reward.rewardName} - Value: {reward.value} - FloatValue: {reward.floatValue}");

        // Add reward to acquired list
        string rewardDisplay = $"{reward.rewardName}";
        if (reward.value > 0) rewardDisplay += $" +{reward.value}";
        if (reward.floatValue > 0) rewardDisplay += $" +%{reward.floatValue}";

        acquiredRewards.Add(rewardDisplay);

        switch (reward.type)
        {
            case RewardType.Health:
                maxHealthBonus += reward.value;
                UpdatePlayerHealth(reward.value);
                Debug.Log($"💧 Maximum Oxygen +{reward.value} (Total Bonus: {maxHealthBonus})");
                break;

            case RewardType.MovementSpeed:
                movementSpeedBonus += reward.floatValue;
                _currentMovementSpeedBonus = movementSpeedBonus;
                Debug.Log($"🏃 Movement Speed +%{reward.floatValue} (Total: %{movementSpeedBonus})");
                break;

            case RewardType.AttackSpeed:
                attackSpeedBonus += reward.floatValue;
                _currentAttackSpeedBonus = attackSpeedBonus;
                Debug.Log($"⚡ Attack Speed +%{reward.floatValue} (Total: %{attackSpeedBonus})");
                break;

            case RewardType.BulletSpeed:
                bulletSpeedBonus += reward.floatValue;
                _currentBulletSpeedBonus = bulletSpeedBonus;
                Debug.Log($"💨 Bullet Speed +%{reward.floatValue} (Total: %{bulletSpeedBonus})");
                break;

            case RewardType.Damage:
                damageBonus += reward.floatValue;
                _currentDamageBonus = damageBonus;
                Debug.Log($"💥 Damage +%{reward.floatValue} (Total: %{damageBonus}, Multiplier: {GetDamageMultiplier()})");
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
            Debug.Log($"❤️ +{newBonus} bonus health added to PlayerHealth");
        }
        else
        {
            Debug.LogWarning("❌ PlayerHealth not found!");
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

    // ESC Menu methods
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
        string summary = "TOTAL STATISTICS\n\n";
        summary += $"🏃 Movement Speed: +%{movementSpeedBonus:F1}\n\n";
        summary += $"⚡ Attack Speed: +%{attackSpeedBonus:F1}\n\n";
        summary += $"💨 Bullet Speed: +%{bulletSpeedBonus:F1}\n\n";
        summary += $"💥 Damage: +%{damageBonus:F1}\n\n";
        summary += $"💧 Oxygen Bonus: +{maxHealthBonus}";

        return summary;
    }

    public void DebugStats()
    {
        Debug.Log($"🎯 PLAYER STATS DEBUG:");
        Debug.Log($"💥 Damage Bonus: %{damageBonus} (Multiplier: {GetDamageMultiplier()})");
        Debug.Log($"💨 Bullet Speed Bonus: %{bulletSpeedBonus} (Multiplier: {GetBulletSpeedMultiplier()})");
        Debug.Log($"⚡ Attack Speed Bonus: %{attackSpeedBonus} (Multiplier: {GetAttackSpeedMultiplier()})");
        Debug.Log($"🏃 Movement Speed Bonus: %{movementSpeedBonus} (Multiplier: {GetMovementSpeedMultiplier()})");
        Debug.Log($"💧 Oxygen Bonus: +{maxHealthBonus}");
        Debug.Log($"📊 Total Acquired Rewards: {acquiredRewards.Count}");
    }

    // Reset method - for restart
    public void ResetStats()
    {
        movementSpeedBonus = 0f;
        attackSpeedBonus = 0f;
        bulletSpeedBonus = 0f;
        damageBonus = 0f;
        maxHealthBonus = 0;
        acquiredRewards.Clear();

        Debug.Log("🔄 PlayerStats reset");
    }
}