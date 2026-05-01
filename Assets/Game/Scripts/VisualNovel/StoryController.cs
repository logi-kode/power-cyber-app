using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    public static StoryController Instance { get; private set; }

    [Header("Story to Load")]
    [Tooltip("Story yang akan dimainkan saat scene ini diload.")]
    [SerializeField] private StoryData storyToPlay;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string storySceneName = "StoryScene";

    [Header("Tap Area")]
    [Tooltip("Button transparan yang mendeteksi tap layar.")]
    [SerializeField] private Button tapAreaButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameState.Instance == null)
        {
            var go = new GameObject("GameState");
            go.AddComponent<GameState>();
        }

        if (tapAreaButton)
            tapAreaButton.onClick.AddListener(OnTap);

        if (DialogueManager.Instance)
            DialogueManager.Instance.OnStoryEnded += HandleStoryEnded;

        if (storyToPlay != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartStory(storyToPlay);
        }
        else
        {
            Debug.LogError("[StoryController] StoryData atau DialogueManager tidak ditemukan!");
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance)
            DialogueManager.Instance.OnStoryEnded -= HandleStoryEnded;
        if (tapAreaButton)
            tapAreaButton.onClick.RemoveListener(OnTap);
    }

    private void OnTap()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

        if (DialogueManager.Instance)
            DialogueManager.Instance.OnScreenTapped();
    }

    public void SetTapAreaActive(bool active)
    {
        if (tapAreaButton) tapAreaButton.gameObject.SetActive(active);
    }

    private void HandleStoryEnded(EndingType endingType)
    {
        Debug.Log($"[StoryController] Story ended: {endingType} | Skor: {GameState.Instance.CurrentScore}");

        PlayerPrefs.SetInt("LastEndingType", (int)endingType);
        PlayerPrefs.SetInt("LastScore", GameState.Instance.CurrentScore);
        PlayerPrefs.Save();

        SetTapAreaActive(false);
    }

    public void RestartStory()
    {
        SceneManager.LoadScene(storySceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadSpecificStory(StoryData story)
    {
        storyToPlay = story;
        SceneManager.LoadScene(storySceneName);
    }
}