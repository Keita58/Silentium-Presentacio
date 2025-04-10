using UnityEngine;
using UnityEngine.UI;

public class ShowEquippedItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Item equippedItem;
    public void Load(Item item)
    {
        this.GetComponent<Image>().sprite = item.Sprite;
        equippedItem = item;
    }

    public void OnClickEquippedItem()
    {
        InventoryManager.instance.SelectEquippedItem(equippedItem);
        this.GetComponent<Image>().color = Color.magenta;
        this.GetComponent<Button>().interactable = false;
    }
}
