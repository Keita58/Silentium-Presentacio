using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] TextMeshProUGUI textQuantitat;
    public event Action OnUseItem;
    Item selectedItem;
    ShowInventory inventoryUI;

    private void Awake()
    {
        
    }

    public void Load(InventorySO.ItemSlot item)
    {
        this.GetComponent<Image>().sprite=item.item.Sprite;
        this.textQuantitat.text=item.amount.ToString();
        selectedItem=item.item;
    }

    public void ClickItem()
    {
        if (selectedItem != null)
        {
            inventoryUI.ItemSelected(selectedItem);
            //selectedItem.Use();
            //OnUseItem?.Invoke();
        }
    }

    
}
