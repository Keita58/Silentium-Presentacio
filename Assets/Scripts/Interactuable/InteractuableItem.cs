using UnityEngine;

[RequireComponent(typeof(PickItem))]
public class InteractuableItem : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }

    public bool isInteractuable { get;  set; }
    
    [SerializeField] Events events;
    private void Awake()
    {
        isRemarkable = true;
        isInteractuable = true;
    }

    public void Interact()
    {
        Item itemPicked = GetComponent<PickItem>().item;
        InventoryManager.instance.inventory.GetItem(itemPicked);
        if (InventoryManager.instance.inventory.items.Count < 6 || (itemPicked != null && itemPicked.isStackable))
        {
            InventoryManager.instance.AddItem(itemPicked);
            Debug.Log("QUE COJO?" + itemPicked);

            Debug.Log("Entro Coger item");
            if (itemPicked is BookItem)
            {
                Book book = GetComponent<Book>();
                if (book.placed)
                {
                    book.placed = false;
                    book.collider.enabled = true;
                    book.collider.transform.GetComponent<CellBook>().SetBook(null, null);
                    book.collider = null;
                }
            }
            gameObject.SetActive(false);
            if (itemPicked is ThrowableItem || itemPicked is SilencerItem || itemPicked is SaveItem || itemPicked is HealingItem || itemPicked is AmmunitionItem)
                events.PickItem(GetComponentInParent<PickObject>().Id);
            if (itemPicked is BookItem && itemPicked.ItemType == ItemTypes.BOOK2 && !InventoryManager.instance.glitchDone)
            {
                PuzzleManager.instance.ChangePositionPlayerAfterHieroglyphic();
                InventoryManager.instance.glitchDone = true;
            }
        }
        else
        {
            events.ShowWarning("Inventario lleno!");
        }
    }
}
