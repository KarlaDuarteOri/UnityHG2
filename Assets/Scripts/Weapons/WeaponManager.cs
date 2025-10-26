using UnityEngine;
using Unity.Netcode;

public enum WeaponType { None, Sword, Bow, Spear }

public class WeaponManager : NetworkBehaviour
{
    [Header("Weapon References")]
    public GameObject[] weaponPrefabs;
    public Transform weaponHolder;
    public PlayerAnimatorManager animator;
    
    [Header("Weapon Settings")]
    public float pickupRange = 3f;
    
    private WeaponType currentWeapon = WeaponType.None;
    private GameObject currentWeaponObject;

    void Start()
    {
        EquipWeapon(WeaponType.None);
    }

    public void TryPickupWeapon()
    {
        if (!IsOwner) return;
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupRange))
        {
            WeaponPickup pickup = hit.collider.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                PickupWeaponServerRpc((int)pickup.weaponType, hit.collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc]
    private void PickupWeaponServerRpc(int weaponTypeInt, ulong pickupObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(pickupObjectId, out NetworkObject pickupObject))
        {
            WeaponPickup pickup = pickupObject.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                PickupWeaponClientRpc(weaponTypeInt);
                pickupObject.Despawn();
            }
        }
    }

    [ClientRpc]
    private void PickupWeaponClientRpc(int weaponTypeInt)
    {
        WeaponType weaponType = (WeaponType)weaponTypeInt;
        SwitchWeapon(weaponType);
    }

    public void SwitchWeapon(WeaponType newWeapon)
    {
        if (!IsOwner) return;
        
        SwitchWeaponServerRpc((int)newWeapon);
    }

    [ServerRpc]
    private void SwitchWeaponServerRpc(int weaponTypeInt)
    {
        SwitchWeaponClientRpc(weaponTypeInt);
    }

    [ClientRpc]
    private void SwitchWeaponClientRpc(int weaponTypeInt)
    {
        WeaponType newWeapon = (WeaponType)weaponTypeInt;
        EquipWeapon(newWeapon);
    }

    private void EquipWeapon(WeaponType weaponType)
    {
        if (currentWeaponObject != null)
            Destroy(currentWeaponObject);
            
        currentWeapon = weaponType;
        
        if (weaponType != WeaponType.None && weaponPrefabs.Length > (int)weaponType - 1)
        {
            currentWeaponObject = Instantiate(weaponPrefabs[(int)weaponType - 1], weaponHolder);
            currentWeaponObject.transform.localPosition = Vector3.zero;
            currentWeaponObject.transform.localRotation = Quaternion.identity;
        }
        
        if (animator != null)
            animator.SetWeaponType((int)weaponType);
    }

    public void Attack()
    {
        if (!IsOwner || currentWeapon == WeaponType.None) return;
        
        AttackServerRpc((int)currentWeapon);
    }

    [ServerRpc]
    private void AttackServerRpc(int weaponTypeInt)
    {
        AttackClientRpc(weaponTypeInt);
    }

    [ClientRpc]
    private void AttackClientRpc(int weaponTypeInt)
    {
        WeaponType weapon = (WeaponType)weaponTypeInt;
        
        if (animator != null)
            animator.TriggerAttack();
            
        ApplyWeaponDamage(weapon);
    }

    private void ApplyWeaponDamage(WeaponType weapon)
    {
        RaycastHit hit;
        float range = GetWeaponRange(weapon);
        Vector3 start = transform.position + Vector3.up * 1.5f;
        
        if (Physics.Raycast(start, transform.forward, out hit, range))
        {
            PlayerCombat target = hit.collider.GetComponent<PlayerCombat>();
            if (target != null && target.gameObject != gameObject)
            {
                target.TakeDamage(GetWeaponDamage(weapon));
            }
        }
    }

    private int GetWeaponDamage(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sword: return 25;
            case WeaponType.Bow: return 35;
            case WeaponType.Spear: return 30;
            default: return 0;
        }
    }

    private float GetWeaponRange(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sword: return 2f;
            case WeaponType.Bow: return 20f;
            case WeaponType.Spear: return 3f;
            default: return 0f;
        }
    }
    
    public WeaponType GetCurrentWeapon() => currentWeapon;
}
