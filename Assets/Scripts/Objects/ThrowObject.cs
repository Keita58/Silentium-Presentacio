using System;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    [SerializeField] private int soundIntensity;
    [SerializeField] public GameObject camaraPrimera;
    [SerializeField] private PostProcessEvents events;
    void Start()
    {  
        this.AddComponent<CapsuleCollider>();
    }
    bool lanzado = false;
    public void Lanzar()
    {
        if (!lanzado)
        {
            float u = 5 / this.GetComponent<Rigidbody>().mass;
            float t = 2 * u / Physics.gravity.magnitude;
            Vector3 AB = (camaraPrimera.transform.forward + camaraPrimera.transform.forward * 4) - camaraPrimera.transform.forward;
            Vector3 h = AB / t;
            Vector3 H = h * this.GetComponent<Rigidbody>().mass;
            Vector3 F = H + 8 * Vector3.up;
            this.GetComponent<Rigidbody>().AddForce(F, ForceMode.Impulse);
            lanzado = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name != "Player" && lanzado) {
            lanzado=false;
            Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, 30);
            foreach (Collider collider in colliderHits)
            {
                Debug.Log("Enemic: "+collider.gameObject.name);
                if (collider.gameObject.TryGetComponent<Enemy>(out Enemy en))
                {
                    en.ListenSound(this.transform.position, soundIntensity);
                }
            }
            events.MakeThrowImpactSound();
            this.transform.parent.gameObject.SetActive(false);
        }
    }

}
