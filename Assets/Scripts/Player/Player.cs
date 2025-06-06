using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    [SerializeField] private float _VelocityRun = 4.5f;
    [SerializeField] private float _VelocityMove = 3f;

    [Tooltip("Mouse velocity in degrees per second.")]
    [Range(10f, 360f)]
    [SerializeField] private float _LookVelocity = 100;

    [SerializeField] private bool _InvertY = false;
    private Vector2 _LookRotation = Vector2.zero;

    float maxAngle = 90.0f;
    float minAngle = -90.0f;

    bool crouched = false;
    bool aim = false;
    float crouchedCenterCollider = -0.5f;
    float crouchedHeightCollider = 1;
    Vector3 cameraPositionBeforeCrouch = new Vector3(0, 0.627f, -0.198f);
    public int gunAmmo { get; set; }
    public int hp { get; set; }

    [SerializeField]
    public GameObject flashlight;

    public int maxHp { get; private set; }

    [SerializeField] GameObject cameraShenanigansGameObject;

    [SerializeField] Transform shootPosition;
    [SerializeField] GameObject gunGameObject;
    [SerializeField] LayerMask enemyLayerMask;
    [SerializeField] LayerMask interactLayerMask;
    [SerializeField] GameObject interactiveGameObject;
    [SerializeField] Material material;
    [SerializeField] private Material baseMaterial;
    public bool inventoryOpened;
    public bool itemSlotOccuped;
    [SerializeField] public GameObject equipedObject;
    [SerializeField] Transform itemSlot;
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
    public int maxSilencerUses { get; private set; }

    [Header("Gun")]
    [SerializeField] private Transform gunAimPosition;
    private Vector3 gunDefaultPosition = new Vector3(0.456f, -0.313f, 0.505f);
    [SerializeField] private int gunAimFov;
    [SerializeField] private float aimSpeed;
    [SerializeField] private int currentFov;
    [SerializeField] private Animator gunanimator;

    [Header("PostProcess")]
    [SerializeField] private Events events;

    [Header("General")]
    [SerializeField] MenuUI menuManager;
    [SerializeField] Waves waves;

    [Header("Audio")]
    AudioSource GunAudioSource;
    AudioSource playerAudioSource;
    [SerializeField]
    AudioClip ShootGunAudio;
    [SerializeField]
    AudioClip EmptyShootGunAudio;
    [SerializeField]
    AudioClip SilencedShootGunAudio;
    [SerializeField]
    AudioClip RechargeGunAudio;
    [SerializeField]
    AudioClip walkAudio;
    [SerializeField]
    AudioClip runAudio;
    [SerializeField]
    AudioClip healthAudio;
    [SerializeField]
    AudioClip flashlightSound;


    //Events
    public event Action<int> OnMakeSound;
    public event Action<int> OnMakeImpactSound;
    public event Action<GameObject> OnInteractuable;
    //Evento que sirve para limpiar el texto de interactuable.
    public event Action OnNotInteractuable;
    public event Action<int, int> OnHpChange;
    public event Action<int> OnPickItem;
    public event Action<int> OnAmmoChange;
    public event Action OnShoot;
    public event Action OnThrow;

    private bool noteOpened = false;

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
        _inputActions.Player.Flashlight.performed += Flashlight;
        _Rigidbody = GetComponent<Rigidbody>();
        _inputActions.Player.Enable();
        characterController = GetComponent<CharacterController>();
        InventoryManager.instance.OnNote += SetNoteOpened;
        GunAudioSource = gunGameObject.GetComponent<AudioSource>();
        playerAudioSource = this.GetComponent<AudioSource>();
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
            _MoveAction.Disable();
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
            _MoveAction.Enable();
            //OnToggleUI?.Invoke(true);
        }

        if (enableInventory)
            _inputActions.Player.Inventory.Enable();
        else
            _inputActions.Player.Inventory.Disable();
    }

    private void OpenInventory(InputAction.CallbackContext context)
    {
        if (!inventoryOpened && !noteOpened)
            InventoryManager.instance.OpenInventory(this.gameObject);
        else
            InventoryManager.instance.CloseInventory();
    }

    private void OpenMenu(InputAction.CallbackContext context)
    {
        if (!inventoryOpened && !noteOpened)
        {
            Time.timeScale = 0;
            menuManager.OpenMenu();
        }
    }

    void Start()
    {
        Cursor.visible = false;
        coroutineInteract = StartCoroutine(InteractRaycast());
        inventoryOpened = false;

        hp = 6;
        maxHp = 6;
        gunAmmo = 12;
        maxSilencerUses = 10;
        OnAmmoChange?.Invoke(gunAmmo);
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
        //Si el jugador està interactuant amb un objecte
        if (interactiveGameObject != null)
        {
            //Si és una porta cridem a l'Interact de la porta, ja que és diferent (necessita la posició del jugador per obrir-se).
            if (interactiveGameObject.GetComponent<IInteractuable>() is InteractuableDoor)
            {
                if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask))
                {
                    if (hit.collider.TryGetComponent<InteractuableDoor>(out InteractuableDoor door))
                    {
                        door.SoundPlayer();
                        door.Interact(this.transform);
                    }
                }
            }
            //Si és un llibre, cridem a l'Interact del llibre, ja que necessita l'objecte equipat per interactuar.
            else if (interactiveGameObject.GetComponent<IInteractuable>() is InteractuableCellBook)
            {
                if (equipedObject != null)
                {
                    if (equipedObject.GetComponent<PickItem>().item is BookItem)
                    {
                        interactiveGameObject.GetComponent<InteractuableCellBook>().Interact(equipedObject.gameObject);
                    }
                }
            }
            else
            {
                interactiveGameObject.GetComponent<IInteractuable>().Interact();
            }
            //Si l'objecte interactuable és remarkable, li tornem a posar el material base per tal que no es vegi l'outliner quan es fa el puzle per exemple.
            if (interactiveGameObject.GetComponent<IInteractuable>().isRemarkable)
                interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
            //Netegem la referència de l'objecte interactuable per tal que no es pugui tornar a interactuar fins que no es torni a fer un raycast.
            if (interactiveGameObject != null)
                interactiveGameObject = null;
            {
                OnNotInteractuable?.Invoke();
            }
        }
    }

    private void SetNoteOpened(bool opened)
    {
        noteOpened = opened;
    }

    public void ResumeInteract(bool resume)
    {
        if (resume)
        {
            if (coroutineInteract == null)
                coroutineInteract = StartCoroutine(InteractRaycast());
        }
        else
        {
            if (coroutineInteract != null)
            {
                StopCoroutine(coroutineInteract);
                coroutineInteract = null;
            }
        }
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
            equipedObject.transform.TryGetComponent<ThrowObject>(out ThrowObject throwable);
            if (throwable != null)
            {
                events.onHit += MakeSoundThrowable;
                throwable.GetComponent<Rigidbody>().isKinematic = false;
                throwable.camaraPrimera = _Camera.gameObject;
                throwable.Lanzar();
                itemSlotOccuped = false;
                equipedObject = null;
                InventoryManager.instance.UseEquippedItem();
            }
        }
    }

    private void MakeSoundThrowable()
    {

        OnMakeImpactSound?.Invoke(8);
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
            if (!isSilencerEquipped)
                GunAudioSource.PlayOneShot(ShootGunAudio);
            else
                GunAudioSource.PlayOneShot(SilencedShootGunAudio);
            gunanimator.Play("Shoot");
            OnShoot?.Invoke();
            gunAmmo--;
            OnAmmoChange?.Invoke(gunAmmo);
            Debug.DrawRay(shootPosition.transform.position, -shootPosition.transform.right, Color.yellow, 5f);
            Debug.Log("TIRO DEBUGRAY");
            if (isSilencerEquipped)
            {
                Debug.Log("Hago sonido silenciador");
                OnMakeImpactSound?.Invoke(5);
                MakeNoise(5, 40);
                silencerUses--;
                Debug.Log("Silenciador usos: " + silencerUses);
                
                if (silencerUses == 0)
                {
                    isSilencerEquipped = false;
                    silencer.SetActive(false);
                }
            }
            else
            {
                MakeNoise(20, 40); 
                OnMakeImpactSound?.Invoke(8);
            }
            Debug.DrawRay(shootPosition.transform.position, -shootPosition.transform.right);
            if (Physics.Raycast(shootPosition.transform.position, -shootPosition.transform.right, out RaycastHit e, 25f, enemyLayerMask)) // Canviar layer mask per posar les parets
            {
                if (e.transform.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeHealth();
                    Debug.Log("Enemy hit");
                }
            }
        }
        else
        {
            GunAudioSource.PlayOneShot(EmptyShootGunAudio);
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) SceneManager.LoadScene("GameOver");
        events.ActivateVignatteOnHurt();
        OnHpChange?.Invoke(hp, maxHp);
    }

    public void Heal(int hpToheal)
    {
        playerAudioSource.PlayOneShot(healthAudio);
        if(hp + hpToheal <= 6)
            hp += hpToheal;
        OnHpChange?.Invoke(hp, maxHp);
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

    enum PlayerStates { IDLE, MOVE, RUN, CROUCH, CROUCH_IDLE }
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
                OnMakeSound?.Invoke(2);
                break;
            case PlayerStates.MOVE:
                playerAudioSource.Stop();
                playerAudioSource.resource = walkAudio;
                playerAudioSource.loop = true;
                playerAudioSource.Play();
                if (coroutineMove == null)
                    coroutineMove = StartCoroutine(MakeNoiseMove());
                break;
            case PlayerStates.RUN:
                playerAudioSource.Stop();
                playerAudioSource.resource = runAudio;
                playerAudioSource.loop = true;
                playerAudioSource.Play();
                if (coroutineRun == null)
                    coroutineRun = StartCoroutine(MakeNoiseRun());
                break;
            case PlayerStates.CROUCH:
                playerAudioSource.Stop();
                _Camera.transform.localPosition = new Vector3(0f, 0f, -0.198f);
                cameraShenanigansGameObject.transform.localPosition = Vector3.zero;
                _VelocityMove /= 2;
                if (coroutineCrouch == null)
                    coroutineCrouch = StartCoroutine(MakeNoiseCrouch());
                break;
            case PlayerStates.CROUCH_IDLE:
                if (coroutineCrouch != null)
                {
                    StopCoroutine(coroutineCrouch);
                    coroutineCrouch = null;
                    OnMakeSound?.Invoke(2);
                }
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
                else if (movementInput == Vector2.zero)
                    ChangeState(PlayerStates.IDLE);
                break;
            case PlayerStates.CROUCH:
                velocity = (transform.right * movementInput.x +
                transform.forward * movementInput.y).normalized * 1.5f;
                vSpeed -= gravity * Time.deltaTime;
                velocity.y = vSpeed;
                if (!crouched || crouched && movementInput == Vector2.zero)
                {
                    ChangeState(PlayerStates.CROUCH_IDLE);
                }
                break;
            case PlayerStates.CROUCH_IDLE:
                if (crouched && movementInput != Vector2.zero)
                {
                    ChangeState(PlayerStates.CROUCH);
                }
                else if (!crouched)
                {
                    ChangeState(PlayerStates.IDLE);
                }
                break;
        }
        characterController.Move(velocity * Time.deltaTime);
    }

    private void ExitState(PlayerStates exitState)
    {
        switch (exitState)
        {
            case PlayerStates.MOVE:
                playerAudioSource.loop = false;
                playerAudioSource.Stop();
                if (coroutineMove != null)
                {
                    StopCoroutine(coroutineMove);
                    coroutineMove = null;
                }
                break;
            case PlayerStates.RUN:
                playerAudioSource.loop = false;
                playerAudioSource.Stop();
                if (coroutineRun != null)
                {
                    StopCoroutine(coroutineRun);
                    coroutineRun = null;
                }
                break;
            case PlayerStates.CROUCH:
                if (coroutineCrouch != null)
                {
                    StopCoroutine(coroutineCrouch);
                    coroutineCrouch = null;
                }
                break;
            case PlayerStates.CROUCH_IDLE:
                _Camera.transform.localPosition = cameraPositionBeforeCrouch;
                cameraShenanigansGameObject.transform.localPosition = new Vector3(0f, _Camera.transform.localPosition.y, 0f);
                _VelocityMove *= 2;
                break;
            default:
                break;
        }
    }

    #endregion

    public IEnumerator InteractRaycast()
    {
        while (true)
        {
            Debug.DrawRay(_Camera.transform.position, _Camera.transform.forward, Color.magenta, 5f);
            if (Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit, 5f, interactLayerMask))
            {
                if (hit.transform.TryGetComponent<IInteractuable>(out IInteractuable interactuable))
                {
                    if (interactuable.isInteractuable)
                    {
                        //Traiem el material de l'objecte interactuable si es remarkable i és un objecte diferent amb el que vam interactuar
                        if (interactiveGameObject != null && !interactiveGameObject.Equals(hit.transform.gameObject) && interactuable.isRemarkable && interactiveGameObject.GetComponent<MeshRenderer>() != null)
                        {
                            interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                            interactiveGameObject = null;
                        }
                        //Guardem una referència a l'objecte interactuable
                        interactiveGameObject = hit.transform.gameObject;
                        //Si l'objecte interactuable és remarkable, li afegim el material
                        if (interactuable.isRemarkable)
                        {
                            interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                            baseMaterial = interactiveGameObject.GetComponent<MeshRenderer>().materials[0];
                            interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[]
                            {
                            interactiveGameObject.GetComponent<MeshRenderer>().materials[0],

                            material
                            };
                        }
                        OnInteractuable?.Invoke(interactiveGameObject);
                    }
                }
            }
            //Si no hi ha cap objecte interactuable a la vista, netegem la referència i invoquem l'event OnNotInteractuable
            else if (!Physics.Raycast(_Camera.transform.position, _Camera.transform.forward, out RaycastHit hit2, 5f, interactLayerMask))
            {
                if (interactiveGameObject != null)
                {
                    if (interactiveGameObject.GetComponent<IInteractuable>().isRemarkable)
                    {
                        interactiveGameObject.GetComponent<MeshRenderer>().materials = new Material[] { interactiveGameObject.GetComponent<MeshRenderer>().materials[0] };
                        interactiveGameObject = null;
                    }
                    else
                    {
                        interactiveGameObject = null;
                    }
                }
                OnNotInteractuable?.Invoke();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    #region SOUNDS

    IEnumerator MakeNoiseMove()
    {
        while (true)
        {
            MakeNoise(6, 40);
            OnMakeSound?.Invoke(4);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MakeNoiseRun()
    {
        while (true)
        {
            Debug.Log("CORUTINACORRER");
            MakeNoise(8, 40);
            OnMakeSound?.Invoke(7);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MakeNoiseCrouch()
    {
        while (true)
        {
            Debug.Log("CORUTINACROUCH");
            MakeNoise(1, 40);
            OnMakeSound?.Invoke(3);
            yield return new WaitForSeconds(0.5f);
        }
    }
    private void MakeNoise(int noiseQuantity, int radius = 40)
    {
        Collider[] colliderHits = Physics.OverlapSphere(this.transform.position, radius);
        foreach (Collider collider in colliderHits)
        {
            if (collider.gameObject.TryGetComponent<Enemy>(out Enemy en))
            {
                Debug.Log("Entro a crouch");
                en.ListenSound(this.transform.position, noiseQuantity);
            }
        }
    }
    #endregion

    public void SetCurrentFOV(int fovLevel)
    {
        currentFov = fovLevel;
    }

    public void SetSensitivity(float sensibility)
    {
        this._LookVelocity = sensibility;
    }

    public void ReloadAmmo(int numAmmo)
    {
        GunAudioSource.PlayOneShot(RechargeGunAudio);
        this.gunAmmo += numAmmo;
        OnAmmoChange?.Invoke(gunAmmo);
    }

    public void Flashlight(InputAction.CallbackContext context)
    {
        playerAudioSource.PlayOneShot(flashlightSound);
        if (!flashlight.activeSelf) flashlight.SetActive(true);
        else flashlight.SetActive(false);
        PuzzleManager.instance.IsFlashlightning();
    }

    public void ToggleChestAnimation(bool opened)
    {
        if (opened && interactiveGameObject != null && interactiveGameObject.GetComponent<IInteractuable>() is InteractuableChest)
            interactiveGameObject.GetComponent<Animator>().Play("OpenChest");
        else
            interactiveGameObject.GetComponent<Animator>().Play("CloseChest");

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
            _inputActions.Player.Pause.performed -= OpenMenu;
            _inputActions.Player.Flashlight.performed -= Flashlight;
            _inputActions.Player.Disable();
            if(InventoryManager.instance != null)
                InventoryManager.instance.OnNote -= SetNoteOpened;
        }
    }
}
