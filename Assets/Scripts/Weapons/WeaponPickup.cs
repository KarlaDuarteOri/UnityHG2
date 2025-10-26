using UnityEngine;
using Unity.Netcode;

public class WeaponPickup : NetworkBehaviour
{
    public WeaponType weaponType;
    public GameObject weaponModel;
    
    void Start()
    {
        if (weaponModel != null)
        {
            weaponModel.transform.localRotation = Quaternion.Euler(0, 45, 0);
        }
    }
    
    void Update()
    {
        if (weaponModel != null)
        {
            weaponModel.transform.Rotate(0, 50 * Time.deltaTime, 0);
            weaponModel.transform.position = transform.position + new Vector3(0, Mathf.Sin(Time.time) * 0.2f, 0);
        }
    }
}
