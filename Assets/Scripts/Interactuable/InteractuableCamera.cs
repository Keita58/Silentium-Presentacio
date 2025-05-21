using System;
using UnityEngine;

[RequireComponent (typeof(CameraSave))]
public class InteractuableCamera : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    public event Action onCameraClick;

    private void Awake()
    {
        isRemarkable = true;
    }

    public void Interact()
    {
        onCameraClick?.Invoke();
    }
}
