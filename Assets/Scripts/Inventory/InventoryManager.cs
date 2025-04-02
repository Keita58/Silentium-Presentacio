using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }

    [SerializeField] private Item itemSelected;

    [SerializeField] GameObject buttonsActionRoot;

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
}
