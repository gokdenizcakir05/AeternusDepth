using UnityEngine;
using System.Collections.Generic;

public class SpawnZoneManager : MonoBehaviour
{
    [Header("Spawn Zones")]
    public List<GameObject> spawnZones = new List<GameObject>(); // Spawn yapýlacak bölgeler

    [Header("Debug")]
    public bool showDebug = true;
    public bool showZoneGizmos = true;

    public List<Vector3> GetAllSpawnPoints()
    {
        List<Vector3> allSpawnPoints = new List<Vector3>();

        foreach (GameObject zone in spawnZones)
        {
            if (zone != null)
            {
                Collider zoneCollider = zone.GetComponent<Collider>();
                if (zoneCollider != null)
                {
                    // Zone içinde rastgele spawn noktalarý oluþtur
                    for (int i = 0; i < 10; i++) // Her zone için 10 potansiyel nokta
                    {
                        Vector3 randomPoint = GetRandomPointInCollider(zoneCollider);
                        if (IsValidSpawnPoint(randomPoint))
                        {
                            allSpawnPoints.Add(randomPoint);
                        }
                    }
                }
            }
        }

        if (showDebug) Debug.Log($"Toplam {allSpawnPoints.Count} spawn noktasý bulundu");
        return allSpawnPoints;
    }

    Vector3 GetRandomPointInCollider(Collider collider)
    {
        Bounds bounds = collider.bounds;

        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y + 0.5f, // Ground'un biraz üstü
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // Yüksekliði ayarla (ground üzerinde olacak þekilde)
        RaycastHit hit;
        if (Physics.Raycast(randomPoint + Vector3.up * 5f, Vector3.down, out hit, 10f))
        {
            randomPoint.y = hit.point.y + 0.5f;
        }

        return randomPoint;
    }

    bool IsValidSpawnPoint(Vector3 point)
    {
        // Diðer karakterlerle çakýþma kontrolü
        Collider[] colliders = Physics.OverlapSphere(point, 1.5f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player") || col.CompareTag("Enemy"))
            {
                return false;
            }
        }
        return true;
    }

    void OnDrawGizmos()
    {
        if (!showZoneGizmos) return;

        foreach (GameObject zone in spawnZones)
        {
            if (zone != null)
            {
                Collider collider = zone.GetComponent<Collider>();
                if (collider != null)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.3f); // Yeþil, yarý saydam
                    Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                }
            }
        }
    }
}