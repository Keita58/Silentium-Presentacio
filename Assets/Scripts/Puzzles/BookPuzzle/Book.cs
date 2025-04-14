using System;
using UnityEngine;

public class Book : MonoBehaviour
{
    public bool placed = false;
    public BoxCollider collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "GoodPositionBook ")
        {
            other.GetComponent<CellBook>().SetBook(this);
            placed = true;
            collider = other.GetComponent<BoxCollider>();
            collider.enabled = false;
        }else if(other.tag == "BadPositionBook")
        {
            placed = true;
            collider = other.GetComponent<BoxCollider>();
            collider.enabled = false;
            Debug.Log("Skill Issue");
        }
    }
}
