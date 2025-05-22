using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BlindEnemy : Enemy
{
    private enum EnemyStates { PATROL, ATTACK, KNOCKED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private float _StateTime;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private LayerMask _LayerDoor;
    [SerializeField] private GameObject _DetectionSphere;
    [SerializeField] private List<GameObject> _WaypointsFirstPart;
    [SerializeField] private List<GameObject> _WaypointsSecondPart;
    [SerializeField] private Door _DoorLocked;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private Vector3 _PointOfPatrol;
    [SerializeField] private bool _Patrolling;
    [SerializeField] private bool _Search;
    private bool _OpeningDoor;
    private bool _Jumping;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private float _RangeSearchSound;
    private int MAXHEALTH = 2;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeStateToPatrol;
    private Coroutine _AttackCoroutine;
    private Coroutine _RestoreCoroutine;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _PointOfPatrol = transform.position;
        _RangeSearchSound = 55;
        _Patrolling = false;
        _Search = false;
        _OpeningDoor = false;
        _Jumping = false;
        _Hp = MAXHEALTH;

        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += OnEnter;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += OnExit;

        StartCoroutine(OpenDoors());

        _AttackCoroutine = null;
        _ChangeStateToPatrol = null;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
    }

    #region FSM

    private void ChangeState(EnemyStates newState)
    {
        //if (_CurrentState == newState) return;

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
                _PatrolCoroutine = StartCoroutine(Patrol());
                break;
            case EnemyStates.ATTACK:
                _NavMeshAgent.isStopped = true;
                break;
            case EnemyStates.KNOCKED:
                StartCoroutine(WakeUp(5));
                break;
        }
    }

    private void ExitState(EnemyStates exitState)
    {
        switch (exitState)
        {
            case EnemyStates.PATROL:
                _Patrolling = false;
                StopCoroutine(_PatrolCoroutine);
                if (_ChangeStateToPatrol != null)
                {
                    StopCoroutine(_ChangeStateToPatrol);
                    _ChangeStateToPatrol = null;
                }
                break;
            case EnemyStates.ATTACK:
                _PointOfPatrol = transform.position;
                _RangeSearchSound = 25;
                _NavMeshAgent.isStopped = false;
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
        }
    }

    #endregion 

    // Funci� per moure l'enemic pel mapa
    IEnumerator Patrol()
    {
        Vector3 point = Vector3.zero;
        while (true)
        {
            //Debug.Log("Entro al Patrol");
            if (!_Patrolling)
            {
                if (!_Search)
                {
                    while (true)
                    {
                        if (_DoorLocked.isLocked)
                        {
                            point = _WaypointsFirstPart[Random.Range(0, _WaypointsFirstPart.Count)].transform.position;
                        }
                        else
                        {
                            int random = Random.Range(0, 2);

                            switch (random)
                            {
                                case 0:
                                    point = _WaypointsFirstPart[Random.Range(0, _WaypointsFirstPart.Count)].transform.position;
                                    break;
                                case 1:
                                    point = _WaypointsSecondPart[Random.Range(0, _WaypointsSecondPart.Count)].transform.position;
                                    break;
                            }
                        }
                        if (point != _NavMeshAgent.destination)
                            break;
                    }
                }
                else
                {
                    RandomPoint(_SoundPos, _RangeSearchSound, out Vector3 coord);
                    point = coord;
                }
                _Patrolling = true;
                _NavMeshAgent.SetDestination(point);
            }

            if (_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                if (_Search && _CurrentState != EnemyStates.ATTACK && _ChangeStateToPatrol == null)
                {
                    Debug.Log("Activo la corutina WakeUp");
                    _ChangeStateToPatrol = StartCoroutine(WakeUp(10));
                }
                _Patrolling = false;
                yield return new WaitForSeconds(1);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = new Vector3(center.x, center.y, center.z) + Random.insideUnitSphere * range;
            // Debug.Log($"Punt: {randomPoint}");

            //Aqu� s'haur� de comprovar si la y que hem extret est� en algun dels pisos de l'edifici.
            //Si est� aprop la transformem en aquest i ja

            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            NavMeshQueryFilter filter = new()
            {
                agentTypeID = _NavMeshAgent.agentTypeID,
                areaMask = _NavMeshAgent.areaMask,
            };

            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1.0f, filter))
            {
                result = hit.position;
                return true;
            }
        }
        Debug.Log("No he trobat un punt! (Enemic cec)");
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
                lvlSound = 2;
            }
        }

        Debug.Log($"Nivell so: {lvlSound}");

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 1.5f;
                _Search = true;
            }
            else if (lvlSound > 2 && lvlSound <= 3)
            {
                _RangeSearchSound = 1;
                _Search = true;
            }
            else if (lvlSound > 3 && lvlSound <= 5)
            {
                _RangeSearchSound = 0.5f;
                _Search = true;
            }
            else if (lvlSound > 5)
            {
                _RangeSearchSound = 0.5f;
                bool wall = false;
                RaycastHit[] hits2 = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));

                foreach (RaycastHit hit in hits2)
                {
                    if (hit.collider.tag == "Wall" || hit.collider.gameObject.layer == 10)
                    {
                        wall = true;
                        break;
                    }
                }

                if (Vector3.Distance(_SoundPos, transform.position) > 1.5f && Vector3.Distance(_SoundPos, transform.position) <= 8 && !wall && !_Jumping)
                {
                    Debug.Log("Faig salt!");
                    _Jumping = true;
                    ExitState(_CurrentState);
                    Vector3 start = transform.position;
                    Vector3 end = _SoundPos;

                    _NavMeshAgent.enabled = false;
                    GetComponent<Rigidbody>().isKinematic = false;
                    GetComponent<Rigidbody>().AddForce((end - start) * 70);
                    transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, (end - start), 20, 0));

                    _RestoreCoroutine = StartCoroutine(RecoverAgent());
                    StartCoroutine(RecoverJump());
                }
                else if (Vector3.Distance(_SoundPos, transform.position) > 8)
                {
                    _NavMeshAgent.SetDestination(_SoundPos);
                }
            }

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

    IEnumerator WakeUp(int time)
    {
        yield return new WaitForSeconds(time);
        _Search = false;
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator OpenDoors()
    {
        while (true)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2.5f, _LayerDoor))
            {
                Door door = hit.collider.GetComponentInChildren<Door>();

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
                    yield return new WaitForSeconds(2);
                }
            }

            yield return new WaitForSeconds(0.1f);
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
        yield return new WaitForSeconds(1);
        door.Close();
    }

    IEnumerator RecoverAgent()
    {
        yield return new WaitForSeconds(1f);
        GetComponent<Rigidbody>().isKinematic = true;
        _NavMeshAgent.enabled = true;
        if (_CurrentState != EnemyStates.ATTACK)
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator RecoverJump()
    {
        yield return new WaitForSeconds(3);
        _Jumping = false;
    }

    IEnumerator Attack()
    {
        while (true)
        {
            Debug.Log("Entro a l'atac");
            _Player.GetComponent<Player>().TakeDamage(2);
            Debug.Log("Estic fent mal (Enemic cec)");
            yield return new WaitForSeconds(1);
        }
    }

    private void OnEnter()
    {
        if(_CurrentState != EnemyStates.KNOCKED)
        {
            _Search = false;
            if (_ChangeStateToPatrol != null)
            {
                StopCoroutine(_ChangeStateToPatrol);
                _ChangeStateToPatrol = null;
            }

            if (!_NavMeshAgent.enabled)
                _NavMeshAgent.enabled = true;
            ChangeState(EnemyStates.ATTACK);
            if (_AttackCoroutine == null)
                _AttackCoroutine = StartCoroutine(Attack());
        }
    }

    private void OnExit()
    {
        if(_CurrentState != EnemyStates.KNOCKED)
        {
            if (_AttackCoroutine != null)
            {
                StopCoroutine(_AttackCoroutine);
                _AttackCoroutine = null;
            }

            _Search = true;
            _RangeSearchSound = 0.5f;
            _PointOfPatrol = transform.position;
            ChangeState(EnemyStates.PATROL);
            Debug.Log("Activo l'estat de Patrol");
        }
    }
}