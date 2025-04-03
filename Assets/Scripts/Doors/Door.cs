using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isOpen { get; private set;}
    //Es una porta que rota.
    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float speed = 1f;

    [Header("Rotation Configs")]
    [SerializeField] private float rotationAmount=90f;

    [SerializeField] private float forwardDirection=0;

    private Vector3 startRotation;

    //To control if the animation is in progress.
    private Coroutine animationCoroutine;

    private void Awake()
    {
        isOpen = false;
        startRotation = transform.rotation.eulerAngles;
    }

    public void Open(Vector3 playerPosition)
    {
        if (!isOpen)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }


        }

    }
}
