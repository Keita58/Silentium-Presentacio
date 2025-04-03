using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FatEnemy : Enemy
{
    private enum EnemyStates { PATROL, SEARCH, CHASE, ATTACK, KNOCKED, STOPPED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private EnemyStates _LastState;
    [SerializeField] private float _StateTime;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;

    private NavMeshAgent _NavMeshAgent;
    private Vector3 _SoundPos;
    private float _BetaDotProduct;
    private bool _Chase;
    private bool _SearchSound;
    private bool _Detected;
    private bool _Patrolling;
    private bool _Wait;
    private bool _Attack;

    private int _Hp;
    public override int hp => _Hp;

    private int _DownTime;
    public override int downTime => _DownTime;

    private int MAXHEALTH = 3;
    private int _RangeSearchSound;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 0;
        _BetaDotProduct = 60;
        _SearchSound = false;
        _Detected = false;
        _Patrolling = false;
        _Chase = false;
        _Wait = false;
        _Attack = false;
        _Hp = MAXHEALTH;
    }

    private void Start()
    {
        InitState(EnemyStates.PATROL);
        StartCoroutine(LookingPlayer());
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
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 7;
                break;
            case EnemyStates.ATTACK:
                _Attack = true;
                StartCoroutine(AttackPlayer());
                break;
            case EnemyStates.KNOCKED:
                StartCoroutine(WakeUp());
                break;
            case EnemyStates.STOPPED:
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
            case EnemyStates.CHASE:
            case EnemyStates.ATTACK:
                DetectPlayer();
                break;
            case EnemyStates.KNOCKED:
            case EnemyStates.STOPPED:
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

                _NavMeshAgent.speed = Random.Range(3.5f, 6);
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

    private void DetectPlayer()
    {
        Collider[] aux = Physics.OverlapSphere(transform.position, 10f, _LayerPlayer);
        if (aux.Length > 0)
        {
            float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - this.transform.position).normalized);

            //Fem l'arcos del docProduct per poder comprovar si l'angle és menor a la meitat de l'angle de 
            //visió dels enemics
            if (Mathf.Acos(alphaDocProduct) < _BetaDotProduct)
            {
                //Raycast amb les layers de paret i player i si tenim la paret no seguim, sinó seguim el jugador
                if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                {
                    if (info.transform.tag == "Player")
                    {
                        _SearchSound = false;
                        _Chase = true;
                        _Detected = true;
                        StopCoroutine(StopChase());

                        if (Vector3.Distance(transform.position, info.transform.position) > 2)
                        {
                            Debug.Log("Detecto alguna cosa aprop!");
                            transform.LookAt(info.transform.position);
                            _NavMeshAgent.SetDestination(_Player.transform.position);
                            if (_CurrentState != EnemyStates.CHASE)
                                ChangeState(EnemyStates.CHASE);
                        }
                        else
                        {
                            transform.LookAt(info.transform.position);
                            _NavMeshAgent.SetDestination(transform.position);
                            if (_CurrentState != EnemyStates.ATTACK)
                                ChangeState(EnemyStates.ATTACK);
                        }
                    }
                }
            }
            else if (_Chase)
            {
                RandomPoint(_Player.transform.position, 2, out Vector3 point);
                StartCoroutine(StopChase());
                _NavMeshAgent.SetDestination(point);
            }
        }
        else
        {
            Debug.Log("No detecto res a la meva mirada!");
            if (_CurrentState == EnemyStates.CHASE)
                ChangeState(EnemyStates.PATROL);
        }
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
            if (lvlSound > 0 && lvlSound <= 2)
                _NavMeshAgent.SetDestination(_SoundPos);
            else if (lvlSound > 2 && lvlSound <= 5)
            {
                RandomPoint(_SoundPos, 2, out _);
                _RangeSearchSound = 2;
            }
            else if (lvlSound > 5 && lvlSound <= 9)
            {
                RandomPoint(_SoundPos, 5, out _);
                _RangeSearchSound = 5;
            }
            else if (lvlSound > 9 && lvlSound <= 15)
            {
                RandomPoint(_SoundPos, 10, out _);
                _RangeSearchSound = 10;
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

    // Comptador per parar de perseguir al jugador
    IEnumerator StopChase()
    {
        yield return new WaitForSeconds(5);
        _Chase = false;
    }

    IEnumerator WaitChange()
    {
        yield return new WaitForSeconds(3f);
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator LookingPlayer()
    {
        while (true)
        {
            Collider[] aux = Physics.OverlapSphere(transform.position, 15f, _LayerPlayer);
            if (aux.Length > 0)
            {
                transform.LookAt(_Player.transform.position);
                float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - this.transform.position).normalized);
                float alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);
                Debug.Log($"Alpha: {alphaLook}");

                //Fem l'arcos del docProduct per poder comprovar si l'angle és menor a la meitat de l'angle de 
                //visió dels enemics
                if (Mathf.Acos(alphaDocProduct) < _BetaDotProduct)
                {
                    //Raycast amb les layers de paret i player i si tenim la paret no seguim, sinó seguim el jugador
                    if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                    {
                        if (info.transform.tag == "Player")
                        {
                            if (alphaLook < -0.9f)
                            {
                                _NavMeshAgent.SetDestination(transform.position);
                                ChangeState(EnemyStates.STOPPED);
                            }
                            else
                            {
                                if (Vector3.Distance(transform.position, _Player.transform.position) < 2)
                                    ChangeState(EnemyStates.ATTACK);
                                else
                                    ChangeState(EnemyStates.CHASE);
                            }
                        }
                    }
                }
            }
            else if (_CurrentState == EnemyStates.STOPPED)
                ChangeState(EnemyStates.CHASE);

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10f);
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
