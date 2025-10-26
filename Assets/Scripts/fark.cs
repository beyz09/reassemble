using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Fark : MonoBehaviour
{
    public GameObject isaretObjesi;
    public GameManager gameManager; 
    
    // Tik işaretinin ekranda kalacağı süre (örneğin 1 saniye)
    public float gorunurlukSuresi = 1f; 

    private bool bulundu = false;
    
    // ... Start() metodu burada kalmalı ...

    private void OnMouseDown()
    {
        if (!bulundu)
        {
            bulundu = true;
            
            // 1. Tik işaretini gösteren Coroutine'i başlat
            StartCoroutine(TikiGosterVeKapat()); 
            
            // 2. Collider'ı kapat
            GetComponent<Collider2D>().enabled = false;

            // 3. GameManager'a haber ver
            if (gameManager != null)
            {
                gameManager.FarkBulundu(); 
            }
        }
    }

    // YENİ EKLEYECEĞİNİZ FONKSİYON: Tik'i gösterir, bekler ve kapatır
    IEnumerator TikiGosterVeKapat()
    {
        // 1. Tik işaretini aktif et (göster)
        if (isaretObjesi != null)
        {
            isaretObjesi.SetActive(true);
        }

        // 2. Belirlenen süre kadar bekle
        yield return new WaitForSeconds(gorunurlukSuresi);

        // 3. Tik işaretini kapat
        if (isaretObjesi != null)
        {
            isaretObjesi.SetActive(false);
        }

        // NOT: Eğer tik işaretinin bulunduğu yerdeki farkı tekrar bulmak istemiyorsanız, 
        // yukarıdaki bulundu = true ve Collider kapatma mantığı yeterlidir.
    }
}