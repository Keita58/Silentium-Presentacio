using System;
using UnityEngine;

public class DetectMorseEntry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public event Action onMorseRoomEnter;
    public event Action onMorseRoomLeave;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entro collider:" + other.name);
        if (other.TryGetComponent(out Player player)) onMorseRoomEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player)) onMorseRoomLeave?.Invoke();
    }
}
