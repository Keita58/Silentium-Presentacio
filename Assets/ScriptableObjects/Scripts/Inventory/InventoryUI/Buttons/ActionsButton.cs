using UnityEngine;

public class ActionsButton : MonoBehaviour
{
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

    public void CloseImageNote()
    {
        InventoryManager.instance.CloseImageNote();
    }

    public void CloseBookNote()
    {
        InventoryManager.instance.CloseBookNote();
    }
}
