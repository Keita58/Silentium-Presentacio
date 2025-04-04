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

    [SerializeField] InventorySO inventory;

    [SerializeField] GameObject itemPrefab;


    public void Show()
    {
        Hide();
        
        parentGameObject.transform.parent.gameObject.SetActive(true);
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject displayedItem = Instantiate(itemPrefab, parentGameObject.transform.GetChild(i).transform);
            displayedItem.GetComponent<ShowItem>().Load(inventory.items[i]);
            displayedItem.GetComponent<ShowItem>().OnUseItem += Show;
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
    }

    public void ShowHideItemsToCombine(List<Item> itemsToCombine, Item selfItem)
    {
        for (int x = 0; x < itemsToCombine.Count; x++)
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                if (inventory.items.ElementAt(i).item != itemsToCombine.ElementAt(x) || inventory.items.ElementAt(i).item == selfItem)
                {
                    Transform child = parentGameObject.transform;
                    if (child.childCount > i)
                    {
                        Transform innerChild = child.GetChild(i);
                        if (innerChild.childCount > i)
                        {
                            innerChild.GetChild(i).GetComponent<Button>().interactable = false;
                        }
                    }
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
                child.GetChild(i).GetComponent<Image>().color=Color.white;
            }
        }

    }
}
