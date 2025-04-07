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
}
