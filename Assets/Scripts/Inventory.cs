using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public List<ItemSO> items = new List<ItemSO>();

    public event Action<ItemSO> OnItemAdded;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Add(ItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("Inventory: trying to add null ItemSO");
            return;
        }

        // Prevent adding duplicates by id if id is set, otherwise by reference
        if (!string.IsNullOrEmpty(item.id))
        {
            if (Has(item.id))
            {
                Debug.Log("Inventory: item with id '" + item.id + "' is already in inventory");
                return;
            }
        }
        else if (items.Contains(item))
        {
            Debug.Log("Inventory: item is already in inventory: " + item.name);
            return;
        }

        items.Add(item);
        OnItemAdded?.Invoke(item);

        string display = !string.IsNullOrEmpty(item.itemName) ? item.itemName : item.name;
        string idInfo = !string.IsNullOrEmpty(item.id) ? " (id=" + item.id + ")" : "";
        Debug.Log("Inventory: added " + display + idInfo);
    }

    public bool Has(string id) => items.Exists(i => i != null && i.id == id);

    public void Remove(string id)
    {
        var it = items.Find(i => i != null && i.id == id);
        if (it != null) items.Remove(it);
    }
}
