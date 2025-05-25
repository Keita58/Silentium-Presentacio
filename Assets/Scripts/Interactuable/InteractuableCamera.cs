using System;
using UnityEngine;

[RequireComponent (typeof(CameraSave))]
public class InteractuableCamera : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }

    public bool isInteractuable { get; set; }

    public event Action OnCameraClick;

    private void Awake()
    {
        isRemarkable = true;
        isInteractuable = true;
    }

    public void Interact()
    {
        OnCameraClick?.Invoke();
    }
}
