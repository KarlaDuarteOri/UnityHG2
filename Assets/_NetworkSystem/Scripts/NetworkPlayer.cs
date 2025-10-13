using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 2.2f;  // Realistic jump height (~1 meter)
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Components")]
    private CharacterController characterController;

    [Networked] private Vector3 networkPosition { get; set; }
    [Networked] private Quaternion networkRotation { get; set; }
    [Networked] private bool isGrounded { get; set; }
    [Networked] private NetworkButtons previousButtons { get; set; }
    [Networked] private Vector2 lookRotation { get; set; }  // x = pitch, y = yaw

    private Vector3 velocity;
    private float gravity = -25f;  // Earth-like gravity for realistic jump/fall

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 2f;
            characterController.center = new Vector3(0, 1, 0);
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
            // Update grounded state FIRST before any movement logic
            isGrounded = characterController.isGrounded;

            HandleLookRotation(input);
            HandleMovement(input);
            HandleGravity(input);
        }
        else
        {
            // Interpolar posición para jugadores remotos (suaviza el movimiento)
            transform.position = Vector3.Lerp(transform.position, networkPosition, Runner.DeltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Runner.DeltaTime * 10f);
        }
    }

    private void HandleLookRotation(NetworkInputData input)
    {
        // Apply look rotation from mouse input
        // lookRotation.x = pitch (up/down), lookRotation.y = yaw (left/right)
        Vector2 newLookRotation = lookRotation;
        newLookRotation.x -= input.look.y * lookSensitivity;  // Pitch (inverted Y)
        newLookRotation.y += input.look.x * lookSensitivity;  // Yaw

        // Clamp pitch to prevent over-rotation
        newLookRotation.x = Mathf.Clamp(newLookRotation.x, -maxLookAngle, maxLookAngle);

        lookRotation = newLookRotation;

        // Apply rotation to player body (yaw only)
        transform.rotation = Quaternion.Euler(0f, lookRotation.y, 0f);

        // Apply rotation to camera target (pitch + yaw)
        if (cameraTarget != null)
        {
            cameraTarget.localRotation = Quaternion.Euler(lookRotation.x, 0f, 0f);
        }
    }

    private void HandleMovement(NetworkInputData input)
    {
        // Get movement input (relative to player's forward direction)
        Vector3 moveDirection = new Vector3(input.move.x, 0f, input.move.y);

        if (moveDirection.magnitude > 0.1f)
        {
            // Normalize to prevent faster diagonal movement
            moveDirection = moveDirection.normalized;

            // Transform to world space (relative to where player is facing)
            moveDirection = transform.TransformDirection(moveDirection);

            // Apply sprint multiplier if sprint button is held
            float currentSpeed = moveSpeed;
            if (input.buttons.IsSet(InputButtons.Sprint))
            {
                currentSpeed *= sprintMultiplier;
            }

            // Move the character
            Vector3 move = moveDirection * currentSpeed * Runner.DeltaTime;
            characterController.Move(move);
        }

        // Handle jump (only if pressed this tick and grounded)
        var pressed = input.buttons.GetPressed(previousButtons);
        if (pressed.IsSet(InputButtons.Jump) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            Debug.Log("¡Saltando!");
        }

        // Update previous buttons for next tick
        previousButtons = input.buttons;

        // Update networked transform
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    private void HandleGravity(NetworkInputData input)
    {
        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequeña fuerza para mantenerlo pegado al suelo
        }

        // Aplicar gravedad
        velocity.y += gravity * Runner.DeltaTime;

        // Aplicar velocidad vertical
        characterController.Move(velocity * Runner.DeltaTime);
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