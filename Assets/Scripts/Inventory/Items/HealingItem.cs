using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item - Healing", menuName = "Scriptable Objects/Items/Item - Healing")]
public class HealingItem : Item
{
    [Header("Common values")]
    [SerializeField] private string nom;
    public override string Name => name;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

    [Header("Specific values")]
    [SerializeField] private int healing;
    public int Healing => healing;

    [SerializeField] private GameObject prefab;
    public override GameObject prefabToEquip => prefab;


    [SerializeField] private List<Item> itemsToCombine;
    public override List<Item> combinableItems => itemsToCombine;

    [SerializeField] private bool stackable;
    public override bool isStackable => stackable;

    [SerializeField] private ItemTypes itemType;
    public override ItemTypes ItemType => itemType;

    [SerializeField] private bool usable;
    public override bool isUsable => usable;

    [SerializeField] private bool equipable;
    public override bool isEquipable => equipable;

    [SerializeField] private bool combinable;
    public override bool isCombinable => combinable;

    [SerializeField] private int healId;
    public override int id => healId;


    [SerializeField] private Item itemAfterCombineParacetamol;

    public override void Use()
    {
        InventoryManager.instance.UseHealingItem(healing, this);
    }

    public override void Combine(Item item)
    {
        Debug.Log("Rep aquest item per a combinar: " + item);
        Debug.Log("Es throwable?"+item is ThrowableItem);
        if (item.ItemType == ItemTypes.PARACETAMOL && this.itemType==ItemTypes.PARACETAMOL)
        {
            InventoryManager.instance.AddNewItemAfterCombine(itemAfterCombineParacetamol);
        }
    }

    public override void Equip()
    {
        throw new System.NotImplementedException();
    }
}
