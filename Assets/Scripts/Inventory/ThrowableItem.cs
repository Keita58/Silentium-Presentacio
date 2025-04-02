using UnityEngine;

[CreateAssetMenu(fileName = "Item - Throwable", menuName = "Scriptable Objects/Items/Item - Throwable")]
public class ThrowableItem : Item
{
    [Header("Common values")]
    [SerializeField] private string nom;
    public override string Name => name;
    [SerializeField] private string description;
    public override string Description => description;

    [SerializeField] private Sprite sprite;
    public override Sprite Sprite => sprite;

    [SerializeField] private GameObject itemPrefab;

    public GameObject ItemPrefab => itemPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Use()
    {
        GameManager.instance.UseThrowableItem(this, itemPrefab);
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }

}
