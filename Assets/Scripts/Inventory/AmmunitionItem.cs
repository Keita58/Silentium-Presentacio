using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item - Amunition", menuName = "Scriptable Objects/Items/Item - Amunition")]
public class AmmunitionItem : Item
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

    public override List<Item> combinableItems => throw new System.NotImplementedException();

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


    [Header("Specific values")]
    [SerializeField] public int bales;


    public override void Use()
    {
        InventoryManager.instance.UseAmmo(bales, this);
    }

    public override void Combine(Item item)
    {
        throw new System.NotImplementedException();
    }

    public override void Equip()
    {
        throw new System.NotImplementedException();
    }
}
