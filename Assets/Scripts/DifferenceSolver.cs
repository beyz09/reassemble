using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections.Generic; // List<int> için gerekli
 // List<int> için gerekli

public class DifferenceSolver : MonoBehaviour
{
    // =========================================================
    //                REFERANS VE DEĞİŞKENLER
    // =========================================================
    
    [Header("Sıralama UI'ı")]
    public GameObject SiralamaPaneli;
    public Image[] PngSlots = new Image[7];
    public TextMeshProUGUI[] SlotMetinleri = new TextMeshProUGUI[7];

    [Header("Görsel Ayarları")]
    // Not: Bu diziye sadece Sprite tipinde PNG'ler atanmalıdır.
    public Sprite[] FarkGorselleri = new Sprite[7]; 
    public Sprite BosSlotGorseli; // Opsiyonel

    [Header("Bulmaca Ayarları")]
    private List<int> bulunanFarkIndeksleri = new List<int>(); 
    public int[] DogruSiralamaCevabi = new int[7]; 

    [Header("Oyun Sonu Ayarları")]
    public string SonrakiSahneAdi = "bitis"; // Sizin verdiğiniz sahne adı
    public GameObject BasarisizMesajPaneli;

    // =========================================================
    //                BAŞLANGIÇ/SIFIRLAMA
    // =========================================================
    void Start()
    {
        // Panelleri başlangıçta pasif yap
        if (SiralamaPaneli != null) {
            SiralamaPaneli.SetActive(false);
        }
        if (BasarisizMesajPaneli != null) {
            BasarisizMesajPaneli.SetActive(false);
        }
        
        // Slotları temiz ve görünmez hale getir
        TemizleSlotlari(true); 
    }

    /// <summary>
    /// Slotların görünürlüğünü kaldırır ve listeyi sıfırlar.
    /// </summary>
    private void TemizleSlotlari(bool baslangic)
    {
        bulunanFarkIndeksleri.Clear();

        for (int i = 0; i < 7; i++)
        {
            if (PngSlots.Length > i && PngSlots[i] != null) // Hata kontrolü
            {
                // Görseli kaldır ve tamamen şeffaf yap
                PngSlots[i].sprite = BosSlotGorseli;
                // Color.clear = (0, 0, 0, 0) tamamen şeffaf yapar.
                PngSlots[i].color = Color.clear; 
            }
            if (SlotMetinleri.Length > i && SlotMetinleri[i] != null) // Hata kontrolü
            {
                // Metinleri temizle
                SlotMetinleri[i].text = "";
            }
        }
        if (!baslangic && BasarisizMesajPaneli != null) {
             BasarisizMesajPaneli.SetActive(false);
        }
    }


    // =========================================================
    //                FARK BULUNDUĞU AN (TETİKLEYİCİ)
    // =========================================================
    
    /// <summary>
    /// Fark bulunduğunda çağrılacak ana metot.
    /// </summary>
    /// <param name="farkIndex">Bulunan farkın FarkGorselleri dizisindeki indeksi (0-6).</param>
    public void FarkBulundu(int farkIndex)
    {
        // Debug.Log($"Fark bulundu: İndeks {farkIndex}. Slot {bulunanFarkIndeksleri.Count}'a ekleniyor."); // Eski Debug

        if (bulunanFarkIndeksleri.Count >= 7) return; // Zaten doluysa bir şey yapma
        
        int slotIndex = bulunanFarkIndeksleri.Count; // Hangi boş slota ekleyeceğimizi bul
        
        // 1. Paneli aç (Eğer kapalıysa)
        if (SiralamaPaneli != null && !SiralamaPaneli.activeSelf) {
            SiralamaPaneli.SetActive(true);
        }
        
        // GÖRSELİ EKLEME VE GÖRÜNÜR YAPMA
        // Koşul kontrolü: Slot var mı VE kaynak görsel var mı?
        if (PngSlots[slotIndex] != null && FarkGorselleri.Length > farkIndex && FarkGorselleri[farkIndex] != null)
        {
            PngSlots[slotIndex].sprite = FarkGorselleri[farkIndex];
            
            // Görünür yapma (Color.white = R:1, G:1, B:1, A:1)
            PngSlots[slotIndex].color = Color.white; 
        }
        else
        {
             // Hata tespiti için eklenen kritik debug mesajı:
             Debug.LogError($"ATAMA BAŞARISIZ! Slot: {PngSlots[slotIndex] == null}. Görsel: {FarkGorselleri[farkIndex] == null}. Lütfen atamaları kontrol edin!");
             return;
        }
        
        // METİNİ EKLE (Sıra Numarası)
        if (SlotMetinleri.Length > slotIndex && SlotMetinleri[slotIndex] != null)
        {
            int suankiSira = slotIndex + 1; 
            SlotMetinleri[slotIndex].text = suankiSira.ToString();
        }
        
        bulunanFarkIndeksleri.Add(farkIndex); // İndeksi listeye ekle

        // Eğer 7 fark da bulunduysa, kontrolü yap
        if (bulunanFarkIndeksleri.Count == 7)
        {
            KontrolEtVeBolumuGec();
        }
    }

    // =========================================================
    //                KONTROL VE GEÇİŞ
    // =========================================================
    
    private void KontrolEtVeBolumuGec()
    {
        bool siraDogruMu = true;

        for (int i = 0; i < 7; i++)
        {
            if (bulunanFarkIndeksleri[i] != DogruSiralamaCevabi[i])
            {
                siraDogruMu = false;
                break;
            }
        }

        if (siraDogruMu)
        {
            Debug.Log("BULMACA TAMAMLANDI!");
            SceneManager.LoadScene(SonrakiSahneAdi);
        }
        else
        {
            Debug.Log("Bulmaca Yanlış! Tekrar Dene.");
            if (BasarisizMesajPaneli != null) {
                BasarisizMesajPaneli.SetActive(true);
            }
        }
    }

    // Tekrar dene butonuna atanacak metot
    public void TemizleVeTekrarDene()
    {
       TemizleSlotlari(false);
    }
    
    // Opsiyonel Sahne Geçişi
    public void SonrakiSahneyeGit()
    {
        if (!string.IsNullOrEmpty(SonrakiSahneAdi))
        {
            SceneManager.LoadScene(SonrakiSahneAdi);
        }
        else
        {
            Debug.LogError("Sonraki Sahne Adı belirlenmedi!");
        }
    }
}