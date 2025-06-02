using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private int _Damage;
    [SerializeField] private BlindEnemy _Enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Player player)) return;
        if (_Enemy)
        {
            if (_Enemy.Jumping)
            {
                player.TakeDamage(_Damage);
                Debug.Log($"L'enemic cec fa: " + _Damage);
            }
        }
        else
        {
            player.TakeDamage(_Damage);
        }
    }
}
