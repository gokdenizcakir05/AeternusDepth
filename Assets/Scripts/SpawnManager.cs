using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("Boss Arena Spawn Point")]
    public Vector3 bossSpawnPosition = new Vector3(0, 1, 0);
    public Vector3 bossSpawnRotation = Vector3.zero;

    [Header("Health Settings")]
    public bool fullHealthOnBossArena = true; // Boss arenasına geçince can fullensin

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BossArena")
        {
            Debug.Log("🔄 BossArena sahnesi yüklendi, spawn ve can yenileme hazırlanıyor...");
            Invoke("SpawnAndHealPlayer", 0.1f);
        }
    }

    void SpawnAndHealPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 1. ÖNCE POZİSYON AYARLA
            player.transform.position = bossSpawnPosition;
            player.transform.eulerAngles = bossSpawnRotation;

            // 2. FİZİK SIFIRLA
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. CANI FULLE - SENİN KODUNDAKİ METODU KULLANIYORUM
            if (fullHealthOnBossArena)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RestoreFullHealth(); // ✅ SENİN METODUN BU
                    Debug.Log("❤️️ BOSS ARENASI - Can tamamen fullendi: " +
                             playerHealth.currentHealth + "/" + playerHealth.maxHealth);
                }
                else
                {
                    Debug.LogWarning("⚠️ PlayerHealth component'i bulunamadı!");
                }
            }

            Debug.Log($"✅ Player {bossSpawnPosition} pozisyonuna yerleştirildi ve can fullendi!");
        }
        else
        {
            Debug.LogError("❌ Player bulunamadı! 2. deneme yapılıyor...");
            Invoke("SecondSpawnAttempt", 0.5f);
        }
    }

    void SecondSpawnAttempt()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = bossSpawnPosition;
            player.transform.eulerAngles = bossSpawnRotation;

            // 2. denemede de can fullensin
            if (fullHealthOnBossArena)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RestoreFullHealth(); // ✅ SENİN METODUN BU
                    Debug.Log("❤️️ 2. DENEME - Can fullendi!");
                }
            }

            Debug.Log($"✅ Player 2. denemede spawn edildi ve can fullendi!");
        }
        else
        {
            Debug.LogError("❌❌ Player hala bulunamadı!");
        }
    }

    // Debug için - F11 ile manuel can fullleme
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            FullHealthManually();
        }
    }

    public void FullHealthManually()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth(); // ✅ SENİN METODUN BU
                Debug.Log("🔧 Manuel can fullendi!");
            }
        }
    }
}