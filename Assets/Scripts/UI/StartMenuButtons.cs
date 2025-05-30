using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuButtons : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";
    private const string _TemporalSavefileName = "silentium_temp_savegame.json";
    [SerializeField] private Button loadButton;

    private void Start()
    {
        string temporalExistingFile = Application.persistentDataPath + "/" + _TemporalSavefileName;
        string existingFile = Application.persistentDataPath + "/" + _SavefileName;
        
        loadButton.interactable = File.Exists(temporalExistingFile) || File.Exists(existingFile);
    }

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
