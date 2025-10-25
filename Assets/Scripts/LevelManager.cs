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
}
