using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUiManager : MonoBehaviour
{
    // ── Background ─────────────────────────────────
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float bgTransitionDuration = 0.5f;

    // ── Dialogue Box ───────────────────────────────
    [Header("Dialogue Box")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject tapToContinueIcon;

    // ── Typewriter ─────────────────────────────────
    [Header("Typewriter Effect")]
    [SerializeField] private float typewriterSpeed = 0.03f;

    // ── Choices ────────────────────────────────────
    [Header("Choice Panel")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

    // ── Ending Screen ──────────────────────────────
    [Header("Ending Screen")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI endingTitleText;
    [SerializeField] private TextMeshProUGUI endingDescriptionText;
    [SerializeField] private Image endingOverlayColor;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    // ── Score Display (opsional) ───────────────────
    [Header("Score Display (Optional)")]
    [SerializeField] private TextMeshProUGUI scoreText;

    // ── State ──────────────────────────────────────
    public bool IsTyping { get; private set; } = false;
    private Coroutine _typewriterCoroutine;
    private string _fullText;
    private List<GameObject> _choiceButtons = new List<GameObject>();

    // Warna ending
    private static readonly Color GoodEndingColor = new Color(0.2f, 0.8f, 0.3f, 0.35f);
    private static readonly Color BadEndingColor = new Color(0.8f, 0.1f, 0.1f, 0.35f);

    // ──────────────────────────────────────────────
    private void Awake()
    {
        if (endingPanel) endingPanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        if (tapToContinueIcon) tapToContinueIcon.SetActive(false);

        if (GameState.Instance != null)
            GameState.Instance.OnScoreChanged += UpdateScoreDisplay;
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnScoreChanged -= UpdateScoreDisplay;
    }

    // ─────────────────────────────────────────────
    //  Background
    // ─────────────────────────────────────────────

    public void SetBackground(Sprite bg)
    {
        if (backgroundImage == null || bg == null) return;
        StartCoroutine(FadeBackground(bg));
    }

    private IEnumerator FadeBackground(Sprite newBg)
    {
        float t = 0;
        Color c = backgroundImage.color;

        // Fade out
        while (t < bgTransitionDuration / 2)
        {
            t += Time.deltaTime;
            backgroundImage.color = Color.Lerp(c, Color.black, t / (bgTransitionDuration / 2));
            yield return null;
        }

        backgroundImage.sprite = newBg;
        t = 0;

        // Fade in
        while (t < bgTransitionDuration / 2)
        {
            t += Time.deltaTime;
            backgroundImage.color = Color.Lerp(Color.black, c, t / (bgTransitionDuration / 2));
            yield return null;
        }

        backgroundImage.color = c;
    }

    // ─────────────────────────────────────────────
    //  Dialogue
    // ─────────────────────────────────────────────

    public void ShowDialogue(string charName, string text)
    {
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (tapToContinueIcon) tapToContinueIcon.SetActive(false);

        bool hasName = !string.IsNullOrEmpty(charName);
        if (namePanel) namePanel.SetActive(hasName);
        if (characterNameText) characterNameText.text = charName;

        _fullText = text;
        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
    }

    private IEnumerator TypewriterEffect(string text)
    {
        IsTyping = true;
        dialogueBodyText.text = "";

        foreach (char c in text)
        {
            dialogueBodyText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        IsTyping = false;
        if (tapToContinueIcon) tapToContinueIcon.SetActive(true);
    }

    /// <summary>Skip typewriter — langsung tampilkan teks penuh.</summary>
    public void SkipTypewriter()
    {
        if (!IsTyping) return;
        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        dialogueBodyText.text = _fullText;
        IsTyping = false;
        if (tapToContinueIcon) tapToContinueIcon.SetActive(true);
    }

    // ─────────────────────────────────────────────
    //  Choices
    // ─────────────────────────────────────────────

    public void ShowChoices(List<DialogueChoice> choices)
    {
        if (choicePanel == null || choiceButtonPrefab == null)
        {
            Debug.LogError("[DialogueUIManager] ChoicePanel atau ChoiceButtonPrefab belum di-assign!");
            return;
        }

        ClearChoiceButtons();
        if (tapToContinueIcon) tapToContinueIcon.SetActive(false);

        // ✅ Disable TapArea agar tidak menghalangi tombol pilihan
        if (StoryController.Instance != null)
            StoryController.Instance.SetTapAreaActive(false);

        choicePanel.SetActive(true);

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i; // capture index untuk lambda
            var choice = choices[i];

            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);
            _choiceButtons.Add(btn);

            // Set teks pilihan
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText) btnText.text = choice.choiceText;

            // Set warna hint: Positive = hijau muda, Negative = merah muda
            var btnImage = btn.GetComponent<Image>();
            if (btnImage)
            {
                btnImage.color = choice.choiceHint == ChoiceHint.Positive
                    ? new Color(0.8f, 1f, 0.8f)
                    : new Color(1f, 0.8f, 0.8f);
            }

            // Assign listener klik
            var button = btn.GetComponent<Button>();
            if (button)
                button.onClick.AddListener(() => DialogueManager.Instance.SelectChoice(index));
            else
                Debug.LogWarning("[DialogueUIManager] ChoiceButton prefab tidak memiliki komponen Button!");
        }
    }

    public void HideChoices()
    {
        if (choicePanel) choicePanel.SetActive(false);
        ClearChoiceButtons();

        // ✅ Enable kembali TapArea setelah pilihan dipilih
        if (StoryController.Instance != null)
            StoryController.Instance.SetTapAreaActive(true);
    }

    private void ClearChoiceButtons()
    {
        foreach (var btn in _choiceButtons)
            if (btn != null) Destroy(btn);
        _choiceButtons.Clear();
    }

    // ─────────────────────────────────────────────
    //  Ending Screen
    // ─────────────────────────────────────────────

    public void ShowEndingScreen(EndingType type, string title, string description)
    {
        if (endingPanel == null) return;

        HideDialoguePanel();
        HideChoices();

        // ✅ Matikan TapArea agar tidak menghalangi tombol ending
        if (StoryController.Instance != null)
            StoryController.Instance.SetTapAreaActive(false);

        endingPanel.SetActive(true);

        if (endingTitleText)
            endingTitleText.text = string.IsNullOrEmpty(title)
                ? GetDefaultTitle(type)
                : title;

        if (endingDescriptionText)
            endingDescriptionText.text = string.IsNullOrEmpty(description)
                ? GetDefaultDescription(type)
                : description;

        if (endingOverlayColor)
            endingOverlayColor.color = type == EndingType.GoodEnding
                ? GoodEndingColor
                : BadEndingColor;

        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => StoryController.Instance.RestartStory());
        }

        if (mainMenuButton)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => StoryController.Instance.GoToMainMenu());
        }
    }

    private void HideDialoguePanel()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (tapToContinueIcon) tapToContinueIcon.SetActive(false);
    }

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText) scoreText.text = $"Poin: {score}";
    }

    private string GetDefaultTitle(EndingType type) =>
        type == EndingType.GoodEnding ? "✅ ENDING BAIK" : "❌ ENDING BURUK";

    private string GetDefaultDescription(EndingType type) =>
        type == EndingType.GoodEnding
            ? "Kamu berhasil membuat pilihan yang tepat! Kamu telah menjadi pelopor anti-cyberbullying."
            : "Pilihan kamu memperburuk situasi. Coba lagi dan buat pilihan yang lebih bijak!";
}