using UnityEngine;

[CreateAssetMenu(fileName = "NotesSO", menuName = "Scriptable Objects/NotesSO")]
public class NotesSO : ScriptableObject
{
    public string Name;
    public string noteId;
    public string noteContent;
}
