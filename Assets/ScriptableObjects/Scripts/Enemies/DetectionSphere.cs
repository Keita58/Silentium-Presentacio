using System;
using UnityEngine;

public class DetectionSphere : MonoBehaviour
{
    public event Action OnEnter;
    public event Action OnExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
            OnEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
            OnExit?.Invoke();
    }
}
