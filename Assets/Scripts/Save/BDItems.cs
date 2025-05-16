using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "BDItems", menuName = "Scriptable Objects/BDItems")]
public class BDItems : ScriptableObject
{
    [SerializeField]
    List<Item> ScriptableExemples;

    public List<Item> FromIDs(int[] listIDs)
    {
        Item ScriptableFromID(int id)
        {
            foreach (Item currentScriptable in ScriptableExemples)
            {
                if (currentScriptable.id == id)
                    return currentScriptable;
            }
            return null;
        }

        List<Item> scriptableExemples = new List<Item>(listIDs.Length);

        foreach (int id in listIDs)
            scriptableExemples.Add(ScriptableFromID(id));

        return scriptableExemples;
    }

    public Item FromID(int ID)
    {
        Item ScriptableFromID(int id)
        {
            foreach (Item currentScriptable in ScriptableExemples)
            {
                if (currentScriptable.id == id)
                    return currentScriptable;
            }
            return null;
        }

        return ScriptableFromID(ID);
    }

    public int[] ToIDs(ReadOnlyCollection<Item> listScriptables)
    {
        int[] listIDs = new int[listScriptables.Count];

        for (int i = 0; i < listScriptables.Count; ++i)
        {
            listIDs[i] = listScriptables[i].id;
        }

        return listIDs;
    }
}
