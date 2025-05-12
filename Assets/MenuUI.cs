using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] GameObject panelParent;


    public void OpenMenu()
    {
        panelParent.SetActive(true);
    }
}
