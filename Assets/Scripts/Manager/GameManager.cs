using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance { get; private set; }

    public event Action onLoadedScene;
    public event Action onNewScene;

    private bool _Load;
    
    private void Awake()
    {
        if (instance==null)
            instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        _Load = false;
    }

    public void SceneNewGame()
    {
        SceneManager.LoadScene("MapaHector");
        _Load = false;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("Tasca22");
        _Load = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        switch (scene.name)
        {
            case "Tasca22":
                //Posar aquí pantalla de càrrega
                if (_Load)
                {
                    Debug.Log("Invoke");
                    onLoadedScene?.Invoke();
                }
                else
                    onNewScene?.Invoke();
                break;
        }
    }
}
