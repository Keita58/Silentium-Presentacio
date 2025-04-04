using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField] private Item itemSelected;

    [SerializeField] GameObject buttonsActionRoot;

    [SerializeField] private ShowInventory inventoryUI;

    [SerializeField] Player player;
    [SerializeField] InventorySO inventory;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        InitState(ActionStates.NOACTION);
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
        itemSelected = item;
        //inventoryUI.GetComponent<ShowInventory>().ItemSelected(itemSelected);
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
                itemSelected.Use();
                ChangeState(ActionStates.NOACTION);
                break;
            case ActionStates.ACTION_COMBINE:
                inventoryUI.ShowHideItemsToCombine(itemSelected.combinableItems, itemSelected);
                break;
            case ActionStates.ACTION_EQUIP_ITEM:
                itemSelected.Equip();
                ChangeState(ActionStates.NOACTION);
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


    public void UseItem()
    {
        ChangeState(ActionStates.ACTION_USE);
    }

    public void CombineItem()
    {
        ChangeState(ActionStates.ACTION_COMBINE);
    }

    public void EquipItem()
    {
        ChangeState(ActionStates.ACTION_EQUIP_ITEM);
    }

    public void AddItem(Item item)
    {
        inventory.AddItem(item);
        Debug.Log("Afegeixo item " + item.name);
    }

    public void OpenInventory(GameObject target)
    {
        inventoryUI.target = target;
        inventoryUI.Show();
        ChangeState(ActionStates.NOACTION);
    }

    public void CloseInventory()
    {
        inventoryUI.target = null;
        inventoryUI.Hide();
    }

    public void UseHealingItem(int healing, Item item)
    {
        //player.hp+=curacion;
        Debug.Log("Player usa item de curacion");
        inventory.UseItem(item);
    }

    public void UseAmmo(int numAmmo, Item item)
    {
        //player.RecarregaBales(numBales);
        Debug.Log("Player recarrega les bales");
        inventory.UseItem(item);
    }
    public void UseKeyItem(Item item)
    {
        inventory.UseItem(item);
    }

    public void EquipThrowableItem(Item item, GameObject equipableObject)
    {
        inventory.UseItem(item);
        player.EquipItem(equipableObject);
    }
}
