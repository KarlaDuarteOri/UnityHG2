using UnityEngine;
using TMPro;

public class WeaponAmmoHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject ammoPanel;    
    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private Weapon currentWeapon; //Reemplazar aquí la clase de weapon por la de Fredrik, igual en los métodos
    private NetworkPlayer localPlayer;

    void Start()
    {
        if (ammoPanel != null)
            ammoPanel.SetActive(false);

        ConnectToWeapon(currentWeapon);
    }

    void Update()
    {
        if (localPlayer == null)
        {
            FindLocalPlayer();
            return;
        }

        if (currentWeapon == null && localPlayer != null)
        {
            FindPlayerWeapon();
        }
    }

    private void FindLocalPlayer()
    {
        NetworkPlayer[] allPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);

        foreach (NetworkPlayer player in allPlayers)
        {
            if (player.HasStateAuthority)
            {
                localPlayer = player;
                Debug.Log("WeaponHUD conectado al jugador local");
                break;
            }
        }
    }
    
    private void FindPlayerWeapon()
    {
        Weapon weapon = localPlayer.GetComponent<Weapon>();
        if (weapon != null && weapon != currentWeapon)
        {
            ConnectToWeapon(weapon);
        }
        
        if (currentWeapon == null)
        {
            weapon = localPlayer.GetComponentInChildren<Weapon>();
            if (weapon != null && weapon != currentWeapon)
            {
                ConnectToWeapon(weapon);
            }
        }
    }

    public void ConnectToWeapon(Weapon weapon)
    {
        //desconectar la anterior
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
        }

        //conectar la nueva
        currentWeapon = weapon;
        
        if (ammoPanel != null)
            ammoPanel.SetActive(currentWeapon != null);
        
        if (weapon != null && ammoText != null)
        {
            weapon.OnAmmoChanged += UpdateAmmoDisplay;
            //actualizar display con los valores iniciales
            UpdateAmmoDisplay(weapon.GetCurrentAmmo(), weapon.GetMaxAmmo());
        }
    }

    public void DisconnectWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            currentWeapon = null;
        }

        if (ammoPanel != null)
            ammoPanel.SetActive(false);

        Debug.Log("WeaponHUD desconectado del arma");
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"Ammo: {current} / {max}";

        //Cambiar a rojo la letra si queda poca municion
        if (current <= max * 0.2f)
            ammoText.color = Color.red;
        else
            ammoText.color = Color.white;
    }

    private void OnDestroy()
    {
        //Limpiar suscripción
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
        }
    }
}