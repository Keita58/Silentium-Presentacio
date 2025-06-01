using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FastEnemy : Enemy
{
    private readonly int MAXHEALTH = 3;
    private enum EnemyStates { PATROL, CHASE, ATTACK, KNOCKED, STOPPED }
    
    [Header("States")]
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private float _StateTime;
    
    [Header("Player")]
    [SerializeField] private GameObject _Player;
    
    [Header("Layers")]
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerDoor;
    
    [Header("Waypoints")]
    [SerializeField] private List<GameObject> _Waypoints;
    [SerializeField] private GameObject _Waypoint;
    
    [Header("Detection spheres")]
    [SerializeField] private GameObject _DetectionSphere;
    [SerializeField] private GameObject _DetectionDoors;

    [Header("Audio")] 
    [SerializeField] private AudioClip _fastShoutAudioClip;
    [SerializeField] private AudioClip _fastRunAudioClip;
    [SerializeField] private AudioClip _fastAudioClip;
    
    private AudioSource _FastAudioSource;
    private NavMeshAgent _NavMeshAgent;
    private Collider _Collider;
    private Animator _Animator;
    private Vector3 _SoundPos;
    private int _RangeChaseAfterStop;
    private int _RangeSearchSound;
    private float _BetaDotProduct;
    private float _LastLvlSound;
    private bool _OpeningDoor;
    private bool _Patrolling;
    private bool _FirstTime;
    private bool _Looking;
    private bool _Search;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeToPatrolCoroutine;
    private Coroutine _AttackCoroutine;
    private Coroutine _ActivateLookingCoroutine;
    
    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;
    
    private void Awake()
    {
        _Collider = GetComponent<Collider>();
        _FastAudioSource = GetComponent<AudioSource>();
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
        _FirstTime = true;
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
                _FirstTime = true;
                _Patrolling = false;
                if(_PatrolCoroutine == null)
                    _PatrolCoroutine = StartCoroutine(Patrol());
                _Animator.Play("Walk");
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 6f;
                _Animator.Play("Run");
                break;
            case EnemyStates.ATTACK:
                _NavMeshAgent.speed = 0;
                _NavMeshAgent.SetDestination(transform.position);
                if (_AttackCoroutine == null)
                    _AttackCoroutine = StartCoroutine(LookAttackPlayer());
                _Animator.Play("Attack");
                _Animator.speed = 1.75f;
                break;
            case EnemyStates.KNOCKED:
                if(_ActivateLookingCoroutine != null)
                {
                    StopCoroutine(_ActivateLookingCoroutine);
                    _ActivateLookingCoroutine = null;
                }
                _Collider.enabled = false;
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
                if (_PatrolCoroutine != null)
                {
                    StopCoroutine(_PatrolCoroutine);
                    _PatrolCoroutine = null;
                }
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
                _Animator.speed = 1;
                break;
            case EnemyStates.KNOCKED:
                _Collider.enabled = true;
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
            Vector3 point = new Vector3(randomPoint.x, randomPoint.y, randomPoint.z);

            NavMeshHit hit;
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

    //Funcio que criden tant els objectes llençables com el jugador
    //per notificar als enemics que han fet un so, i que aquests actuin acorde
    public override void ListenSound(Vector3 pos, int lvlSound)
    {
        _SoundPos = pos;
        //Fem un raycast des de la posicio de l'enemic fins a l'origen del so i recollim tots els objectes que hem tocat en el cami
        RaycastHit[] hits = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));

        //Per cada objecte que hem tocat mirem si són objectes que atenuen el so
        //Si ho són reduïm el valor del so rebut per paràmetre pel valor que ens dona l'objecte.
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<IAttenuable>(out IAttenuable a))
            {
                lvlSound = a.AttenuateSound(lvlSound);
            }
        }
        //Distància del cec fins al punt del so
        float dist = Vector3.Distance(this.transform.position, _SoundPos);
        Debug.Log($"Distància entre ràpid i punt de so: {dist}");
        
        //Si la distància entre l'origen del so i el monstre és menor a 10 i el monstre té visió directa amb el jugador,
        //el monstre passarà al valor més agressiu de resposta al so.
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
        
        //Llista de resposta al so per part del monstre. Com més fort és el nivell de so
        //més a prop d'aquest va, fins que si és molt alt el monstre atacarà.
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
                    _RangeSearchSound = 0;
                    _Search = true;
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
                        //L'enemic agafa un punt aleatori de l'array de waypoints que té disponibles,
                        //excepte si el que ha agafat és al que anava actualment. En aquest cas agafarà un
                        //altre punt.
                        _Waypoint = _Waypoints[Random.Range(0, _Waypoints.Count)];
                        if (_Waypoint != waypointToGo)
                        {
                            waypointToGo = _Waypoint;
                            break; 
                        }
                    }
                    _NavMeshAgent.SetDestination(_Waypoint.transform.position);
                }
                else
                {
                    //Aquesta comprovació és per evitar que si ha sentit un so
                    //molt fort elimini el SetDestination() cap al punt
                    if(_RangeSearchSound > 0)
                    {
                        RandomPoint(_SoundPos, _RangeSearchSound, out Vector3 coord);
                        _Waypoint.transform.position = coord;
                        _NavMeshAgent.SetDestination(_Waypoint.transform.position);
                    }
                }
                _Animator.Play("Walk");
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

    //Corutina per detectar a quina distància està el jugador una vegada l'enemic es recuperi del noqueig.
    //Depenent de quina distància estigui el jugador o l'atacarà, el perseguirà o passarà a patrullar els waypoints
    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(10);
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
            if(_ActivateLookingCoroutine == null)
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

    //Corutina que realitza tot el càlcul per saber si el jugador i l'enemic s'estan mirant i actuar
    IEnumerator LookingPlayer()
    {
        float alphaLook = 0;
        while (true)
        {
            if(_CurrentState != EnemyStates.KNOCKED)
            {
                //Primer mirem si el jugador està dins de l'àrea per actuar.
                Collider[] aux = Physics.OverlapSphere(transform.position, _RangeChaseAfterStop, _LayerPlayer);
                if (aux.Length > 0)
                {
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
                            if (_FirstTime)
                            {
                                _FastAudioSource.PlayOneShot(_fastShoutAudioClip, 6.5f);
                                _FirstTime = false;
                            }
                            if (!_Looking)
                            {
                                transform.LookAt(new Vector3(info.transform.position.x, 0.9f, info.transform.position.z));
                                _Looking = true;
                            }

                            //Si el càlcul del Vector3.Dot ens dona que l'enemic i el jugador s'estan mirant 
                            //l'enemic passarà a l'estat d'aturat, on no es mourà, només mirarà al jugador constantment.
                            if (alphaLook < -0.8f)
                            {
                                if (_CurrentState != EnemyStates.STOPPED)
                                {
                                    _FastAudioSource.resource = _fastAudioClip;
                                    _FastAudioSource.Play();
                                    _NavMeshAgent.SetDestination(transform.position);
                                    ChangeState(EnemyStates.STOPPED);
                                }
                            }
                            else
                            {
                                //Tornem a calcular l'alphaLook per si el jugador encara ens segueix mirant
                                transform.LookAt(new Vector3(info.transform.position.x, 0.9f, info.transform.position.z));
                                alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);
                                
                                //Si després de tornar a calcular el Vector3.Dot l'enemic i el jugador no s'estan mirant canviarem l'estat de l'enemic
                                //per actuar contra el jugador
                                if (alphaLook >= -0.8f)
                                {
                                    //Si el jugador està lluny canviarà a chase i el perseguirà
                                    if (Vector3.Distance(info.transform.position, transform.position) > 2)
                                    {
                                        Debug.Log("Veig al jugador lluny");
                                        if (_CurrentState != EnemyStates.CHASE)
                                        {
                                            _FastAudioSource.resource = _fastRunAudioClip;
                                            _FastAudioSource.Play();
                                            _RangeChaseAfterStop = 28;
                                            ChangeState(EnemyStates.CHASE);
                                        }
                                    }
                                    //Si es troba prou a prop l'atacarà directament
                                    else if (Vector3.Distance(info.transform.position, transform.position) <= 2)
                                    {
                                        Physics.Raycast(transform.position,
                                            (_Player.transform.position - transform.position), out RaycastHit thing, 12,
                                            _LayerObjectsAndPlayer);
                                        if (thing.transform.CompareTag("Player"))
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
                        }
                        //Si estava aturat i el jugador surt de la seva àrea de detecció perseguirà també al jugador,
                        //no es pot escapar una vegada s'han mirat.
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
                    //Si el jugador s'ha amagat darrere alguna paret o a un dels armaris que hi ha també el perseguirà
                    else
                    {
                        _Looking = false;
                        //Si estava en aquest moment aturat el perseguirà, ja que
                        //s'estaven mirant
                        if(_CurrentState == EnemyStates.STOPPED)
                        {
                            Debug.Log("El jugador ha sortit del rang de visió");
                            if (_CurrentState != EnemyStates.CHASE)
                            {
                                _RangeChaseAfterStop = 28;
                                ChangeState(EnemyStates.CHASE);
                            }
                        }
                        //Si no estava aturat passarà a patrulla
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