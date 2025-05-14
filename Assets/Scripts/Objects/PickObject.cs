using System;
using UnityEngine;

[Serializable]
public class PickObject : MonoBehaviour
{
    [SerializeField] private Player _Player;
    public GameObject Object;
    [SerializeField] public bool Picked;
    [SerializeField] public int Id; 

    public PickObject (bool picked, int id)
    {
        this.Picked = picked;
        this.Id = id;
    }

    private void Awake()
    {
        Picked = false;
        _Player.onPickItem += SetPicked;
    }

    public void SetPicked(int id)
    {
        if(id == this.Id)
            Picked = true;
        _Player.onPickItem -= SetPicked;
    }

    [Serializable]
    public class PickObjectSave
    {
        [SerializeField] public int Id;
        [SerializeField] public bool Picked;

        public PickObjectSave(int id, bool picked)
        {
            this.Id = id;
            this.Picked = picked;
        }
    }
}
