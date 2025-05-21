using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static UnityEditor.Progress;

[CreateAssetMenu(fileName = "BDBooks", menuName = "Scriptable Objects/BDBooks")]
public class BDBooks : ScriptableObject
{
    [SerializeField]
    List<GameObject> ScriptableExemples;

    public List<GameObject> FromIDs(int[] listIDs)
    {
        GameObject ScriptableFromID(int id)
        {
            foreach (GameObject currentScriptable in ScriptableExemples)
            {
                if (currentScriptable.GetComponent<PickItem>().item.id == id)
                    return currentScriptable;
            }
            return null;
        }

        List<GameObject> scriptableExemples = new List<GameObject>(listIDs.Length);

        foreach (int id in listIDs)
            scriptableExemples.Add(ScriptableFromID(id));

        return scriptableExemples;
    }

    public GameObject FromID(int ID)
    {
        GameObject ScriptableFromID(int id)
        {
            foreach (GameObject currentScriptable in ScriptableExemples)
            {
                if (currentScriptable.GetComponent<PickItem>().item.id == id)
                    return currentScriptable;
            }
            return null;
        }

        return ScriptableFromID(ID);
    }

    public int[] ToIDs(ReadOnlyCollection<GameObject> listScriptables)
    {
        int[] listIDs = new int[listScriptables.Count];

        for (int i = 0; i < listScriptables.Count; ++i)
        {
            listIDs[i] = listScriptables[i].GetComponent<PickItem>().item.id;
        }

        return listIDs;
    }
}
