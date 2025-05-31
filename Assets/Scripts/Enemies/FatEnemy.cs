using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FatEnemy : Enemy
{
    private enum EnemyStates { PATROL, ATTACK, KNOCKED, CHASE }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private LayerMask _LayerDoor;
    [SerializeField] private GameObject _DetectionSphere;
    [SerializeField] private GameObject _DetectionDoors;
    [SerializeField] private GameObject _DetectionAttack;
    [SerializeField] private List<GameObject> _Waypoints;
    [SerializeField] private GameObject _Waypoint;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    [SerializeField] private bool _Patrolling;
    [SerializeField] private bool _Search;
    private bool _OpeningDoor;
    private Animator _Animator;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private readonly int MAXHEALTH = 6;
    private int _RangeSearchSound;

    private Coroutine _PatrolCoroutine;
    private Coroutine _AttackCoroutine;
    private Coroutine _ChangeToPatrolCoroutine;
    private Coroutine _ChaseCoroutine;
    private Coroutine _ChangeToChase;

    [Header("Audio")]
    private AudioSource _fatAudioSource;
    [SerializeField]
    AudioClip _fatAudioClip;
    private void Awake()
    {
        _fatAudioSource = GetComponent<AudioSource>();
        _fatAudioSource.loop = true;
        _Animator = GetComponent<Animator>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 25;
        _Patrolling = false;
        _Search = false;
        _OpeningDoor = false;
        _Hp = MAXHEALTH;

        _ChaseCoroutine = null;
        _ChangeToChase = null;

        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateChaseCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateChaseCoroutine;
        _DetectionAttack.GetComponent<DetectionSphere>().OnEnter += ActivateAttack;
        _DetectionAttack.GetComponent<DetectionSphere>().OnExit += DeactivateAttack;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor += OpenDoors;
    }

    private void OnDestroy()
    {
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateChaseCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateChaseCoroutine;
        _DetectionAttack.GetComponent<DetectionSphere>().OnEnter += ActivateAttack;
        _DetectionAttack.GetComponent<DetectionSphere>().OnExit += DeactivateAttack;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor -= OpenDoors;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
        _fatAudioSource.Play();
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
                _PatrolCoroutine = StartCoroutine(Patrol());
                if (_Search)
                    _ChangeToPatrolCoroutine = StartCoroutine(ChangeToPatrol(7));
                break;
            case EnemyStates.ATTACK:
                _Animator.Play("Attack");
                _NavMeshAgent.SetDestination(transform.position);
                _AttackCoroutine = StartCoroutine(LookAttackPlayer());
                break;
            case EnemyStates.KNOCKED:
                _NavMeshAgent.SetDestination(transform.position);
                _Animator.Play("Idle");
                StartCoroutine(WakeUp());
                break;
            case EnemyStates.CHASE:
                _Animator.Play("Walk");
                if (_ChaseCoroutine == null)
                    _ChaseCoroutine = StartCoroutine(ChasePlayer());
                break;
        }
    }

    private void ExitState(EnemyStates exitState)
    {
        switch (exitState)
        {
            case EnemyStates.PATROL:
                StopCoroutine(_PatrolCoroutine);
                if (_ChangeToPatrolCoroutine != null)
                {
                    StopCoroutine(_ChangeToPatrolCoroutine);
                    _ChangeToPatrolCoroutine = null;
                }
                break;
            case EnemyStates.ATTACK:
                if(_AttackCoroutine != null)
                {
                    StopCoroutine(_AttackCoroutine);
                    _AttackCoroutine = null;
                }
                if(_ChangeToChase != null)
                {
                    StopCoroutine(_ChangeToChase);
                    _ChangeToChase = null;
                }
                _NavMeshAgent.SetDestination(transform.position);
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
            case EnemyStates.CHASE:
                if (_ChaseCoroutine != null)
                {
                    StopCoroutine(_ChaseCoroutine);
                    _ChaseCoroutine = null;
                }
                break;
        }
    }

    #endregion 

    // Funcio per moure l'enemic pel mapa
    IEnumerator Patrol()
    {
        GameObject waypointToGo = null;
        while (true)
        {
            if (!_Patrolling)
            {
                if (!_Search)
                {
                    while (true)
                    {
                        _Waypoint = _Waypoints[Random.Range(0, _Waypoints.Count)];
                        if (_Waypoint != waypointToGo)
                        {
                            waypointToGo = _Waypoint;
                            break; 
                        }
                    }
                }
                else
                {
                    if(_RangeSearchSound > 0)
                    {
                        RandomPoint(_SoundPos, _RangeSearchSound, out Vector3 coord);
                        _Waypoint.transform.position = coord;
                    }
                }
                _Animator.Play("Walk");
                _NavMeshAgent.SetDestination(_Waypoint.transform.position);
                _Patrolling = true;
            }
            
            if (!_NavMeshAgent.pathPending && _NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _Animator.Play("Idle");
                _Patrolling = false;
                if (!_Search)
                    yield return new WaitForSeconds(2);
                else
                    yield return new WaitForSeconds(1);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    //Busca punt aleatori dins del NavMesh
    private void RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = new Vector3(center.x, center.y, center.z) + Random.insideUnitSphere * range;

            //Aqu� s'haur� de comprovar si la y que hem extret est� en algun dels pisos de l'edifici.
            //Si est� aprop la transformem en aquest i ja

            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1.0f, NavMesh.GetAreaFromName("Walkable")))
            {
                result = hit.position;
            }
        }
        Debug.Log("No he trobat un punt! (Enemic gras)");
        result = center;
    }

    public override void ListenSound(Vector3 pos, int lvlSound)
    {
        _SoundPos = pos;
        RaycastHit[] hits = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<IAttenuable>(out IAttenuable a))
            {
                lvlSound = a.AttenuateSound(lvlSound);
            }
        }

        float dist = Vector3.Distance(this.transform.position, _SoundPos);
        Debug.Log($"Distància entre cec i punt de so: {dist}");
        if (dist < 10 && Physics.Raycast(this.transform.position, (_Player.transform.position - transform.position), out RaycastHit info, dist, _LayerObjectsAndPlayer))
        {
            if (info.collider.TryGetComponent<Player>(out _))
            {
                lvlSound = 8;
            }
            else
            {
                lvlSound = 3;
            }
        }

        Debug.Log(lvlSound);

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 3;
                _Search = true;
            }
            else if (lvlSound > 2 && lvlSound <= 4)
            {
                _RangeSearchSound = 2;
                _Search = true;
            }
            else if (lvlSound > 4 && lvlSound <= 7)
            {
                _RangeSearchSound = 1;
                _Search = true;
            }
            else if (lvlSound > 7)
            {
                _RangeSearchSound = 0;
                _Search = true;
                _NavMeshAgent.SetDestination(_SoundPos);
            }

            if (_CurrentState == EnemyStates.PATROL)
                ChangeState(EnemyStates.PATROL);
        }
    }

    public override void TakeHealth()
    {
        if (_CurrentState != EnemyStates.KNOCKED)
        {
            _Hp--;
            if (_Hp == 0)
                ChangeState(EnemyStates.KNOCKED);
        }
    }

    private void OpenDoors(Door door)
    {
        if (!door.isLocked && !door.isOpen && !_OpeningDoor)
        {
            door.onDoorOpen += Resume;
            _NavMeshAgent.isStopped = true;
            door.Open(transform.position);
            _OpeningDoor = true;
        }
        else if (door.isLocked)
        {
            _NavMeshAgent.SetDestination(transform.position);
        }
    }

    private void Resume(Door door)
    {
        _NavMeshAgent.isStopped = false;
        StartCoroutine(CloseDoorAutomatic(door));
        door.onDoorOpen -= Resume;
        _OpeningDoor = false;
    }

    IEnumerator CloseDoorAutomatic(Door door)
    {
        yield return new WaitForSeconds(3);
        door.Close();
    }

    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(10);
        _Hp = MAXHEALTH;
        Collider[] aux = Physics.OverlapSphere(transform.position, 1.5f, _LayerPlayer);
        Collider[] aux2 = Physics.OverlapSphere(transform.position, 4f, _LayerPlayer);
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
        else if (aux2.Length > 0)
        {
            ChangeState(EnemyStates.CHASE);
        }
        else
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator LookAttackPlayer()
    {
        while (true)
        {
            transform.LookAt(_Player.transform.position);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChasePlayer()
    {
        while (true)
        {
            transform.LookAt(_Player.transform.position);
            _NavMeshAgent.SetDestination(_Player.transform.position);
            Debug.Log(_NavMeshAgent.destination);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChangeToPatrol(float time)
    {
        yield return new WaitForSeconds(time);
        _Search = false;
        ChangeState(EnemyStates.PATROL);
    }

    private void ActivateChaseCoroutine()
    {
        ChangeState(EnemyStates.CHASE);
    }

    private void DeactivateChaseCoroutine()
    {
        _Search = false;
        ChangeState(EnemyStates.PATROL);
    }

    private void ActivateAttack()
    {
        Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit thing, 12,
            _LayerObjectsAndPlayer);
        if(thing.transform.gameObject.TryGetComponent(out Player player))
        {
            _Animator.SetBool("Chase", false);
            ChangeState(EnemyStates.ATTACK);
        }
    }

    private void DeactivateAttack()
    {
        _Animator.SetBool("Chase", true);
        if(_ChangeToChase == null)
            StartCoroutine(ChangeToChase());
    }

    IEnumerator ChangeToChase()
    {
        while(true)
        {
            if (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                ChangeState(EnemyStates.CHASE);
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}