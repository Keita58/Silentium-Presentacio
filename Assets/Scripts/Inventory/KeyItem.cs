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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Use()
    {
        GameManager.instance.UseKeyItem(this);
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}
