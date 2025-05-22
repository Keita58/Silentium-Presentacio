using UnityEngine;

[RequireComponent(typeof(Door))]
public class InteractuableDoor : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    public bool isInteractuable { get;  set; }
    Door door;

    private void Awake()
    {
        isRemarkable = false;
        isInteractuable = true;
    }

    public void Interact()
    {
    }

    public void Interact(Transform player)
    {
        if (door.isLocked)
        {
            InventorySO.ItemSlot aux = null;
            foreach (InventorySO.ItemSlot item in InventoryManager.instance.inventory.items)
            {
                if (item.item == door.itemNeededToOpen)
                {
                    door.isLocked = false;
                    aux = item;
                    InventoryManager.instance.inventory.items.Remove(aux);
                    break;
                }
            }
            if (door.isOpen)
            {
                door.Close();
            }
            else
            {
                door.Open(player.position);
            }
        }
        else
        {
            if (door.isOpen)
            {
                door.Close();
            }
            else
            {
                door.Open(player.position);
            }
        }
    }
}
