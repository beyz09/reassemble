using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
public class GameManager : MonoBehaviour
{
    public int hedefFarkSayisi = 7; 
    private int bulunanFarkSayisi = 0;

    // Inspector'dan bağlanacak UI Text'i
    public TextMeshProUGUI farkSayaciText;

    void Start()
    {
        // Başlangıçta sayacı ekranda göster
        if (farkSayaciText != null)
        {
            farkSayaciText.text = "0 / " + hedefFarkSayisi;
        }
    }

    // Fark objeleri bu metodu çağıracak
    public void FarkBulundu()
    {
        bulunanFarkSayisi++;
        
        // Sayacı güncelle
        if (farkSayaciText != null)
        {
            farkSayaciText.text = bulunanFarkSayisi + " / " + hedefFarkSayisi;
        }
        
        Debug.Log("Bulunan fark sayısı: " + bulunanFarkSayisi);

        if (bulunanFarkSayisi >= hedefFarkSayisi)
        {
            OyunKazanildi();
        }
    }

    private void OyunKazanildi()
    {
        Debug.Log("Tebrikler! Tüm farkları buldunuz!");
        // Oyun bitti mantığı buraya gelir
    }
}
