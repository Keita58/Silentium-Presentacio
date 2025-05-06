using System.Linq;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private Transform[] _ImportantSpawns;
    [SerializeField] private Transform[] _NormalSpawns;

    [SerializeField] private Item[] _ImportantObjects;
    [SerializeField] private Item[] _NormalObjects;

    void Start()
    {
        int amp = 0;
        int cint = 0;
        foreach(Transform spawn in _ImportantSpawns)
        {
            int rand = Random.Range(0, 7);
            int pos = 0;

            if (rand >= 0 && rand < 3)
            {
                if ((amp + cint) % 2 == 1)
                {
                    if(amp > cint)
                    {
                        pos = 1; // Cinta americana
                        cint++;
                    }
                    else
                    {
                        pos = 0; // Ampolla de plastic
                        amp++;
                    }
                }
                else
                {
                    pos = 0; // Ampolla de plastic
                    amp++;
                }
            }
            else if (rand >= 3 && rand < 6)
            {
                if ((amp + cint) % 2 == 1)
                {
                    if(cint > amp)
                    {
                        pos = 0; // Ampolla de plastic
                        amp++;
                    }
                    else
                    {
                        pos = 1; // Cinta americana
                        cint++;
                    }
                }
                else
                {
                    pos = 1; // Cinta americana
                    cint++;
                }
            }
            else
                pos = 2; // Foto per guardar

            GameObject spawnObject = Instantiate(_ImportantObjects[pos].prefabToEquip, spawn);
        }

        int[] countItems = new int[_NormalObjects.Length];

        for(int i = 0; i < _NormalObjects.Length; i++)
        {
            countItems[i] = 0;
        }

        foreach (Transform spawn in _NormalSpawns)
        {
            int rand = Random.Range(0, 8);
            int pos = 0;


            if (rand == 0)
            {
                if (countItems[0] == 2)
                {
                    pos = 4; // Bales
                    countItems[4]++;
                }
                else
                {
                    pos = 0; // Llibre
                    countItems[0]++;
                }
            }
            else if (rand == 1)
            {
                if (countItems[0] == 2)
                {
                    pos = 3; // Paracetamol
                    countItems[3]++;
                }
                else
                {
                    pos = 1; // Ampolla
                    countItems[1]++;
                }
            }
            else if(rand == 2)
            {
                if (countItems[0] == 2)
                {
                    pos = 4; // Bales
                    countItems[4]++;
                }
                else
                {
                    pos = 2; // Boli
                    countItems[2]++;
                }
            }
            else if (rand >= 3 && rand < 5)
            {
                pos = 3; // Paracetamol
                countItems[3]++;
            }
            else
            {
                pos = 4; // Bales
                countItems[4]++;
            }

            GameObject spawnObject2 = Instantiate(_NormalObjects[pos].prefabToEquip, spawn);
        }
    }
}
