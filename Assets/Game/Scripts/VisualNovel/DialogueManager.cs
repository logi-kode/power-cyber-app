using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUiManager uiManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Settings")]
    [Tooltip("Delay sebelum sistem siap menerima input setelah typewriter selesai.")]
    [SerializeField] private float autoContinueDelay = 0.3f;

 
    private StoryData _currentStory;
    private DialogueNode _currentNode;
    private bool _isProcessing = false;
    private bool _waitingForInput = false;


    public System.Action<DialogueNode> OnNodeChanged;
    public System.Action<EndingType> OnStoryEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogError("[DialogueManager] StoryData null! Pastikan Story To Play sudah di-assign di StoryController.");
            return;
        }

        _currentStory = story;
        GameState.Instance.ResetAll();
        LoadNode(story.startNode);
    }

    public void OnScreenTapped()
    {
        if (_isProcessing) return;

        if (uiManager.IsTyping)
        {
            uiManager.SkipTypewriter();
            return;
        }

        if (_waitingForInput && _currentNode != null)
        {
            bool hasChoices = _currentNode.choices != null && _currentNode.choices.Count > 0;
            if (!hasChoices)
            {
                AdvanceToNextNode();
            }
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (_currentNode == null || choiceIndex >= _currentNode.choices.Count) return;

        var choice = _currentNode.choices[choiceIndex];

        if (choice.scoreModifier != 0)
            GameState.Instance.AddScore(choice.scoreModifier);

        GameState.Instance.SetFlags(choice.setFlags);

        GameState.Instance.RecordChoice(choice.choiceText);

        uiManager.HideChoices();

        if (choice.targetNode != null)
            LoadNode(choice.targetNode);
        else
            Debug.LogWarning($"[DialogueManager] Choice '{choice.choiceText}' tidak memiliki target node!");
    }

    private void AdvanceToNextNode()
    {
        if (_currentNode.nextNode != null)
        {
            LoadNode(_currentNode.nextNode);
        }
        else
        {
            GoToEnding();
        }
    }

    private void GoToEnding()
    {
        if (_currentStory == null) return;

        EndingType endingType = GameState.Instance.DetermineEnding(_currentStory);

        DialogueNode endingNode = endingType == EndingType.GoodEnding
            ? _currentStory.goodEndingNode
            : _currentStory.badEndingNode;

        if (endingNode != null)
        {
            Debug.Log($"[DialogueManager] Menuju ending: {endingType} (Skor: {GameState.Instance.CurrentScore})");
            LoadNode(endingNode);
        }
        else
        {
            Debug.LogError($"[DialogueManager] Ending node untuk {endingType} belum di-assign di StoryData!");
        }
    }

    private void LoadNode(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogError("[DialogueManager] Tried to load null node.");
            return;
        }

        _currentNode = node;
        _isProcessing = true;
        _waitingForInput = false;

        if (node.scoreValue != 0)
            GameState.Instance.AddScore(node.scoreValue);

        GameState.Instance.SetFlags(node.setFlags);

        if (audioManager != null)
        {
            if (node.backgroundMusic != null) audioManager.PlayMusic(node.backgroundMusic);
            if (node.dialogueSFX != null) audioManager.PlaySFX(node.dialogueSFX);
        }

        OnNodeChanged?.Invoke(node);

        switch (node.nodeType)
        {
            case NodeType.Ending:
                StartCoroutine(HandleEndingNode(node));
                break;
            case NodeType.Narration:
            case NodeType.Dialogue:
            default:
                StartCoroutine(HandleDialogueNode(node));
                break;
        }
    }

    private IEnumerator HandleDialogueNode(DialogueNode node)
    {
        if (node.backgroundSprite != null)
            uiManager.SetBackground(node.backgroundSprite);

        uiManager.ShowDialogue(node.characterName, node.dialogueText);

        yield return new WaitUntil(() => !uiManager.IsTyping);
        yield return new WaitForSeconds(autoContinueDelay);

        _isProcessing = false;
        _waitingForInput = true;

        if (node.choices != null && node.choices.Count > 0)
        {
            var availableChoices = GetAvailableChoices(node.choices);
            if (availableChoices.Count > 0)
                uiManager.ShowChoices(availableChoices);
            else
                AdvanceToNextNode(); 
        }
    }

    private IEnumerator HandleEndingNode(DialogueNode node)
    {
        if (node.backgroundSprite != null)
            uiManager.SetBackground(node.backgroundSprite);

        if (!string.IsNullOrEmpty(node.dialogueText))
        {
            uiManager.ShowDialogue(node.characterName, node.dialogueText);
            yield return new WaitUntil(() => !uiManager.IsTyping);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        _isProcessing = false;

        EndingType ending = node.endingType;
        OnStoryEnded?.Invoke(ending);

        // Tampilkan ending screen
        uiManager.ShowEndingScreen(ending, node.endingTitle, node.endingDescription);
    }

    private List<DialogueChoice> GetAvailableChoices(List<DialogueChoice> allChoices)
    {
        var available = new List<DialogueChoice>();
        foreach (var choice in allChoices)
        {
            if (GameState.Instance.HasAllFlags(choice.requiredFlags))
                available.Add(choice);
        }
        return available;
    }

    public DialogueNode GetCurrentNode() => _currentNode;
    public StoryData GetCurrentStory() => _currentStory;
}