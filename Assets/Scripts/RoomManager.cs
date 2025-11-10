using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RoomManager : MonoBehaviour
{
    [Header("=== ODA AYARLARI ===")]
    public string roomName = "Oda 1";
    public GameObject guaranteedChestPrefab;

    [Header("=== ODA DÜŞMAN SPAWN AYARLARI ===")]
    public bool useCustomSpawnSettings = false;
    public CharacterSpawnInfo[] roomSpecificSpawns;

    [Header("=== ODA ALANI ===")]
    public float roomRadius = 50f;
    public bool showGizmos = true;

    [Header("=== SPAWN MANAGER ENTEGRASYONU ===")]
    public RandomSpawnManager spawnManager;
    public float spawnCheckInterval = 3f;
    public bool waitForSpawn = true;

    [Header("=== CHEST SPAWN AYARLARI ===")]
    public Vector3 chestSpawnPosition = new Vector3(0, 0.1f, 0);
    public Vector3 chestSpawnRotation = new Vector3(0, 0, 0);

    [Header("=== DEBUG ===")]
    public bool showDebug = true;

    private bool chestSpawned = false;
    private bool roomActive = false;
    private List<GameObject> roomEnemies = new List<GameObject>();
    private Coroutine spawnCheckCoroutine;

    void Start()
    {
        Debug.Log($"🎯 {roomName} başlatılıyor...");

        // CRITICAL FIX: Enemy ölüm event'ini dinle
        EnemyHealth.OnEnemyDeath += OnEnemyDied;

        if (waitForSpawn && spawnManager != null)
        {
            Debug.Log($"⏳ SpawnManager bekleniyor...");
            spawnCheckCoroutine = StartCoroutine(WaitForSpawnManager());
        }
        else
        {
            InitializeRoom();
        }
    }

    void OnDestroy()
    {
        // Event bağlantısını temizle
        EnemyHealth.OnEnemyDeath -= OnEnemyDied;
    }

    // YENİ METOD: Düşman öldüğünde çağrılır
    void OnEnemyDied(GameObject enemy)
    {
        if (roomEnemies.Contains(enemy))
        {
            if (showDebug) Debug.Log($"✅ {enemy.name} ölümü algılandı! Kalan: {CountAliveEnemies()}");

            // Canlı düşman kontrolü
            if (roomActive && CountAliveEnemies() == 0)
            {
                Debug.Log($"🎯 {roomName}: TÜM DÜŞMANLAR ÖLDÜ! CHEST GELİYOR...");
                SpawnChest();
                chestSpawned = true;
            }
        }
    }

    IEnumerator WaitForSpawnManager()
    {
        yield return new WaitForSeconds(2f); // Başlangıç bekleme

        int maxAttempts = 10;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            attempt++;
            FindEnemiesInRoom();

            if (roomEnemies.Count > 0)
            {
                Debug.Log($"✅ {roomEnemies.Count} düşman bulundu! Oda aktif.");
                roomActive = true;
                yield break;
            }

            if (showDebug) Debug.Log($"🔍 Düşman aranıyor... Deneme {attempt}/{maxAttempts}");
            yield return new WaitForSeconds(spawnCheckInterval);
        }

        Debug.LogWarning($"⚠️ {roomName}'de zaman aşımı! Düşman bulunamadı.");
        FindEnemiesInRoom(); // Son bir deneme
    }

    void FindEnemiesInRoom()
    {
        roomEnemies.Clear();

        // 1. YÖNTEM: SpawnManager'dan düşmanları al
        if (spawnManager != null)
        {
            List<GameObject> spawnedCharacters = spawnManager.GetSpawnedCharacters();
            foreach (GameObject character in spawnedCharacters)
            {
                if (character != null && character.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, character.transform.position);
                    if (distance <= roomRadius)
                    {
                        roomEnemies.Add(character);
                        if (showDebug) Debug.Log($"✅ {character.name} SpawnManager'dan eklendi ({distance:F1}m)");
                    }
                }
            }
        }

        // 2. YÖNTEM: Sahnedeki tüm Enemy tag'li objeleri bul
        if (roomEnemies.Count == 0)
        {
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy != null && !roomEnemies.Contains(enemy))
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance <= roomRadius)
                    {
                        roomEnemies.Add(enemy);
                        if (showDebug) Debug.Log($"✅ {enemy.name} sahneden eklendi ({distance:F1}m)");
                    }
                }
            }
        }

        Debug.Log($"🎯 {roomName} düşman sayısı: {roomEnemies.Count}");
    }

    void InitializeRoom()
    {
        FindEnemiesInRoom();
        roomActive = roomEnemies.Count > 0;

        if (roomEnemies.Count == 0)
        {
            Debug.LogWarning($"⚠️ {roomName}'de hiç düşman yok!");
        }
    }

    void Update()
    {
        if (!roomActive || chestSpawned) return;

        // Null düşmanları temizle
        roomEnemies.RemoveAll(enemy => enemy == null);

        // Manuel kontrol (yedek)
        if (roomEnemies.Count > 0 && CountAliveEnemies() == 0)
        {
            Debug.Log($"🎯 {roomName}: TÜM DÜŞMANLAR ÖLDÜ! CHEST GELİYOR...");
            SpawnChest();
            chestSpawned = true;
        }
    }

    int CountAliveEnemies()
    {
        int aliveCount = 0;
        foreach (GameObject enemy in roomEnemies)
        {
            if (enemy != null)
            {
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null && !health.IsDead())
                {
                    aliveCount++;
                }
                else if (health == null)
                {
                    // EnemyHealth yoksa obje canlı say
                    aliveCount++;
                }
            }
        }
        return aliveCount;
    }

    void SpawnChest()
    {
        if (guaranteedChestPrefab != null)
        {
            Vector3 spawnPos = transform.position + chestSpawnPosition;
            Quaternion spawnRot = Quaternion.Euler(chestSpawnRotation);

            GameObject chest = Instantiate(guaranteedChestPrefab, spawnPos, spawnRot);

            Debug.Log($"🎁 {roomName} TEMİZLENDİ! CHEST DÜŞTÜ! 🎉");
            Debug.Log($"📍 Pozisyon: {spawnPos}");
        }
        else
        {
            Debug.LogError("❌ Chest prefab'ı atanmamış!");
        }
    }

    [ContextMenu("🔍 ODA DURUMUNU KONTROL ET")]
    public void CheckRoomStatus()
    {
        Debug.Log($"=== {roomName} DURUMU ===");
        Debug.Log($"📍 Oda Merkezi: {transform.position}");
        Debug.Log($"📏 Oda Yarıçapı: {roomRadius}");
        Debug.Log($"🎯 Chest Spawned: {chestSpawned}");
        Debug.Log($"🎯 Oda Aktif: {roomActive}");
        Debug.Log($"👹 Toplam Düşman: {roomEnemies.Count}");
        Debug.Log($"❤️ Canlı Düşman: {CountAliveEnemies()}");

        foreach (GameObject enemy in roomEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();

                if (health != null)
                {
                    bool isDead = health.IsDead();
                    Debug.Log($"{(isDead ? "💀" : "❤️")} {enemy.name} - {(isDead ? "ÖLÜ" : "CANLI")} - Mesafe: {distance:F1}m - Health: {health.GetCurrentHealth()}");
                }
                else
                {
                    Debug.Log($"❌ {enemy.name} - EnemyHealth YOK! - Mesafe: {distance:F1}m");
                }
            }
            else
            {
                Debug.Log($"💀 NULL - ÖLÜ");
            }
        }
    }

    [ContextMenu("🔄 DÜŞMANLARI YENİDEN TARA")]
    public void RescanEnemies()
    {
        Debug.Log("🔄 Düşmanlar yeniden taranıyor...");
        FindEnemiesInRoom();
        Debug.Log($"🎯 Tarama tamamlandı. Toplam: {roomEnemies.Count} düşman");
    }

    public CharacterSpawnInfo[] GetRoomSpawnSettings()
    {
        if (useCustomSpawnSettings && roomSpecificSpawns != null)
        {
            return roomSpecificSpawns;
        }
        return null;
    }

    public int GetTotalEnemiesForRoom()
    {
        int total = 0;
        if (useCustomSpawnSettings && roomSpecificSpawns != null)
        {
            foreach (CharacterSpawnInfo spawnInfo in roomSpecificSpawns)
            {
                total += spawnInfo.spawnCount;
            }
        }
        return total;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Oda alanı
        Gizmos.color = chestSpawned ? Color.green : (roomActive ? Color.yellow : Color.red);
        Gizmos.DrawWireSphere(transform.position, roomRadius);

        // Chest spawn noktası
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + chestSpawnPosition, Vector3.one * 0.5f);
    }
}