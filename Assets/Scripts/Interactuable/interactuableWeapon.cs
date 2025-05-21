using UnityEngine;

public class InteractuableWeapon : MonoBehaviour, IInteractuable
{
    public bool isRemarkable {  get; private set; }

    private void Awake()
    {
        isRemarkable = true;
    }

    public void Interact()
    {
        PuzzleManager.instance.InteractWeaponPuzzle();
    }
}
