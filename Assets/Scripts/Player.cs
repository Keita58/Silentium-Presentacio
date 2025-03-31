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
    float crouchedCenterCollider = -0.5f;
    float crouchedHeightCollider = 1;
    float cameraPositionBeforeCrouch = 0.74f;


    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Crouch.performed += Crouch;
        _MoveAction = _inputActions.Player.Move;
        _LookAction = _inputActions.Player.Look;
        _RunAction = _inputActions.Player.Run;
        _Rigidbody= GetComponent<Rigidbody>();
        _inputActions.Player.Enable();

    }

    void Start()
    {
        Cursor.visible = false;
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
        if (!crouched)
        {
            this.GetComponent<CapsuleCollider>().center = new Vector3(0f,  crouchedCenterCollider, 0f);
            this.GetComponent<CapsuleCollider>().height = crouchedHeightCollider;
            _Camera.transform.localPosition = Vector3.zero;
            crouched = true;
            _VelocityMove /= 2;
            //moving = false;
            //StartCoroutine(EmetreSOCrouch());
        }
        else
        {
            this.GetComponent<CapsuleCollider>().center = Vector3.zero;
            this.GetComponent<CapsuleCollider>().height = 2;
            _Camera.transform.localPosition = new Vector3 (0f, cameraPositionBeforeCrouch, 0f);
            crouched = false;
            //moving = true;
            _VelocityMove *= 2;
        }
    }

    #region FSM

    enum PlayerStates {IDLE, MOVE, RUN, HURT, RUNMOVE }
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
                if (movementInput != Vector2.zero)
                    ChangeState(PlayerStates.MOVE);
                break;
            case PlayerStates.MOVE:
                //moving = true;
                //StartCoroutine(EmetreSOMove());
                if (movementInput == Vector2.zero)
                    ChangeState(PlayerStates.IDLE);
                
                if (_RunAction.IsPressed() && !crouched)
                {
                    ChangeState(PlayerStates.RUN);
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
            default:
                break;
        }
    }

    #endregion


}
