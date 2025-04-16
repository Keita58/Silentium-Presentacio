using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

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
    [SerializeField] private List<Transform> _Waypoints;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private Vector3 _PointOfPatrol;
    private float _BetaDotProduct;
    private bool _Patrolling;
    private bool _Search;

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

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _PointOfPatrol = transform.position;
        _RangeSearchSound = 30;
        _RangeChaseAfterStop = 25;
        _BetaDotProduct = 60;
        _Patrolling = false;
        _Hp = MAXHEALTH;

        _DetectionSphere.GetComponent<DetectionSphere>().OnEnter += ActivateLookingCoroutine;
        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += DeactivateLookingCoroutine;

        StartCoroutine(OpenDoors());
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

        switch(_CurrentState)
        {
            case EnemyStates.PATROL:
                _Patrolling = false;
                _PatrolCoroutine = StartCoroutine(Patrol(_RangeSearchSound, _PointOfPatrol));
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 7;
                break;
            case EnemyStates.ATTACK:
                _AttackCoroutine = StartCoroutine(AttackPlayer());
                break;
            case EnemyStates.KNOCKED:
                StartCoroutine(WakeUp());
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
        switch(exitState)
        {
            case EnemyStates.PATROL:
                StopCoroutine(_PatrolCoroutine);
                _ChangeToPatrolCoroutine = null;
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 3.5f;
                _RangeChaseAfterStop = 25;
                break;
            case EnemyStates.ATTACK:
                StopCoroutine(_AttackCoroutine);
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
            case EnemyStates.STOPPED:
                _ChangeToPatrolCoroutine = null;
                break;
        }
    }

    #endregion 

    private void Update()
    {
        UpdateState(_CurrentState);
    }  

    //Busca punt aleatori dins del NavMesh
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for(int i = 0; i < 50; i++) 
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
            if (NavMesh.SamplePosition(point, out hit, 1.0f, NavMesh.AllAreas) && Vector3.Distance(point, center) > 1.75f)
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
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
            if (hit.collider.TryGetComponent<IAtenuacio>(out IAtenuacio a))
            {
                lvlSound = a.atenuarSo(lvlSound);
            }
        }

        if(lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 5;
                _Search = true;
            }
            else if (lvlSound > 2 && lvlSound <= 5)
            {
                _RangeSearchSound = 3;
                _Search = true;
            }
            else if (lvlSound > 5 && lvlSound <= 9)
            {
                _RangeSearchSound = 1;
                _Search = true;
            }
            else if (lvlSound > 9)
            {
                _NavMeshAgent.SetDestination(_SoundPos);
            }

            _PointOfPatrol = pos;
            if (_ChangeToPatrolCoroutine == null)
                _ChangeToPatrolCoroutine = StartCoroutine(StopChase());
            ChangeState(EnemyStates.PATROL);
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

    // Funció per moure l'enemic pel mapa
    IEnumerator Patrol(int range, Vector3 pointOfSearch)
    {
        Transform point = default;
        while (true)
        {
            if (!_Patrolling)
            {
                if(!_Search) 
                {
                    while(true)
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
                    yield break;
                }
            }
        }
        else
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator AttackPlayer()
    {
        while(true)
        {
            //Animation -> attack
            _Player.GetComponent<Player>().TakeDamage(1);
            yield return new WaitForSeconds(1);
        }
    }

    // Comptador per parar de perseguir al jugador
    IEnumerator StopChase()
    {
        yield return new WaitForSeconds(5);
        _RangeSearchSound = 50;
        _PointOfPatrol = transform.position;
        _Search = true;
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator OpenDoors()
    {
        while(true)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f, _LayerDoor))
            {
                if(hit.collider.TryGetComponent<Door>(out Door door) && !door.isLocked)
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
        yield return new WaitForSeconds(1);
        door.Close();
    }

    IEnumerator LookingPlayer()
    {
        while(true)
        {
            Collider[] aux = Physics.OverlapSphere(transform.position, _RangeChaseAfterStop, _LayerPlayer);
            if (aux.Length > 0)
            {
                float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - this.transform.position).normalized);
                float alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);

                //Fem l'arcos del docProduct per poder comprovar si l'angle és menor a la meitat de l'angle de 
                //visió dels enemics
                //Ho hem de convertir a graus perque l'arccos retorna radiants de merda
                if (Mathf.Acos(alphaDocProduct) * Mathf.Rad2Deg < _BetaDotProduct)
                {
                    //Raycast amb les layers de paret i player i si tenim la paret no seguim, sinó seguim el jugador
                    if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                    {
                        if (info.transform.tag == "Player")
                        {
                            transform.LookAt(info.transform.position);

                            if (alphaLook < -0.9f)
                            {
                                _NavMeshAgent.SetDestination(transform.position);
                                ChangeState(EnemyStates.STOPPED);
                            }
                            else
                            {
                                if (Vector3.Distance(info.transform.position, transform.position) > 2)
                                {
                                    Debug.Log("Veig al jugador lluny");
                                    _RangeChaseAfterStop = 28;
                                    if (_CurrentState != EnemyStates.CHASE)
                                    {
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
                        else 
                        {
                            Debug.Log("No veig al jugador!");
                            if (_ChangeToPatrolCoroutine == null && _CurrentState != EnemyStates.PATROL)
                            {
                                Debug.Log("Activo corutina");
                                _ChangeToPatrolCoroutine = StartCoroutine(StopChase());
                            }
                        }
                    }
                    else
                    {
                        ChangeState(EnemyStates.PATROL);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ActivateLookingCoroutine()
    {
        _ActivateLookingCoroutine = StartCoroutine(LookingPlayer());
    }

    private void DeactivateLookingCoroutine()
    {
        StopCoroutine(_ActivateLookingCoroutine);

        if (_CurrentState == EnemyStates.STOPPED)
        {
            _RangeChaseAfterStop = 28;
            ChangeState(EnemyStates.CHASE);
        }
    }
}
