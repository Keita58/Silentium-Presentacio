using UnityEngine;

public class InteractuableChest : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    private void Awake()
    {
        isRemarkable = true;
    }
    public void Interact()
    {
        InventoryManager.instance.OpenChest();
    }

}
