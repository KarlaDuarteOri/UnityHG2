using Fusion;
using UnityEngine;


public class PlayerHealth : NetworkBehaviour
{
    [Header("Atributos de Vida")]
    [SerializeField] private int maxHealth = 100;
    [Networked]
    public int currentHealth { get; set; }

    [Networked]
    public bool IsAlive { get; set; }

    public System.Action<int, int> OnHealthChanged; //current, max
    public System.Action OnDeath;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
            IsAlive = true;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }


    public void TakeDamage(int damageAmount)
    {
        if (!HasStateAuthority || !IsAlive) return;

        currentHealth -= damageAmount;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); //Limitar la vida entre 0 y maxHealth

        Debug.Log(gameObject.name + " recibió " + damageAmount + " de daño. Vida actual: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth); //Notificar de cambios al HUD

        if (currentHealth <= 0)
        {
            StartCoroutine(DieAfterFrame()); //Si no se hace esto el hud de vida no se termina de actualizar 
                                             // porque el jugador se inactiva antes
        }
    }

    private System.Collections.IEnumerator DieAfterFrame()
    {
        yield return new WaitForSeconds(1);
        Die();
    }


    public void Heal(int healAmount)
    {
        if (!HasStateAuthority || !IsAlive) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(gameObject.name + " fue curado. Vida actual: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (HasStateAuthority && IsAlive)
        {
            IsAlive = false;
            Debug.Log(gameObject.name + " ha muerto!");
            
            OnDeath?.Invoke();
            
            // Notificar al contador de jugadores
            AlivePlayersCounter counter = FindFirstObjectByType<AlivePlayersCounter>();
            if (counter != null) 
                //counter.NotifyPlayerDeath();
            
            gameObject.SetActive(false);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
