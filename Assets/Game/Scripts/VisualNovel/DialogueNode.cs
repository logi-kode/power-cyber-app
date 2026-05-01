using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Merepresentasikan satu node dialog dalam story tree.
/// Setiap node bisa berupa monolog, dialog dengan pilihan, atau ending.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Visual Novel/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Node Identity")]
    [Tooltip("ID unik untuk node ini. Digunakan untuk referensi antar node.")]
    public string nodeID;

    [Header("Character Info")]
    [Tooltip("Nama karakter yang berbicara. Kosongkan untuk narasi.")]
    public string characterName;

    [Header("Dialogue Content")]
    [Tooltip("Teks dialog yang akan ditampilkan.")]
    [TextArea(3, 6)]
    public string dialogueText;

    [Tooltip("Background scene untuk node ini. Kosongkan jika tidak berubah.")]
    public Sprite backgroundSprite;

    [Tooltip("Audio clip untuk background music. Kosongkan jika tidak berubah.")]
    public AudioClip backgroundMusic;

    [Tooltip("Sound effect saat dialog ini muncul.")]
    public AudioClip dialogueSFX;

    [Header("Node Type")]
    [Tooltip("Tipe node ini.")]
    public NodeType nodeType = NodeType.Dialogue;

    [Header("Branching Choices")]
    [Tooltip("Pilihan yang tersedia untuk pemain. Kosongkan jika node lanjut otomatis.")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Tooltip("Node selanjutnya jika tidak ada pilihan (auto-continue).")]
    public DialogueNode nextNode;

    [Header("Ending Configuration")]
    [Tooltip("Hanya diisi jika NodeType = Ending.")]
    public EndingType endingType = EndingType.None;

    [Tooltip("Teks/judul ending yang ditampilkan.")]
    public string endingTitle;

    [TextArea(2, 4)]
    [Tooltip("Deskripsi ending.")]
    public string endingDescription;

    [Header("Score & Flags")]
    [Tooltip("Poin yang ditambahkan saat node ini dijalankan. Bisa negatif.")]
    public int scoreValue = 0;

    [Tooltip("Flag yang di-set saat node ini dijalankan (misal: 'met_victim').")]
    public List<string> setFlags = new List<string>();
}

// ─────────────────────────────────────────────
//  Supporting Data Structures
// ─────────────────────────────────────────────

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("Teks pilihan yang ditampilkan ke pemain.")]
    public string choiceText;

    [Tooltip("Node yang dituju setelah pilihan ini dipilih.")]
    public DialogueNode targetNode;

    [Tooltip("Poin yang ditambahkan/dikurangi saat pilihan ini dipilih.")]
    public int scoreModifier = 0;

    [Tooltip("Flag yang diperlukan agar pilihan ini muncul. Kosongkan = selalu muncul.")]
    public List<string> requiredFlags = new List<string>();

    [Tooltip("Flag yang di-set saat pilihan ini dipilih.")]
    public List<string> setFlags = new List<string>();

    [Tooltip("Warna hint pilihan: Positive = Hijau, Negative = Merah.")]
    public ChoiceHint choiceHint = ChoiceHint.Positive;
}

public enum NodeType
{
    Dialogue,   // Dialog biasa atau dengan pilihan
    Narration,  // Narasi tanpa karakter
    Ending      // Node akhir (Good/Bad)
}

public enum EndingType
{
    None,
    GoodEnding,
    BadEnding
}

public enum ChoiceHint
{
    Positive,   // Hijau - mengarah ke good ending
    Negative    // Merah - mengarah ke bad ending
}