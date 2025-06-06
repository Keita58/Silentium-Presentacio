using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BookPuzzle : MonoBehaviour
{
    [Header("Posicion de cada libro")]
    [SerializeField]
    List<CellBook> bookList;

    [Header("Audio")]
    AudioSource bookAudioSource;
    [SerializeField]
    AudioClip finishPuzzle;
    private void Awake()
    {
        bookAudioSource = GetComponent<AudioSource>();
    }
    public void checkBookPosition()
    {
        if (bookList.ElementAt(0).GetBook() != null && bookList.ElementAt(1).GetBook() != null && bookList.ElementAt(2).GetBook() != null && bookList.ElementAt(3).GetBook() != null && bookList.ElementAt(4).GetBook() != null){
            if (bookList.ElementAt(0).GetBook().GetComponent<PickItem>().item.ItemType == ItemTypes.BOOK2 && bookList.ElementAt(1).GetBook().GetComponent<PickItem>().item.ItemType == ItemTypes.BOOK4 && bookList.ElementAt(2).GetBook().GetComponent<PickItem>().item.ItemType == ItemTypes.BOOK3 && bookList.ElementAt(3).GetBook().GetComponent<PickItem>().item.ItemType == ItemTypes.BOOK5 && bookList.ElementAt(4).GetBook().GetComponent<PickItem>().item.ItemType == ItemTypes.BOOK1)
            {
                bookAudioSource.PlayOneShot(finishPuzzle);
                PuzzleManager.instance.BookPuzzleCompleted();
                Debug.Log("Lo has completado!!!!!!");
            }
        }
        else { Debug.Log("No has completado el puzzle"); }
    }
}