using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterSpawnInfo
{
    public GameObject characterPrefab;
    public int spawnCount = 1;
    [HideInInspector] public int spawnedCount = 0;
}

public class RandomSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public CharacterSpawnInfo[] characterSpawns;
    public float spawnRadius = 2f;
    public string groundTag = "Ground";
    public LayerMask groundLayer = 1;

    [Header("Room-Specific Spawning")]
    public bool prioritizeRoomSpawning = true;
    public RoomManager[] roomManagers;

    [Header("No Spawn Zones")]
    public string[] noSpawnTags = { "NoSpawnGround", "Koridor", "Path" };
    public string[] noSpawnNameKeywords = { "koridor", "path", "yol", "corridor" };

    [Header("Spawn Timing")]
    public float spawnDelay = 1f;

    [Header("Debug")]
    public bool showDebug = true;
    public bool showSpawnGizmos = true;

    private Dictionary<RoomManager, List<Vector3>> roomSpawnPoints = new Dictionary<RoomManager, List<Vector3>>();
    private List<Vector3> generalSpawnPoints = new List<Vector3>();
    private List<GameObject> spawnedCharacters = new List<GameObject>();
    private int totalCharactersToSpawn = 0;

    void Start()
    {
        CalculateTotalSpawnCount();
        Invoke("SpawnAllCharacters", spawnDelay);
    }

    void CalculateTotalSpawnCount()
    {
        totalCharactersToSpawn = 0;

        // ÖNCE ODA SPAWN'LARINI HESAPLA
        if (prioritizeRoomSpawning && roomManagers != null)
        {
            foreach (RoomManager room in roomManagers)
            {
                if (room != null && room.useCustomSpawnSettings && room.roomSpecificSpawns != null)
                {
                    foreach (CharacterSpawnInfo spawnInfo in room.roomSpecificSpawns)
                    {
                        totalCharactersToSpawn += spawnInfo.spawnCount;
                        spawnInfo.spawnedCount = 0;
                        if (showDebug) Debug.Log($"🎯 {room.roomName} - {spawnInfo.characterPrefab.name}: {spawnInfo.spawnCount} adet");
                    }
                }
            }
        }

        // SONRA GENEL SPAWN'LARI HESAPLA
        if (characterSpawns != null)
        {
            foreach (CharacterSpawnInfo spawnInfo in characterSpawns)
            {
                totalCharactersToSpawn += spawnInfo.spawnCount;
                spawnInfo.spawnedCount = 0;
            }
        }

        if (showDebug) Debug.Log($"🎯 TOPLAM SPAWN: {totalCharactersToSpawn} karakter");
    }

    void SpawnAllCharacters()
    {
        FindAllSpawnPoints();

        // ✅ HER ODA İÇİN SPAWN NOKTASI KONTROLÜ
        if (prioritizeRoomSpawning && roomManagers != null)
        {
            foreach (RoomManager room in roomManagers)
            {
                if (room != null && room.useCustomSpawnSettings && room.roomSpecificSpawns != null)
                {
                    if (!roomSpawnPoints.ContainsKey(room) || roomSpawnPoints[room].Count == 0)
                    {
                        Debug.LogError($"❌ {room.roomName} için spawn noktası bulunamadı!");
                        return;
                    }
                }
            }
        }

        // ✅ BASİT KONTROL: Spawn bilgisi var mı?
        bool hasSpawnInfo = (characterSpawns != null && characterSpawns.Length > 0) ||
                           (prioritizeRoomSpawning && roomManagers != null && roomManagers.Length > 0);

        if (!hasSpawnInfo)
        {
            Debug.LogError("Spawn: Karakter spawn bilgileri atanmamış!");
            return;
        }

        // ✅ SPAWN SİSTEMİ
        bool allSpawned = RoomBasedSpawnSystem();

        if (showDebug)
        {
            Debug.Log($"🎯 SPAWN TAMAMLANDI: {spawnedCharacters.Count}/{totalCharactersToSpawn} karakter");
            PrintSpawnSummary();
        }
    }

    // ✅ ODA BAZLI SPAWN SİSTEMİ
    bool RoomBasedSpawnSystem()
    {
        int totalSpawned = 0;

        // 1. ÖNCE ODA SPAWN'LARI - HER ODA KENDİ SPAWN NOKTALARINI KULLANSIN
        if (prioritizeRoomSpawning && roomManagers != null)
        {
            foreach (RoomManager room in roomManagers)
            {
                if (room != null && room.useCustomSpawnSettings && room.roomSpecificSpawns != null)
                {
                    if (!roomSpawnPoints.ContainsKey(room)) continue;

                    List<Vector3> currentRoomSpawnPoints = roomSpawnPoints[room];

                    if (showDebug) Debug.Log($"🎯 {room.roomName} SPAWN BAŞLIYOR... ({currentRoomSpawnPoints.Count} nokta)");

                    // ✅ BU ODA İÇİN TÜM KARAKTER TÜRLERİNİ SPAWNLA
                    foreach (CharacterSpawnInfo roomSpawnInfo in room.roomSpecificSpawns)
                    {
                        if (roomSpawnInfo.characterPrefab == null)
                        {
                            Debug.LogError($"❌ {room.roomName} - Prefab null!");
                            continue;
                        }

                        if (showDebug) Debug.Log($"🎯 {room.roomName} - {roomSpawnInfo.characterPrefab.name}: {roomSpawnInfo.spawnCount} adet spawnlanacak");

                        // ✅ BU KARAKTER TÜRÜNÜN TÜM ADETLERİNİ SPAWNLA
                        for (int i = 0; i < roomSpawnInfo.spawnCount; i++)
                        {
                            if (currentRoomSpawnPoints.Count > 0)
                            {
                                Vector3 spawnPoint = GetRandomSpawnPointFromList(currentRoomSpawnPoints);
                                if (spawnPoint != Vector3.zero)
                                {
                                    SpawnCharacterAtPosition(roomSpawnInfo.characterPrefab, spawnPoint, room.roomName);
                                    roomSpawnInfo.spawnedCount++;
                                    totalSpawned++;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"⚠️ {room.roomName} İÇİN SPAWN NOKTASI KALMADI! {roomSpawnInfo.characterPrefab.name} spawnlanamadı.");
                                break;
                            }
                        }
                    }
                }
            }
        }

        // 2. SONRA GENEL SPAWN'LAR - GENEL SPAWN NOKTALARINI KULLANSIN
        if (characterSpawns != null)
        {
            foreach (CharacterSpawnInfo spawnInfo in characterSpawns)
            {
                for (int i = 0; i < spawnInfo.spawnCount; i++)
                {
                    if (generalSpawnPoints.Count > 0)
                    {
                        Vector3 spawnPoint = GetRandomSpawnPointFromList(generalSpawnPoints);
                        if (spawnPoint != Vector3.zero)
                        {
                            SpawnCharacterAtPosition(spawnInfo.characterPrefab, spawnPoint, "Genel");
                            spawnInfo.spawnedCount++;
                            totalSpawned++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return totalSpawned >= totalCharactersToSpawn;
    }

    // ✅ ODA BAZLI SPAWN NOKTASI BULMA
    void FindAllSpawnPoints()
    {
        roomSpawnPoints.Clear();
        generalSpawnPoints.Clear();

        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag(groundTag);

        if (showDebug) Debug.Log($"🔍 Bulunan ground sayısı: {groundObjects.Length}");

        // ✅ ÖNCE HER ODA İÇİN SPAWN NOKTASI BUL
        if (prioritizeRoomSpawning && roomManagers != null)
        {
            foreach (RoomManager room in roomManagers)
            {
                if (room != null)
                {
                    List<Vector3> roomPoints = GetSpawnPointsForRoom(room);
                    roomSpawnPoints[room] = roomPoints;

                    if (showDebug) Debug.Log($"🎯 {room.roomName} için {roomPoints.Count} spawn noktası bulundu");
                }
            }
        }

        // ✅ SONRA GENEL SPAWN NOKTALARI BUL (odaya ait olmayanlar)
        foreach (GameObject ground in groundObjects)
        {
            if (IsNoSpawnByTag(ground) || IsNoSpawnByName(ground))
                continue;

            Bounds groundBounds = GetGroundBounds(ground);

            // ✅ BU GROUND'UN HERHANGİ BİR ODAYA AİT OLUP OLMADIĞINI KONTROL ET
            bool isInAnyRoom = false;
            if (prioritizeRoomSpawning && roomManagers != null)
            {
                foreach (RoomManager room in roomManagers)
                {
                    if (room != null)
                    {
                        float distance = Vector3.Distance(room.transform.position, groundBounds.center);
                        if (distance <= room.roomRadius)
                        {
                            isInAnyRoom = true;
                            break;
                        }
                    }
                }
            }

            // ✅ EĞER ODAYA AİT DEĞİLSE, GENEL SPAWN NOKTASI OLARAK EKLE
            if (!isInAnyRoom)
            {
                int pointsPerGround = Mathf.Max(5, totalCharactersToSpawn);

                for (int i = 0; i < pointsPerGround; i++)
                {
                    Vector3 randomPoint = GetRandomPointOnGround(groundBounds);
                    if (IsValidSpawnPoint(randomPoint))
                    {
                        generalSpawnPoints.Add(randomPoint);
                    }
                }
            }
        }

        if (showDebug) Debug.Log($"🌍 Genel için {generalSpawnPoints.Count} spawn noktası bulundu");

        // ✅ TOPLAM SPAWN NOKTASI KONTROLÜ
        int totalSpawnPoints = generalSpawnPoints.Count;
        foreach (var roomPoints in roomSpawnPoints.Values)
        {
            totalSpawnPoints += roomPoints.Count;
        }

        if (totalSpawnPoints == 0)
        {
            Debug.LogError("❌ HİÇ SPAWN NOKTASI BULUNAMADI!");
        }
        else
        {
            if (showDebug) Debug.Log($"✅ TOPLAM {totalSpawnPoints} SPAWN NOKTASI BULUNDU");
        }
    }

    // ✅ BELİRLİ BİR ODA İÇİN SPAWN NOKTASI BUL
    List<Vector3> GetSpawnPointsForRoom(RoomManager room)
    {
        List<Vector3> roomPoints = new List<Vector3>();

        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag(groundTag);

        foreach (GameObject ground in groundObjects)
        {
            if (IsNoSpawnByTag(ground) || IsNoSpawnByName(ground))
                continue;

            Bounds groundBounds = GetGroundBounds(ground);

            // ✅ GROUND'UN ODA İÇİNDE OLUP OLMADIĞINI KONTROL ET
            float distanceToRoom = Vector3.Distance(room.transform.position, groundBounds.center);
            if (distanceToRoom <= room.roomRadius)
            {
                int pointsPerGround = Mathf.Max(10, totalCharactersToSpawn * 2);

                for (int i = 0; i < pointsPerGround; i++)
                {
                    Vector3 randomPoint = GetRandomPointOnGround(groundBounds);
                    if (IsValidSpawnPoint(randomPoint))
                    {
                        roomPoints.Add(randomPoint);
                    }
                }
            }
        }

        return roomPoints;
    }

    // ✅ LİSTEDEN RASTGELE SPAWN NOKTASI AL
    Vector3 GetRandomSpawnPointFromList(List<Vector3> spawnList)
    {
        if (spawnList.Count == 0) return Vector3.zero;

        int randomIndex = Random.Range(0, spawnList.Count);
        Vector3 spawnPoint = spawnList[randomIndex];
        spawnList.RemoveAt(randomIndex);

        return spawnPoint;
    }

    // ✅ SPAWN METODU
    void SpawnCharacterAtPosition(GameObject characterPrefab, Vector3 spawnPoint, string location = "")
    {
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0;
        Vector3 finalSpawnPosition = spawnPoint + randomOffset;

        GameObject newCharacter = Instantiate(characterPrefab, finalSpawnPosition, Quaternion.identity);
        newCharacter.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        spawnedCharacters.Add(newCharacter);

        if (showDebug)
        {
            Debug.Log($"🎯 [{location}] {characterPrefab.name} → {finalSpawnPosition}");
        }
    }

    bool IsNoSpawnByTag(GameObject ground)
    {
        foreach (string noSpawnTag in noSpawnTags)
        {
            if (ground.CompareTag(noSpawnTag))
                return true;
        }
        return false;
    }

    bool IsNoSpawnByName(GameObject ground)
    {
        string groundName = ground.name.ToLower();
        foreach (string keyword in noSpawnNameKeywords)
        {
            if (groundName.Contains(keyword.ToLower()))
                return true;
        }
        return false;
    }

    Bounds GetGroundBounds(GameObject ground)
    {
        Renderer renderer = ground.GetComponent<Renderer>();
        Collider collider = ground.GetComponent<Collider>();

        if (renderer != null) return renderer.bounds;
        if (collider != null) return collider.bounds;

        return new Bounds(ground.transform.position, ground.transform.localScale);
    }

    Vector3 GetRandomPointOnGround(Bounds bounds)
    {
        float padding = 1f;
        float randomX = Random.Range(bounds.min.x + padding, bounds.max.x - padding);
        float randomZ = Random.Range(bounds.min.z + padding, bounds.max.z - padding);
        float y = bounds.max.y + 0.5f;

        return new Vector3(randomX, y, randomZ);
    }

    bool IsValidSpawnPoint(Vector3 point)
    {
        Collider[] colliders = Physics.OverlapSphere(point, 1.5f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player") || col.CompareTag("Enemy"))
                return false;
        }

        return Physics.Raycast(point + Vector3.up * 0.5f, Vector3.down, 3f, groundLayer);
    }

    void PrintSpawnSummary()
    {
        Debug.Log("=== 🎯 SPAWN ÖZETİ ===");
        int totalSpawned = 0;

        // Oda-spesifik spawn özeti
        if (prioritizeRoomSpawning && roomManagers != null)
        {
            foreach (RoomManager room in roomManagers)
            {
                if (room != null && room.useCustomSpawnSettings && room.roomSpecificSpawns != null)
                {
                    Debug.Log($"--- 🏠 {room.roomName} ---");
                    foreach (CharacterSpawnInfo spawnInfo in room.roomSpecificSpawns)
                    {
                        string status = spawnInfo.spawnedCount >= spawnInfo.spawnCount ? "✅" : "❌";
                        Debug.Log($"{status} {spawnInfo.characterPrefab.name}: {spawnInfo.spawnedCount}/{spawnInfo.spawnCount}");
                        totalSpawned += spawnInfo.spawnedCount;
                    }
                }
            }
        }

        Debug.Log($"📊 TOPLAM: {totalSpawned}/{totalCharactersToSpawn}");

        if (totalSpawned < totalCharactersToSpawn)
        {
            Debug.LogWarning($"⚠️ EKSİK: {totalCharactersToSpawn - totalSpawned} adet spawnlanamadı!");
        }
        else
        {
            Debug.Log("🎉 TÜM KARAKTERLER SPAWNLANDI!");
        }

        Debug.Log("=====================");
    }

    public List<GameObject> GetSpawnedCharacters() { return spawnedCharacters; }

    void OnDrawGizmos()
    {
        if (!showSpawnGizmos) return;

        // Oda spawn noktaları
        Gizmos.color = Color.blue;
        foreach (var roomPoints in roomSpawnPoints)
        {
            foreach (Vector3 point in roomPoints.Value)
            {
                Gizmos.DrawWireCube(point, Vector3.one * 0.3f);
            }
        }

        // Genel spawn noktaları
        Gizmos.color = Color.green;
        foreach (Vector3 point in generalSpawnPoints)
        {
            Gizmos.DrawWireCube(point, Vector3.one * 0.3f);
        }

        // Spawnlanmış karakterler
        Gizmos.color = Color.red;
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null)
            {
                Gizmos.DrawWireSphere(character.transform.position, 0.5f);
            }
        }
    }
}