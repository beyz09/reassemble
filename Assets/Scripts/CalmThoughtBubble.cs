// CalmThoughtBubble.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CalmThoughtBubble : MonoBehaviour
{
    void Start()
    {
        // Butona tıklandığında OnClick fonksiyonunu çağır
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // RoomManager'a "anksiyeteyi azalt" komutu gönder
        RoomManager.Instance.OnCalmBubbleClicked();
        // Kendini yok et
        Destroy(gameObject);
    }

    // Ekranın dışında kalırsa kendini yok et (isteğe bağlı)
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}