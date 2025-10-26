using UnityEngine;
using Unity.Netcode;

public class BowWeapon : NetworkBehaviour
{
    [Header("Bow References")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public Transform bowString;
    
    [Header("Bow Settings")]
    public float drawStrength = 1f;
    public float maxDrawTime = 2f;
    public float arrowForce = 30f;
    
    private bool isDrawing = false;
    private float drawStartTime;
    private Vector3 bowStringInitialPos;
    private GameObject currentArrow;

    void Start()
    {
        bowStringInitialPos = bowString.localPosition;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        HandleBowInput();
        UpdateBowAnimation();
    }

    private void HandleBowInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            ReleaseArrow();
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        drawStartTime = Time.time;
        CreateArrowVisual();
    }

    private void CreateArrowVisual()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation, arrowSpawnPoint);
        }
    }

    private void UpdateBowAnimation()
    {
        if (isDrawing && bowString != null)
        {
            float drawProgress = Mathf.Clamp01((Time.time - drawStartTime) / maxDrawTime);
            bowString.localPosition = bowStringInitialPos - new Vector3(0, 0, drawProgress * 0.3f);
            
            if (currentArrow != null)
            {
                currentArrow.transform.localPosition = new Vector3(0, 0, -drawProgress * 0.2f);
            }
        }
    }

    private void ReleaseArrow()
    {
        if (!isDrawing) return;
        
        float drawTime = Time.time - drawStartTime;
        float strength = Mathf.Clamp01(drawTime / maxDrawTime);
        
        ShootArrowServerRpc(arrowSpawnPoint.position, arrowSpawnPoint.rotation, strength);
        ResetBow();
    }

    private void ResetBow()
    {
        isDrawing = false;
        bowString.localPosition = bowStringInitialPos;
        
        if (currentArrow != null)
            Destroy(currentArrow);
    }

    [ServerRpc]
    private void ShootArrowServerRpc(Vector3 position, Quaternion rotation, float strength)
    {
        ShootArrowClientRpc(position, rotation, strength);
    }

    [ClientRpc]
    private void ShootArrowClientRpc(Vector3 position, Quaternion rotation, float strength)
    {
        GameObject arrow = Instantiate(arrowPrefab, position, rotation);
        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.velocity = rotation * Vector3.forward * (arrowForce * strength);
        }
        
        if (arrowScript != null)
        {
            arrowScript.damage = Mathf.RoundToInt(35f * strength);
        }
        
        Destroy(arrow, 5f);
    }

    void OnDestroy()
    {
        if (currentArrow != null)
            Destroy(currentArrow);
    }
}
