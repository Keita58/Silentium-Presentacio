using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FilmGrainManager : MonoBehaviour
{
    Volume volume;
    FilmGrain filmGrain;
    [SerializeField] Events postProcessEvents;
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out filmGrain);
        postProcessEvents.OnIncreaseIntesityFilmGrain += IncreaseIntensity;
    }

    private void IncreaseIntensity(float intensity)
    {
        if (!filmGrain.IsActive())
            filmGrain.active = true;

        filmGrain.intensity.value += intensity;
    }
    private void OnDestroy()
    {
        postProcessEvents.OnIncreaseIntesityFilmGrain -= IncreaseIntensity;
    }
}
