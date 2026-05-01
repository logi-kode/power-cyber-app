using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayStory()
    {
        SceneManager.LoadScene("MainStory");
    }

    public void Materi()
    {
        SceneManager.LoadScene("MateriScene");
    }

    public void Quiz()
    {
        SceneManager.LoadScene("QuizScene");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
