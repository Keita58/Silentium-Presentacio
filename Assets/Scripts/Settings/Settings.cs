using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Player player;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] TMP_Dropdown fpsDropdown;
    [SerializeField] Slider fovSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    public void SetVolume()
    {
        audioMixer.SetFloat("Volume", musicSlider.value);
    }

    public void SetSfxValue()
    {
    }

    public void SetFPS()
    {
        //quiero descheckear el toggle como lo hago?
        vSyncToggle.isOn = fpsDropdown.value == 0 ? true : false; // If FPS is set to unlimited, enable VSync toggle
        vSyncToggle.enabled = fpsDropdown.value == 0 ? true : false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fpsDropdown.value switch
        {
            0 => -1,
            1 => 30,
            2 => 60,
            3=> 120,
            _ => -1 //default
        };
    }

    public void SetVSync()
    {
        QualitySettings.vSyncCount= vSyncToggle.isOn ? 1 : 0;
        vSyncToggle.transform.GetChild(1).GetComponent<Text>().text = vSyncToggle.isOn ? "Activado" : "Desactivado";
    }

    public void SetFOV()
    {
        Camera.main.fieldOfView = fovSlider.value;
        player.SetCurrentFOV((int)fovSlider.value);
    }

}
