using UnityEngine;
using UnityEngine.UI;
/*Clase que controla la vida de un personaje*/
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private Image healthBar;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealth();
    }
    /*Decrementamos la vida actual del personaje*/
    public void DecrementHealth(int amount)
    {
        currentHealth -= amount;
        UpdateHealth();
        CheckDead();
    }
    /*Actualiza solamente la barra de vida*/
    public void UpdateHealth()
    {
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;
    }
    /*Verifica si el personaje tiene vida o no, en caso de no tener se destruye el objeto*/
    public void CheckDead()
    {
        if(currentHealth <= 0)
        {
            Debug.Log("Dead " + transform.name);
            Destroy(gameObject);
        }
    }
}
