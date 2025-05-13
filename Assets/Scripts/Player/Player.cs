using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    [SerializeField] Camera _Camera;

    Rigidbody _Rigidbody;

    public event Action onCameraClick;

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

    bool crouched = false;
    bool aim = false;
    float crouchedCenterCollider = -0.5f;
    float crouchedHeightCollider = 1;
    Vector3 cameraPositionBeforeCrouch = new Vector3(0, 0.627f, -0.198f);
    public int gunAmmo { get; set; }
    public int hp { get; set; }

    [SerializeField] GameObject cameraShenanigansGameObject;

    [SerializeField] Transform shootPosition;
    [SerializeField] GameObject gunGameObject;
    [SerializeField] LayerMask enemyLayerMask;
    [SerializeField] LayerMask interactLayerMask;
    //[SerializeField] Camera weaponCamera;
    [SerializeField] GameObject interactiveGameObject;
    [SerializeField] Material material;
    GameObject equippedItem;
    [SerializeField] private Material baseMaterial;
    private GameObject itemEquipped;
    public bool inventoryOpened;
    private bool itemSlotOccuped;
    [SerializeField] private GameObject equipedObject;
    [SerializeField] Transform itemSlot;
    [SerializeField]
    bool door = false;
    bool clockPuzzle = false;

    //interactive booleans
    [SerializeField]
    bool item = false;
    [SerializeField]
    bool note = false;
    [SerializeField]
    bool chest = false;
    [SerializeField]
    bool book = false;
    [SerializeField]
    bool keypad = false;
    [SerializeField]
    bool bookItem= false;
    [SerializeField]
    bool picture = false;
    [SerializeField]
    bool morse = false;
    private GameObject clockGameObject;
    private bool weaponPuzzle;
    //Coroutines
    private Coroutine coroutineRun;
    private Coroutine coroutineMove;
    private Coroutine coroutineCrouch;
    private Coroutine coroutineInteract;

    //Character controller
    CharacterController characterController;
    Vector3 velocity;
    float gravity = 9.8f;
    float vSpeed = 0f;

    [Header("Silencer")]
    [SerializeField] GameObject silencer;
    public bool isSilencerEquipped { get; set; }
    public int silencerUses { get; set; }
    public int maxSilencerUses { get; set; }

    //Chest
    private GameObject chestGO;

    [Header("Gun")]
    [SerializeField] private Transform gunAimPosition;
    private Vector3 gunDefaultPosition = new Vector3(0.456f, -0.313f, 0.505f);
    [SerializeField] private int gunAimFov;
    [SerializeField] private float aimSpeed;
    [SerializeField] private int currentFov;
    [SerializeField] private Animator gunanimator;

    [Header("PostProcess")]
    [SerializeField] private PostProcessEvents events;

    [Header("General")]
    [SerializeField] MenuUI menuManager;

    public event Action onPickItem;

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
        _inputActions.Player.Throw.performed += ThrowItem;
        _inputActions.Player.Pause.performed += OpenMenu;
        _Rigidbody = GetComponent<Rigidbody>();
        _inputActions.Player.Enable();
        characterController = GetComponent<CharacterController>();
    }

    public void ToggleInputPlayer(bool enable, bool enableInventory)
    {
        if (!enable)
        {
            Cursor.visible = true;
            _inputActions.Player.Shoot.Disable();
            _inputActions.Player.Aim.Disable();
            _inputActions.Player.Move.Disable();
            _inputActions.Player.Look.Disable();
            _inputActions.Player.Crouch.Disable();
            _inputActions.Player.PickUpItem.Disable();
            _RunAction.Disable();
        }
        else
        {
            Cursor.visible = false;
            _inputActions.Player.Shoot.Enable();
            _inputActions.Player.Aim.Enable();
            _inputActions.Player.Move.Enable();
            _inputActions.Player.Look.Enable();
            _inputActions.Player.Crouch.Enable();
            _inputActions.Player.PickUpItem.Enable();
            _RunAction.Enable();
        }
        if (enableInventory) _inputActions.Player.Inventory.Enable();
        else _inputActions.Player.Inventory.Disable();

    }

    private void OpenInventory(InputAction.CallbackContext context)
    {
        if (!inventoryOpened)
        {
            Cursor.visible = true;
            InventoryManager.instance.OpenInventory(this.gameObject);
            inventoryOpened = true;
            ToggleInputPlayer(false, true);
        }
        else
        {
            if(chestGO != null)
                chestGO.GetComponent<Animator>().Play("CloseChest");
            Cursor.visible = false;
            InventoryManager.instance.CloseInventory();
            inventoryOpened = false;
            ToggleInputPlayer(true, true);
        }
    }

    private void OpenMenu(InputAction.CallbackContext context)
    {
        menuManager.OpenMenu();
    }

    void Start()
    {
        Cursor.visible = false;
        coroutineInteract = StartCoroutine(InteractuarRaycast());
        inventoryOpened = false;

        hp = 5;
        gunAmmo = 20;
        maxSilencerUses = 10;
        StartCoroutine(EsperarIActuar(5f, ()=>TakeDamage(1)));
    }

    IEnumerator EsperarIActuar(float tempsDespera, Action accio)
    {
        if (tempsDespera > 0)
            yield return new WaitForSeconds(tempsDespera);
        else
            yield return null;
        accio();
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


    private void LateUpdate()
    {
        if (aim)
        {
            gunGameObject.transform.localPosition = Vector3.Lerp(gunGameObject.transform.localPosition, gunAimPosition.localPosition, aimSpeed * Time.deltaTime);
            SetFieldOfView(Mathf.Lerp(_Camera.fieldOfView, gunAimFov, aimSpeed * Time.deltaTime));

        }
        else
        {
            if (_Camera.fieldOfView != currentFov)
            {
                gunGameObject.transform.localPosition = Vector3.Lerp(gunGameObject.transform.localPosition, gunDefaultPosition, aimSpeed * Time.deltaTime);
                SetFieldOfView(Mathf.Lerp(_Camera.fieldOfView, currentFov, aimSpeed * Time.deltaTime));
            }
        }
    }

    private void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("ENTRO?");
        if (interactiveGameObject != null)
        {
            if (item)
            {
                Debug.Log("ENTRO DEFINITIVAMENTE");
                Item itemPicked = interactiveGameObject.GetComponent<PickItem>().item;
                if (InventoryManager.instance.inventory.items.Count<6)
                    InventoryManager.instance.AddItem(itemPicked);
                else
                    Debug.Log("Inventory full");
                Debug.Log("QUE COJO?" + itemPicked);

                Debug.Log("Entro Coger item");
                if (bookItem)
                {
                    Book book = interactiveGameObject.GetComponent<Book>();
                    if (book.placed)
                    {
                        book.placed = false;
                        book.collider.enabled = true;
                        book.collider.transform.GetComponent<CellBook>().SetBook(null);
                        book.collider = null;
                        bookItem = false;
                        item = false;
                    }
                }
                interactiveGameObject.gameObject.SetActive(false);
                onPickItem?.Invoke();
                if (itemPicked is BookItem && itemPicked.ItemType == ItemTypes.BOOK2) PuzzleManager.instance.ChangePositionPlayerAfterHieroglyphic();
                interactiveGameObject = null;
            }
            else
            {
                if (!book && !door && !weaponPuzzle)
                    interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };

                if (clockPuzzle)
                {
                    PuzzleManager.instance.InteractClockPuzzle();
                    StopCoroutine(coroutineInteract);
                    clockPuzzle = false;

                }else if (weaponPuzzle)
                {
                    PuzzleManager.instance.InteractWeaponPuzzle();
                    StopCoroutine(coroutineInteract);
                    weaponPuzzle = false;
                }
                else if (note)
                {
                    NotesSO note = interactiveGameObject.GetComponent<Notes>().note;
                    if (note.noteId < 6)
                    {
                        InventoryManager.instance.DiscoverNote(note);
                        interactiveGameObject.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (note.noteId == 10)
                        {
                            InventoryManager.instance.ShowNoteScroll(note);
                        }else if (note.noteType == NotesSO.NoteType.Image)
                        {
                            InventoryManager.instance.ShowImageNote(note.noteContent);

                        }else if (note.noteType == NotesSO.NoteType.Book)
                        {
                            InventoryManager.instance.ShowBookNote(note.noteContent);
                        }
                        else
                            InventoryManager.instance.ShowNote(note);
                    }
                }
                else if (book)
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
                }
                else if (keypad)
                {
                    PuzzleManager.instance.InteractHieroglyphicPuzzle();
                }
                else if (morse)
                {
                    PuzzleManager.instance.InteractMorsePuzzle();
                }
                else if (picture)
                {
                    Debug.Log("Entro en interact picture");
                    PuzzleManager.instance.picturesClicked.Add(interactiveGameObject.GetComponent<Picture>());
                    PuzzleManager.instance.TakePoemPart();
                    picture = false;

                }
                else if (chest)
                {
                    chestGO.GetComponent<Animator>().Play("OpenChest");
                    InventoryManager.instance.OpenChest();
                    chest = false;
                }
            }
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
                            //InventorySO.ItemSlot aux = null;
                            foreach (InventorySO.ItemSlot item in InventoryManager.instance.inventory.items)
                            {
                                if (item.item == door.itemNeededToOpen)
                                {
                                    door.isLocked = false;
                                    //aux = item;
                                    InventoryManager.instance.inventory.items.Remove(item);
                                }
                            }
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

    private void ThrowItem(InputAction.CallbackContext context)
    {
        if (itemSlotOccuped)
        {
            equipedObject.transform.GetChild(0).TryGetComponent<ThrowObject>(out ThrowObject throwable);
            if (throwable != null)
            {
                throwable.GetComponent<Rigidbody>().isKinematic = false;
                throwable.camaraPrimera = _Camera.gameObject;
                throwable.Lanzar();
                if (throwable.transform.parent == itemSlot)
                {
                    throwable.transform.parent = null;
                }
                else
                {
                    throwable.transform.parent.parent = null;
                }
            }
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
            gunanimator.Play("Shoot");
            gunAmmo--;
            Debug.DrawRay(shootPosition.transform.position, -shootPosition.transform.right, Color.magenta, 5f);
            Debug.Log("TIRO DEBUGRAY");
            if (isSilencerEquipped)
            {
                if (silencerUses > 0)
                {
                    silencerUses--;
                    Debug.Log("Silenciador usos: " + silencerUses);
                }
                else
                {
                    isSilencerEquipped = false;
                    silencer.SetActive(false);
                }
            }
            if (Physics.Raycast(shootPosition.transform.position, -shootPosition.transform.right, out RaycastHit e, 5f, enemyLayerMask))
            {
                e.transform.GetComponent<Enemy>().TakeHealth();
                if (isSilencerEquipped)
                {
                    Debug.Log("Hago sonido silenciador");
                    MakeNoise(5, 5);
                }
                else
                {
                    MakeNoise(10, 10);
                }
                Debug.Log("Enemy hit");
            }
            //OnDisparar?.Invoke();
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        events.ActivateVignatteOnHurt();
    }


    private void Aim(InputAction.CallbackContext context)
    {
        aim = !aim;
    }

    private void SetFieldOfView(float fov)
    {
        _Camera.fieldOfView = fov;
    }

    public void UseSilencer()
    {
        if (!isSilencerEquipped)
        {
            silencer.SetActive(true);
            isSilencerEquipped = true;
            silencerUses = maxSilencerUses;
        }
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
                break;
            case PlayerStates.MOVE:
                coroutineMove = StartCoroutine(MakeNoiseMove());
                break;
            case PlayerStates.RUN:
                coroutineRun = StartCoroutine(MakeNoiseRun());
                break;
            case PlayerStates.CROUCH:
                this.GetComponent<CharacterController>().center = new Vector3(0f, crouchedCenterCollider, 0f);
                this.GetComponent<CharacterController>().height = crouchedHeightCollider;
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

                if (_RunAction.IsPressed() && !crouched && !aim)
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
                if (!_RunAction.IsPressed() || aim)
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
                }else if (crouched && movementInput == Vector2.zero)
                {
                   if (coroutineCrouch != null)
                    {
                        StopCoroutine(coroutineCrouch);
                        coroutineCrouch = null;
                    }
                }
                else
                {
                    if (coroutineCrouch == null)
                    {
                        coroutineCrouch = StartCoroutine(MakeNoiseCrouch());
                    }
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
                this.GetComponent<CharacterController>().center = Vector3.zero;
                this.GetComponent<CharacterController>().height = 2;
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
        while (true)
        {
            Debug.DrawRay(_Camera.transform.position, _Camera.transform.forward, Color.magenta, 5f);
            //Lanzar Raycast interactuar con el mundo.

            if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask)){
                if (!hit.collider.gameObject.Equals(interactiveGameObject) && hit.transform.gameObject.layer != 10)
                {
                    if (interactiveGameObject!=null && interactiveGameObject.transform.gameObject.layer != 15 && interactiveGameObject.transform.gameObject.layer!=18)
                    {
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                        interactiveGameObject = null;
                    }
                    interactiveGameObject = hit.collider.gameObject;
                    if (hit.transform.gameObject.layer != 15 && hit.transform.gameObject.layer != 18)
                    {
                        baseMaterial = interactiveGameObject.GetComponent<MeshRenderer>().materials[0];
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[]
                        {
                    interactiveGameObject.GetComponent<MeshRenderer>().materials[0],

                    material
                        };
                    }
                    else if(interactiveGameObject.layer != 18)
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

                    //if(hit.transform.TryGetComponent<Interactuable>(out Interactuable aux))
                    if(hit.transform.gameObject.layer == 24)
                    {
                        onCameraClick?.Invoke();
                    }

                    //S'ha de canviar aix� per una sola layer
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
                        chestGO = hit.transform.gameObject;
                    }
                    else if (hit.transform.gameObject.layer == 15)
                    {
                        book = true;
                    }
                    else if (hit.transform.gameObject.layer == 23)
                    {
                        keypad = true;
                    }else if (hit.transform.gameObject.layer == 18)
                    {
                        weaponPuzzle = true;
                    }else if (hit.transform.gameObject.layer == 20)
                    {
                        morse = true;
                        door = false;
                    }
                    else if (hit.transform.gameObject.layer == 17)
                    {
                        Debug.Log("Entro en la layer de picture");
                        picture = true;
                    }
                }
                else if (hit.transform.gameObject.layer == 10)
                {
                    if (!morse)
                        door = true;
                }

            }
            else if (!Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit2, 10f, interactLayerMask))
            {
                door=false;
                book=false;
                keypad=false;
                item = false;
                morse=false;
                clockPuzzle = false;
                bookItem = false;
                picture = false;
                note = false;
                weaponPuzzle = false;
                chest = false;

                chestGO = null;

                if (interactiveGameObject != null)
                {
                    if (interactiveGameObject.transform.gameObject.layer != 15 && interactiveGameObject.transform.gameObject.layer != 18)
                    {
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                        interactiveGameObject = null;
                    }
                    else
                    {
                        interactiveGameObject=null;
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
            MakeNoise(30, 3);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MakeNoiseRun()
    {
        while (true)
        {
            Debug.Log("CORUTINACORRER");
            MakeNoise(37, 7);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MakeNoiseCrouch()
    {
        while (true)
        {
            Debug.Log("CORUTINACROUCH");
            MakeNoise(5, 1);
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion

    private void MakeNoise(int radius, int noiseQuantity)
    {
        Collider[] colliderHits = Physics.OverlapSphere(this.transform.position,radius);
        foreach (Collider collider in colliderHits)
        {
            if (collider.gameObject.TryGetComponent<Enemy>(out Enemy en))
            {
                Debug.Log("Entro a crouch");
                en.ListenSound(this.transform.position, noiseQuantity);
            }
        }
    }

    public void SetCurrentFOV(int fovLevel)
    {
        currentFov = fovLevel;
    }

    private void OnDestroy()
    {
        if (_inputActions.Player.enabled)
        {
        _inputActions.Player.Shoot.performed -= Shoot;
        _inputActions.Player.Aim.performed -= Aim;
        _inputActions.Player.PickUpItem.performed -= Interact;
        _inputActions.Player.Inventory.performed -= OpenInventory;
        _inputActions.Player.Crouch.performed -= Crouch;
        _inputActions.Player.Throw.performed -= ThrowItem;
        }

    }
}
