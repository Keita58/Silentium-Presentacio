using System;
using System.Collections.Generic;
using UnityEngine;

public class BookPuzzle : MonoBehaviour
{
    [Header("Posicion de cada libro")]
    [SerializeField]
    List<CellBook> bookList;
    [Header("Libros que hay que mirar")]
    [SerializeField]
    List<GameObject> booksToCheck;
    private void Start()
    {

    }

    void checkBookPosition()
    {
        if (bookList[0].GetBook() == booksToCheck[2] && bookList[1].GetBook() == booksToCheck[0] && bookList[2].GetBook() == booksToCheck[4] && bookList[3].GetBook() == booksToCheck[1] && bookList[4].GetBook() == booksToCheck[3])
        {
            Console.WriteLine("Lo has completado!!!!!!");
        }
    }


}