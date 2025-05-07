using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item - Save", menuName = "Scriptable Objects/Items/Item - Save")]
public class SaveItem : Item
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

    public override void Use()
    {
        throw new System.NotImplementedException();
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
