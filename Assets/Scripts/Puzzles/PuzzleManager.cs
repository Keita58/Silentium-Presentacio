using NavKeypad;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance { get; set; }

    [Header("Clock Puzzle")]
    [SerializeField]
    private GameObject KeyClock;
    [SerializeField]
    private Camera cam_Clock;
    [SerializeField]
    private Camera cam_Player;
    private InputSystem_Actions inputActionPlayer;
    public bool clockPuzzleCompleted { get; set; }

    [Header("Hieroglyphic Puzzle")]
    [SerializeField]
    private Camera cam_Hierogliphic;
    [SerializeField]
    private AnimationClip doorsHieroglyphic;   
    [SerializeField]
    private Animator hieroglyphicAnimator;
    [SerializeField]
    private Camera cam_HieroglyphicAnimation;
    [SerializeField]
    public bool isHieroglyphicCompleted { get; set; }

    [Header("Book Puzzle")]
    [SerializeField]
    private BookPuzzle bookPuzzle;
    [SerializeField]
    private GameObject bookWall;
    public bool bookPuzzleCompleted { get; set; }

    [Header("Poem Puzzle")]
    [SerializeField]
    private List<Picture> pictureList;
    [SerializeField]
    public List<Picture> picturesClicked;
    [SerializeField]
    private Door DoorPoem3;
    public bool poemPuzzleCompleted { get; set; }

    [Header("Morse Puzzle")]
    [SerializeField]
    Player player;
    [SerializeField]
    private Camera cam_morse;
    [SerializeField]
    MorseKeypad morseKeypad;
    [SerializeField]
    private GameObject cam_doorsMorseAnimation;
    [SerializeField]
    private AnimationClip doorsMorseAnimation;
    [SerializeField]
    private Animator morseAnimator;
    public bool isMorseCompleted { get; set; }

    [Header("Weapon Puzzle")]
    [SerializeField]
    private Camera cam_WeaponPuzzle;
    [SerializeField]
    private BoxCollider allWeapon;
    public bool weaponPuzzleCompleted { get; set; }

    [Header("Glitch")]
    [SerializeField]
    AnimationClip glitchEffect;
    [SerializeField]
    Animator glitchAnimator;
    [SerializeField] bool glitchStarted = false;

    [Header("Positions after puzzles")]
    [SerializeField]
    private Transform positionAfterHieroglyphic;
    [SerializeField]
    private Transform positionAfterBook;
    [SerializeField]
    private Transform positionAfterPoem;

    [Header("All puzzles")]
    private float animationTime;
    [SerializeField]
    private AnimationClip fadeOut;
    [SerializeField]
    private Animator fadeAnimator;
    private bool fadeOutStarted = false;
    private bool teleported = false;
    private Transform positionToTeleport;

    [Header("Glitch")]
    [SerializeField]
    private Material material;
    [SerializeField]
    private float noiseAmount;
    [SerializeField]
    private float glitchStrength;
    [SerializeField]
    private float scanLinesStrength;
    [SerializeField]
    private float FlickeringStrength;
    [SerializeField] PostProcessEvents events;

    private void Awake()
    {
        inputActionPlayer = new InputSystem_Actions();
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        clockPuzzleCompleted = false;
        isHieroglyphicCompleted = false;
        bookPuzzleCompleted = false;
        poemPuzzleCompleted = false;
        isMorseCompleted = false;
        weaponPuzzleCompleted = false;
        animationTime = 0f;
    }

    public void InteractClockPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Clock.gameObject.SetActive(true);
        cam_Player.transform.parent.position = new Vector3(-32.8191757f, 6.21000004f, -32.4704895f);
        cam_Player.transform.parent.rotation = new Quaternion(0, -0.608760536f, 0, 0.793354094f);
        player._inputActions.Player.Disable();
        player.ResumeInteract(false);
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Enable();
    }
    public void ExitClockPuzzle()
    {
        cam_Player.gameObject.SetActive(true);
        cam_Clock.gameObject.SetActive(false);
        player._inputActions.Player.Enable();
        player.ResumeInteract(true);
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Disable();
    }
    public void ClockSolved()
    {
        ExitClockPuzzle();
        player.ResumeInteract(true);
        this.KeyClock.SetActive(true);
        clockPuzzleCompleted = true;
    }

    public void InteractHieroglyphicPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Hierogliphic.gameObject.SetActive(true);
        player._inputActions.Player.Disable();
        player.ResumeInteract(false);
        //cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Enable();
        cam_Hierogliphic.transform.parent.GetComponent<LineRendererExample>()._inputAction.Hieroglyphic.Enable();
        Cursor.visible = true;
    }

    public void HieroglyphicPuzzleExitAnimation()
    {
        cam_Hierogliphic.gameObject.SetActive(false);
        cam_HieroglyphicAnimation.gameObject.SetActive(true);
        hieroglyphicAnimator.Play("HieroDoor");
        animationTime = 0f;
        isHieroglyphicCompleted = true;
    }

    public void HieroglyphicPuzzleExit(bool isCompleted)
    {
        if (isCompleted)
            cam_HieroglyphicAnimation.gameObject.SetActive(false);
        else
            cam_Hierogliphic.gameObject.SetActive(false);

        player._inputActions.Player.Enable();
        cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Disable();
        player.ResumeInteract(true);
        cam_Player.gameObject.SetActive(true);
        Cursor.visible = false;
    }

    public void ExitMorsePuzzleAnimation()
    {
        cam_morse.gameObject.SetActive(false);
        cam_doorsMorseAnimation.SetActive(true);
        morseAnimator.Play("FinalDoor");
        animationTime = 0f;
        isMorseCompleted = true;
    }

    public void ExitMorsePuzzle(bool isCompleted)
    {
        if (isCompleted)
            cam_doorsMorseAnimation.SetActive(false);
        else
            cam_morse.gameObject.SetActive(false);

        player.ToggleInputPlayer(true, true);
        morseKeypad.inputActions.Morse.Disable();
        player.ResumeInteract(true);
        cam_Player.gameObject.SetActive(true);
        Cursor.visible = false;
        isMorseCompleted = true;
    }

    public void InteractMorsePuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_morse.gameObject.SetActive(true);
        player._inputActions.Player.Disable();
        morseKeypad.inputActions.Morse.Enable();
        player.ResumeInteract(false);
        Cursor.visible = true;
    }

    public void CheckBookPuzzle()
    {
        Debug.Log("CheckBookPuzzle");
        bookPuzzle.checkBookPosition();
    }

    public void BookPuzzleCompleted()
    {
        bookWall.SetActive(false);
        player.ToggleInputPlayer(false, false);
        positionToTeleport = positionAfterBook;
        events.ToggleCustomPass(false);
        glitchAnimator.Play("Glitch");
        glitchStarted = true;
        animationTime = 0f;
        bookPuzzleCompleted = true;
    }

    public void TakePoemPart()
    {
        for (int i = 0; i < picturesClicked.Count; i++)
        {
            if (picturesClicked.ElementAt(i) == pictureList.ElementAt(i))
            {
                if (i == pictureList.Count - 1)
                {
                    for (int x = 0; x < picturesClicked.Count; x++)
                    {
                        pictureList.ElementAt(x).gameObject.layer = 0;
                    }
                    DoorPoem3.isLocked = false;
                    player.ToggleInputPlayer(false, false);
                    positionToTeleport = positionAfterPoem;
                    glitchAnimator.Play("Glitch");
                    glitchStarted = true;
                    animationTime = 0f;
                    poemPuzzleCompleted = true;
                }
            }
            else
            {
                picturesClicked.Clear();
            }
        }
    }

    public void InteractWeaponPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_WeaponPuzzle.gameObject.SetActive(true);
        cam_WeaponPuzzle.transform.parent.GetComponent<WeaponPuzzle>().inputAction.WeaponPuzzle.Enable();
        player._inputActions.Player.Disable();
        allWeapon.enabled = false;
        Cursor.visible = true;
        player.ResumeInteract(false);
    }

    public void ExitWeaponPuzzle()
    {
        cam_WeaponPuzzle.transform.parent.GetComponent<WeaponPuzzle>().inputAction.WeaponPuzzle.Disable();
        player.ResumeInteract(true);
        player._inputActions.Player.Enable();      
        cam_Player.gameObject.SetActive(true);
        cam_WeaponPuzzle.gameObject.SetActive(false);
        Cursor.visible = false;
        weaponPuzzleCompleted = true;
    }

    void Update()
    {
        if (isMorseCompleted)
        {
            animationTime += Time.deltaTime;
            if (animationTime >= 2.4f)
            {
                ExitMorsePuzzle(true);
                animationTime = 0f;
                isMorseCompleted = false;
            }
        }
        else if (isHieroglyphicCompleted)
        {
            animationTime += Time.deltaTime;
            if (animationTime >= 2.4f)
            {
                HieroglyphicPuzzleExit(true);
                animationTime = 0f;
                isHieroglyphicCompleted = false;
            }
        }else if (glitchStarted)
        {
            animationTime += Time.deltaTime;
            if (animationTime >= 2f && !teleported)
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = positionToTeleport.position;
                player.GetComponent<CharacterController>().enabled = true;
                teleported = true;
                positionToTeleport = null;
            }
            if (animationTime >= glitchEffect.length && teleported)
            {
                animationTime = 0f;
                glitchStarted = false;
                player.ToggleInputPlayer(true, true);
                events.ToggleCustomPass(true);
            }
        }
    }

    public void ChangePositionPlayerAfterHieroglyphic()
    {
        player.ToggleInputPlayer(false, false);
        glitchAnimator.Play("Glitch");
        glitchStarted = true;
        animationTime = 0f;
        positionToTeleport = positionAfterHieroglyphic;
    }

    public void LoadGame()
    {

    }
}
