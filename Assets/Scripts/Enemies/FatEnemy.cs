using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FatEnemy : Enemy
{
    private enum EnemyStates { PATROL, ATTACK, KNOCKED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private GameObject _DetectionSphere;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private Vector3 _PointOfPatrol;
    private bool _Patrolling;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private readonly int MAXHEALTH = 6;
    private int _RangeSearchSound;

    private Coroutine _PatrolCoroutine;
    private Coroutine _AttackCoroutine;
    private Coroutine _ActivateAttackCoroutine;
    private Coroutine _ChangeToPatrolCoroutine;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _PointOfPatrol = transform.position;
        _RangeSearchSound = 50;
        _Patrolling = false;
        _Hp = MAXHEALTH;

        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateAttackCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateAttackCoroutine;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
    }

    #region FSM

    private void ChangeState(EnemyStates newState)
    {
        Debug.Log($"---------------------- Sortint de {_CurrentState} a {newState} ------------------------");
        ExitState(_CurrentState);

        Debug.Log($"---------------------- Entrant a {newState} ------------------------");
        InitState(newState);
    }

    private void InitState(EnemyStates newState)
    {
        _CurrentState = newState;
 
        switch (_CurrentState)
        {
            case EnemyStates.PATROL:
                _Patrolling = false;
                _PatrolCoroutine = StartCoroutine(Patrol(_RangeSearchSound, _PointOfPatrol));
                break;
            case EnemyStates.ATTACK:
                _AttackCoroutine = StartCoroutine(AttackPlayer());
                break;
            case EnemyStates.KNOCKED:
                StartCoroutine(WakeUp());
                break;
        }
    }

    private void ExitState(EnemyStates exitState)
    {
        switch (exitState)
        {
            case EnemyStates.PATROL:
                StopCoroutine(_PatrolCoroutine);
                _ChangeToPatrolCoroutine = null;
                break;
            case EnemyStates.ATTACK:
                StopCoroutine(_AttackCoroutine);
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
        }
    }

    #endregion 

    // Funció per moure l'enemic pel mapa
    IEnumerator Patrol(int range, Vector3 pointOfSearch)
    {
        Vector3 point = Vector3.zero;
        while (true)
        {
            if (!_Patrolling)
            {
                RandomPoint(pointOfSearch, range, out Vector3 coord);
                point = coord;
                _NavMeshAgent.SetDestination(new Vector3(point.x, point.y, point.z));
                _Patrolling = true;
            }

            if (_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _Patrolling = false;
                yield return new WaitForSeconds(20);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    //Busca punt aleatori dins del NavMesh
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = new Vector3(center.x, center.y, center.z) + Random.insideUnitSphere * range;

            //Aquí s'haurà de comprovar si la y que hem extret està en algun dels pisos de l'edifici.
            //Si està aprop la transformem en aquest i ja

            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            NavMeshHit hit;
            
            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(point, out hit, 1.0f, NavMesh.AllAreas) && Vector3.Distance(point, center) > 1.75f)
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    public override void ListenSound(Vector3 pos, int lvlSound)
    {
        _SoundPos = pos;
        RaycastHit[] hits = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<IAtenuacio>(out IAtenuacio a))
            {
                lvlSound = a.atenuarSo(lvlSound);
            }
        }

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 3)
            {
                _RangeSearchSound = 7;
            }
            else if (lvlSound > 3 && lvlSound <= 7)
            {
                _RangeSearchSound = 5;
            }
            else if (lvlSound > 7 && lvlSound <= 10)
            {
                _RangeSearchSound = 2;
            }
            else if (lvlSound > 10)
            {
                _NavMeshAgent.SetDestination(_SoundPos);
            }

            _PointOfPatrol = pos;
            if (_ChangeToPatrolCoroutine == null)
            {
                _ChangeToPatrolCoroutine = StartCoroutine(ChangeToPatrol());
                ChangeState(EnemyStates.PATROL);
            }
        }
    }

    public void TakeHealth()
    {
        if (_CurrentState != EnemyStates.KNOCKED)
        {
            _Hp--;
            if (_Hp == 0)
                ChangeState(EnemyStates.KNOCKED);
        }
    }

    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(5);
        Collider[] aux = Physics.OverlapSphere(transform.position, 2f, _LayerPlayer);
        if (aux.Length > 0)
        {
            if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
            {
                if (info.transform.tag == "Player")
                {
                    ChangeState(EnemyStates.ATTACK);
                }
            }
        }
        else
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator AttackPlayer()
    {
        while (true)
        {
            //Animation -> attack
            _Player.GetComponent<Player>().TakeDamage(1);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator ChangeToPatrol()
    {
        yield return new WaitForSeconds(10f);
        _PointOfPatrol = transform.position;
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator WaitingAttack()
    {
        while (true)
        {
            Collider[] aux = Physics.OverlapSphere(transform.position, 2f, _LayerPlayer);
            if (aux.Length > 0)
            {
                if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                {
                    if (info.transform.tag == "Player")
                    {
                        transform.LookAt(info.transform.position);
                        _NavMeshAgent.SetDestination(transform.position);
                        if(_CurrentState != EnemyStates.ATTACK)
                            ChangeState(EnemyStates.ATTACK);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ActivateAttackCoroutine()
    {
        _ActivateAttackCoroutine = StartCoroutine(WaitingAttack());
    }

    private void DeactivateAttackCoroutine()
    {
        StopCoroutine(_ActivateAttackCoroutine);
        if (_CurrentState == EnemyStates.ATTACK)
            ChangeState(EnemyStates.PATROL);
    }
}
