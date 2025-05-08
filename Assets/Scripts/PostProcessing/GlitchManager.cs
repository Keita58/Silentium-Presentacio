using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GlitchManager : MonoBehaviour
{
    [Header("Glitch")]
    [SerializeField]
    private Material material;
    [SerializeField]
    private float noiseAmount;
    [SerializeField]
    private float glitchStrength;
    [SerializeField]
    private float scanLinesStrength;
    [SerializeField]
    private float flickeringStrength;

    private void Update()
    {
        material.SetFloat("_NoiseAmount", noiseAmount);
        material.SetFloat("_GlitchStrength", glitchStrength);
        material.SetFloat("_ScanLinesStrength", scanLinesStrength);
        material.SetFloat("_FlickerStrength", flickeringStrength);
    }
}
