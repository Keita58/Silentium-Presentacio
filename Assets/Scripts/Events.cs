using System;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "Events", menuName = "Scriptable Objects/Events")]
public class Events : ScriptableObject
{

    //Events chromaticAberration
    public event Action<float> OnIncreaseIntensity;

    public void IncreaseIntensityChAb(float intensity)
    {
        OnIncreaseIntensity?.Invoke(intensity);
    }

    public event Action<int> OnIncreaseSamples;

    public void IncreaseSampleChAb(int samples)
    {
        OnIncreaseSamples?.Invoke(samples);
    }

    //Events Vignette
    public event Action OnVignetteActive;

    public void ActivateVignatteOnHurt()
    {
        OnVignetteActive?.Invoke();
    }

    //Events filmGrain
    public event Action<float> OnIncreaseIntesityFilmGrain;

    public void IncreaseIntensityFilmGrain(float intensity)
    {
        OnIncreaseIntesityFilmGrain?.Invoke(intensity);
    }

    //Events fog
    public event Action<bool> OnToggleFog;

    public void ToggleFog(bool enable)
    {
        OnToggleFog?.Invoke(enable);
    }

    //Events customPass
    public event Action<bool> OnToggleCustomPass;
    public void ToggleCustomPass(bool enable)
    {
        OnToggleCustomPass?.Invoke(enable);
    }

    //Event throw
    public event Action onHit;
    public void MakeThrowImpactSound()
    {
        onHit?.Invoke();
    }

    //EVENTS UI
    //Event Warning Inventari ple
    public event Action<string> OnWarning;
    public void ShowWarning(string text)
    {
        OnWarning?.Invoke(text);
    }

    //Per amagar les waves.
    public event Action<bool> OnToggleUI;
    public void ToggleUI(bool active)
    {
        OnToggleUI?.Invoke(active);
    }

    //EVENT Objects
    public event Action<int> OnPickItem;
    public void PickItem(int itemId)
    {
        OnPickItem?.Invoke(itemId);
    }
}
