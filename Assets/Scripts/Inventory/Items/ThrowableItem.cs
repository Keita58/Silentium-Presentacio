using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item - Throwable", menuName = "Scriptable Objects/Items/Item - Throwable")]
public class ThrowableItem : Item
{
    [Header("Common values")]
    [SerializeField] private string itemName;
    public override string Name => itemName;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

    [SerializeField] private GameObject prefab;
    public override GameObject prefabToEquip => prefab;

    [SerializeField] private GameObject prefabInst;
    public override GameObject prefabToInstantiate => prefabInst;

    [SerializeField] private bool stackable;
    public override bool isStackable => stackable;

    [SerializeField] private ItemTypes itemType;
    public override ItemTypes ItemType => itemType;

    public override List<Item> combinableItems => throw new System.NotImplementedException();

    [SerializeField] private bool usable;
    public override bool isUsable => usable;

    [SerializeField] private bool equipable;
    public override bool isEquipable => equipable;

    [SerializeField] private bool combinable;
    public override bool isCombinable => combinable;

    [SerializeField] private int throwableId;
    public override int id => throwableId;

    public override void Equip()
    {
        InventoryManager.instance.EquipThrowableItem(this, prefab);
    }
}
