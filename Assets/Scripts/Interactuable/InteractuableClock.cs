using UnityEngine;

public class InteractuableClock : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    private void Awake()
    {
        isRemarkable = true;
    }
    public void Interact()
    {
        PuzzleManager.instance.InteractClockPuzzle();
    }
}
