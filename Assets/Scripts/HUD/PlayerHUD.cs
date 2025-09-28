using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;

    private PlayerHealth playerHealth;
    private PlayerShield playerShield;

    void Start()
    {
        
        playerHealth = GetComponent<PlayerHealth>();
        playerShield = GetComponent<PlayerShield>();

        healthBar.maxValue = 100;
        shieldBar.maxValue = 50;

        healthBar.value = playerHealth.GetCurrentHealth();
        shieldBar.value = playerShield.GetCurrentShield();
    }

    
    void Update()
    {
        healthBar.value = playerHealth.GetCurrentHealth();
        shieldBar.value = playerShield.GetCurrentShield();
    }
    
}
