using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private int _Damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
            player.TakeDamage(_Damage);
    }
}
