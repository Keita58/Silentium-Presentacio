using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotesButton : MonoBehaviour
{
    [SerializeField] NotesSO note;

    [SerializeField] public int id;

    public void OnSelectNote()
    {
        InventoryManager.instance.OpenNote(note);
    }
}
