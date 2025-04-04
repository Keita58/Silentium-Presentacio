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

    public override void Use()
    {
        InventoryManager.instance.UseHealingItem(healing, this);
    }

    public override void Combine()
    {
        
    }

    public override void Equip()
    {
        throw new System.NotImplementedException();
    }
}
