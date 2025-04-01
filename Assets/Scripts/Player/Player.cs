using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class Player : MonoBehaviour
{
    [SerializeField] GameObject _Camera;

    Rigidbody _Rigidbody;

    InputSystem_Actions _inputActions;

    InputAction _MoveAction;
    InputAction _LookAction;
    InputAction _ScrollAction;
    InputAction _RunAction;

    [Tooltip("Player movement speed")]
    [SerializeField] private float _VelocityRun = 6f;
    [SerializeField] private float _VelocityMove = 3f;

    [Tooltip("Mouse velocity in degrees per second.")]
    [Range(10f, 360f)]
    [SerializeField] private float _LookVelocity = 100;

    [SerializeField] private bool _InvertY = false;
    private Vector2 _LookRotation = Vector2.zero;

    float maxAngle = 45.0f;
    float minAngle = -30.0f;

    bool crouched=false;
    bool aim=false;
    float crouchedCenterCollider = -0.5f;
    float crouchedHeightCollider = 1;
    Vector3 cameraPositionBeforeCrouch = new Vector3(0, 0.627f, -0.198f);
    int gunAmmo =3;

    [SerializeField] GameObject cameraShenanigansGameObject;

    [SerializeField] Transform shootPosition;
    [SerializeField] GameObject gunGameObject;
    [SerializeField] LayerMask enemyLayerMask;
    [SerializeField] LayerMask interactLayerMask;
    [SerializeField] Camera weaponCamera;
    [SerializeField] GameObject interactiveGameObject;
    [SerializeField] Material material;
    GameObject equippedItem;
    [SerializeField] private Material baseMaterial;


    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Crouch.performed += Crouch;
        _MoveAction = _inputActions.Player.Move;
        _LookAction = _inputActions.Player.Look;
        _RunAction = _inputActions.Player.Run;
        _inputActions.Player.Shoot.performed+=Shoot;
        _inputActions.Player.Aim.performed +=Aim;
        _Rigidbody= GetComponent<Rigidbody>();
        _inputActions.Player.Enable();

    }

    void Start()
    {
        Cursor.visible = false;
        StartCoroutine(interactuarRaycast());
    }

    private void Update()
    {
        // To move the camera
        Vector2 lookInput = _LookAction.ReadValue<Vector2>();
        _LookRotation.x += lookInput.x * _LookVelocity * Time.deltaTime;
        _LookRotation.y += (_InvertY ? 1 : -1) * lookInput.y * _LookVelocity * Time.deltaTime;

        _LookRotation.y = Mathf.Clamp(_LookRotation.y, minAngle, maxAngle);
        transform.rotation = Quaternion.Euler(0, _LookRotation.x, 0);
        _Camera.transform.localRotation = Quaternion.Euler(_LookRotation.y, 0, 0);

        UpdateState();

    }

    private void Crouch(InputAction.CallbackContext context)
    {
        crouched = !crouched;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (gunAmmo >= 1)
        {
            Debug.DrawRay(shootPosition.transform.position, -shootPosition.transform.right, Color.magenta, 5f);
            Debug.Log("TIRO DEBUGRAY");
            if (Physics.Raycast(shootPosition.transform.position, -shootPosition.transform.right, enemyLayerMask))
            {
                //e.RebreMal(5);
                Debug.Log("Enemy hit");
            }
            //_Bales--;
            //OnDisparar?.Invoke();
        }
    }


    private void Aim(InputAction.CallbackContext context)
    {
        aim= !aim;
        gunGameObject.transform.localPosition = aim ? new Vector3 (0.057f, -0.312999994f, 0.391000003f) : new Vector3(0.456f, -0.313f, 0.505f);
        weaponCamera.transform.localPosition= aim ?  new Vector3 (0f, 0f, -0.28f) : Vector3.zero;
    }

    #region FSM

    enum PlayerStates {IDLE, MOVE, RUN, HURT, RUNMOVE, CROUCH }
    [SerializeField] PlayerStates actualState;
    [SerializeField] float stateTime;


    private void ChangeState(PlayerStates newstate)
    {
        ExitState(actualState);
        IniState(newstate);
    }

    private void IniState(PlayerStates initState)
    {
        actualState = initState;
        stateTime = 0f;

        switch (actualState)
        {
            case PlayerStates.IDLE:
                _Rigidbody.linearVelocity = Vector3.zero;
                _Rigidbody.angularVelocity=Vector3.zero;
                break;
            case PlayerStates.MOVE:
                //moving = true;
                //StartCoroutine(EmetreSOMove());
       

                break;
            case PlayerStates.RUN:
                //running = true;
                //StartCoroutine(EmetreSORun());
                //StartCoroutine(EmetrePols());
                break;
            case PlayerStates.CROUCH:
                this.GetComponent<CapsuleCollider>().center = new Vector3(0f, crouchedCenterCollider, 0f);
                this.GetComponent<CapsuleCollider>().height = crouchedHeightCollider;
                _Camera.transform.localPosition = new Vector3(0f, 0f, -0.198f);
                cameraShenanigansGameObject.transform.localPosition = Vector3.zero;
                _VelocityMove /= 2;
                //StartCoroutine(EmetreSOCrouch());
                break;
            default:
                break;
        }
    }
    private void UpdateState()
    {
        Vector2 movementInput = _MoveAction.ReadValue<Vector2>();

        switch (actualState)
        {

            case PlayerStates.IDLE:
                if (movementInput != Vector2.zero && !crouched)
                {
                    ChangeState(PlayerStates.MOVE);
                }else if (crouched)
                {
                    ChangeState(PlayerStates.CROUCH);
                }
                break;
            case PlayerStates.MOVE:
                //moving = true;
                //StartCoroutine(EmetreSOMove());
                if (movementInput == Vector2.zero)
                    ChangeState(PlayerStates.IDLE);
                
                if (_RunAction.IsPressed() && !crouched)
                {
                    ChangeState(PlayerStates.RUN);
                }else if (crouched)
                {
                    ChangeState(PlayerStates.CROUCH);
                }

                _Rigidbody.linearVelocity =
                (transform.right * movementInput.x +
                transform.forward * movementInput.y)
                .normalized * _VelocityMove;
                break;
            case PlayerStates.RUN:
                //running = true;

                _Rigidbody.linearVelocity =
                (transform.right * movementInput.x +
                transform.forward * movementInput.y)
                .normalized * _VelocityRun;


                if (!_RunAction.IsPressed())
                    ChangeState(PlayerStates.MOVE);
                //StartCoroutine(EmetreSORun());
                //StartCoroutine(EmetrePols());
                break;
            case PlayerStates.CROUCH:
                _Rigidbody.linearVelocity =
                (transform.right * movementInput.x +
                transform.forward * movementInput.y)
                .normalized * _VelocityMove/2;
                if (!crouched && movementInput == Vector2.zero)
                {
                    ChangeState(PlayerStates.IDLE);
                }else if (!crouched)
                {
                    ChangeState(PlayerStates.MOVE);
                }
                break;
            default:
                break;

        }
    }


        private void ExitState(PlayerStates exitState)
    {
        switch (exitState)
        {
            case PlayerStates.MOVE:
                //Comentar por si hacemos que haya mini estados.
                //rb.linearVelocity = Vector2.zero;
                //rb.angularVelocity = Vector3.zero;
                //moving = false;
                break;
            case PlayerStates.RUN:
                //corriendo = false;
                break;
            case PlayerStates.CROUCH:
                this.GetComponent<CapsuleCollider>().center = Vector3.zero;
                this.GetComponent<CapsuleCollider>().height = 2;
                _Camera.transform.localPosition = cameraPositionBeforeCrouch;
                cameraShenanigansGameObject.transform.localPosition = new Vector3(0f, _Camera.transform.localPosition.y, 0f);
                _VelocityMove *= 2;
                break;
            default:
                break;
        }
    }

    #endregion

    public IEnumerator interactuarRaycast()
    {
        while (true)
        {
            Debug.DrawRay(_Camera.transform.position, _Camera.transform.forward, Color.magenta, 5f);
            //Lanzar Raycast interactuar con el mundo.

            if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask)
                && !hit.collider.gameObject.Equals(interactiveGameObject))
            {
                interactiveGameObject = hit.collider.gameObject;
                baseMaterial = interactiveGameObject.GetComponent<MeshRenderer>().materials[0];
                interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[]
                {
                    interactiveGameObject.GetComponent<MeshRenderer>().materials[0],

                    material
                };
               // onInteractuable?.Invoke();
            }
            else if (!Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit2, 10f, interactLayerMask))
            {
                if (interactiveGameObject != null)
                {
                    interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                    interactiveGameObject = null;
                }
                //onNotInteractuable?.Invoke();
            }
            yield return new WaitForSeconds(0.5f);
        }


    }
}
