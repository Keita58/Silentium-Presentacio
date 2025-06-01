using System;
using UnityEngine;

public class DetectionDoorSphere : MonoBehaviour
{
    public event Action<Door> OnDetectDoor;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out InteractuableDoor _))
            OnDetectDoor?.Invoke(other.GetComponent<Door>());
    }
}
