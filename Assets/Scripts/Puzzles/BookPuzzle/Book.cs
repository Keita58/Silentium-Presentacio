using System;
using UnityEngine;

public class Book : MonoBehaviour
{
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
        }else if(other.tag == "BadPositionBook")
        {
            Debug.Log("Skill Issue");
        }
    }
}
