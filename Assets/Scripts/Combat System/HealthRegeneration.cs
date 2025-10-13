using Fusion;
using UnityEngine;

public class HealthRegeneration : NetworkBehaviour
{
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private int regenRate = 1;
    [SerializeField] private float healInterval = 0.1f;

    [Networked]
    private float timeSinceLastDamage { get; set; }
    [Networked]
    private bool isTakingDamage { get; set; }
    [Networked]
    private float timeSinceLastHeal { get; set; }

    [Networked]
    private TickTimer RegenTimer { get; set; }
    
    private PlayerHealth playerHealth;

    public override void Spawned()
    {
        playerHealth = GetComponent<PlayerHealth>();
        
        if (HasStateAuthority)
        {
            timeSinceLastDamage = 0f;
            timeSinceLastHeal = 0f;
            isTakingDamage = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (!playerHealth.IsAlive) return;

        if (!isTakingDamage)
        {
            timeSinceLastDamage += Runner.DeltaTime;

            if (timeSinceLastDamage >= regenDelay && playerHealth.GetCurrentHealth() < playerHealth.GetMaxHealth())
            {
                RegenerateHealth();
            }
        }
    }

    //corregir esto, si hacen daño nuevamente al jugador mientras está esperando la corutina entonces 
    //la primer corutina va a establecer en falso el recibir daño antes de tiempo y estaría incorrecto
    public void OnDamageTaken()
    {
        if (!HasStateAuthority) return;

        isTakingDamage = true;
        timeSinceLastDamage = 0f;
        RegenTimer = TickTimer.CreateFromSeconds(Runner, regenDelay);
    }


    public override void Render()
    {
        // Verificar si el timer de regeneración expiró
        if (HasStateAuthority && isTakingDamage && RegenTimer.Expired(Runner))
        {
            isTakingDamage = false;
        }
    }

    private void RegenerateHealth()
    {
        timeSinceLastHeal += Runner.DeltaTime;
        
        if (timeSinceLastHeal >= healInterval)
        {
            playerHealth.Heal(regenRate);
            timeSinceLastHeal = 0f;
        }
    }
}