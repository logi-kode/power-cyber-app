using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton yang menyimpan state game saat ini:
/// score pemain, flags yang aktif, dan riwayat pilihan.
/// Persists selama satu sesi bermain (tidak dihapus saat ganti scene).
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    // ── Score ──────────────────────────────────────
    public int CurrentScore { get; private set; } = 0;

    // ── Flags ──────────────────────────────────────
    private HashSet<string> _activeFlags = new HashSet<string>();

    // ── Choice History ─────────────────────────────
    public List<string> ChoiceHistory { get; private set; } = new List<string>();

    // ── Events ─────────────────────────────────────
    public System.Action<int> OnScoreChanged;

    // ──────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Score Methods ──────────────────────────────

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"[GameState] Score: {CurrentScore} (+{amount})");
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    // ── Flag Methods ───────────────────────────────

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            _activeFlags.Add(flag);
            Debug.Log($"[GameState] Flag set: {flag}");
        }
    }

    public void SetFlags(List<string> flags)
    {
        if (flags == null) return;
        foreach (var flag in flags)
            SetFlag(flag);
    }

    public bool HasFlag(string flag) => _activeFlags.Contains(flag);

    /// <summary>Cek apakah semua flag dalam list aktif.</summary>
    public bool HasAllFlags(List<string> flags)
    {
        if (flags == null || flags.Count == 0) return true;
        foreach (var flag in flags)
            if (!HasFlag(flag)) return false;
        return true;
    }

    public void ClearFlag(string flag) => _activeFlags.Remove(flag);
    public void ClearAllFlags() => _activeFlags.Clear();

    // ── History ────────────────────────────────────

    public void RecordChoice(string choiceText)
    {
        ChoiceHistory.Add(choiceText);
    }

    // ── Full Reset ─────────────────────────────────

    public void ResetAll()
    {
        ResetScore();
        ClearAllFlags();
        ChoiceHistory.Clear();
        Debug.Log("[GameState] Full reset done.");
    }

    // ── Ending Determination ───────────────────────

    /// <summary>
    /// Tentukan ending berdasarkan skor.
    /// Hanya ada 2 ending: Good atau Bad.
    /// </summary>
    public EndingType DetermineEnding(StoryData story)
    {
        if (CurrentScore >= story.goodEndingThreshold)
            return EndingType.GoodEnding;
        return EndingType.BadEnding;
    }
}