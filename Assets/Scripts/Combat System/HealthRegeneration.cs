using UnityEngine;

public class HealthRegeneration : MonoBehaviour
{
    [Header("Configuración de Regeneración")]
    [SerializeField] private float regenDelay = 5f;   
    [SerializeField] private int regenRate = 5;     

    private PlayerHealth playerHealth;
    private float timeSinceLastDamage;  
    private bool isTakingDamage;        

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        timeSinceLastDamage = 0f;
    }

    void Update()
    {
        
        if (!isTakingDamage)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenDelay && playerHealth.GetCurrentHealth() < 100)
            {
                RegenerateHealth();
            }
        }
        else
        {
            timeSinceLastDamage = 0f;
            isTakingDamage = false;
        }
    }

    public void OnDamageTaken()
    {
        isTakingDamage = true;
    }

    private void RegenerateHealth()
    {
        playerHealth.Heal(Mathf.RoundToInt(regenRate * Time.deltaTime));
    }
}
