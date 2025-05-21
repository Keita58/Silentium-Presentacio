using UnityEngine;

[RequireComponent (typeof(CellBook))]
public class InteractuableCellBook : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    private void Awake()
    {
        isRemarkable = false;
    }

    public void Interact()
    {

    }

    public void Interact(GameObject equipedObject)
    {
        GetComponent<CellBook>().SetBook(equipedObject.GetComponent<Book>(), equipedObject);
        equipedObject.GetComponent<Book>().collider = GetComponent<CellBook>().GetComponent<BoxCollider>();
        equipedObject.GetComponent<Book>().placed = true;
        PuzzleManager.instance.CheckBookPuzzle();
        GetComponent<CellBook>().GetComponent<BoxCollider>().enabled = false;
        equipedObject.transform.rotation = Quaternion.identity;
        equipedObject.transform.parent = transform;
        equipedObject.transform.position = transform.GetChild(0).transform.position;
        InventoryManager.instance.UseEquippedItem();
    }
}
