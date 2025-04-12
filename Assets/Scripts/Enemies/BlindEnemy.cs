using System.Collections;
using System.Drawing;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BlindEnemy : Enemy
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

    private float _RangeSearchSound;
    private int MAXHEALTH = 2;

    private Coroutine _PatrolCoroutine;
    private Coroutine _ChangeStateToPatrol;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _PointOfPatrol = new Vector3(-35, 7.2f, 0);
        _NavMeshAgent.SetDestination(_PointOfPatrol);
        _RangeSearchSound = 55;
        _Patrolling = false;
        _Hp = MAXHEALTH;

        _DetectionSphere.GetComponent<DetectionSphere>().OnExit += OnExit;
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
                break;
            case EnemyStates.ATTACK:
                AttackPlayer();
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
                break;
            case EnemyStates.ATTACK:
                _PointOfPatrol = transform.position;
                _ChangeStateToPatrol = null;
                _RangeSearchSound = 25;
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
                /*RandomPoint(pointOfSearch, range, out Vector3 coord);
                point = coord;
                _NavMeshAgent.SetDestination(new Vector3(point.x, point.y, point.z));
                _Patrolling = true;*/
            }

            if (_NavMeshAgent.remainingDistance <= _NavMeshAgent.stoppingDistance)
            {
                _Patrolling = false;
                yield return new WaitForSeconds(3);
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

        Debug.Log($"Nivell so: {lvlSound}");

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 3;
            }
            else if (lvlSound > 2 && lvlSound <= 3)
            {
                _RangeSearchSound = 1;
            }
            else if (lvlSound > 3 && lvlSound <= 5)
            {
                _RangeSearchSound = 0.3f;
            }
            else if (lvlSound > 5)
            {
                _PointOfPatrol = pos;
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

    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(5);
        ChangeState(EnemyStates.PATROL);
    }

    private void AttackPlayer()
    {
        //Animation -> attack
        transform.LookAt(_Player.transform.position);
        _NavMeshAgent.SetDestination(transform.position);
        if (Vector3.Distance(_Player.transform.position, transform.position) > 2)
        {
            _NavMeshAgent.speed = 15;
            GameObject aux = new GameObject();
            aux.AddComponent<NavMeshLink>();
            NavMesh.SamplePosition(_SoundPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);
            aux.GetComponent<NavMeshLink>().endPoint = hit.position;
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit2, 1.0f, NavMesh.AllAreas);
            aux.GetComponent<NavMeshLink>().startPoint = hit2.position;
            aux.GetComponent<NavMeshLink>().autoUpdate = true;
            aux.GetComponent<NavMeshLink>().width = 3;
            aux.GetComponent<NavMeshLink>().agentTypeID = _NavMeshAgent.agentTypeID;
            aux.GetComponent<NavMeshLink>().area = 3;
            _NavMeshAgent.SetDestination(_SoundPos);
        }
        _Player.GetComponent<Player>().TakeDamage(3);    
    }

    IEnumerator RemoveAttackState()
    {
        yield return new WaitForSeconds(1.5f);
        ChangeState(EnemyStates.PATROL);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
            ChangeState(EnemyStates.ATTACK); 
    }

    private void OnExit()
    {
        _ChangeStateToPatrol = StartCoroutine(RemoveAttackState());
    }
}
