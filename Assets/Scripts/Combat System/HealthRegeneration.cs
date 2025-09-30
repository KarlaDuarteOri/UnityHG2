using UnityEngine;

public class HealthRegeneration : MonoBehaviour
{
    //[Header("Configuración de Regeneración")]
    //[SerializeField]
    private float regenDelay = 8f;
    //[SerializeField]
    private int regenRate = 1;
    //[SerializeField]
    private float healInterval = 0.1f; 

    private PlayerHealth playerHealth;
    private float timeSinceLastDamage;  
    private bool isTakingDamage;        
    private float timeSinceLastHeal; 

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        timeSinceLastDamage = 0f;
        timeSinceLastHeal = 0f;
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
    }

    public void OnDamageTaken()
    {
        isTakingDamage = true;
        timeSinceLastDamage = 0f;
        StartCoroutine(ResetDamageFlag());
    }


    private System.Collections.IEnumerator ResetDamageFlag()
    {
        yield return new WaitForSeconds(regenDelay);
        isTakingDamage = false;
    }

    private void RegenerateHealth()
    {
        timeSinceLastHeal += Time.deltaTime;
        
        if (timeSinceLastHeal >= healInterval)
        {
            playerHealth.Heal(regenRate);
            timeSinceLastHeal = 0f;
        }
    }
}