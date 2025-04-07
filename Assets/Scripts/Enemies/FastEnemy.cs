using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class FastEnemy : Enemy
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
    private bool _Patrolling;
    private bool _Wait;

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
    private Coroutine _SoundCoroutine;
    private Coroutine _StopChaseCoroutine;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _SoundPos = Vector3.zero;
        _RangeSearchSound = 0;
        _RangeChaseAfterStop = 10;
        _BetaDotProduct = 60;
        _Patrolling = false;
        _Chase = false;
        _Wait = false;
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

        switch(_CurrentState)
        {
            case EnemyStates.PATROL:
                _Patrolling = false;
                _PatrolCoroutine = StartCoroutine(Patrol());
                break;
            case EnemyStates.SEARCH:
                _ChangeToPatrolCoroutine = StartCoroutine(WaitChange()); //Temps d'espera per canviar a patrulla
                _SoundCoroutine = StartCoroutine(Search(_RangeSearchSound));
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
            case EnemyStates.STOPPED:
                if(_PatrolCoroutine != null)
                    StopCoroutine(_PatrolCoroutine);
                if(_ChangeToPatrolCoroutine != null)
                    StopCoroutine(_ChangeToPatrolCoroutine);
                if(_AttackCoroutine != null)
                    StopCoroutine(_AttackCoroutine);
                if(_SoundCoroutine != null)
                    StopCoroutine(_SoundCoroutine);
                if(_StopChaseCoroutine != null)
                    StopCoroutine(_StopChaseCoroutine);
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
        switch(exitState)
        {
            case EnemyStates.PATROL:
                StopCoroutine(_PatrolCoroutine);
                break;
            case EnemyStates.SEARCH:
                StopCoroutine(_SoundCoroutine);
                break;
            case EnemyStates.CHASE:
                _NavMeshAgent.speed = 3.5f;
                _RangeChaseAfterStop = 10;
                break;
            case EnemyStates.ATTACK:
                StopCoroutine(_AttackCoroutine);
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
        Vector3 point = Vector3.zero;
        while (true)
        {
            if (!_Patrolling)
            {
                RandomPoint(transform.position, 15, out Vector3 coord);
                point = coord;
                _NavMeshAgent.SetDestination(new Vector3(point.x, transform.position.y, point.z));
                _Patrolling = true;
            }

            if (Vector3.Distance(point, transform.position) < 3)
            {
                _Patrolling = false;
                yield return new WaitForSeconds(2);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator Search(int range)
    {
        Debug.Log("Entro al Search");
        while (true)
        {
            if (Vector3.Distance(_NavMeshAgent.destination, transform.position) < 3)
            {
                RandomPoint(_SoundPos, range, out Vector3 hit);
                _NavMeshAgent.SetDestination(hit);
            }
            else
                yield return new WaitForSeconds(1f);
        }
    }

    //Busca punt aleatori dins del NavMesh
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for(int i = 0; i < 50; i++) 
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

    private void DetectPlayer()
    {
        Collider[] aux;
        if(_CurrentState == EnemyStates.CHASE)
            aux = Physics.OverlapSphere(transform.position, _RangeChaseAfterStop, _LayerPlayer);
        else
            aux = Physics.OverlapSphere(transform.position, 10f, _LayerPlayer);

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
                        ChangeState(EnemyStates.STOPPED);

                        if(Vector3.Distance(transform.position, info.transform.position) > 2)
                        {
                            _Chase = true;
                            Debug.Log("Veig al jugador lluny");
                            transform.LookAt(info.transform.position);
                            _NavMeshAgent.SetDestination(_Player.transform.position);
                            if (_CurrentState != EnemyStates.CHASE)
                                ChangeState(EnemyStates.CHASE);
                        }
                        else if (Vector3.Distance(transform.position, info.transform.position) < 2)
                        {
                            Debug.Log("Tinc al jugador al davant!");
                            transform.LookAt(info.transform.position);
                            _NavMeshAgent.SetDestination(transform.position);
                            if(_CurrentState != EnemyStates.ATTACK)
                                ChangeState(EnemyStates.ATTACK);
                        }
                    }
                    else if(!_Wait)
                    {
                        _Wait = true;
                        _ChangeToPatrolCoroutine = StartCoroutine(WaitChange());
                    }
                }
            }
            else if (_Chase)
            {
                RandomPoint(_Player.transform.position, 2, out Vector3 point);
                _StopChaseCoroutine = StartCoroutine(StopChase());
                _NavMeshAgent.SetDestination(point);
            }
        }
        else
        {
            Debug.Log("No detecto res a la meva mirada!");
            if(_CurrentState == EnemyStates.CHASE)
            {
                ChangeState(EnemyStates.PATROL);
            }
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

        if(lvlSound > 0 && _CurrentState == EnemyStates.PATROL)
        {
            if (lvlSound > 0 && lvlSound <= 2)
            {
                _RangeSearchSound = 10;
            }
            else if (lvlSound > 2 && lvlSound <= 5)
            {
                _RangeSearchSound = 5;
            }
            else if (lvlSound > 5 && lvlSound <= 9)
            {
                _RangeSearchSound = 2;
            }
            else if (lvlSound > 9)
            {
                _NavMeshAgent.SetDestination(_SoundPos);
            }

            if(_CurrentState != EnemyStates.SEARCH)
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
        Collider[] aux = Physics.OverlapSphere(transform.position, 2f, _LayerPlayer);
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
        _Chase = false;
    }

    IEnumerator WaitChange()
    {
        yield return new WaitForSeconds(5f);
        _Wait = false;
        ChangeState(EnemyStates.PATROL);
    }

    IEnumerator LookingPlayer()
    {
        while(true)
        {
            Collider[] aux = Physics.OverlapSphere(transform.position, 25f, _LayerPlayer);
            if (aux.Length > 0)
            {
                float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - this.transform.position).normalized);
                float alphaLook = Vector3.Dot(transform.forward, _Player.transform.forward);

                //Fem l'arcos del docProduct per poder comprovar si l'angle és menor a la meitat de l'angle de 
                //visió dels enemics
                if (Mathf.Acos(alphaDocProduct) < _BetaDotProduct)
                {
                    //Raycast amb les layers de paret i player i si tenim la paret no seguim, sinó seguim el jugador
                    if (Physics.Raycast(transform.position, (_Player.transform.position - transform.position), out RaycastHit info, _LayerObjectsAndPlayer))
                    {
                        if (info.transform.tag == "Player")
                        {
                            transform.LookAt(_Player.transform.position);
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
                                {
                                    _RangeChaseAfterStop = 25;
                                    ChangeState(EnemyStates.CHASE);
                                }
                            }
                        }
                        else
                        {
                            _NavMeshAgent.SetDestination(transform.position);
                            _SoundPos = transform.position;
                            ChangeState(EnemyStates.SEARCH);
                        }
                    }
                }
            }
            else if (_CurrentState == EnemyStates.STOPPED)
            {
                _RangeChaseAfterStop = 25;
                ChangeState(EnemyStates.CHASE);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10f);
        Gizmos.DrawWireSphere(transform.position, 15f);
        Gizmos.DrawWireSphere(transform.position, 25f);
    }
}
