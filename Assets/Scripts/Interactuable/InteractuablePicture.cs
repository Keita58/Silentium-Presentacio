using UnityEngine;

[RequireComponent (typeof(Picture))]
public class InteractuablePicture : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    private void Awake()
    {
        isRemarkable = true;
    }
    public void Interact()
    {
        PuzzleManager.instance.picturesClicked.Add(GetComponent<Picture>());
        PuzzleManager.instance.TakePoemPart();
    }
}
