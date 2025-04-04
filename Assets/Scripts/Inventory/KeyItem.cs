using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item - Key", menuName = "Scriptable Objects/Items/Item - Key")]

public class KeyItem : Item
{
    [Header("Common values")]
    [SerializeField] private string nom;
    public override string Name => nom;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

    [SerializeField] private GameObject prefab;
    public override GameObject prefabToEquip => prefab;

    public override List<Item> combinableItems => throw new System.NotImplementedException();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Use()
    {
        InventoryManager.instance.UseKeyItem(this);
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
