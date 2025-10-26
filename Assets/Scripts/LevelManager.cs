using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages level state: collected items, cleared ghosts, level completion.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Tooltip("How many items must be collected to finish the level")]
    public int totalItems = 5;

    int collectedItems = 0;
    HashSet<string> clearedGhosts = new HashSet<string>();

    public UnityEvent onLevelComplete;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ItemCollected()
    {
        collectedItems++;
        Debug.Log("LevelManager: collected " + collectedItems + " / " + totalItems);
        if (collectedItems >= totalItems)
        {
            Debug.Log("Level complete!");
            onLevelComplete?.Invoke();
        }
    }

    public bool IsGhostCleared(string ghostId)
    {
        if (string.IsNullOrEmpty(ghostId)) return true;
        return clearedGhosts.Contains(ghostId);
    }

    public void ClearGhost(string ghostId)
    {
        if (string.IsNullOrEmpty(ghostId)) return;
        if (!clearedGhosts.Contains(ghostId))
        {
            clearedGhosts.Add(ghostId);
            Debug.Log("LevelManager: cleared ghost " + ghostId);
        }
    }

    /// <summary>
    /// Show only the ghost with the given id (hide other ghost GameObjects).
    /// If a ghost is already cleared it will not be shown.
    /// </summary>
    public void ShowGhostById(string ghostId)
    {
        var all = Object.FindObjectsOfType<Ghost>(true);
        foreach (var g in all)
        {
            if (g == null) continue;
            bool isCleared = IsGhostCleared(g.ghostId);
            if (isCleared)
            {
                // hide cleared ghosts
                g.gameObject.SetActive(false);
                continue;
            }

            if (!string.IsNullOrEmpty(ghostId) && g.ghostId == ghostId)
            {
                g.gameObject.SetActive(true);
            }
            else
            {
                g.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Show all ghost GameObjects that are not cleared.
    /// </summary>
    public void ShowAllGhosts()
    {
        var all = Object.FindObjectsOfType<Ghost>(true);
        foreach (var g in all)
        {
            if (g == null) continue;
            if (!IsGhostCleared(g.ghostId)) g.gameObject.SetActive(true);
            else g.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hide all ghost GameObjects.
    /// </summary>
    public void HideAllGhosts()
    {
        var all = Object.FindObjectsOfType<Ghost>(true);
        foreach (var g in all)
        {
            if (g == null) continue;
            g.gameObject.SetActive(false);
        }
    }
}
