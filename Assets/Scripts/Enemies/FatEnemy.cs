using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

public class FatEnemy : Enemy
{
    private enum EnemyStates { PATROL, ATTACK, KNOCKED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private LayerMask _LayerDoor;
    [SerializeField] private GameObject _DetectionSphere;
    [SerializeField] private List<Transform> _Waypoints;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private Vector3 _PointOfPatrol;
    private bool _Patrolling;
    private bool _Search;

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
        _RangeSearchSound = 25;
        _Patrolling = false;
        _Search = false;
        _Hp = MAXHEALTH;

        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateAttackCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateAttackCoroutine;

        StartCoroutine(OpenDoors());
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
    }

    #region FSM

    private void ChangeState(EnemyStates newState)
    {
      //  Debug.Log($"---------------------- Sortint de {_CurrentState} a {newState} ------------------------");
        ExitState(_CurrentState);

        //Debug.Log($"---------------------- Entrant a {newState} ------------------------");
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

    // Funci� per moure l'enemic pel mapa
    IEnumerator Patrol(int range, Vector3 pointOfSearch)
    {
        Transform point = default;
        while (true)
        {
            if (!_Patrolling)
            {
                if (!_Search)
                {
                    while (true)
                    {
                        point = _Waypoints[Random.Range(0, _Waypoints.Count)];
                        if (point.position != _NavMeshAgent.destination)
                            break;
                    }
                }
                else
                {
                    RandomPoint(pointOfSearch, range, out Vector3 coord);
                    point.position = coord;
                }
                _NavMeshAgent.SetDestination(new Vector3(point.position.x, point.position.y, point.position.z));
                _Patrolling = true;
            }

            if (_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _Patrolling = false;
                yield return new WaitForSeconds(2);
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

            //Aqu� s'haur� de comprovar si la y que hem extret est� en algun dels pisos de l'edifici.
            //Si est� aprop la transformem en aquest i ja

            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1.0f, 0) && Vector3.Distance(point, center) > 1.75f)
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

        float dist = Vector3.Distance(this.transform.position, pos);
        Debug.Log("DISTANCIA: "+dist);

        if (dist > 10)
        {
            Debug.Log("dist>10");
            while (Mathf.Abs(dist) > 0)
            {
                if (dist > 10)
                {
                    dist -= 10;
                    lvlSound -= 1;
                }
                else
                {
                    dist = 0;
                }

                if(dist <= 0)
                {
                    break;
                }
            }
        }
        else if(Physics.Raycast(this.transform.position, pos, out RaycastHit info))
        {
            if (info.collider.TryGetComponent<Player>(out Player player))
            {
                Debug.Log("dist<10 y se multiplica");
                lvlSound *= 4;
            }
            else
            {
                lvlSound = 8;
                Debug.Log("dist<10 y se PONE A 8");
            }
        }

        Debug.Log(lvlSound);

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 3)
            {
                _RangeSearchSound = 7;
                Debug.Log("El mas bajo");
                _Search = true;
            }
            else if (lvlSound > 3 && lvlSound <= 7)
            {
                Debug.Log("El bajo-Medio");
                _RangeSearchSound = 5;
                _Search = true;
            }
            else if (lvlSound > 7 && lvlSound <= 10)
            {
                Debug.Log("El medio-Alto (Aqui entra cuando el 8");
                _RangeSearchSound = 2;
                _Search = true;
            }
            else if (lvlSound > 10)
            {
                Debug.Log("El mas alto");
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

    IEnumerator OpenDoors()
    {
        while (true)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2.5f, _LayerDoor))
            {
                if (hit.collider.TryGetComponent<Door>(out Door door) && !door.isLocked)
                {
                    if (!door.isOpen)
                    {
                        _NavMeshAgent.isStopped = true;
                        door.Open(transform.position);
                        yield return new WaitForSeconds(0.4f);
                        _NavMeshAgent.isStopped = false;
                        StartCoroutine(CloseDoorAutomatic(door));
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CloseDoorAutomatic(Door door)
    {
        yield return new WaitForSeconds(3);
        door.Close();
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
        _Search = false;
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
