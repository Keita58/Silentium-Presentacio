using UnityEngine;

public class InteractuableChest : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }

    public bool isInteractuable { get; set; }

    private void Awake()
    {
        isRemarkable = true;
        isInteractuable = true;
    }
    public void Interact()
    {
        InventoryManager.instance.OpenChest();
    }

}
