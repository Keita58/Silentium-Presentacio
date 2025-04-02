using UnityEngine;

public class ShowInventory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public GameObject target;

    [SerializeField] GameObject parentGameObject;

    [SerializeField] InventorySO inventory;

    [SerializeField] GameObject itemPrefab;

    [SerializeField] private Item itemSelected;

    [SerializeField] GameObject buttonsActionRoot;


    private void Start()
    {
        InitState(ActionStates.NOACTION);
    }
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

    #region FSM
    enum ActionStates { NOACTION, SELECT_ACTION, ACTION_USE, ACTION_COMBINE, ACTION_EQUIP_ITEM }
    [SerializeField] ActionStates actionState;
    private void ChangeState(ActionStates newState)
    {
        Debug.Log($"---------------------- Sortint de {actionState} a {newState} ------------------------");
        ExitState(actionState);

        Debug.Log($"---------------------- Entrant a {newState} ------------------------");
        InitState(newState);
    }

    public void ItemSelected(Item item)
    {
        itemSelected=item;
        ChangeState(ActionStates.SELECT_ACTION);
    }

    private void ToggleActionsButtons(bool show)
    {
        for (int i = 0; i < buttonsActionRoot.transform.childCount; i++)
        {
            buttonsActionRoot.transform.GetChild(i).gameObject.SetActive(show);
        }
    }

    private void InitState(ActionStates newState) 
    {
        actionState = newState;
        switch (actionState)
        {
            case ActionStates.NOACTION:
                ToggleActionsButtons(false);
                break;
            case ActionStates.SELECT_ACTION:
                ToggleActionsButtons(true);
                break;
            case ActionStates.ACTION_USE:
                break;
            case ActionStates.ACTION_COMBINE:
                break;
            case ActionStates.ACTION_EQUIP_ITEM:
                break;
        }
        
    }
    private void ExitState(ActionStates currentState)
    {
        switch (currentState)
        {
            case ActionStates.NOACTION:
                break;
            case ActionStates.SELECT_ACTION:
                break;
            case ActionStates.ACTION_USE:
                break;
            case ActionStates.ACTION_COMBINE:
                break;
            case ActionStates.ACTION_EQUIP_ITEM:
                break;
        }
    }
    #endregion


}
