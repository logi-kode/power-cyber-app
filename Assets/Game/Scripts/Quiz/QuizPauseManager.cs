using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizPauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button keluarButton;

    [Header("Scene Names")]
    [SerializeField] private string quizScene = "QuizScene";
    [SerializeField] private string menuUtamaScene = "MainMenu";

    private bool _isPaused = false;

    private void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);

        if (pauseButton) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (keluarButton) keluarButton.onClick.AddListener(Keluar);
    }

    private void OnDestroy()
    {
        if (pauseButton) pauseButton.onClick.RemoveListener(TogglePause);
        if (resumeButton) resumeButton.onClick.RemoveListener(Resume);
        if (restartButton) restartButton.onClick.RemoveListener(Restart);
        if (keluarButton) keluarButton.onClick.RemoveListener(Keluar);
    }

    public void TogglePause()
    {
        if (_isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(quizScene);
    }

    private void Keluar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuUtamaScene);
    }

    public bool IsPaused => _isPaused;
}
