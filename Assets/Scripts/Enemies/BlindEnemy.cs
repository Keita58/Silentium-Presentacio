using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BlindEnemy : Enemy
{
    private enum EnemyStates { PATROL, ATTACK, KNOCKED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private EnemyStates _LastState;
    [SerializeField] private float _StateTime;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private bool _SearchSound;
    private bool _Detected;
    private bool _Patrolling;
    private bool _Wait;
    private bool _Attack;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private int MAXHEALTH = 2;
    private int _RangeSearchSound;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 0;
        _SearchSound = false;
        _Detected = false;
        _Patrolling = false;
        _Wait = false;
        _Attack = false;
        _Hp = MAXHEALTH;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
    }

    #region FSM

    private void ChangeState(EnemyStates newState)
    {
        if (newState == _CurrentState)
            return;

        ExitState(_CurrentState);
        InitState(newState);
    }

    private void InitState(EnemyStates newState)
    {
        _LastState = _CurrentState;
        _CurrentState = newState;
        _StateTime = 0.0f;

        switch (_CurrentState)
        {
            case EnemyStates.PATROL:
                _Detected = false;
                _Patrolling = false;
                StartCoroutine(Patrol());
                break;
            case EnemyStates.ATTACK:
                AttackPlayer();
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
            case EnemyStates.PATROL:
            case EnemyStates.ATTACK:
            case EnemyStates.KNOCKED:
                break;
        }
    }

    private void ExitState(EnemyStates exitState)
    {
        switch (exitState)
        {
            case EnemyStates.PATROL:
                _Detected = true;
                break;
            case EnemyStates.ATTACK:
                _Attack = false;
                break;
            case EnemyStates.KNOCKED:
                _Hp = MAXHEALTH;
                break;
        }
    }

    #endregion 

    private void Update()
    {
        UpdateState(_CurrentState);
    }

    // Funció per moure l'enemic pel mapa
    IEnumerator Patrol()
    {
        Vector3 coord = Vector3.zero;
        float range = 15.0f;
        while (!_Detected)
        {
            if (!_Patrolling)
            {
                if (RandomPoint(transform.position, range, out coord))
                {
                    Debug.DrawRay(coord, Vector3.up, UnityEngine.Color.black, 1.0f);
                }

                _NavMeshAgent.speed = 4;
                _NavMeshAgent.SetDestination(new Vector3(coord.x, transform.position.y, coord.z));
                _Patrolling = true;
            }

            if (transform.position == new Vector3(coord.x, transform.position.y, coord.z))
            {
                _Patrolling = false;
            }
            yield return new WaitForSeconds(2);
        }
    }

    //Busca punt aleatori dins del NavMesh
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            //Agafa un punt aleatori dins de l'esfera amb el radi que passem per parametre
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;

            //Comprovem que el punt que hem agafat esta dins del NavMesh
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
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
                RandomPoint(_SoundPos, 5, out _);
            }
            else if (lvlSound > 2 && lvlSound <= 3)
            {
                RandomPoint(_SoundPos, 3, out _);
            }
            else if (lvlSound > 3 && lvlSound <= 5)
            {
                RandomPoint(_SoundPos, 2, out _);
            }
            else if (lvlSound > 5)
            {
                if (_Attack && Vector3.Distance(_NavMeshAgent.destination, _SoundPos) < 2)
                    ChangeState(EnemyStates.ATTACK);
                else
                {
                    _NavMeshAgent.SetDestination(_SoundPos);
                    _Attack = true;
                    StartCoroutine(RemoveAttackState());
                }
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
        _NavMeshAgent.speed = 15;
        _NavMeshAgent.SetDestination(_SoundPos);
        _Player.GetComponent<Player>().TakeDamage(3);    
    }

    IEnumerator WaitChange()
    {
        yield return new WaitForSeconds(5f);
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator RemoveAttackState()
    {
        yield return new WaitForSeconds(1.5f);
        ChangeState(EnemyStates.PATROL);
    }
}
