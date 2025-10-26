using UnityEngine;

public class ItemPickup : Interactable
{
    public ItemSO item;
    [Tooltip("If set, this item is blocked until the ghost with this id is cleared.")]
    public string blockedByGhostId;

    public override void Interact()
    {
        if (item == null)
        {
            Debug.LogWarning("ItemPickup has no ItemSO assigned: " + gameObject.name);
            return;
        }

        if (!string.IsNullOrEmpty(blockedByGhostId) && !LevelManager.Instance.IsGhostCleared(blockedByGhostId))
        {
            var hint = displayName + " is blocked by a ghost. You need to deal with it first.";
            // Show hint in dialog UI if present, otherwise log
            var ds = DialogueSystem.Instance;
            if (ds != null) ds.ShowHint(hint, 2.0f);
            else Debug.Log(hint);

            // Reveal only the ghost that blocks this item so the player sees which ghost to deal with
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ShowGhostById(blockedByGhostId);
            }
            return;
        }

        Inventory.Instance.Add(item);
        LevelManager.Instance.ItemCollected();
        string display = !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
        Debug.Log("Picked up: " + display + (string.IsNullOrEmpty(item.id) ? "" : " (" + item.id + ")"));
        Destroy(gameObject);
    }
}
