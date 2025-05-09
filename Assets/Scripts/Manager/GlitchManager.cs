using Kino.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;

public class GlitchManager : MonoBehaviour
{
    Volume volume;
    Glitch glitch;
    [SerializeField] PostProcessEvents events;
    [SerializeField] 
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out glitch);
    }

    private void ActivateGlitch()
    {
        if (!glitch.IsActive())
            glitch.active = true;

        //while ()
    }
}
