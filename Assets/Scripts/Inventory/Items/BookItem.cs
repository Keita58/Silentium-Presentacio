using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item - Book", menuName = "Scriptable Objects/Items/Item - Book")]
public class BookItem : Item
{
    [Header("Common values")]
    [SerializeField] private string nom;
    public override string Name => name;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

    [Header("Specific values")]

    [SerializeField] private GameObject prefab;
    public override GameObject prefabToEquip => prefab;

    [SerializeField] private GameObject prefabInst;
    public override GameObject prefabToInstantiate => prefabInst;

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

    [SerializeField] private int bookId;
    public override int id => bookId;

    public override void Use()
    {
    }

    public override void Combine(Item item)
    {

    }

    public override void Equip()
    {
        InventoryManager.instance.EquipThrowableItem(this, prefab);

    }
}
