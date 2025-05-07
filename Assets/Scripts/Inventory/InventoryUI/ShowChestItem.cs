using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowChestItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textQuantitat;
    Item selectedItem;
    public void Load(InventorySO.ItemSlot item)
    {
        this.GetComponent<Image>().sprite = item.item.Sprite;
        this.textQuantitat.text = item.amount.ToString();
        selectedItem = item.item;
    }

    public void OnClickChestItem()
    {
        InventoryManager.instance.SelectChestItem(selectedItem);
    }
}
