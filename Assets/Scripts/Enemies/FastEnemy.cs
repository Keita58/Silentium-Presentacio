using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FastEnemy : Enemy
{
    private enum EnemyStates { PATROL, CHASE, ATTACK, KNOCKED, STOPPED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private float _StateTime;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private LayerMask _LayerDoor;
    [SerializeField] private GameObject _DetectionSphere;
    [SerializeField] private GameObject _DetectionDoors;
    [SerializeField] private List<GameObject> _Waypoints;
    [SerializeField] private GameObject _Waypoint;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private float _BetaDotProduct;
    private float _LastLvlSound;
    private bool _Patrolling;
    private bool _Search;
    private bool _OpeningDoor;
    private bool _Looking;
    private Animator _Animator;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private readonly int MAXHEALTH = 3;
    private int _RangeSearchSound;
    private int _RangeChaseAfterStop;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeToPatrolCoroutine;
    private Coroutine _AttackCoroutine;
    private Coroutine _ActivateLookingCoroutine;
    [Header("Audio")]
    AudioSource _fastAudioSource;
    [SerializeField]
    AudioClip _fastAudioClip;
    [SerializeField]
    AudioClip _fastShoutAudioClip;
    [SerializeField]
    AudioClip _fastRunAudioClip;
    bool firstTime;
    private void Awake()
    {
        _fastAudioSource = GetComponent<AudioSource>();
        _Animator = GetComponent<Animator>();
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 30;
        _RangeChaseAfterStop = 12;
        _BetaDotProduct = 90;
        _LastLvlSound = 0;
        _Patrolling = false;
        _OpeningDoor = false;
        _Hp = MAXHEALTH;
        firstTime = true;
        _PatrolCoroutine = null;
        _ChangeToPatrolCoroutine = null;
        _AttackCoroutine = null;
        _ActivateLookingCoroutine = null;
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateLookingCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateLookingCoroutine;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor += OpenDoors;
    }

    private void OnDestroy()
    {
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter -= ActivateLookingCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit -= DeactivateLookingCoroutine;
        _DetectionDoors.GetComponent<DetectionDoorSphere>().OnDetectDoor -= OpenDoors;
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
        _StateTime = 0.0f;

        switch (_CurrentState)
        {
            case EnemyStates.PATROL:
                firstTime = true;
                _Patrolling = false;
                _PatrolCoroutine = StartCoroutine(Patrol());
                _Animator.Play("Walk");
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 5.5f;
                _Animator.Play("Run");
                break;
            case EnemyStates.ATTACK:
                _NavMeshAgent.speed = 0;
                _NavMeshAgent.SetDestination(transform.position);
                if (_AttackCoroutine == null)
                    _AttackCoroutine = StartCoroutine(LookAttackPlayer());
                _Animator.Play("Attack");
                break;
            case EnemyStates.KNOCKED:
                if(_ActivateLookingCoroutine != null)
                    StopCoroutine(_ActivateLookingCoroutine);
                _Animator.Play("Idle");
                StartCoroutine(WakeUp());
                break;
            case EnemyStates.STOPPED:
                _Animator.enabled = false;
                break;
        }
    }

    private void UpdateState(EnemyStates updateState)
    {
        _StateTime += Time.deltaTime;

        switch (updateState)
        {
            case EnemyStates.CHASE:
                ChasePlayer();
                break;
        }
    }

    private void ExitState(EnemyStates exitState)
    {
        switch (exitState)
        {
            case EnemyStates.PATROL:
                StopCoroutine(_PatrolCoroutine);
                if(_ChangeToPatrolCoroutine != null)
                {
                    StopCoroutine(_ChangeToPatrolCoroutine);
                    _ChangeToPatrolCoroutine = null;
                }
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 3;
                _RangeChaseAfterStop = 12;
                break;
            case EnemyStates.ATTACK:
                if (_AttackCoroutine != null)
                {
                    StopCoroutine(_AttackCoroutine);
                    _AttackCoroutine = null;
                }
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
            case EnemyStates.STOPPED:
                _Animator.enabled = true;
                break;
        }
    }

    #endregion 

    private void Update()
    {
        UpdateState(_CurrentState);
    }

    //Busca punt aleatori dins del NavMesh
    private void RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = new Vector3(center.x, center.y, center.z) + Random.insideUnitSphere * range;
            Debug.Log($"Punt: {randomPoint}");

            //Aquí s'haurà de comprovar si la y que hem extret està en algun dels pisos de l'edifici.
            //Si està aprop la transformem en aquest i ja

            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            NavMeshHit hit;
            print(NavMesh.GetAreaNames().Length);

            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(point, out hit, 1.0f, NavMesh.GetAreaFromName("Walkable")))
            {
                result = hit.position;
            }
        }
        result = center;
    }

    private void ChasePlayer()
    {
        _NavMeshAgent.SetDestination(_Player.transform.position);
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
        Debug.Log($"Distància entre ràpid i punt de so: {dist}");
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

        if(lvlSound >= _LastLvlSound)
        {
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
                    _NavMeshAgent.SetDestination(_SoundPos);
                }

                ChangeState(EnemyStates.PATROL);
            }

            _LastLvlSound = lvlSound;
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

    // Funció per moure l'enemic pel mapa
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
                    RandomPoint(_SoundPos, _RangeSearchSound, out Vector3 coord);
                    _Waypoint.transform.position = coord;
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

    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(5);
        _Hp = MAXHEALTH;
        Collider[] aux = Physics.OverlapSphere(transform.position, 2f, _LayerPlayer);
        Collider[] aux2 = Physics.OverlapSphere(transform.position, 15, _LayerPlayer);
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
            _ActivateLookingCoroutine = StartCoroutine(LookingPlayer());
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

    private void AttackPlayer()
    {
        _Player.GetComponent<Player>().TakeDamage(3);
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

    IEnumerator LookingPlayer()
    {
        float alphaLook = 0;
        while (true)
        {
            if(_CurrentState != EnemyStates.KNOCKED)
            {
                Collider[] aux = Physics.OverlapSphere(transform.position, _RangeChaseAfterStop, _LayerPlayer);
                if (aux.Length > 0)
                {
                    //Això és l'angle de visio de l'enemic (ho comparem sobre 60 ja que se'ns dona la meitat) de 120º
                    //float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - transform.position).normalized);
                    
                    //Això és el càlcul de les direccions de les mirades, si s'estan mirant l'un a l'altre serà -1 i anira augmentant
                    //fins 1 quan estan mirant cap a la mateixa direcció
                    alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);

                    //Raycast amb les layers de paret i player i si tenim la paret no seguim, si no seguim el jugador
                    //El raycast és de distància 12 per a tenir un marge amb el trigger de l'esfera de deteccio, perquè no es torni boig
                    Debug.DrawRay(transform.position, (_Player.transform.position - transform.position), Color.blue, 4);
                    if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, 12, _LayerObjectsAndPlayer))
                    {
                        if (info.transform.tag == "Player")
                        {
                            if (firstTime)
                            {
                                _fastAudioSource.PlayOneShot(_fastShoutAudioClip, 6.5f);
                                firstTime = false;
                            }
                            if (!_Looking)
                            {
                                transform.LookAt(new Vector3(info.transform.position.x, 0.9f, info.transform.position.z));
                                _Looking = true;
                            }

                            if (alphaLook < -0.8f)
                            {
                                if (_CurrentState != EnemyStates.STOPPED)
                                {
                                    _fastAudioSource.resource = _fastAudioClip;
                                    _fastAudioSource.Play();
                                    _NavMeshAgent.SetDestination(transform.position);
                                    ChangeState(EnemyStates.STOPPED);
                                }
                            }
                            else
                            {
                                //Tornem a calcular l'alphaLook per si el jugador encara ens segueix mirant
                                transform.LookAt(new Vector3(info.transform.position.x, 0.9f, info.transform.position.z));
                                alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);
                                if (alphaLook >= -0.8f)
                                {
                                    if (Vector3.Distance(info.transform.position, transform.position) > 2)
                                    {
                                        Debug.Log("Veig al jugador lluny");
                                        if (_CurrentState != EnemyStates.CHASE)
                                        {
                                            _fastAudioSource.resource = _fastRunAudioClip;
                                            _fastAudioSource.Play();
                                            _RangeChaseAfterStop = 28;
                                            ChangeState(EnemyStates.CHASE);
                                        }
                                    }
                                    else if (Vector3.Distance(info.transform.position, transform.position) <= 2)
                                    {
                                        Debug.Log("Tinc al jugador al davant!");
                                        _NavMeshAgent.SetDestination(transform.position);
                                        if (_CurrentState != EnemyStates.ATTACK)
                                        {
                                            ChangeState(EnemyStates.ATTACK);
                                        }
                                    }
                                }
                            }
                        }
                        else if (_CurrentState == EnemyStates.STOPPED) 
                        {
                            Debug.Log("El jugador ha sortit del rang de visió");
                            if (_CurrentState != EnemyStates.CHASE)
                            {
                                _Looking = false;
                                _RangeChaseAfterStop = 28;
                                ChangeState(EnemyStates.CHASE);
                            }
                        }
                    }
                    else
                    {
                        _Looking = false;
                        if(_CurrentState == EnemyStates.STOPPED)
                        {
                            Debug.Log("El jugador ha sortit del rang de visió");
                            if (_CurrentState != EnemyStates.CHASE)
                            {
                                _RangeChaseAfterStop = 28;
                                ChangeState(EnemyStates.CHASE);
                            }
                        }
                        else if(_CurrentState != EnemyStates.PATROL)
                        {
                            Debug.Log("Tiro raycast i no em trobo res");
                            ChangeState(EnemyStates.PATROL);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ActivateLookingCoroutine()
    {

        if (_ActivateLookingCoroutine == null)
            _ActivateLookingCoroutine = StartCoroutine(LookingPlayer());        
    }

    private void DeactivateLookingCoroutine()
    {
        if (_ActivateLookingCoroutine != null)
        {
            StopCoroutine(_ActivateLookingCoroutine);
            _ActivateLookingCoroutine = null;
        }

        if (_CurrentState == EnemyStates.STOPPED)
        {
            _RangeChaseAfterStop = 28;
            ChangeState(EnemyStates.CHASE);
        }
    }
}