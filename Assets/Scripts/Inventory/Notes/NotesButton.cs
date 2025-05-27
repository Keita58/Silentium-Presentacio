using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotesButton : MonoBehaviour
{
    [SerializeField] NotesSO note;

    [SerializeField] public int id;

    public void OnSelectNote()
    {
        if (note.noteType != NotesSO.NoteType.Scroll)
            InventoryManager.instance.OpenDiaryNote(note);
        else
            InventoryManager.instance.OpenDiaryNoteScroll(note);
    }
}
