using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuButtons : MonoBehaviour
{
    public void NewGame()
    {
        GameManager.instance.SceneNewGame();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Load()
    {
        GameManager.instance.LoadScene();
    }
}
