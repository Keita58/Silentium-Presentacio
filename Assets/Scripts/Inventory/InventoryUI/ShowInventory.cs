using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShowInventory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public GameObject target;

    [SerializeField] GameObject parentGameObject;
    [SerializeField] GameObject chestRootGameObject;
    [SerializeField] GameObject notesRoot;

    [SerializeField] InventorySO inventory;
    [SerializeField] InventorySO chestInventory;

    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject itemEquippedSlotPrefab;
    [SerializeField] GameObject chestSlot;

    [SerializeField] GameObject equippedItemSlot;

    [SerializeField] Item equippedItem;



    public void Show()
    {
        Hide();
        InventoryManager.instance.ShowDiscoveredNotes();
        
        parentGameObject.transform.parent.gameObject.SetActive(true);
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject displayedItem = Instantiate(itemPrefab, parentGameObject.transform.GetChild(i).transform);
            displayedItem.GetComponent<ShowItem>().Load(inventory.items[i]);
            displayedItem.GetComponent<ShowItem>().OnUseItem += Show;
        }
        
        if (equippedItem!=null)
        {
            GameObject usingItem = Instantiate(itemEquippedSlotPrefab, equippedItemSlot.transform);
            usingItem.GetComponent<ShowEquippedItem>().Load(equippedItem);
        }

        if (InventoryManager.instance.chestOpened)
        {
            ShowChest();
        }

    }

    public void ShowChest()
    {
        notesRoot.SetActive(false);
        chestRootGameObject.SetActive(true) ;
        for (int i=0; i < chestInventory.items.Count; i++)
        {
            GameObject displayedItem = Instantiate(chestSlot, chestRootGameObject.transform.GetChild(i).transform);
            displayedItem.GetComponent<ShowChestItem>().Load(chestInventory.items[i]);
        }

    }
    public void Hide()
    {
        parentGameObject.transform.parent.gameObject.SetActive(false);
        foreach (Transform child in parentGameObject.transform)
        {
            for (int i = 0; i < child.childCount; i++)
            {
                Destroy(child.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < equippedItemSlot.transform.childCount; i++)
        {
            Destroy(equippedItemSlot.transform.GetChild(i).gameObject);
        }

        foreach (Transform child in chestRootGameObject.transform)
        {
            for (int i = 0; i < child.childCount; i++)
            {
                Destroy(child.GetChild(i).gameObject);
            }
        }

        notesRoot.SetActive(true);
        chestRootGameObject.SetActive(false);

    }

    public void ShowHideItemsToCombine(List<Item> itemsToCombine, Item selfItem)
    {
        if (itemsToCombine.Count > 0)
        {
            for (int x = 0; x < itemsToCombine.Count; x++)
            {
                for (int i = 0; i < inventory.items.Count; i++)
                {
                    if (inventory.items.ElementAt(i).item != itemsToCombine.ElementAt(x))
                    {
                        Transform child = parentGameObject.transform;
                        if (child.childCount > i)
                        {
                            Transform innerChild = child.GetChild(i);
                            innerChild.GetChild(0).GetComponent<Button>().interactable=false;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                Transform child = parentGameObject.transform;
                if (child.childCount > i)
                {
                    Transform innerChild = child.GetChild(i);
                    innerChild.GetChild(0).GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void ChangeItemSelected()
    {
        foreach (Transform child in parentGameObject.transform)
        {
            for (int i = 0; i < child.childCount; i++)
            {
                child.GetChild(i).GetComponent<Button>().interactable=true;
                child.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void SetEquippedItem(Item item, bool show)
    {
        equippedItem = item;
        if (show)
            this.Show();
    }
}
