using NavKeypad;
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
    BookPuzzle bookPuzzle;
    [SerializeField]
    GameObject WallPoem1;
    [SerializeField]
    GameObject WallPoem2;
    [SerializeField]
    Door DoorPoem3;
    [SerializeField]
    Player player;
    [SerializeField]
    private Camera cam_morse;
    [SerializeField]
    MorseKeypad morseKeypad;
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

    public void HieroglyphicPuzzleExit()
    {
        player._inputActions.Player.Enable();
        cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Disable();
        player.ResumeInteract();
        cam_Player.gameObject.SetActive(true);
        cam_Hierogliphic.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    public void ExitMorsePuzzle()
    {
        player._inputActions.Player.Enable();
        morseKeypad.inputActions.Morse.Disable();
        player.ResumeInteract();
        cam_Player.gameObject.SetActive(true);
        cam_morse.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    public void InteractMorsePuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_morse.gameObject.SetActive(true);
        player._inputActions.Player.Disable();
        morseKeypad.inputActions.Morse.Enable();
        Cursor.visible= true;
    }

    public void CheckBookPuzzle()
    {
        Debug.Log("CheckBookPuzzle");
        bookPuzzle.checkBookPosition();
    }
    public void TakePoemPart(Notes poem)
    {
        if (poem.note.noteId == 8)
        {
            WallPoem1.SetActive(false);
        }
        else if (poem.note.noteId == 9)
        {
            WallPoem2.SetActive(false);
        }
        else if (poem.note.noteId == 10)
        {
            DoorPoem3.isLocked = false; ;
        }
    }

}
