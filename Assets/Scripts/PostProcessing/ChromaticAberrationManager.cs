using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ChromaticAberrationManager : MonoBehaviour
{
    Volume volume;
    ChromaticAberration chAb;
    [SerializeField] Events postProcessEvent;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out chAb);
        postProcessEvent.OnIncreaseIntensity += IncreaseChAbIntensity;
        postProcessEvent.OnIncreaseSamples += IncreaseSample;
    }

    private void IncreaseChAbIntensity(float intensity)
    {
        if (!chAb.IsActive())
            chAb.active = true;
        chAb.intensity.value += intensity;
    }

    private void IncreaseSample(int number)
    {
        chAb.maxSamples += number;
    }

    private void OnDestroy()
    {
        postProcessEvent.OnIncreaseIntensity -= IncreaseChAbIntensity;
        postProcessEvent.OnIncreaseSamples -= IncreaseSample;
    }
}
