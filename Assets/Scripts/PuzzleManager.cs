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
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Disable();
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Clock.Enable();
    }
    public void ExitClockPuzzle()
    {
        cam_Player.gameObject.SetActive(true);
        cam_Clock.gameObject.SetActive(false);
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Player.Enable();
        cam_Player.transform.parent.GetComponent<Player>()._inputActions.Clock.Disable();
    }
    public void ClockSolved()
    {
        this.KeyClock.SetActive(true);
    }
}
