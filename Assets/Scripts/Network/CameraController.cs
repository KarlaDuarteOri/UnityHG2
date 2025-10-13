using UnityEngine;

/// <summary>
/// First-person camera controller for networked player
/// Follows the local player's camera target with smooth interpolation
/// Uses singleton pattern for easy access from NetworkPlayer
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public static CameraController EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var existing = FindFirstObjectByType<CameraController>();
        if (existing != null)
        {
            existing.SetupCameraComponents();
            Instance = existing;
            return Instance;
        }

        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();

        var controller = cameraObject.AddComponent<CameraController>();
        controller.SetupCameraComponents();
        Instance = controller;
        return controller;
    }

    private Transform target;
    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CameraController] Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupCameraComponents();

        GameSettings.OnFOVChanged += ApplyFOV;
    }

    private void Start()
    {
        // Apply FOV from settings on start
        ApplyFOV();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        GameSettings.OnFOVChanged -= ApplyFOV;
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

    /// <summary>
    /// Apply FOV setting from GameSettings
    /// </summary>
    private void ApplyFOV()
    {
        if (cam != null)
        {
            cam.fieldOfView = GameSettings.FieldOfView;
            Debug.Log($"[CameraController] Applied FOV: {GameSettings.FieldOfView}");
        }
    }

    private void SetupCameraComponents()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }
        }

        if (GetComponent<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
    }
}
