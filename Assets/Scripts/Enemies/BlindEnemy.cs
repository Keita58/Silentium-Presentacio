using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

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

    private float _RangeSearchSound;
    private int MAXHEALTH = 2;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeStateToPatrol;
    private Coroutine _LeaveAttack;
    private Coroutine _ChangeState;
    private Coroutine _AttackCoroutine;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _PointOfPatrol = transform.position;
        _RangeSearchSound = 55;
        _Patrolling = false;
        _Search = false;
        _Hp = MAXHEALTH;

        _LeaveAttack = null;
        _ChangeState = null;

        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += OnExit;

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

        switch (_CurrentState)
        {
            case EnemyStates.PATROL:
                _Patrolling = false;
                _NavMeshAgent.speed = 4.5f;
                _PatrolCoroutine = StartCoroutine(Patrol(_RangeSearchSound, _PointOfPatrol));
                if (_Search)
                    _ChangeStateToPatrol = StartCoroutine(WakeUp(15));
                break;
            case EnemyStates.ATTACK:
                AttackPlayer();
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
                if(_PatrolCoroutine != null)
                    StopCoroutine(_PatrolCoroutine);
                if(_ChangeStateToPatrol != null)
                    StopCoroutine(_ChangeStateToPatrol);
                break;
            case EnemyStates.ATTACK:
                _PointOfPatrol = transform.position;
                _ChangeStateToPatrol = null;
                _RangeSearchSound = 25;
                _LeaveAttack = null;
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
        }
    }

    #endregion 

    // Funció per moure l'enemic pel mapa
    IEnumerator Patrol(float range, Vector3 pointOfSearch)
    {
        Vector3 point = Vector3.zero;
        while (true)
        {
            if (!_Patrolling)
            {
                if (!_Search)
                {
                    while (true)
                    {
                        point = _Waypoints[Random.Range(0, _Waypoints.Count)].position;
                        if (point != _NavMeshAgent.destination)
                            break;
                    }
                }
                else
                {
                    RandomPoint(_PointOfPatrol, 25, out Vector3 coord);
                    point = coord;
                }
                _NavMeshAgent.SetDestination(new Vector3(point.x, point.y, point.z));
                _Patrolling = true;
            }

            if (_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _Patrolling = false;
                yield return new WaitForSeconds(2);
            }
            else if (_NavMeshAgent.speed == 0)
            {
                GameObject aux = new GameObject();
                NavMeshLink link = aux.AddComponent<NavMeshLink>();

                Vector3 randomPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z) + Random.insideUnitSphere * 2;

                NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);
                link.endPoint = hit.position;
                NavMesh.SamplePosition(transform.position, out NavMeshHit hit2, 1.0f, NavMesh.AllAreas);
                link.startPoint = hit2.position;
                link.autoUpdate = true;
                link.width = 3;
                link.agentTypeID = _NavMeshAgent.agentTypeID;
                link.area = 3;

                StartCoroutine(DeleteLink(aux));

                yield return new WaitForSeconds(0.5f);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

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

        Debug.Log($"Nivell so: {lvlSound}");

        if (lvlSound > 0)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 3;
                _Search = true;
                ChangeState(EnemyStates.PATROL);
            }
            else if (lvlSound > 2 && lvlSound <= 3)
            {
                _RangeSearchSound = 1;
                _Search = true;
                ChangeState(EnemyStates.PATROL);
            }
            else if (lvlSound > 3 && lvlSound <= 5)
            {
                _RangeSearchSound = 0.5f;
                _Search = true;
                ChangeState(EnemyStates.PATROL);
            }
            else if (lvlSound > 5)
            {
                Debug.Log("Entro a l'atac");
                ChangeState(EnemyStates.ATTACK);
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

    IEnumerator WakeUp(int time)
    {
        yield return new WaitForSeconds(time);
        _Search = false;
        ChangeState(EnemyStates.PATROL);
    }

    private void AttackPlayer()
    {
        //Animation -> attack
        bool wall = false;
        RaycastHit[] hits = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.tag == "Wall")
            {
                wall = true;
                break;
            }
        }

        if (Vector3.Distance(_SoundPos, transform.position) > 1 && !wall)
        {
            _NavMeshAgent.speed = 7;
            GameObject aux = new GameObject();
            NavMeshLink link = aux.AddComponent<NavMeshLink>();
            NavMesh.SamplePosition(_SoundPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas);
            link.endPoint = hit.position;
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit2, 1.5f, NavMesh.AllAreas);
            link.startPoint = hit2.position;
            link.width = 3;
            link.agentTypeID = _NavMeshAgent.agentTypeID;
            link.area = 3;

            _NavMeshAgent.SetDestination(_SoundPos);

            StartCoroutine(DeleteLink(aux));

            if (_ChangeState == null)
                _ChangeState = StartCoroutine(ChangeState());
        }
        else if (Vector3.Distance(_SoundPos, transform.position) > 1)
        {
            _NavMeshAgent.SetDestination(_SoundPos);

            if(_ChangeState == null)
                _ChangeState = StartCoroutine(ChangeState());
        }
        else
        {
            transform.LookAt(_SoundPos);
            _NavMeshAgent.SetDestination(transform.position);
        }
    }

    IEnumerator OpenDoors()
    {
        while (true)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f, _LayerDoor))
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
        yield return new WaitForSeconds(1);
        door.Close();
    }

    IEnumerator DeleteLink(GameObject go)
    {
        yield return new WaitForSeconds(2);
        Destroy(go);
    }

    IEnumerator ChangeState()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            if(_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _LeaveAttack = StartCoroutine(RemoveAttackState());
                _ChangeState = null;
                break;
            }
        }
    }

    IEnumerator Attack()
    {
        while (true)
        {
            _Player.GetComponent<Player>().TakeDamage(3);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator RemoveAttackState()
    {
        yield return new WaitForSeconds(3f);
        _Search = true;
        _RangeSearchSound = 2;
        _PointOfPatrol = transform.position;
        ChangeState(EnemyStates.PATROL);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_LeaveAttack != null)
        {
            StopCoroutine(_LeaveAttack);
            _LeaveAttack = null;
        }
        if (_ChangeState != null)
        {
            StopCoroutine(_ChangeState);
            _ChangeState = null;
        }
        _AttackCoroutine = StartCoroutine(Attack());
    }

    private void OnExit()
    {
        StopCoroutine(_AttackCoroutine);
        _LeaveAttack = StartCoroutine(RemoveAttackState());
    }
}
