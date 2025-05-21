using UnityEngine;

public class InteractuebleClock : MonoBehaviour, IInteractuable
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
