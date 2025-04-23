using System;
using UnityEngine;

public class DetectMorseEntry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public event Action onMorseRoomEnter;
    public event Action onMorseRoomLeave;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer==6) onMorseRoomEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.layer == 6) onMorseRoomLeave?.Invoke();
    }
}
