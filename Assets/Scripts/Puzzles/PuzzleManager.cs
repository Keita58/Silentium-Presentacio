using NavKeypad;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance { get; private set; }
    [SerializeField]
    private GameObject KeyClock;
    [SerializeField]
    private Camera cam_Clock;
    [SerializeField]
    private Camera cam_Player;
    private InputSystem_Actions inputActionPlayer;
    [SerializeField]
    private Camera cam_Hierogliphic;
    [SerializeField]
    private Camera cam_WeaponPuzzle;
    [SerializeField]
    private Camera cam_HieroglyphicAnimation;
    [SerializeField]
    BookPuzzle bookPuzzle;
    [SerializeField]
    private List<Picture> pictureList;
    [SerializeField]
    public List<Picture> picturesClicked;
    [SerializeField]
    Door DoorPoem3;
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
    private AnimationClip doorsHieroglyphic;
    [SerializeField]
    private Animator morseAnimator;
    [SerializeField]
    private Animator hieroglyphicAnimator;
    float animationTime = 0f;
    bool isMorseCompleted = false;
    [SerializeField]
    private bool isHieroglyphicCompleted = false;
    [SerializeField]
    private AnimationClip fadeOut;
    [SerializeField]
    private Animator fadeAnimator;
    bool fadeOutStarted = false;
    bool teleported = false;
    Transform positionToTeleport;
    [SerializeField]
    GameObject bookWall;

    [Header("Positions after puzzles")]
    [SerializeField]
    private Transform positionAfterHieroglyphic;
    [SerializeField]
    private Transform positionAfterBook;
    private void Awake()
    {
        inputActionPlayer = new InputSystem_Actions();
        if (instance == null)
            instance = this;
    }
    public void InteractClockPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Clock.gameObject.SetActive(true);
        cam_Player.transform.parent.position = new Vector3(-32.8191757f, 6.21000004f, -32.4704895f);
        cam_Player.transform.parent.rotation = new Quaternion(0, -0.608760536f, 0, 0.793354094f);
        player._inputActions.Player.Disable();
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Enable();
    }
    public void ExitClockPuzzle()
    {
        cam_Player.gameObject.SetActive(true);
        cam_Clock.gameObject.SetActive(false);
        player._inputActions.Player.Enable();
        player.ResumeInteract();
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Disable();
    }
    public void ClockSolved()
    {
        ExitClockPuzzle();
        player.ResumeInteract();
        this.KeyClock.SetActive(true);
    }

    public void InteractHieroglyphicPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Hierogliphic.gameObject.SetActive(true);
        player._inputActions.Player.Disable();
        cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Enable();
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
        player.ResumeInteract();
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
        player.ResumeInteract();
        cam_Player.gameObject.SetActive(true);
        Cursor.visible = false;

    }

    public void InteractMorsePuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_morse.gameObject.SetActive(true);
        player._inputActions.Player.Disable();
        morseKeypad.inputActions.Morse.Enable();
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
        fadeAnimator.Play("FadeOut");
        fadeOutStarted = true;
        animationTime = 0f;
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
        cam_Player.transform.parent.position = new Vector3(-32.8191757f, 6.21000004f, -32.4704895f);
        cam_Player.transform.parent.rotation = new Quaternion(0, -0.608760536f, 0, 0.793354094f);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Disable();
        cam_WeaponPuzzle.transform.parent.GetComponent<WeaponPuzzle>().inputActions.WeaponPuzzle.Enable();
    }
    public void ExitWeaponPuzzle()
    {
        cam_Player.gameObject.SetActive(true);
        cam_WeaponPuzzle.gameObject.SetActive(false);
        cam_Player.transform.parent.position = new Vector3(-32.8191757f, 6.21000004f, -32.4704895f);
        cam_Player.transform.parent.rotation = new Quaternion(0, -0.608760536f, 0, 0.793354094f);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Enable();
        cam_WeaponPuzzle.transform.parent.GetComponent<WeaponPuzzle>().inputActions.WeaponPuzzle.Disable();
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
        }else if (fadeOutStarted)
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
            if (animationTime >= fadeOut.length && teleported)
            {
                animationTime = 0f;
                fadeOutStarted = false;
                player.ToggleInputPlayer(true, true);
            }
        }
    }

    public void ChangePositionPlayerAfterHieroglyphic()
    {
        player.ToggleInputPlayer(false, false);
        fadeAnimator.Play("FadeOut");
        fadeOutStarted = true;
        animationTime = 0f;
        positionToTeleport = positionAfterHieroglyphic;
    }

}
