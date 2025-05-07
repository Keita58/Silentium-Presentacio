using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class VignetteManager : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    [SerializeField] private PostProcessEvents postProcessEvents;

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
    }
}
