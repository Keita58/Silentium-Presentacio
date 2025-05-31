using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject _DetectionDoors;
    [SerializeField] private List<GameObject> _WaypointsFirstPart;
    [SerializeField] private List<GameObject> _WaypointsSecondPart;
    [SerializeField] private Door _DoorLocked;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    [SerializeField] private bool _Patrolling;
    [SerializeField] private bool _Search;
    private bool _OpeningDoor;
    private bool _Jumping;
    private Animator _Animator;
    private Rigidbody _Rigidbody;
    private Collider _Collider;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private float _RangeSearchSound;
    private int MAXHEALTH = 2;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeStateToPatrol;
    private Coroutine _AttackCoroutine;

    [Header("Audio")]
    AudioSource _BlindAudioSource;
    [SerializeField]
    AudioClip _blindAudioClip;
    [SerializeField]
    AudioClip _blindAttackAudioClip;

    public event Action OnJump;
    
    private void Awake()
    {
        _Collider = GetComponent<Collider>();
        _Rigidbody = GetComponent<Rigidbody>();
        _BlindAudioSource = GetComponent<AudioSource>();
        _Animator = GetComponent<Animator>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 55;
        _Patrolling = false;
        _Search = false;
        _OpeningDoor = false;
        _Jumping = false;
        _Hp = MAXHEALTH;

        _PatrolCoroutine = null;
        _AttackCoroutine = null;
        _ChangeStateToPatrol = null;
        
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += OnEnter;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor += OpenDoors;
    }

    private void OnDestroy()
    {
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter -= OnEnter;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor -= OpenDoors;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
        _BlindAudioSource.Play();
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
                _BlindAudioSource.resource = _blindAudioClip;
                _BlindAudioSource.Play();
                if (_PatrolCoroutine == null)
                    _PatrolCoroutine = StartCoroutine(Patrol());
                break;
            case EnemyStates.ATTACK:
                _Animator.enabled = false;
                _NavMeshAgent.enabled = false;
                _Rigidbody.isKinematic = false;
                _Collider.enabled = false;
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
                if (_PatrolCoroutine != null)
                {
                    StopCoroutine(_PatrolCoroutine);
                    _PatrolCoroutine = null;
                }
                if (_ChangeStateToPatrol != null)
                {
                    StopCoroutine(_ChangeStateToPatrol);
                    _ChangeStateToPatrol = null;
                }
                break;
            case EnemyStates.ATTACK:
                _RangeSearchSound = 25;
                if(!_NavMeshAgent.enabled)
                    _NavMeshAgent.enabled = true;
        
                _Rigidbody.isKinematic = true;
                _Animator.enabled = true;
                _Collider.enabled = true;
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
                            int random = Random.Range(0, 3);

                            switch (random)
                            {
                                case 0:
                                    point = _WaypointsFirstPart[Random.Range(0, _WaypointsFirstPart.Count)].transform.position;
                                    break;
                                case 1:
                                case 2:
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
                    if(_RangeSearchSound > 0)
                    {
                        RandomPoint(_SoundPos, _RangeSearchSound, out Vector3 coord);
                        point = coord;
                    }
                }
                _Animator.enabled = true;
                _Patrolling = true;
                _NavMeshAgent.SetDestination(point);
            }

            if (!_NavMeshAgent.pathPending && _NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                if (_Search && _CurrentState != EnemyStates.ATTACK && _ChangeStateToPatrol == null)
                {
                    Debug.Log("Activo la corutina WakeUp");
                    _ChangeStateToPatrol = StartCoroutine(WakeUp(10));
                }
                _Animator.enabled = false;
                _Patrolling = false;
                yield return new WaitForSeconds(1);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    private void RandomPoint(Vector3 center, float range, out Vector3 result)
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
            }
        }
        Debug.Log("No he trobat un punt! (Enemic cec)");
        result = center;
    }

    public override void ListenSound(Vector3 pos, int lvlSound)
    {
        bool wall = false;
        _SoundPos = pos;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, _SoundPos - transform.position, Vector3.Distance(_SoundPos, this.transform.position));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<IAttenuable>(out IAttenuable a))
            {
                lvlSound = a.AttenuateSound(lvlSound);
                wall = true;
            }
        }

        //Distància del cec fins al punt del so
        float dist = Vector3.Distance(transform.position, _SoundPos);
        float distPlayer = Vector3.Distance(transform.position, _Player.transform.position);
        Debug.Log($"Distància entre cec i punt de so: {dist}");

        if (dist < 10 && Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, dist, _LayerObjectsAndPlayer))
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
                if (dist > 1.5f && dist <= 8 && !wall && !_Jumping)
                {
                    Debug.Log("Faig salt!");
                    _Jumping = true;
                    ChangeState(EnemyStates.ATTACK);
                    Vector3 start = transform.position;
                    Vector3 end = _SoundPos;

                    //_Animator.enabled = false;
                    //_NavMeshAgent.enabled = false;
                    //_Rigidbody.isKinematic = false;
                    //_Collider.enabled = false;

                    //transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, _SoundPos, 20, 0));
                    //transform.LookAt(_SoundPos);
                    _Rigidbody.AddForce((end - start) * 140);
                    OnJump?.Invoke();

                    //No necessiten comprovació ja que només entrarà aquí quan pugui tornar a saltar,
                    //que es recupera quan acaba la corutina de RecoverJump
                    _BlindAudioSource.PlayOneShot(_blindAttackAudioClip);
                    StartCoroutine(RecoverAgent());
                    StartCoroutine(RecoverJump());
                }
                else if (Vector3.Distance(_SoundPos, transform.position) > 8)
                {
                    _RangeSearchSound = 0;
                    _Search = true;
                    _NavMeshAgent.SetDestination(_SoundPos);
                    Debug.Log($"Soc l'enemic cec i vaig al punt {_NavMeshAgent.destination}");
                }
            }
            
            if(lvlSound <= 5)
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
        if(_CurrentState == EnemyStates.KNOCKED)
            _Hp = MAXHEALTH;
        _Search = false;
        ChangeState(EnemyStates.PATROL);
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
        yield return new WaitForSeconds(1);
        door.Close();
    }

    IEnumerator RecoverAgent()
    {
        yield return new WaitForSeconds(1f);
        //if(!_NavMeshAgent.enabled)
        //    _NavMeshAgent.enabled = true;
        //
        //_Rigidbody.isKinematic = true;
        //_Animator.enabled = true;
        //_Collider.enabled = true;
        if (_CurrentState != EnemyStates.ATTACK)
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator RecoverJump()
    {
        yield return new WaitForSeconds(3);
        _Jumping = false;
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
        }
    }
}