using System;
using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panelParent;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Player player;
    [SerializeField] private Events events;
    [SerializeField] private GameObject waves;
    [SerializeField] private TextMeshProUGUI ammoQuantity;
    [SerializeField] private GameObject pointer;

    public void OpenMenu()
    {
        if (optionsPanel.activeSelf && panelParent.activeSelf)
        {
            optionsPanel.SetActive(false);
        }else if (!optionsPanel.activeSelf && panelParent.activeSelf)
        {
            Time.timeScale = 1;
            player.ToggleInputPlayer(true, true);
            player.ResumeInteract(true);
            panelParent.SetActive(false);
        }
        else
        {
           panelParent.SetActive(true);
           player.ToggleInputPlayer(false, false);
           player.ResumeInteract(false);
        }
    }

    private void Start()
    {
        events.OnToggleUI += ToggleUI;
        player.OnAmmoChange += ChangeNumAmmo;
    }

    private void OnDestroy()
    {
        events.OnToggleUI -= ToggleUI;
        player.OnAmmoChange -= ChangeNumAmmo;
    }

    private void ToggleUI(bool active)
    {
        if (waves != null)
            waves.SetActive(active);
        if (ammoQuantity != null)
            ammoQuantity.gameObject.SetActive(active);
        if(pointer != null)
            pointer.SetActive(active);
    }

    private void ChangeNumAmmo(int ammo)
    {
        ammoQuantity.text = ammo.ToString();
    }
}
