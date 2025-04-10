using UnityEngine;

public class CellBook : MonoBehaviour
{
    [SerializeField]
    private Book book;

    public void SetBook(Book b)
    {
        this.book = b;
    }
    public Book GetBook()
    {
        return book; ;
    }
}
