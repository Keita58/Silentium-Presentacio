using System;
using UnityEngine;

public class DetectionBox : MonoBehaviour
{
    public event Action<Player> OnEnter;
    public event Action OnExit;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
            OnEnter?.Invoke(player);
    }
}
