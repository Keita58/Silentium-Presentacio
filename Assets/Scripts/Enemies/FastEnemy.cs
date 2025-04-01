using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FastEnemy : Enemy
{
    private enum EnemyStates { PATROL, SEARCH, CHASE, ATTACK, KNOCKED, STOPPED }
    [SerializeField] private EnemyStates _CurrentState;
    [SerializeField] private float _StateTime;
    [SerializeField] private GameObject _Player;
    [SerializeField] private LayerMask _LayerPlayer;
    [SerializeField] private LayerMask _LayerObjectsAndPlayer;

    private NavMeshAgent _NavMeshAgent;
    private float _BetaDotProduct;
    private Vector3 _SoundPos;
    private bool _Chase;
    private bool _SearchSound;
    private bool _Detected;
    private bool _Patrolling;
    private bool _Wait;
    private bool _Attack;

    private void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        _BetaDotProduct = 60;
        _SearchSound = false;
        _Detected = false;
        _Patrolling = false;
        _Chase = false;
        _Wait = false;
        _Attack = false;
        _SoundPos = Vector3.zero;
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
        _CurrentState = newState;
        _StateTime = 0.0f;

        switch(_CurrentState)
        {
            case EnemyStates.PATROL:
                _Detected = false;
                _Patrolling = false;
                StartCoroutine(Patrol());
                break;
            case EnemyStates.SEARCH:

                break;
            case EnemyStates.CHASE:

                break;
            case EnemyStates.ATTACK:

                break;
            case EnemyStates.KNOCKED:

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
                DetectPlayer();
                break;
            case EnemyStates.ATTACK:
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
                _Detected = true;
                break;
            case EnemyStates.SEARCH:
                _SearchSound = false;
                _Wait = false;
                break;
            case EnemyStates.ATTACK:
                _Attack = false;
                break;
        }
    }

    #endregion 

    private void Update()
    {
        UpdateState(_CurrentState);
    }

    private IEnumerator Patrol()
    {
        Vector3 coord = Vector3.zero;
        float range = 30.0f;
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
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Investigar()
    {
        while (_SearchSound)
        {
            if (transform.position == _NavMeshAgent.destination)
            {
                if (!_Wait)
                {
                    _Wait = true;
                    StartCoroutine(WaitChange()); //Temps d'espera per canviar a patrulla (te posat un change a patrulla)
                }
                yield return new WaitForSeconds(2.5f);
                RandomPoint(_SoundPos, 5f, out Vector3 hit);
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
        if (Physics.OverlapSphere(transform.position, 10f, _LayerPlayer) != null)
        {
            float alphaDocProduct = Vector3.Dot(transform.forward, (_Player.transform.position - this.transform.position).normalized);

            //Fem l'arcos del docProduct per poder comprovar si l'angle és menor a la meitat de l'angle de 
            //visió dels enemics
            if (Mathf.Acos(alphaDocProduct) < _BetaDotProduct)
            {
                //Raycast amb les layers de paret i player i si tenim la paret no seguim, sinó seguim el jugador
                if (Physics.Raycast(transform.position, _Player.transform.position, out RaycastHit info, _LayerObjectsAndPlayer))
                {
                    if (info.transform.gameObject == _Player)
                    {
                        //Debug.Log("Detecto alguna cosa aprop!");
                        _SearchSound = false;
                        _Chase = true;
                        StopCoroutine(StopChase());
                        _NavMeshAgent.SetDestination(_Player.transform.position);
                        if (_CurrentState != EnemyStates.CHASE)
                            ChangeState(EnemyStates.CHASE);
                    }
                }
            }
            else if (_Chase)
            {
                StartCoroutine(StopChase());
                _NavMeshAgent.SetDestination(_Player.transform.position);
            }
        }
        else
        {
            //Debug.Log("No detecto res a la meva mirada!");
            if (_CurrentState == EnemyStates.CHASE)
                ChangeState(EnemyStates.SEARCH);
        }
    }

    public void Listen(Vector3 pos, int lvlSound)
    {
        _SoundPos = pos;
        RaycastHit[] hits = Physics.RaycastAll(this.transform.position, _SoundPos - this.transform.position, Vector3.Distance(_SoundPos, this.transform.position));
        
        /*
        foreach (RaycastHit hit in hits)
        {
            
            if (hit.collider.TryGetComponent<IAtenuacio>(out IAtenuacio a))
            {
                lvlSound = a.atenuarSo(lvlSound);
            }
        }*/

        if (lvlSound >= 1)
        {
            if (_CurrentState == EnemyStates.SEARCH)
                _NavMeshAgent.SetDestination(_SoundPos);
            else if (_CurrentState == EnemyStates.PATROL)
            {
                RandomPoint(_SoundPos, 5f, out _);
                ChangeState(EnemyStates.SEARCH);
            }
        }
    }

    IEnumerator StopChase()
    {
        yield return new WaitForSeconds(2);
        _Chase = false;
    }

    IEnumerator WaitChange()
    {
        yield return new WaitForSeconds(10);
        ChangeState(EnemyStates.PATROL);
    }
}
