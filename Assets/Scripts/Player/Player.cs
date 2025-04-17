using JetBrains.Annotations;
using NavKeypad;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    [SerializeField] GameObject _Camera;

    Rigidbody _Rigidbody;

    public InputSystem_Actions _inputActions { get; private set; }

    InputAction _MoveAction;
    InputAction _LookAction;
    InputAction _ScrollAction;
    InputAction _RunAction;

    [Tooltip("Player movement speed")]
    [SerializeField] private float _VelocityRun = 6f;
    [SerializeField] private float _VelocityMove = 3f;

    [Tooltip("Mouse velocity in degrees per second.")]
    [UnityEngine.Range(10f, 360f)]
    [SerializeField] private float _LookVelocity = 100;

    [SerializeField] private bool _InvertY = false;
    private Vector2 _LookRotation = Vector2.zero;

    float maxAngle = 45.0f;
    float minAngle = -45.0f;
    float gravity = 9.8f;
    float vSpeed = 0f;

    bool crouched = false;
    bool aim = false;
    float crouchedCenterCollider = -0.5f;
    float crouchedHeightCollider = 1;
    Vector3 cameraPositionBeforeCrouch = new Vector3(0, 0.627f, -0.198f);
    int gunAmmo = 3;
    int hp = 5;

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
    private GameObject itemEquipped;
    public bool inventoryOpened;
    private bool itemSlotOccuped;
    [SerializeField] private GameObject equipedObject;
    [SerializeField] Transform itemSlot;
    bool door = false;
    bool clockPuzzle = false;
    [SerializeField]
    bool item = false;
    bool note = false;
    bool chest = false;

    [SerializeField]
    bool book = false;
    bool keypad = false;
    [SerializeField]
    bool bookItem= false;
    private GameObject clockGameObject;

    private Coroutine coroutineRun;
    private Coroutine coroutineMove;
    private Coroutine coroutineCrouch;
    private Coroutine coroutineInteract;
    CharacterController characterController;
    Vector3 velocity;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Crouch.performed += Crouch;
        _MoveAction = _inputActions.Player.Move;
        _LookAction = _inputActions.Player.Look;
        _RunAction = _inputActions.Player.Run;
        _inputActions.Player.Shoot.performed += Shoot;
        _inputActions.Player.Aim.performed += Aim;
        _inputActions.Player.PickUpItem.performed += Interact;
        _inputActions.Player.Inventory.performed += OpenInventory;
        _Rigidbody = GetComponent<Rigidbody>();
        _inputActions.Player.Enable();
        characterController = GetComponent<CharacterController>();
    }

    public void ToggleInputPlayer(bool enable)
    {
        if (!enable)
        {
            _inputActions.Player.Shoot.Disable();
            _inputActions.Player.Aim.Disable();
            _inputActions.Player.Move.Disable();
            _inputActions.Player.Look.Disable();
            _inputActions.Player.Crouch.Disable();
            _inputActions.Player.PickUpItem.Disable();
        }
        else
        {
            _inputActions.Player.Shoot.Enable();
            _inputActions.Player.Aim.Enable();
            _inputActions.Player.Move.Enable();
            _inputActions.Player.Look.Enable();
            _inputActions.Player.Crouch.Enable();
            _inputActions.Player.PickUpItem.Enable();
        }

    }


    private void OpenInventory(InputAction.CallbackContext context)
    {

        InventoryManager.instance.OpenInventory(this.gameObject);
        if (!inventoryOpened)
        {
            Cursor.visible = true;
            InventoryManager.instance.OpenInventory(this.gameObject);
            inventoryOpened = true;
            ToggleInputPlayer(false);
        }
        else
        {
            Cursor.visible = false;
            InventoryManager.instance.CloseInventory();
            inventoryOpened = false;
            ToggleInputPlayer(true);
        }
    }

    void Start()
    {
        Cursor.visible = false;
        coroutineInteract = StartCoroutine(InteractuarRaycast());
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

    private void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("ENTRO?");
        if (interactiveGameObject != null && item)
        {
            
            Debug.Log("ENTRO DEFINITIVAMENTE");
            InventoryManager.instance.AddItem(interactiveGameObject.GetComponent<PickItem>().item);
            Debug.Log("QUE COJO?" + interactiveGameObject.GetComponent<PickItem>().item);

            Debug.Log("Entro Coger item");
            if (bookItem)
            {
                if (interactiveGameObject.GetComponent<Book>().placed)
                {
                    interactiveGameObject.GetComponent<Book>().placed = false;
                    interactiveGameObject.GetComponent<Book>().collider.enabled = true;
                    interactiveGameObject.GetComponent<Book>().collider.transform.GetComponent<CellBook>().SetBook(null);
                    interactiveGameObject.GetComponent<Book>().collider = null;
                    bookItem = false;
                    item = false;
                }
            }            
            interactiveGameObject.gameObject.SetActive(false);
            interactiveGameObject = null;

        }
        else
        {
            if (door)
            {
                Debug.DrawRay(_Camera.transform.position, _Camera.transform.forward, Color.magenta, 5f);
                if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask))
                {
                    if (hit.collider.TryGetComponent<Door>(out Door door))
                    {
                        if (door.isLocked)
                        {
                            InventorySO.ItemSlot aux = null;
                            foreach (InventorySO.ItemSlot item in InventoryManager.instance.inventory.items)
                            {
                                if (item.item == door.itemNeededToOpen)
                                {
                                    door.isLocked = false;
                                    aux = item;
                                }
                            }
                            InventoryManager.instance.inventory.items.Remove(aux);
                            if (door.isOpen)
                            {
                                door.Close();
                            }
                            else
                            {
                                door.Open(transform.position);
                            }
                        }
                        else
                        {
                            if (door.isOpen)
                            {
                                door.Close();
                            }
                            else
                            {
                                door.Open(transform.position);
                            }
                        }
                    }
                }
            }
            else if (clockPuzzle)
            {
                PuzzleManager.instance.InteractClockPuzzle();
                StopCoroutine(coroutineInteract);
                clockPuzzle = false;

            }
            else if (interactiveGameObject != null && note)
            {
                if (interactiveGameObject.GetComponent<Notes>().note.noteId < 6)
                {
                    InventoryManager.instance.DiscoverNote(interactiveGameObject.GetComponent<Notes>().note);
                    interactiveGameObject.gameObject.SetActive(false);
                }
                else
                {
                    InventoryManager.instance.ShowNote(interactiveGameObject.GetComponent<Notes>().note);
                }
            }
            else if (chest)
            {
                InventoryManager.instance.OpenChest();
                chest = false;
            }else if (book)
            {
                if (equipedObject != null)
                {
                    if (equipedObject.GetComponent<PickItem>().item is BookItem)
                    {
                        interactiveGameObject.GetComponent<CellBook>().SetBook(equipedObject.GetComponent<Book>());
                        equipedObject.GetComponent<Book>().collider = interactiveGameObject.GetComponent<CellBook>().GetComponent<BoxCollider>();
                        equipedObject.GetComponent<Book>().placed = true;
                        PuzzleManager.instance.CheckBookPuzzle();
                        interactiveGameObject.GetComponent<CellBook>().GetComponent<BoxCollider>().enabled = false;
                        equipedObject.transform.rotation = Quaternion.identity;
                        equipedObject.transform.parent = null;
                        equipedObject.transform.position = interactiveGameObject.transform.GetChild(0).transform.position;
                        equipedObject = null;
                        itemSlotOccuped = false;
                        InventoryManager.instance.UseEquippedItem();
                    }
                }
            }else if (keypad)
            {
                PuzzleManager.instance.InteractHieroglyphicPuzzle();
            }
        }

    }

    public void ResumeInteract()
    {
        coroutineInteract = StartCoroutine(InteractuarRaycast());
    }

    public void EquipItem(GameObject itemAEquipar)
    {
        if (!itemSlotOccuped)
        {
            GameObject equip = Instantiate(itemAEquipar, itemSlot.transform);
            equip.transform.position = itemSlot.transform.position;
            itemSlotOccuped = true;
            equipedObject = equip;
        }
    }

    public void UnequipItem()
    {
        itemSlotOccuped = false;
        Destroy(equipedObject);
        equipedObject = null;
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
            if (Physics.Raycast(shootPosition.transform.position, -shootPosition.transform.right, 5f, enemyLayerMask))
            {
                //e.RebreMal(5);
                Debug.Log("Enemy hit");
            }
            gunAmmo--;
            //OnDisparar?.Invoke();
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }


    private void Aim(InputAction.CallbackContext context)
    {
        aim = !aim;
        gunGameObject.transform.localPosition = aim ? new Vector3(0.057f, -0.312999994f, 0.391000003f) : new Vector3(0.456f, -0.313f, 0.505f);
        weaponCamera.transform.localPosition = aim ? new Vector3(0f, 0f, -0.28f) : Vector3.zero;
    }

    #region FSM

    enum PlayerStates { IDLE, MOVE, RUN, HURT, RUNMOVE, CROUCH }
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
                _Rigidbody.angularVelocity = Vector3.zero;
                break;
            case PlayerStates.MOVE:
                coroutineMove = StartCoroutine(MakeNoiseMove());
                break;
            case PlayerStates.RUN:
                coroutineRun = StartCoroutine(MakeNoiseRun());
                break;
            case PlayerStates.CROUCH:
                this.GetComponent<CapsuleCollider>().center = new Vector3(0f, crouchedCenterCollider, 0f);
                this.GetComponent<CapsuleCollider>().height = crouchedHeightCollider;
                _Camera.transform.localPosition = new Vector3(0f, 0f, -0.198f);
                cameraShenanigansGameObject.transform.localPosition = Vector3.zero;
                _VelocityMove /= 2;
                coroutineCrouch = StartCoroutine(MakeNoiseCrouch());
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

                }
                else if (crouched)
                {
                    ChangeState(PlayerStates.CROUCH);
                }
                break;
            case PlayerStates.MOVE:
                if (movementInput == Vector2.zero)
                    ChangeState(PlayerStates.IDLE);

                if (_RunAction.IsPressed() && !crouched)
                {
                    ChangeState(PlayerStates.RUN);
                }
                else if (crouched)
                {
                    ChangeState(PlayerStates.CROUCH);
                }

                velocity = (transform.right * movementInput.x +
                  transform.forward * movementInput.y).normalized * _VelocityMove;
                vSpeed -= gravity * Time.deltaTime;
                velocity.y = vSpeed;

                break;
            case PlayerStates.RUN:
                velocity = (transform.right * movementInput.x +
                  transform.forward * movementInput.y).normalized * _VelocityRun;
                vSpeed -= gravity * Time.deltaTime;
                velocity.y = vSpeed;
                if (!_RunAction.IsPressed())
                    ChangeState(PlayerStates.MOVE);
                break;
            case PlayerStates.CROUCH:
                velocity = (transform.right * movementInput.x +
               transform.forward * movementInput.y).normalized * 1.5f;
                vSpeed -= gravity * Time.deltaTime;
                velocity.y = vSpeed;
                if (!crouched && movementInput == Vector2.zero)
                {
                    ChangeState(PlayerStates.IDLE);
                }
                else if (!crouched)
                {
                    ChangeState(PlayerStates.MOVE);
                }
                break;
            default:
                break;
        }
        characterController.Move(velocity * Time.deltaTime);
    }


    private void ExitState(PlayerStates exitState)
    {
        switch (exitState)
        {
            case PlayerStates.MOVE:
                if (coroutineMove != null)
                    StopCoroutine(coroutineMove);
                break;
            case PlayerStates.RUN:
                if (coroutineRun != null)
                    StopCoroutine(coroutineRun);
                break;
            case PlayerStates.CROUCH:
                if (coroutineCrouch != null)
                    StopCoroutine(coroutineCrouch);
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

    public IEnumerator InteractuarRaycast()
    {
        //Interactuar con todo y mirar que me devuelve;
        while (true)
        {
            Debug.DrawRay(_Camera.transform.position, _Camera.transform.forward, Color.magenta, 5f);
            //Lanzar Raycast interactuar con el mundo.

            if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask)){
                if (interactiveGameObject != null)
                {
                    if ( interactiveGameObject.transform.gameObject.layer != 15)
                    {
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                        interactiveGameObject = null;
                    }
                }
                if (!hit.collider.gameObject.Equals(interactiveGameObject) && hit.transform.gameObject.layer != 10)
                {
                    interactiveGameObject = hit.collider.gameObject;
                    if (hit.transform.gameObject.layer != 15)
                    {
                        baseMaterial = interactiveGameObject.GetComponent<MeshRenderer>().materials[0];
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[]
                        {
                    interactiveGameObject.GetComponent<MeshRenderer>().materials[0],

                    material
                        };
                    }
                    else
                    {
                        Transform child =interactiveGameObject.transform.GetChild(0);
                        if (child.childCount > 0)
                        {
                            Transform bookChild = child.GetChild(0);
                            baseMaterial = bookChild.GetComponent<MeshRenderer>().materials[0];
                            bookChild.GetComponent<MeshRenderer>().materials = new Material[]
                            {
                                bookChild.GetComponent<MeshRenderer>().materials[0],

                                material
                            };
                        }
                    }
                    if (hit.transform.gameObject.layer == 9)
                    {
                        item = true;
                        if (hit.collider.GetComponent<PickItem>().item is BookItem)
                        {
                            bookItem = true;
                        }
                    }
                    else if (hit.transform.gameObject.layer == 12)
                    {
                        note = true;
                    }
                    else if (hit.transform.gameObject.layer == 11)
                    {
                        clockPuzzle = true;

                    }
                    else if (hit.transform.gameObject.layer == 13)
                    {
                        chest = true;
                    }
                    else if (hit.transform.gameObject.layer == 15)
                    {
                        book = true;
                    }
                    else if (hit.transform.gameObject.layer == 14)
                    {
                        keypad = true;
                    }
                }
                else if (hit.transform.gameObject.layer == 10)
                {
                    door = true;
                }

            }
            else if (!Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit2, 10f, interactLayerMask))
            {
                door=false;
                book=false;
                keypad=false;
                item = false;
                clockPuzzle = false;
                bookItem = false;
                if (interactiveGameObject != null)
                {
                    if (interactiveGameObject.transform.gameObject.layer != 15)
                    {
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                        interactiveGameObject = null;
                    }
                }
                //onNotInteractuable?.Invoke();
            }
            // onInteractuable?.Invoke();

            yield return new WaitForSeconds(0.1f);
        }
    } 

    #region SOUNDS

    IEnumerator MakeNoiseMove()
    {
        while (true)
        {
            Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, 30);
            foreach (Collider collider in colliderHits)
            {
                Debug.Log("CORUTINAMOVER");
                if (collider.gameObject.TryGetComponent<Enemy>(out Enemy en))
                {
                    en.ListenSound(this.transform.position, 2);
                }
            }
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator MakeNoiseRun()
    {
        while (true)
        {
            Debug.Log("CORUTINACORRER");
            Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, 7);
            if (GetComponent<Collider>().gameObject.TryGetComponent<Enemy>(out Enemy en))
            {
                en.ListenSound(this.transform.position, 7);
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator MakeNoiseCrouch()
    {
        while (true)
        {
            Debug.Log("CORUTINACROUCH");
            Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, 5);
            if (GetComponent<Collider>().gameObject.TryGetComponent<Enemy>(out Enemy en))
            {
                en.ListenSound(this.transform.position, 5);
            }
            yield return new WaitForSeconds(1);
        }
    }
    #endregion

    private void OnDestroy()
    {
        _inputActions.Player.Shoot.performed -= Shoot;
        _inputActions.Player.Aim.performed -= Aim;
        _inputActions.Player.PickUpItem.performed -= Interact;
        _inputActions.Player.Inventory.performed -= OpenInventory;
        _inputActions.Player.Crouch.performed -= Crouch;

    }
}
