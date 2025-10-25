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
            Debug.Log(displayName + " is blocked by a ghost. You need to deal with it first.");
            // Ideally trigger a UI hint or play a sound
            return;
        }

        Inventory.Instance.Add(item);
        LevelManager.Instance.ItemCollected();
        string display = !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
        Debug.Log("Picked up: " + display + (string.IsNullOrEmpty(item.id) ? "" : " (" + item.id + ")"));
        Destroy(gameObject);
    }
}
