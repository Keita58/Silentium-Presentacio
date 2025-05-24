using UnityEngine;

public class Wall : MonoBehaviour, IAttenuable
{
    public int AttenuateSound(int soundLvl)
    {
        return soundLvl -= 1;
    }
}