using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ChromaticAberrationManager : MonoBehaviour
{
    Volume volume;
    ChromaticAberration chAb;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out chAb);
    }

    public void IncreaseChAbIntensity(float intensity)
    {
        chAb.intensity.value += intensity;
    }
}
