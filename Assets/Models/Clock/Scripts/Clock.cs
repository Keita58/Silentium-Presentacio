using System.Collections;
using UnityEngine;

public class Clock : MonoBehaviour
{

    //-- set start time 00:00
    [SerializeField] 
    private float minutes = 0;
    [SerializeField] 
    private float hour = 0;
    [SerializeField] 
    private int seconds = 0;
    public bool realTime = true;
    [SerializeField]
    private GameObject pointerSeconds;
    [SerializeField] 
    private GameObject pointerMinutes;
    [SerializeField] 
    private GameObject pointerHours;

    [SerializeField]
    private bool selectedMinnute = false;
    [SerializeField]
    private bool selected = false;
    //-- time speed factor
    private float clockSpeed = 10.0f;     // 1.0f = realtime, < 1.0f = slower, > 1.0f = faster

    //-- internal vars
    float msecs = 0;

    [SerializeField]
    private bool solved = false;
    void Start()
    {
        //-- set real time
        if (realTime)
        {
            hour = System.DateTime.Now.Hour;
            minutes = System.DateTime.Now.Minute;
            seconds = System.DateTime.Now.Second;
            float rotationMinutes = (360.0f / 60.0f) * minutes;
            pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
            float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);
            pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
        }
    }

    void Update()
    {
        // moveMinutes();
        StartCoroutine(Finished());
    }
    public void MoveMinutes()
    {
        msecs += Time.deltaTime * clockSpeed;
        if (msecs >= 1.0f)
        {
            msecs -= 1.0f;
            minutes++;
            if (minutes > 60)
            {
                minutes = 0;
                hour += 1;
                if (hour >= 24)
                    hour = 0;
            }
            float rotationMinutes = (360.0f / 60.0f) * minutes;
            pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        }
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
        if((hour >= 8.9 && hour <9.7 || hour >= 20.9 && hour < 21.7) && minutes >= 20.9 && minutes < 21.7)
        {        
            print("Lo hago");
            yield return new WaitForSeconds(1.5f);
            if ((hour >= 8.9 && hour < 9.7 || hour >= 20.9 && hour < 21.7) && minutes >= 20.9 && minutes < 21.7)
                print("Ahora activo reloj");
                PuzzleManager.instance.ClockSolved();
        }
    }
}
