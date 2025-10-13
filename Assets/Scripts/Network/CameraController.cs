using UnityEngine;

/// <summary>
/// First-person camera controller for networked player
/// Follows the local player's camera target with smooth interpolation
/// Uses singleton pattern for easy access from NetworkPlayer
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private Transform target;
    private Camera cam;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[CameraController] Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraController] No Camera component found!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Set the camera target to follow (called by local player on spawn)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"[CameraController] Target set to: {newTarget?.name ?? "null"}");

        // Snap to target position immediately when setting
        if (target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Instantly follow target (no smoothing - prevents dizziness/lag)
        // LateUpdate ensures this happens after all player movement calculations
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
