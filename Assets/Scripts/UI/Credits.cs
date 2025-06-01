using System;
using UnityEngine;
using UnityEngine.Video;

public class Credits : MonoBehaviour
{
    private VideoPlayer video;
    private bool credits = false;
    [SerializeField] Canvas canvas;
    private void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.enabled = false;
    }

    float deltaTime = 0f;
    
    public void StartCredits()
    {
        canvas.enabled = false;
        credits = true;
        deltaTime = 0f;
        video.enabled = true;
    }
    private void Update()
    {
        if (credits)
        {
            deltaTime += Time.deltaTime;
            if (deltaTime >= video.clip.length)
            {
                deltaTime = 0f;
                credits = false;
                canvas.enabled = true;
                video.enabled = false;
            }
        }
    }
}
