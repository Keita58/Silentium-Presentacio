using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FatEnemy : Enemy
{
    private enum EnemyStates { PATROL, SEARCH, ATTACK, KNOCKED }
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

    private int MAXHEALTH = 6;
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
        StartCoroutine(WaitingAttack());
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
            case EnemyStates.SEARCH:
                _SearchSound = true;
                StartCoroutine(Search(_RangeSearchSound));
                break;
            case EnemyStates.ATTACK:
                _Attack = true;
                StartCoroutine(AttackPlayer());
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
            case EnemyStates.SEARCH:
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
            case EnemyStates.SEARCH:
                _SearchSound = false;
                _Wait = false;
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
        float range = 45.0f;
        while (!_Detected)
        {
            if (!_Patrolling)
            {
                if (RandomPoint(transform.position, range, out coord))
                {
                    Debug.DrawRay(coord, Vector3.up, UnityEngine.Color.black, 1.0f);
                }

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

    IEnumerator Search(int range)
    {
        while (_SearchSound)
        {
            if (transform.position.x == _NavMeshAgent.destination.x && transform.position.z == _NavMeshAgent.destination.z)
            {
                if (!_Wait)
                {
                    _Wait = true;
                    StartCoroutine(WaitChange()); //Temps d'espera per canviar a patrulla (te posat un change a patrulla)
                }
                yield return new WaitForSeconds(0.5f);
                RandomPoint(_SoundPos, range, out Vector3 hit);
                _NavMeshAgent.destination = hit;
            }
            else
                yield return new WaitForSeconds(1f);
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

        if (lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 3)
            {
                RandomPoint(_SoundPos, 5, out _);
                _RangeSearchSound = 5;
            }
            else if (lvlSound > 3 && lvlSound <= 5)
            {
                RandomPoint(_SoundPos, 3, out _);
                _RangeSearchSound = 3;
            }
            else if (lvlSound > 5 && lvlSound <= 7)
            {
                RandomPoint(_SoundPos, 2, out _);
                _RangeSearchSound = 2;
            }
            else if (lvlSound > 7)
            {
                _NavMeshAgent.SetDestination(_SoundPos);
            }

            if (_CurrentState != EnemyStates.SEARCH)
                ChangeState(EnemyStates.SEARCH);
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
        Collider[] aux = Physics.OverlapSphere(transform.position, 3f, _LayerPlayer);
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
        else
            ChangeState(EnemyStates.PATROL);
    }

    IEnumerator AttackPlayer()
    {
        while (_Attack)
        {
            //Animation -> attack
            _Player.GetComponent<Player>().TakeDamage(1);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator WaitChange()
    {
        yield return new WaitForSeconds(10f);
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator WaitingAttack()
    {
        while (true)
        {
            Collider[] aux = Physics.OverlapSphere(transform.position, 3f, _LayerPlayer);
            if (aux.Length > 0)
            {
                if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                {
                    if (info.transform.tag == "Player")
                    {
                        transform.LookAt(info.transform.position);
                        _NavMeshAgent.SetDestination(transform.position);
                        ChangeState(EnemyStates.ATTACK);
                    }
                }
            }
            else if (_CurrentState == EnemyStates.ATTACK)
                ChangeState(EnemyStates.PATROL);

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
