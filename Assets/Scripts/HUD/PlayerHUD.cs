using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;

public class PlayerHUD : MonoBehaviour
{
    [Header("Prefab del Canvas HUD")]
    [SerializeField] private GameObject hudPrefab;

    [Header("Referencias UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private Image healthFill; 
    [SerializeField] private Image shieldFill;

    [Header("Animación")]
    [SerializeField] private float smoothSpeed = 5f; 

    private PlayerHealth playerHealth;
    private PlayerShield playerShield;
    //private AlivePlayersCounter playersCounter;

    private float currentHealthDisplay;
    private float currentShieldDisplay;

    public static PlayerHUD Instance { get; private set; }
    private GameObject hudInstance;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    void Start()
    {
        InstantiateHUD();
        InitializeHUDReferences();
        FindLocalPlayer();
        //playersCounter = FindFirstObjectByType<AlivePlayersCounter>();
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

    private void InstantiateHUD()
    {
        if (hudPrefab != null)
        {
            hudInstance = Instantiate(hudPrefab);
        }
        else
        {
            Debug.LogError("HUD Canvas Prefab no asignado en el Inspector");
        }
    }

    private void InitializeHUDReferences()
    {
        if (hudInstance == null)
        {
            Debug.LogError("HUD instance es null, no se pueden inicializar referencias");
            return;
        }

        healthBar = GameObject.Find("HealthBar")?.GetComponent<Slider>();
        shieldBar = GameObject.Find("ShieldBar")?.GetComponent<Slider>();
        healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        shieldText = GameObject.Find("ShieldText")?.GetComponent<TextMeshProUGUI>();
        healthFill = GameObject.Find("HealthFill")?.GetComponent<Image>();
        shieldFill = GameObject.Find("ShieldFill")?.GetComponent<Image>();

        if (healthBar == null) Debug.LogError("HealthBar no encontrado en el HUD");
        if (shieldBar == null) Debug.LogError("ShieldBar no encontrado en el HUD");
        if (healthText == null) Debug.LogError("HealthText no encontrado en el HUD");
        if (shieldText == null) Debug.LogError("ShieldText no encontrado en el HUD");

        currentHealthDisplay = 0f;
        currentShieldDisplay = 0f;

        if (healthBar != null) 
        {
            healthBar.maxValue = 100;
            healthBar.value = 0;
        }
        if (shieldBar != null)
        {
            shieldBar.maxValue = 50;
            shieldBar.value = 0;
        }

        //playersCounter = FindFirstObjectByType<AlivePlayersCounter>();
    }


    private void FindLocalPlayer()
    {

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in playerObjects)
        {
            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
            

            if (health != null && health.HasStateAuthority)
            {
                if (playerHealth != null)
                {
                    playerHealth.OnHealthChanged -= OnHealthChanged;
                }
                if (playerShield != null)
                {
                    playerShield.OnShieldChanged -= OnShieldChanged;
                }

                playerHealth = health;
                playerShield = playerObj.GetComponent<PlayerShield>();

                if (playerHealth == null || playerShield == null)
                {
                    Debug.LogWarning("Jugador encontrado pero falta PlayerHealth o PlayerShield");
                    continue;
                }

                playerHealth.OnHealthChanged += OnHealthChanged;
                playerShield.OnShieldChanged += OnShieldChanged;

                currentHealthDisplay = playerHealth.GetCurrentHealth();
                currentShieldDisplay = playerShield.GetCurrentShield();

                if (healthBar != null) healthBar.value = currentHealthDisplay;
                if (shieldBar != null) shieldBar.value = currentShieldDisplay;

                UpdateTexts();

                Debug.Log("HUD Global conectado al jugador local");
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
        if (playerHealth == null || healthBar == null) return;

        float targetHealth = playerHealth.GetCurrentHealth();
        currentHealthDisplay = Mathf.Lerp(currentHealthDisplay, targetHealth, Time.deltaTime * smoothSpeed);
        healthBar.value = currentHealthDisplay;
    }

    private void UpdateShieldDisplay()
    {
        if (playerShield == null || shieldBar == null) return;

        float targetShield = playerShield.GetCurrentShield();
        currentShieldDisplay = Mathf.Lerp(currentShieldDisplay, targetShield, Time.deltaTime * smoothSpeed);
        shieldBar.value = currentShieldDisplay;
    }

    private void UpdateTexts()
    {
        if (healthText != null && playerHealth != null)
            healthText.text = $"{playerHealth.GetCurrentHealth()} / {healthBar.maxValue} HP";
        
        if (shieldText != null && playerShield != null)
            shieldText.text = $"{playerShield.GetCurrentShield()} / {shieldBar.maxValue} Shield";
    }

    private void UpdateHealthColor()
    {
        if (healthFill != null && healthBar != null)
        {
            float healthPercent = healthBar.value / healthBar.maxValue;

            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.red;
        }
        
        if (shieldFill != null)
        {
            shieldFill.color = Color.blue;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
        if (playerShield != null)
            playerShield.OnShieldChanged -= OnShieldChanged;
    }
}
