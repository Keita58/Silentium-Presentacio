using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class FogManager : MonoBehaviour
{
    Volume volume;
    Fog fog;
    [SerializeField] PostProcessEvents postProcessEvent;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out fog);
    }

    private void ActivateFog(bool enable)
    {
        fog.enabled.value = enable;
    }
}
