using NavKeypad;
using UnityEngine;

[RequireComponent (typeof(MorseKeypad))]
public class InteractuableMorse : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    private void Awake()
    {
        isRemarkable = true;
    }
    public void Interact()
    {
        PuzzleManager.instance.InteractMorsePuzzle();
    }
}
