using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotesButton : MonoBehaviour
{
    [SerializeField] NotesSO note;

    [SerializeField] public int id;


    private void Awake()
    {
        this.GetComponent<Button>().interactable = false;
    }
    public void OnSelectNote()
    {
        InventoryManager.instance.OpenNote(note);
    }
}
