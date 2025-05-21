using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] GameObject panelParent;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Player player;
    [SerializeField] GameObject waves;
    [SerializeField] TextMeshProUGUI ammoQuantity;

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
        player.OnToggleUI += ToggleUI;
        player.OnAmmoChange += ChangeNumAmmo;
    }

    private void ToggleUI(bool active)
    {
       waves.SetActive(active);
       ammoQuantity.gameObject.SetActive(active);
    }

    private void ChangeNumAmmo(int ammo)
    {
        ammoQuantity.text = ammo.ToString();
    }
}
