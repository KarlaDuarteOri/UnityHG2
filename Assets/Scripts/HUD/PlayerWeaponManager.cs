using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponAmmoHUD ammoHUD;
    [SerializeField] private Weapon currentWeapon;

    void Start()
    {
        // Conectar el HUD al arma actual al iniciar
        ConnectWeaponToHUD();
    }

    void Update()
    {
        // Pruebas con teclas
        HandleTestInput();
    }

    void ConnectWeaponToHUD()
    {
        if (ammoHUD != null && currentWeapon != null)
        {
            ammoHUD.ConnectToWeapon(currentWeapon);
            Debug.Log("Arma conectada al HUD");
        }
        else
        {
            Debug.LogWarning("Faltan referencias en PlayerWeaponManager");
            
            if (ammoHUD == null) Debug.LogWarning("AmmoHUD no asignado");
            if (currentWeapon == null) Debug.LogWarning("CurrentWeapon no asignado");
        }
    }

    void HandleTestInput()
    {
        if (currentWeapon == null) return;

        // Método 1: New Input System
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            currentWeapon.Shoot();
        }

        if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            currentWeapon.Reload();
        }
        #else
        // Método 2: Old Input System (como fallback)
        if (Input.GetMouseButtonDown(0))
        {
            currentWeapon.Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.Reload();
        }
        #endif
    }

    
    public void EquipWeapon(Weapon newWeapon)
    {
        currentWeapon = newWeapon;
        ConnectWeaponToHUD();
    }
}