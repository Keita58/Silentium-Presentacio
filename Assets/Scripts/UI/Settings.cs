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

    [Header("General")]
    //booleano que controla si es la escena inicial o no.
    public bool isInitialScene;
    //Variables temporales para guardar el valor actual
    [SerializeField] public float currentVolumeValue;
    [SerializeField] public float currentSfxValue;
    [SerializeField] public int currentFpsValue = 0;
    [SerializeField] public int currentVSyncState = 0;
    [SerializeField] public float currentFOVValue = 60;

    public event Action onSaveConfig;

    //Cuando el usuario abre el menu de opciones se ponen los valores de los componentes seg�n el valor de las variables temporales
    //si no se ha modificado nada, pues se quedan los valores m�nimos, si no se ponen los valores guardados anteriormente.
    private void OnEnable()
    {
        StartOptions();
    }

    public void StartOptions()
    {
        fovSlider.value = currentFOVValue;
        vSyncToggle.isOn = currentVSyncState == 1 ? true:false;
        fpsDropdown.value = currentFpsValue;

        ApplySettings();
    }

    public void SetVolume()
    {
        audioMixer.SetFloat("Volume", musicSlider.value);
    }

    public void SetSfxValue()
    {
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
    }

    public void ApplySettings()
    {
        SetFPS();
        SetFOV();
        SetVSync();
        UpdateCurrentValues();
        //SetVolume();
        //SetSfxValue();
        onSaveConfig?.Invoke();
    }

}
