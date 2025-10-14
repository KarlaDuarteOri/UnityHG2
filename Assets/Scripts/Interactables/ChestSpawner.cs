using UnityEngine;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject chestPrefab;

    [Header("Centro del Mapa (100% spawn rate)")]
    public Vector3 centerPosition = Vector3.zero;
    public float centerRadius = 20f;
    public int minChestsCenter = 2;
    public int maxChestsCenter = 4;

    [Header("Periferia del Mapa (70% spawn rate)")]
    public Vector3 peripheryCenter = Vector3.zero;
    public float peripheryInnerRadius = 30f;
    public float peripheryOuterRadius = 80f;
    public float peripherySpawnRate = 0.7f; // 70%
    public int minChestsPeriphery = 3;
    public int maxChestsPeriphery = 6;

    [Header("Configuración General")]
    public float minDistanceBetweenChests = 15f;
    public LayerMask groundLayer;
    public bool visualizeSpawnAreas = true;

    private List<Vector3> centerSpawnPoints = new List<Vector3>();
    private List<Vector3> peripherySpawnPoints = new List<Vector3>();
    private List<GameObject> spawnedChests = new List<GameObject>();

    private void Start()
    {
        SpawnChests();
    }

    public void SpawnChests()
    {
        // Limpiar cofres anteriores
        foreach (GameObject chest in spawnedChests)
        {
            Destroy(chest);
        }
        spawnedChests.Clear();
        centerSpawnPoints.Clear();
        peripherySpawnPoints.Clear();

        if (chestPrefab == null)
        {
            Debug.LogError("Chest Prefab no está asignado en el Inspector!");
            return;
        }

        // Generar cofres en el centro
        GenerateCenterChests();

        // Generar cofres en la periferia
        GeneratePeripheryChests();

        Debug.Log($"Total de cofres generados: {spawnedChests.Count} " +
                  $"(Centro: {centerSpawnPoints.Count}, Periferia: {peripherySpawnPoints.Count})");
    }

    private void GenerateCenterChests()
    {
        int chestCount = Random.Range(minChestsCenter, maxChestsCenter + 1);

        // El centro tiene 100% de spawn rate, así que siempre aparecen
        for (int i = 0; i < chestCount; i++)
        {
            Vector3 spawnPos = GenerateRandomPointInCircle(centerPosition, centerRadius);

            if (IsValidSpawnPoint(spawnPos))
            {
                SpawnChestAtPosition(spawnPos);
                centerSpawnPoints.Add(spawnPos);
                Debug.Log($"Cofre centro #{i + 1} generado en {spawnPos}");
            }
        }
    }

    private void GeneratePeripheryChests()
    {
        int potentialChests = Random.Range(minChestsPeriphery, maxChestsPeriphery + 1);

        for (int i = 0; i < potentialChests; i++)
        {
            // Con spawn rate (70%)
            if (Random.value > peripherySpawnRate)
            {
                Debug.Log($"Cofre periferia #{i + 1} no apareció (70% spawn rate)");
                continue;
            }

            Vector3 spawnPos = GenerateRandomPointInAnnulus(
                peripheryCenter,
                peripheryInnerRadius,
                peripheryOuterRadius
            );

            if (IsValidSpawnPoint(spawnPos))
            {
                SpawnChestAtPosition(spawnPos);
                peripherySpawnPoints.Add(spawnPos);
                Debug.Log($"Cofre periferia #{i + 1} generado en {spawnPos}");
            }
        }
    }

    private Vector3 GenerateRandomPointInCircle(Vector3 center, float radius)
    {
        // Generar punto aleatorio dentro de un círculo
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, radius);

        float x = center.x + distance * Mathf.Cos(angle);
        float z = center.z + distance * Mathf.Sin(angle);

        // Raycast para obtener altura en el terreno
        float y = GetGroundHeight(new Vector3(x, center.y + 100f, z));

        return new Vector3(x, y, z);
    }

    private Vector3 GenerateRandomPointInAnnulus(Vector3 center, float innerRadius, float outerRadius)
    {
        // Generar punto aleatorio en un anillo (círculo con agujero en el centro)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(innerRadius, outerRadius);

        float x = center.x + distance * Mathf.Cos(angle);
        float z = center.z + distance * Mathf.Sin(angle);

        // Raycast para obtener altura en el terreno
        float y = GetGroundHeight(new Vector3(x, center.y + 100f, z));

        return new Vector3(x, y, z);
    }

    private float GetGroundHeight(Vector3 position)
    {
        // Raycast hacia abajo para encontrar el terreno
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 200f, groundLayer))
        {
            return hit.point.y;
        }

        // Si no encuentra terreno, retorna la posición Y por defecto
        return position.y;
    }

    private bool IsValidSpawnPoint(Vector3 position)
    {
        // Verificar distancia mínima con otros cofres
        foreach (GameObject chest in spawnedChests)
        {
            if (Vector3.Distance(position, chest.transform.position) < minDistanceBetweenChests)
            {
                Debug.LogWarning("Posición muy cercana a otro cofre, rechazando...");
                return false;
            }
        }

        // Verificar que no esté en el agua o fuera del mapa
        if (position.y < 0)
        {
            Debug.LogWarning("Posición bajo el nivel del agua, rechazando...");
            return false;
        }

        return true;
    }

    private void SpawnChestAtPosition(Vector3 position)
    {
        GameObject newChest = Instantiate(chestPrefab, position, Quaternion.identity);
        newChest.name = $"Chest_{spawnedChests.Count}";
        spawnedChests.Add(newChest);

        // Asigna el script ChestInteractable si no lo tiene
        ChestInteractable chestScript = newChest.GetComponent<ChestInteractable>();
        if (chestScript == null)
        {
            newChest.AddComponent<ChestInteractable>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!visualizeSpawnAreas) return;

        // Visualizar área central (100% spawn rate)
        Gizmos.color = Color.green;
        DrawCircle(centerPosition, centerRadius, 30);

        // Visualizar área de periferia (70% spawn rate)
        Gizmos.color = Color.yellow;
        DrawCircle(peripheryCenter, peripheryInnerRadius, 30);
        DrawCircle(peripheryCenter, peripheryOuterRadius, 30);

        // Visualizar distancia mínima entre cofres
        Gizmos.color = Color.red;
        foreach (GameObject chest in spawnedChests)
        {
            DrawCircle(chest.transform.position, minDistanceBetweenChests, 16);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

        for (int i = 0; i <= segments; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                radius * Mathf.Cos(rad),
                0,
                radius * Mathf.Sin(rad)
            );

            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
            angle += angleStep;
        }
    }

    // Método para regenerar cofres (para pruebas)
    public void RegenerateChests()
    {
        SpawnChests();
    }
}