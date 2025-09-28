using UnityEngine;
using UnityEngine.InputSystem; 

public class DebugDamageController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int debugDamageAmount = 10;
    
    private PlayerCombat playerCombat;
    private Keyboard keyboard;  

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        keyboard = Keyboard.current;  
    }

    void Update()
    {
        if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
        {
            ApplyDebugDamage();
        }
    }

    void ApplyDebugDamage()
    {
        if (playerCombat != null)
        {
            Debug.Log($"Aplicando {debugDamageAmount} de daño");
            playerCombat.TakeDamage(debugDamageAmount);
        }
    }
}