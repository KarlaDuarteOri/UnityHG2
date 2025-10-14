using Fusion;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerHealth : NetworkBehaviour
{
    [Header("Atributos de Vida")]
    [SerializeField] private int maxHealth = 100;
    [Networked]
    public int currentHealth { get; set; }

    [Networked]
    public bool IsAlive { get; set; }
    private GameEndDetector gameEndDetector;
    private bool hasNotifiedDeath = false;

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

        if (gameEndDetector == null)
            gameEndDetector = FindFirstObjectByType<GameEndDetector>();
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
        if (HasStateAuthority && IsAlive && !hasNotifiedDeath)
        {
            hasNotifiedDeath = true;
            IsAlive = false;
            Debug.Log(gameObject.name + " ha muerto!");

            OnDeath?.Invoke();

            if (AlivePlayersCounter.Instance != null)
            {
                AlivePlayersCounter.Instance.NotifyPlayerDeath();
                Debug.Log("Notificación enviada al contador");
            }
            else
            {
                Debug.LogError("No se encontró AlivePlayersCounter.Instance");
            }

            if (gameEndDetector != null)
            {
                gameEndDetector.CheckForGameEnd();
            }

            StartCoroutine(DespawnAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Runner.Despawn(Object);
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
