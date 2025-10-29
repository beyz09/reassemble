// ClickableObject.cs
using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    [Header("Object Info")]
    public int itemID; // Bu nesnenin ID'si (0, 1, 2, veya 3)
    public GameObject ghostToActivate; // Bu nesneye tıklandığında hangi hayalet çıkacak?

    [Header("Dialogue & Choice")]
    [TextArea(3, 10)]
    public string[] dialogueLines; // Bu nesnenin hikayesi
    public string choiceQuestion; // Hayaletin soracağı soru

    private bool isCollected = false;

    private void OnMouseDown()
    {
        // Eğer zaten toplandıysa veya oyun başka bir şeyle meşgulse tıklamayı reddet
        if (isCollected || !RoomManager.Instance.CanInteract()) return;

        isCollected = true; // Tekrar tıklanmasın
        
        // RoomManager'a bu nesneyle etkileşimi başlatmasını söyle
        RoomManager.Instance.StartObjectInteraction(this);
    }
}