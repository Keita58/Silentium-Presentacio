using UnityEngine;

public class Wall : MonoBehaviour, IAtenuacio
{
    public int atenuarSo(int nivellSo)
    {
        return nivellSo -= 1;
    }
}