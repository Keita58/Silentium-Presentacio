using UnityEngine;

[RequireComponent(typeof(WeaponPuzzle))]
public class InteractuableWeapon : MonoBehaviour, IInteractuable
{
    public bool isRemarkable {  get; private set; }
    public bool isInteractuable { get;  set; }

    private void Awake()
    {
        isRemarkable = false;
        isInteractuable = true;
    }

    public void Interact()
    {
        PuzzleManager.instance.InteractWeaponPuzzle();
    }
}
