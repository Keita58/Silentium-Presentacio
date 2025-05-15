using System;
using UnityEngine;

[Serializable]
public class CellBook : MonoBehaviour
{
    [SerializeField]
    private Book book;

    [SerializeField]
    public GameObject bookGO;

    public void SetBook(Book b, GameObject go)
    {
        this.book = b;
        this.bookGO = go;
    }

    public Book GetBook()
    {
        return book; 
    }

    [Serializable]
    public class CellBookSave
    {
        [SerializeField] public int bookGO;

        public CellBookSave(int bookGO) 
        {
            this.bookGO = bookGO;
        }
    }
}
