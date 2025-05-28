using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CustomPassesManager : MonoBehaviour
{
    GameObject weaponCustomPass;
    [SerializeField] Events postProcessEvents;
    void Start()
    {
        weaponCustomPass = this.gameObject;
        postProcessEvents.OnToggleCustomPass += ToggleCustomPass;
    }

    private void ToggleCustomPass(bool enable)
    {
        weaponCustomPass.GetComponents<CustomPassVolume>()[1].enabled = enable; //weapon
        weaponCustomPass.GetComponents<CustomPassVolume>()[2].enabled = enable; //silencer
    }
    private void OnDestroy()
    {
        postProcessEvents.OnToggleCustomPass -= ToggleCustomPass;

    }
}
