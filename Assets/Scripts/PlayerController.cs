using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    
    [Header("Camera")]
    public Transform playerCamera;
    public Transform weaponPivot;
    
    [Header("Weapon System")]
    public WeaponManager weaponManager;
    
    private CharacterController characterController;
    private float xRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        
        HandleMovement();
        HandleMouseLook();
        HandleWeaponInput();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleWeaponInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            weaponManager.TryPickupWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponManager.SwitchWeapon(WeaponType.Sword);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponManager.SwitchWeapon(WeaponType.Bow);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            weaponManager.SwitchWeapon(WeaponType.Spear);
            
        if (Input.GetMouseButtonDown(0))
        {
            weaponManager.Attack();
        }
    }
}
