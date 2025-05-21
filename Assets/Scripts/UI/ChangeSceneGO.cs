using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneGO : MonoBehaviour
{

    public void ChangeScene()
    {
        SceneManager.LoadScene("InitialScene");
    }
}
