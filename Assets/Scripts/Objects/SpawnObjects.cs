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

        foreach (Transform spawn in _NormalSpawns)
        {
            int rand = Random.Range(0, 9);
            int pos = 0;

            if (rand >= 0 && rand < 2)
            {
                pos = 0; // Llibre
            }
            else if (rand >= 2 && rand < 4)
            {
                pos = 1; // Ampolla
            }
            else if(rand >= 4 && rand < 6)
            {
                pos = 2; // Boli
            }
            else if (rand >= 6 && rand < 8)
            {
                pos = 3; // Paracetamol
            }
            else
                pos = 4; // Bales

            GameObject spawnObject2 = Instantiate(_NormalObjects[pos].prefabToEquip, spawn);
        }
    }
}
