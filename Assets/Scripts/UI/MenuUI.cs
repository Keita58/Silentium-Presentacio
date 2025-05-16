using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] GameObject panelParent;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Player player;

    public void OpenMenu()
    {
        if (optionsPanel.activeSelf && panelParent.activeSelf)
        {
            optionsPanel.SetActive(false);
        }else if (!optionsPanel.activeSelf && panelParent.activeSelf)
        {
            Time.timeScale = 1;
            player.ToggleInputPlayer(true, true);
            player.ResumeInteract(true  );
            panelParent.SetActive(false);
        }
        else
        {
           panelParent.SetActive(true);
           player.ToggleInputPlayer(false, false);
           player.ResumeInteract(false);
        }
    }
}
