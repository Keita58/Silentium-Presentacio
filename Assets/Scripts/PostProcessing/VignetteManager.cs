using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class VignetteManager : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    [SerializeField] private Events postProcessEvents;
    //public DigitalGlitch glitch;
    //Glitch glitch;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        vignette.active = false;
        postProcessEvents.OnVignetteActive += ActivateVignette;
        
    }

    private void ActivateVignette()
    {
        vignette.active = true;
        StartCoroutine(DeactivateVignette());
    }

    private IEnumerator DeactivateVignette()
    {
        yield return new WaitForSeconds(0.5f);
        while (vignette.intensity.value > 0)
        {
            vignette.intensity.value -= 0.1f;
            Debug.Log("Resto" +vignette.intensity.value);
            yield return new WaitForSeconds(0.1f);
        }
        vignette.active = false;
        vignette.intensity.value = 0.545f;
    }

    private void OnDestroy()
    {
        postProcessEvents.OnVignetteActive -= ActivateVignette;
    }
}
