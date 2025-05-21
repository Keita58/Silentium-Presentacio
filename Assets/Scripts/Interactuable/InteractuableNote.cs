using UnityEngine;

[RequireComponent (typeof(Notes))]
public class InteractuableNote : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }

    private void Awake()
    {
        isRemarkable = true;
    }
    public void Interact()
    {
        NotesSO noteSO = GetComponent<Notes>().note;
        if (noteSO.noteId < 6)
        {
            InventoryManager.instance.DiscoverNote(noteSO);
            gameObject.SetActive(false);
        }
        else
        {
            if (noteSO.noteType == NotesSO.NoteType.Scroll)
            {
                InventoryManager.instance.ShowNoteScroll(noteSO);
            }
            else if (noteSO.noteType == NotesSO.NoteType.Image)
            {
                InventoryManager.instance.ShowImageNote(noteSO.noteContent);

            }
            else if (noteSO.noteType == NotesSO.NoteType.Book)
            {
                InventoryManager.instance.ShowBookNote(noteSO.noteContent);
            }
            else
                InventoryManager.instance.ShowNote(noteSO);
        }
    }
}
