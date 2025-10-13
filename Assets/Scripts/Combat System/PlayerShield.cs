using Fusion;
using UnityEngine;

public class PlayerShield : NetworkBehaviour
{
    [Header("Atributos de Escudo")]
    [SerializeField] private int maxShield = 50;

    [Networked]
    public int currentShield { get; set; }

    public System.Action<int, int> OnShieldChanged; //current, max

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            currentShield = maxShield;
            OnShieldChanged?.Invoke(currentShield, maxShield);
        }
    }


    public int AbsorbDamage(int damageAmount)
    {
        if (!HasStateAuthority) return damageAmount;

        if (currentShield > 0)
        {
            int damageAbsorbed = Mathf.Min(damageAmount, currentShield);
            currentShield -= damageAbsorbed;

            Debug.Log(gameObject.name + " absorbió " + damageAbsorbed + " de daño. Escudo actual: " + currentShield);

            OnShieldChanged?.Invoke(currentShield, maxShield);

            //La idea aquí es retornar el daño sobrante si el escudo no alcanzó
            //Se lo pasaríamos a PlayerHealth para que quite ese sobrante a la vida
            return damageAmount - damageAbsorbed;
        }

        //Si no hay escudo todo el daño pasa a la vida
        return damageAmount;
    }

    public void RestoreShield(int shieldAmount)
    {
        if (!HasStateAuthority) return;

        currentShield += shieldAmount;

        currentShield = Mathf.Clamp(currentShield, 0, maxShield);

        Debug.Log(gameObject.name + " recuperó " + shieldAmount + " de escudo. Escudo actual: " + currentShield);

        OnShieldChanged?.Invoke(currentShield, maxShield);
    }

    public int GetCurrentShield()
    {
        return currentShield;
    }

    public int GetMaxShield()
    {
        return maxShield;
    }

}
