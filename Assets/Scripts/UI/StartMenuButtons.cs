using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuButtons : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("MapaHector");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Load()
    {
        //Codigo load.
    }
}
