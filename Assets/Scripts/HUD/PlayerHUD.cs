using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private Image healthFill; 

    [Header("Animación")]
    [SerializeField] private float smoothSpeed = 5f; 

    private PlayerHealth playerHealth;
    private PlayerShield playerShield;
    private AlivePlayersCounter playersCounter;

    private float currentHealthDisplay;
    private float currentShieldDisplay;

    void Start()
    {
        currentHealthDisplay = 0f;
        currentShieldDisplay = 0f;

        healthBar.maxValue = 100; 
        shieldBar.maxValue = 50;

        playersCounter = FindFirstObjectByType<AlivePlayersCounter>();
    }

    void Update()
    {
        if (playerHealth == null || playerShield == null)
        {
            FindLocalPlayer();
            return;
        }

        UpdateHealthDisplay();
        UpdateShieldDisplay();
        UpdateHealthColor();
    }

    private void FindLocalPlayer()
    {
        PlayerHealth[] allHealth = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        
        foreach (PlayerHealth health in allHealth)
        {
            if (health.HasStateAuthority) //jugador local
            {
                playerHealth = health;
                playerShield = health.GetComponent<PlayerShield>();
                
                
                playerHealth.OnHealthChanged += OnHealthChanged;
                playerShield.OnShieldChanged += OnShieldChanged;
                
                currentHealthDisplay = playerHealth.GetCurrentHealth();
                currentShieldDisplay = playerShield.GetCurrentShield();
                
                healthBar.value = currentHealthDisplay;
                shieldBar.value = currentShieldDisplay;
                
                UpdateTexts();
                
                Debug.Log("HUD conectado al jugador local");
                break;
            }
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        currentHealthDisplay = current;
        UpdateTexts();
    }

    private void OnShieldChanged(int current, int max)
    {
        currentShieldDisplay = current;
        UpdateTexts();
    }

    private void UpdateHealthDisplay()
    {
        float targetHealth = playerHealth.GetCurrentHealth();
        currentHealthDisplay = Mathf.Lerp(currentHealthDisplay, targetHealth, Time.deltaTime * smoothSpeed);
        healthBar.value = currentHealthDisplay;
    }

    private void UpdateShieldDisplay()
    {
        float targetShield = playerShield.GetCurrentShield();
        currentShieldDisplay = Mathf.Lerp(currentShieldDisplay, targetShield, Time.deltaTime * smoothSpeed);
        shieldBar.value = currentShieldDisplay;
    }

    private void UpdateTexts()
    {
        if (healthText != null)
            healthText.text = $"{playerHealth.GetCurrentHealth()} / {healthBar.maxValue} HP";
        
        if (shieldText != null)
            shieldText.text = $"{playerShield.GetCurrentShield()} / {shieldBar.maxValue} Shield";
    }

    private void UpdateHealthColor()
    {
        if (healthFill != null)
        {
            float healthPercent = healthBar.value / healthBar.maxValue;
            
            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.red;
        }
    }

    private void OnDestroy()
    {
        //Limpiar suscripciones
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
        if (playerShield != null)
            playerShield.OnShieldChanged -= OnShieldChanged;
    }
}
