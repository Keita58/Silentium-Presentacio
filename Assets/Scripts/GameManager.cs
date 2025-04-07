using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance { get; private set; }
    [SerializeField] Player player;
    [SerializeField] InventorySO inventory;
    [SerializeField] ShowInventory inventoryUI;
    private void Awake()
    {
        if (instance==null)
            instance = this;
    }

    public void SolvedClock()
    {

    }
}
