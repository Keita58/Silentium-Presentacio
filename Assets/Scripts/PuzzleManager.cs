using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance { get; private set; }
    [SerializeField] 
    private GameObject KeyClock;
    private void Awake()
    {
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

    public void ClockSolved()
    {
        this.KeyClock.SetActive(true);
    }
}
