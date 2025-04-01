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

    public override void Use()
    {
        GameManager.instance.UseHealingItem(healing, this);
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}
