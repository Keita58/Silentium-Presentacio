using NavKeypad;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private void Awake()
    {
        inputActionPlayer = new InputSystem_Actions();
        if (instance == null)
            instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InteractClockPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Clock.gameObject.SetActive(true);
        cam_Player.transform.parent.position = new Vector3(-32.8191757f, 6.21000004f, -32.4704895f);
        cam_Player.transform.parent.rotation = new Quaternion(0, -0.608760536f, 0, 0.793354094f);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Disable();
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Enable();
    }
    public void ExitClockPuzzle()
    {
        cam_Player.gameObject.SetActive(true);
        cam_Clock.gameObject.SetActive(false);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Enable();
        cam_Player.transform.parent.GetComponent<Player>().ResumeInteract();
        //cam_Player.transform.parent.GetComponent<Player>().ResumeInteract();
        cam_Clock.transform.parent.GetComponent<Clock>().inputActions.Clock.Disable();
    }
    public void ClockSolved()
    {
        ExitClockPuzzle();
        this.KeyClock.SetActive(true);
    }

    public void InteractHieroglyphicPuzzle()
    {
        cam_Player.gameObject.SetActive(false);
        cam_Hierogliphic.gameObject.SetActive(true);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Disable();
        cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Enable();
        Cursor.visible = true;
    }

    public void HieroglyphicPuzzleExit()
    {
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Enable();
        cam_Hierogliphic.transform.parent.GetComponent<Keypad>().inputActions.Hieroglyphic.Disable();
        cam_Player.transform.parent.GetComponent<Player>().ResumeInteract();
        cam_Player.gameObject.SetActive(true);
        cam_Hierogliphic.gameObject.SetActive(false);
        Cursor.visible = false;
    }
}
