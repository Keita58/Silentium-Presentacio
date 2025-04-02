using UnityEngine;

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
        int i = 0;
        for (i = 0; i < inventory.items.Count; i++)
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

   


}
