using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isOpen { get; private set; }
    //Es una porta que rota.
    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float speed = 1f;

    [Header("Rotation Configs")]
    [SerializeField] private float rotationAmount = 90f;

    [SerializeField] private float forwardDirection = 0;

    private Vector3 startRotation;

    private void Awake()
    {
        isOpen = false;
        startRotation = transform.rotation.eulerAngles;
    }

    public void Open(Vector3 playerPosition)
    {
        if (!isOpen)
        {
            StopAllCoroutines();
            //dado el forward que le pasamos y el segundo parametro es la direccion al player normalizada que nos devuelve negativo o positivo para saber si esta delante o detras.
            float dot = Vector3.Dot(this.transform.forward, (playerPosition - transform.position).normalized);
            StartCoroutine(DoRotationOpen(dot));
        }
    }

    private IEnumerator DoRotationOpen(float forwardAmount)
    {
        Quaternion startRotation = transform.parent.rotation;
        Quaternion endRotation;

        if (forwardAmount >= forwardDirection)
        {
            //se rotara negativamente
            endRotation = Quaternion.Euler(new Vector3(0, startRotation.y - rotationAmount, 0));
        }
        else
        {
            //se rotara positivamente

            endRotation = Quaternion.Euler(new Vector3(0, startRotation.y + rotationAmount, 0));
        }
        isOpen = true;
        float time = 0;
        while (time < 1)
        {
            //interpolamos entre el start y el end dado el time.
            transform.parent.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }

    }

    public void Close()
    {
        if (isOpen)
        {
            StopAllCoroutines();
            //dado el forward que le pasamos y el segundo parametro es la direccion al player normalizada que nos devuelve negativo o positivo para saber si esta delante o detras.
            //float dot = Vector3.Dot(this.transform.forward, (playerPosition - transform.position).normalized);
            StartCoroutine(DoRotationClose());
        }
    }

    private IEnumerator DoRotationClose()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(new Vector3(transform.parent.rotation.x, transform.parent.rotation.y, transform.parent.rotation.z));

        isOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.parent.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}
