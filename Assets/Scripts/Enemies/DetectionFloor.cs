using UnityEngine;
using UnityEngine.AI;

public class DetectionFloor : MonoBehaviour
{
    [SerializeField] private BlindEnemy _Enemy;
    [SerializeField] private Collider _Collider;
    [SerializeField] private Rigidbody _Rigidbody;
    [SerializeField] private NavMeshAgent _NavMeshAgent;
    [SerializeField] private Animator _Animator;
    
    private bool _IsJumping;

    private void Awake()
    {
        _Enemy.OnJump += ActivateBool;
    }

    private void OnDestroy()
    {
        _Enemy.OnJump -= ActivateBool;
    }

    private void ActivateBool()
    {
        _IsJumping = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_IsJumping && other.CompareTag("Floor"))
        {
            _Rigidbody.isKinematic = true;
            _NavMeshAgent.enabled = true;
            _Animator.enabled = true;
            _Collider.enabled = true;
            _Enemy.ActivatePatrol();
        }
    }
}