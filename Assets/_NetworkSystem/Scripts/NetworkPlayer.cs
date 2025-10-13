using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 10f;  // KCC uses impulse instead of force
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Components")]
    private SimpleKCC kcc;

    [Networked] private NetworkButtons previousButtons { get; set; }
    [Networked] private float cameraPitch { get; set; }  // Track pitch separately for camera

    private void Awake()
    {
        kcc = GetComponent<SimpleKCC>();
        if (kcc == null)
        {
            Debug.LogError("SimpleKCC component missing on Player prefab!");
        }
    }

    public override void Spawned()
    {
        // Se llama cuando el objeto es spawneado en la red
        if (HasStateAuthority)
        {
            Debug.Log($"[NetworkPlayer] Jugador LOCAL spawneado con autoridad - ID: {Object.InputAuthority.PlayerId}");

            // Setup camera for local player
            if (cameraTarget != null && CameraController.Instance != null)
            {
                CameraController.Instance.SetTarget(cameraTarget);
            }

            // Cambiar color para distinguir jugador local
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }
        else
        {
            Debug.Log($"[NetworkPlayer] Jugador REMOTO spawneado - ID: {Object.InputAuthority.PlayerId}");

            // Color diferente para jugadores remotos
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.blue;
            }
        }

        // Asignar nombre al jugador
        gameObject.name = $"Player_{Object.InputAuthority.PlayerId}";
    }

    public override void FixedUpdateNetwork()
    {
        // Get network input for this tick
        if (!GetInput<NetworkInputData>(out var input))
        {
            // No input available (can happen for remote players on client)
            return;
        }

        // Only players with state authority process input
        if (HasStateAuthority)
        {
            HandleLookRotation(input);
            HandleMovement(input);
        }
        // KCC handles remote player interpolation automatically - no manual sync needed!
    }

    private void HandleLookRotation(NetworkInputData input)
    {
        // Update pitch for camera (up/down look)
        cameraPitch -= input.look.y * lookSensitivity;  // Inverted Y
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        // Update yaw for character body (left/right rotation)
        float yawDelta = input.look.x * lookSensitivity;
        kcc.AddLookRotation(0f, yawDelta);  // Only add yaw to KCC

        // Apply pitch rotation to camera target
        if (cameraTarget != null)
        {
            cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement(NetworkInputData input)
    {
        // Get movement input (relative to player's forward direction)
        Vector3 moveDirection = new Vector3(input.move.x, 0f, input.move.y);

        // Apply sprint multiplier if sprint button is held
        float currentSpeed = moveSpeed;
        if (input.buttons.IsSet(InputButtons.Sprint))
        {
            currentSpeed *= sprintMultiplier;
        }

        // Transform input to world space using KCC's rotation and calculate velocity
        Vector3 moveVelocity = kcc.TransformRotation * moveDirection * currentSpeed;

        // Handle jump (only if pressed this tick and grounded)
        float jumpImpulseValue = 0f;
        var pressed = input.buttons.GetPressed(previousButtons);
        if (pressed.IsSet(InputButtons.Jump) && kcc.IsGrounded)
        {
            jumpImpulseValue = jumpImpulse;
            Debug.Log("¡Saltando!");
        }

        // Move the KCC (KCC handles gravity automatically!)
        kcc.Move(moveVelocity, jumpImpulseValue);

        // Update previous buttons for next tick
        previousButtons = input.buttons;
    }

    public override void Render()
    {
        // Se llama cada frame (no cada tick de red)
        // Útil para efectos visuales, animaciones, etc.
    }

    // Método para obtener información del jugador
    public int GetPlayerId()
    {
        return Object.InputAuthority.PlayerId;
    }

    public bool IsLocalPlayer()
    {
        return HasStateAuthority;
    }
}