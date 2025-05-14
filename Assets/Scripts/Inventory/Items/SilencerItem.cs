using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item - Silencer", menuName = "Scriptable Objects/Items/Item - Silencer")]
public class SilencerItem : Item
{
    [Header("Common values")]
    [SerializeField] private string nom;
    public override string Name => name;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

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

    [SerializeField] private int silencerId;
    public override int id => silencerId;

    [Header("Item resulting of combination")]
    [SerializeField] private Item silencer;

    public override void Use()
    {
        InventoryManager.instance.UseSilencer(this);
    }

    public override void Combine(Item item)
    {
        Debug.Log("Rep aquest item per a combinar: " + item);
        if ((item.ItemType == ItemTypes.TAPE && this.itemType == ItemTypes.PLASTICBOTTLE) ||( item.ItemType == ItemTypes.PLASTICBOTTLE && this.itemType == ItemTypes.TAPE))
        {
            InventoryManager.instance.AddNewItemAfterCombine(silencer);
        }
    }

    public override void Equip()
    {
    }
}
