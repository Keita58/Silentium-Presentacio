using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FatEnemy : Enemy
{
    private readonly int MAXHEALTH = 6;
    private enum EnemyStates { PATROL, ATTACK, KNOCKED, CHASE }
    
    [Header("States")]
    [SerializeField] private EnemyStates _CurrentState;
    
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
    [SerializeField] private GameObject _DetectionAttack;
    [SerializeField] private GameObject _DetectionDoors;

    [Header("Audio")]
    [SerializeField] private AudioClip _fatAudioClip;
    private AudioSource _fatAudioSource;
    
    private NavMeshAgent _NavMeshAgent;
    private Collider _Collider;
    private Animator _Animator;
    private Vector3 _SoundPos;
    private int _RangeSearchSound;
    private bool _OpeningDoor;
    private bool _Patrolling;
    private bool _Search;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;
    
    private Coroutine _ChangeToPatrolCoroutine;
    private Coroutine _PatrolCoroutine;
    private Coroutine _AttackCoroutine;
    private Coroutine _ChaseCoroutine;
    private Coroutine _ChangeToChase;

    private void Awake()
    {
        _Collider = GetComponent<Collider>();
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
        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter -= ActivateChaseCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit -= DeactivateChaseCoroutine;
        _DetectionAttack.GetComponent<DetectionSphere>().OnEnter -= ActivateAttack;
        _DetectionAttack.GetComponent<DetectionSphere>().OnExit -= DeactivateAttack;
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
                _Collider.enabled = false;
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
                _Collider.enabled = true;
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

    //Busca punt aleatori dins del NavMesh
    private void RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 50; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = new Vector3(center.x, center.y, center.z) + Random.insideUnitSphere * range;
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

    //Funcio que criden tant els objectes llençables com el jugador
    //per notificar als enemics que han fet un so, i que aquests actuin acorde
    public override void ListenSound(Vector3 pos, int lvlSound)
    {
        _SoundPos = pos;
        //Fem un raycast des de la posicio de l'enemic fins a l'origen del so i recollim tots els objectes que hem tocat en el cami
        RaycastHit[] hits = Physics.RaycastAll(transform.position, _SoundPos - transform.position, Vector3.Distance(_SoundPos, transform.position));

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
        Debug.Log($"Distància entre cec i punt de so: {dist}");
        
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

        Debug.Log(lvlSound);
        
        //Llista de resposta al so per part del monstre. Com més fort és el nivell de so
        //més a prop d'aquest va, fins que si és molt alt el monstre atacarà.
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
        if(_CurrentState != EnemyStates.KNOCKED)
            ChangeState(EnemyStates.CHASE);
    }

    private void DeactivateChaseCoroutine()
    {
        _Search = false;
        ChangeState(EnemyStates.PATROL);
    }

    private void ActivateAttack()
    {
        if (_CurrentState != EnemyStates.KNOCKED)
        {
            //Si el jugador està davant de l'enemic quan entra dins de l'àrea d'atac (i aquest no està noquejat) l'atacarà.
            Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit thing, 12,
                _LayerObjectsAndPlayer);
            if(thing.transform.gameObject.TryGetComponent(out Player player))
            {
                _Animator.SetBool("Chase", false);
                ChangeState(EnemyStates.ATTACK);
            }
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
            //Esperem que el monstre acabi l'animació d'atac per poder canviar d'estat
            if (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && _CurrentState != EnemyStates.KNOCKED)
            {
                //Si aquest està a prop del jugador el perseguirà, si no passarà a patrullar.
                if(Vector3.Distance(transform.position, _Player.transform.position) <= 3.5f)
                    ChangeState(EnemyStates.CHASE);
                else
                    ChangeState(EnemyStates.PATROL);
                
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}