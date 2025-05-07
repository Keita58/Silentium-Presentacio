using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteInventorySO", menuName = "Scriptable Objects/NoteInventorySO")]
public class NoteInventorySO : ScriptableObject
{

    [SerializeField] 
    public List<NotesSO> notes;


    public void AddNote(NotesSO note)
    {
        if (!notes.Contains(note))
        {
            notes.Add(note);
        }
    }
    
}
