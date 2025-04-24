using UnityEngine;

public class ActionsButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ClickUseButton()
    {
        InventoryManager.instance.UseItem();
    }

    public void ClickCombineButton()
    {
        InventoryManager.instance.CombineItem();
    }

    public void ClickEquipButton()
    {
        InventoryManager.instance.EquipItem();
    }

    public void ClickUnequipButton()
    {
        InventoryManager.instance.UnequipItem();
    }

    public void ClickStoreInChest()
    {
        InventoryManager.instance.StoreInChest();
    }

    public void CloseNote()
    {
        InventoryManager.instance.CloseNote();
    }    
    public void CloseNoteScroll()
    {
        InventoryManager.instance.CloseNoteScroll();
    }
}
