using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isOpen { get; private set;}
    //Es una porta que rota.
    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float speed = 1f;

    private void Awake()
    {
        isOpen = false;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
