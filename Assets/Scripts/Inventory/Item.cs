using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public Sprite itemIcon;
    public ItemType itemType = ItemType.Weapon;

    public enum ItemType
    {
        Weapon,
        Shield,
        Compass,
        Consumable
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[ITEM] Jugador tocó: {gameObject.name}");
            Debug.Log($"[ITEM] itemName en Inspector: '{itemName}'");
            Debug.Log($"[ITEM] InventorySystem.Instance: {InventorySystem.Instance}");

            if (InventorySystem.Instance != null)
            {
                bool added = InventorySystem.Instance.AddItem(this.gameObject);
                Debug.Log($"[ITEM] ¿Se agregó? {added}");

                if (added)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

}