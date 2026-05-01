using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButtonPause;
    [SerializeField] private Button mainMenuButtonPause;

    private bool _isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (restartButtonPause) restartButtonPause.onClick.AddListener(Restart);
        if (mainMenuButtonPause) mainMenuButtonPause.onClick.AddListener(GoToMainMenu);
    }

    private void OnDestroy()
    {
        if (pauseButton) pauseButton.onClick.RemoveListener(TogglePause);
        if (resumeButton) resumeButton.onClick.RemoveListener(Resume);
        if (restartButtonPause) restartButtonPause.onClick.RemoveListener(Restart);
        if (mainMenuButtonPause) mainMenuButtonPause.onClick.RemoveListener(GoToMainMenu);
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

        // Disable TapArea agar tidak bisa tap saat pause
        if (StoryController.Instance != null)
            StoryController.Instance.SetTapAreaActive(false);

        Debug.Log("[PauseManager] Game paused.");
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);

        if (StoryController.Instance != null)
            StoryController.Instance.SetTapAreaActive(true);

        Debug.Log("[PauseManager] Game resumed.");
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        StoryController.Instance.RestartStory();
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        StoryController.Instance.GoToMainMenu();
    }

    public bool IsPaused => _isPaused;
}
