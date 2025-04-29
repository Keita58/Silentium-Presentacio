using UnityEngine;

[CreateAssetMenu(fileName = "NotesSO", menuName = "Scriptable Objects/NotesSO")]
public class NotesSO : ScriptableObject
{
    public string Name;
    public int noteId;
    public string noteContent;
    public enum NoteType
    {
        Image,
        Scroll,
        Normal,
        Book
    }
    public NoteType noteType;
}
