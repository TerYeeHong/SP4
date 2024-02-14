// Made by: Matt Palero
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgt : MonoBehaviour
{

    public void ChangeScene(string Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
