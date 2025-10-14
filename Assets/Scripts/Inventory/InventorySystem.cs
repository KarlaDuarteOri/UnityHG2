using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InventorySystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public Image[] slotImages;
    public Text[] slotQuantityTexts; // Para mostrar cantidad

    [Header("Inventory Settings")]
    public int maxSlots = 6;
    public int maxStackSize = 99;

    // Clase para representar un item apilable
    [System.Serializable]
    public class InventorySlot
    {
        public string itemName;
        public Sprite itemIcon;
        public Item.ItemType itemType;
        public int quantity;

        public InventorySlot(string name, Sprite icon, Item.ItemType type, int qty)
        {
            itemName = name;
            itemIcon = icon;
            itemType = type;
            quantity = qty;
        }
    }

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private bool isInventoryOpen = false;

    public static InventorySystem Instance;

    public bool IsInventoryOpen => isInventoryOpen;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Inicializar slots vacíos
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(null);
        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // Inicializar UI
        UpdateInventoryUI();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            Debug.Log(isInventoryOpen ? "Inventario ABIERTO" : "Inventario CERRADO");
        }
        else
        {
            Debug.LogError("InventoryPanel no está asignado");
        }

        Time.timeScale = isInventoryOpen ? 0f : 1f;
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
    }

    public bool AddItem(GameObject itemObject)
    {
        Item itemComponent = itemObject.GetComponent<Item>();
        if (itemComponent == null)
        {
            Debug.LogError("El objeto no tiene componente Item");
            return false;
        }

        Debug.Log($"Intentando añadir: {itemComponent.itemName} (itemType: {itemComponent.itemType})");

        // Buscar si ya existe un item del MISMO TIPO (comparar por itemName)
        int existingSlotIndex = FindItemSlot(itemComponent.itemName);

        if (existingSlotIndex != -1)
        {
            // Item existe, aumentar cantidad
            if (inventorySlots[existingSlotIndex].quantity < maxStackSize)
            {
                inventorySlots[existingSlotIndex].quantity++;
                Debug.Log($"✓ {itemComponent.itemName} apilado. Nuevo total: x{inventorySlots[existingSlotIndex].quantity}");
                UpdateInventoryUI();
                return true;
            }
            else
            {
                Debug.Log("✗ Stack lleno para este item");
                return false;
            }
        }
        else
        {
            // Buscar primer slot vacío
            int emptySlotIndex = FindEmptySlot();

            if (emptySlotIndex != -1)
            {
                // Crear nuevo slot
                InventorySlot newSlot = new InventorySlot(
                    itemComponent.itemName,
                    itemComponent.itemIcon,
                    itemComponent.itemType,
                    1
                );

                inventorySlots[emptySlotIndex] = newSlot;
                Debug.Log($"✓ {itemComponent.itemName} añadido a slot {emptySlotIndex}");
                UpdateInventoryUI();
                return true;
            }
            else
            {
                Debug.Log("✗ Inventario lleno!");
                return false;
            }
        }
    }

    private int FindItemSlot(string itemName)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].itemName == itemName)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                // Mostrar icono
                slotImages[i].sprite = inventorySlots[i].itemIcon;
                slotImages[i].color = Color.white;

                // Mostrar cantidad
                if (slotQuantityTexts[i] != null)
                {
                    slotQuantityTexts[i].text = inventorySlots[i].quantity.ToString();
                    slotQuantityTexts[i].gameObject.SetActive(true);

                    Debug.Log($"Slot {i}: {inventorySlots[i].itemName} x{inventorySlots[i].quantity}");
                }
            }
            else
            {
                // Slot vacío
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0.3f);

                if (slotQuantityTexts[i] != null)
                {
                    slotQuantityTexts[i].text = "";
                    slotQuantityTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public bool IsInventoryFull()
    {
        // Inventario lleno si todos los slots tienen items
        int filledSlots = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null)
                filledSlots++;
        }
        return filledSlots >= maxSlots;
    }

    // Método para obtener un item del inventario
    public InventorySlot GetItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            return inventorySlots[slotIndex];
        }
        return null;
    }

    // Método para remover cantidad de un item
    public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count || inventorySlots[slotIndex] == null)
            return false;

        inventorySlots[slotIndex].quantity -= quantity;

        if (inventorySlots[slotIndex].quantity <= 0)
        {
            inventorySlots[slotIndex] = null;
        }

        UpdateInventoryUI();
        return true;
    }
}