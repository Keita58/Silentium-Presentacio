using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Clock : MonoBehaviour
{

    //-- set start time 00:00
    [SerializeField]
    private float minutes = 0;
    [SerializeField]
    private float hour = 0;
    [SerializeField]
    private int seconds = 0;
    [SerializeField]
    private GameObject pointerSeconds;
    [SerializeField]
    private GameObject pointerMinutes;
    [SerializeField]
    private GameObject pointerHours;
    //-- time speed factor
    private float clockSpeed = 10.0f;     // 1.0f = realtime, < 1.0f = slower, > 1.0f = faster

    //-- internal vars
    float msecs = 0;

    public InputSystem_Actions inputActions { get; private set; }
    private InputAction moveClock;

    private Coroutine MinutesCoroutine;
    private Coroutine MinutesCoroutineReversed;
    private Coroutine FinishedCoroutine;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Clock.MoveClockHand.performed += HandleInputMinutes;
        inputActions.Clock.MoveClockHand.canceled += HandleInputMinutes;
        inputActions.Clock.MoveClockHandReversed.performed += HandleInputMinutesReverse;
        inputActions.Clock.MoveClockHandReversed.canceled += HandleInputMinutesReverse;
        inputActions.Clock.FinishClock.started += HandleMinutesFinished;
        inputActions.Clock.FinishClock.canceled += HandleMinutesFinished;
        moveClock = inputActions.Clock.MoveClockHand;
        inputActions.Clock.Exit.started += Exit;
        //    inputActions.Clock.Enable();
    }
    void Start()
    {
        //-- set real time
        float rotationMinutes = (360.0f / 60.0f) * minutes;
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);
        pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
    }

    void Update()
    {
        // moveMinutes();
        //StartCoroutine(Finished());
    }

    private void HandleInputMinutes(InputAction.CallbackContext context)
    {
        if (context.performed)
            MinutesCoroutine = StartCoroutine(MoveMinutes());
        else
            StopCoroutine(MinutesCoroutine);
    }
    private void HandleMinutesFinished(InputAction.CallbackContext context)
    {
        FinishedCoroutine = StartCoroutine(Finished());
    }

    private IEnumerator MoveMinutes()
    {
        float updateTime = 0.5f;
        while (true)
        {
            minutes++;
            if (minutes > 60)
            {
                minutes = 0;
                hour += 1;
                if (hour >= 24)
                    hour = 0;
            }
            float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);
            pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
            float rotationMinutes = (360.0f / 60.0f) * minutes;
            pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);

            yield return new WaitForSeconds(updateTime);
            updateTime = Mathf.Max(updateTime * 0.5f, 0.02f);
        }
    }

    private void HandleInputMinutesReverse(InputAction.CallbackContext context)
    {
        if (context.performed)
            MinutesCoroutineReversed = StartCoroutine(MoveMinutesReverse());
        else
            StopCoroutine(MinutesCoroutineReversed);
    }

    private IEnumerator MoveMinutesReverse()
    {
        float updateTime = 0.5f;
        while (true)
        {
            minutes--;
            if (minutes <= 0)
            {
                minutes = 60;
                hour -= 1;
                if (hour <= 00)
                    hour = 24;
            }
            float rotationMinutes = (360.0f / 60.0f) * minutes;
            pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
            float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);
            pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
            yield return new WaitForSeconds(updateTime);
            updateTime = Mathf.Max(updateTime * 0.5f, 0.02f);
        }
    }
    public void Exit(InputAction.CallbackContext context)
    {
        PuzzleManager.instance.ExitClockPuzzle();
    }
    public void MoveHour()
    {
        msecs += Time.deltaTime * clockSpeed;
        if (msecs >= 1.0f)
        {
            msecs -= 1.0f;
            hour += 0.1f;
            if (hour >= 24)
                hour = 0;
            float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);
            pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
        }
    }

    private IEnumerator Finished()
    {
        if ((hour >= 8.9 && hour < 9.7 || hour >= 20.9 && hour < 21.7) && minutes >= 20.9 && minutes < 21.7)
        {
            print("Lo hago");
            yield return new WaitForSeconds(1.5f);
            if ((hour >= 8.9 && hour < 9.7 || hour >= 20.9 && hour < 21.7) && minutes >= 20.9 && minutes < 21.7)
            {
                this.gameObject.layer = 0;
                PuzzleManager.instance.ClockSolved();
            }
        }
    }
}
