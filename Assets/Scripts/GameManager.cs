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

    public void AddItem(Item item)
    {
        inventory.AddItem(item);
        Debug.Log("Afegeixo item "+item.name);
    }

    public void OpenInventory(GameObject target)
    {
        inventoryUI.target = target;
        inventoryUI.Show();
    }

    public void CloseInventory()
    {
        inventoryUI.target = null;
        inventoryUI.Hide();
    }

    public void UseHealingItem(int healing, Item item)
    {
        //player.hp+=curacion;
        Debug.Log("Player usa item de curaci�n");
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
