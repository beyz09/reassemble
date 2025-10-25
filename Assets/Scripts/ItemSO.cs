using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
}
