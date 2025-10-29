using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; // Sahneyi yeniden başlatmak için gerekli

public class GameManager : MonoBehaviour
{
    // OYUN İSTATİSTİKLERİ
    public int hedefFarkSayisi = 7;
    private int bulunanFarkSayisi = 0;
    private bool oyunBitti = false; 

    // UI BİLEŞENLERİ
    public TextMeshProUGUI farkSayaciText;
    public GameObject OyunBittiPaneli;       // Bitiş ekranı paneli
    public TextMeshProUGUI SonucYazisiText;  // Kazanma/Kaybetme metni

    void Start()
    {
        // Oyun başladığında zamanı normal hıza (1f) ayarla
        Time.timeScale = 1f;
        
        // Başlangıçta paneli gizle
        if (OyunBittiPaneli != null)
        {
            OyunBittiPaneli.SetActive(false); 
        }

        // Fark sayacını başlat
        if (farkSayaciText != null)
        {
            farkSayaciText.text = "0 / " + hedefFarkSayisi;
        }
    }

    // Fark objeleri bu metodu çağıracak
    public void FarkBulundu()
    {
        if (oyunBitti) return;

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

    // HEM KAZANMA HEM KAYBETMEDE ORTAK İŞLEM (Zamanı Durdur ve Paneli Göster)
    public void OyunuDurdur()
    {
        if (oyunBitti) return;

        oyunBitti = true;
        Time.timeScale = 0f; // Oyun zamanını durdur!
        
        // PANELİ GÖSTER!
        if (OyunBittiPaneli != null)
        {
            OyunBittiPaneli.SetActive(true);
        }
    }

    private void OyunKazanildi()
    {
        OyunuDurdur();
        Debug.Log("Tebrikler! Tüm farkları buldunuz! (KAZANDINIZ)");
        
        // KAZANMA MESAJINI AYARLA
        if (SonucYazisiText != null)
        {
            SonucYazisiText.text = "TEBRİKLER!\nTüm Farkları Buldunuz!";
        }
    }

    // GeriSayimSayaci bu metodu çağırır.
    public void OyunKaybedildi()
    {
        OyunuDurdur();
        Debug.Log("Süre Bitti! (KAYBETTİNİZ)");

        // KAYBETME MESAJINI AYARLA
        if (SonucYazisiText != null)
        {
            SonucYazisiText.text = "SÜRE BİTTİ!\nTekrar Dene.";
        }
    }

    // YENİDEN BAŞLAT BUTONUNA BAĞLANACAK METOT
    public void YenidenBaslat()
    {
        // 1. Oyun zamanını tekrar normale çevir
        Time.timeScale = 1f;

        // 2. Mevcut sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}