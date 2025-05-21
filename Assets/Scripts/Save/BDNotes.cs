using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "BDNotes", menuName = "Scriptable Objects/BDNotes")]
public class BDNotes : ScriptableObject
{
    [SerializeField]
    List<NotesSO> ScriptableExemples;

    public List<NotesSO> FromIDs(int[] listIDs)
    {
        NotesSO ScriptableFromID(int id)
        {
            foreach (NotesSO currentScriptable in ScriptableExemples)
            {
                if (currentScriptable.noteId == id)
                    return currentScriptable;
            }
            return null;
        }

        List<NotesSO> scriptableExemples = new List<NotesSO>(listIDs.Length);

        foreach (int id in listIDs)
            scriptableExemples.Add(ScriptableFromID(id));

        return scriptableExemples;
    }

    public int[] ToIDs(ReadOnlyCollection<NotesSO> listScriptables)
    {
        int[] listIDs = new int[listScriptables.Count];

        for (int i = 0; i < listScriptables.Count; ++i)
        {
            listIDs[i] = listScriptables[i].noteId;
        }

        return listIDs;
    }
}
