using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Player player;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] TMP_Dropdown fpsDropdown;
    [SerializeField] Slider fovSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider sensitivitySlider;

    [Header("General")]
    //booleano que controla si es la escena inicial o no.
    public bool isInitialScene;
    //Variables temporales para guardar el valor actual
    [SerializeField] public float currentVolumeValue = 1;
    [SerializeField] public float currentSfxValue = 1;
    [SerializeField] public int currentFpsValue = 0;
    [SerializeField] public int currentVSyncState = 0;
    [SerializeField] public float currentFOVValue = 60;
    [SerializeField] public float currentSensitivityValue = 30;

    public event Action onSaveConfig;

    //Cuando el usuario abre el menu de opciones se ponen los valores de los componentes segun el valor de las variables temporales
    //si no se ha modificado nada, pues se quedan los valores minimos, si no se ponen los valores guardados anteriormente.
    private void OnEnable()
    {
        StartOptions();
    }
    public void StartOptions()
    {
        fovSlider.value = currentFOVValue;
        vSyncToggle.isOn = currentVSyncState == 1 ? true:false;
        fpsDropdown.value = currentFpsValue;
        sfxSlider.value = currentSfxValue;
        musicSlider.value = currentVolumeValue;
        sensitivitySlider.value = currentSensitivityValue;
        ApplySettings();
    }

    public void SetVolume()
    {
        float volume = Mathf.Log10(musicSlider.value) * 20;
        audioMixer.SetFloat("Music", volume);
    }

    public void SetSfxValue()
    {
        float volume = Mathf.Log10(sfxSlider.value) * 20;
        audioMixer.SetFloat("SFX", volume);
    }

    public void SetSensitivity()
    {
        if (!isInitialScene)
        {
            player.SetSensitivity(sensitivitySlider.value);
        }
    }

    public void ToggleCheckVSync()
    {
        vSyncToggle.enabled = fpsDropdown.value == 0 ? true : false;
    }

    public void SetFPS()
    {
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

    public void ChangeTextVSync()
    {
        vSyncToggle.transform.GetChild(1).GetComponent<Text>().text = vSyncToggle.isOn ? "Activado" : "Desactivado";
    }

    public void SetVSync()
    {
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
    }

    public void SetFOV()
    {
        if (!isInitialScene)
        {
            Camera.main.fieldOfView = fovSlider.value;
            player.SetCurrentFOV((int)fovSlider.value);
        }
    }

    //Si se le da al bot�n de aplicar actualizamos las variables temporales.
    private void UpdateCurrentValues()
    {
        currentFOVValue = fovSlider.value;
        currentVSyncState = QualitySettings.vSyncCount;
        currentFpsValue = fpsDropdown.value;
        currentVolumeValue = musicSlider.value;
        currentSfxValue = sfxSlider.value;
        currentSensitivityValue = sensitivitySlider.value;
    }

    public void ApplySettings()
    {
        SetFPS();
        SetFOV();
        SetVSync();
        SetVolume();
        SetSfxValue();
        SetSensitivity();
        UpdateCurrentValues();
        onSaveConfig?.Invoke();
    }

}
