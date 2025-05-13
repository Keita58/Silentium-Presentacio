using UnityEngine;

public class PickObject : MonoBehaviour
{
    [SerializeField] private Player _Player;
    public GameObject Object;
    public bool Picked { get; private set; }

    private void Awake()
    {
        Picked = false;
        _Player.onPickItem += SetPicked;
    }

    public void SetPicked()
    {
        Picked = true;
        _Player.onPickItem -= SetPicked;
    }
}
