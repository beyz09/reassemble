// DraggableItem.cs
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private CatGameManager gameManager;
    private Collider2D col; // Collider'ı hafızada tutmak için

    private void Start()
    {
        // GameManager'ı bul
        gameManager = FindObjectOfType<CatGameManager>();
        // Orijinal pozisyonu kaydet
        startPosition = transform.position; 
        // Bu objenin Collider'ını bul ve hafızaya al
        col = GetComponent<Collider2D>(); 
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.nearClipPlane + 10f; // Kamera mesafesi
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDown()
    {
        // Tıkladığımızda objenin merkezi ile farenin pozisyonu arasındaki farkı kaydet
        offset = transform.position - GetMouseWorldPos();
        // Sürüklemeye başladığımızı GameManager'a bildir
        gameManager.StartDragging(this);
        
        // ÖNEMLİ: Raycast'in arkadaki objeyi görmesi için collider'ı kapat
        col.enabled = false; 
    }

    private void OnMouseDrag()
    {
        // Sürüklerken objeyi fare pozisyonuna taşı
        transform.position = GetMouseWorldPos() + offset;
    }

    private void OnMouseUp()
    {
        // Fareyi bıraktığımızda GameManager'a bildir
        gameManager.StopDragging(this);
        
        // ÖNEMLİ: Collider'ı tekrar aç ki bir sonraki tıklamayı algılasın
        col.enabled = true; 
    }

    // Objenin orijinal pozisyonuna dönmesini sağlayan fonksiyon
    // Bu, GameManager tarafından çağrılır
    public void SnapToStart()
    {
        transform.position = startPosition;
    }
}