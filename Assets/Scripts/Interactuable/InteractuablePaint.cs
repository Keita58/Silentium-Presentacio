using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InteractuablePaint : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    public bool isInteractuable { get; set; }

    private void Awake()
    {
        isRemarkable = true;
        isInteractuable = true;
    }

    public void Interact()
    {
        PuzzleManager.instance.InteractHieroglyphicPuzzle();
    }

}
