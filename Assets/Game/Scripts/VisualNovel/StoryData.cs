using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewStory", menuName = "Visual Novel/Story Data")]
public class StoryData : ScriptableObject
{
    [Header("Story Info")]
    public string storyID;
    public string storyTitle;

    [TextArea(2, 4)]
    public string storySynopsis;

    [Header("Entry Point")]
    [Tooltip("Node pertama saat story dimulai.")]
    public DialogueNode startNode;

    [Header("Ending Nodes")]
    [Tooltip("Node yang dituju jika skor >= goodEndingThreshold.")]
    public DialogueNode goodEndingNode;

    [Tooltip("Node yang dituju jika skor < goodEndingThreshold.")]
    public DialogueNode badEndingNode;

    [Header("Score Threshold")]
    [Tooltip("Skor minimum untuk mendapatkan Good Ending.")]
    public int goodEndingThreshold = 60;

    [Header("All Nodes (Referensi)")]
    [Tooltip("Opsional: daftar semua node dalam story ini untuk kemudahan tracking.")]
    public List<DialogueNode> allNodes = new List<DialogueNode>();
}