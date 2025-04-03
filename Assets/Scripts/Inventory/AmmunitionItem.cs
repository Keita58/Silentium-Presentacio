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

    [Header("Specific values")]
    [SerializeField] public int bales;


    public override void Use()
    {
        GameManager.instance.UseAmmo(bales, this);
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }

    public override void Equip()
    {
        throw new System.NotImplementedException();
    }
}
