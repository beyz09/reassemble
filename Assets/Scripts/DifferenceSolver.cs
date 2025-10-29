using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanımı için gerekli
using System.Collections.Generic;

public class DifferenceSolver : MonoBehaviour
{
    // =========================================================
    //                DEĞİŞKEN TANIMLARI
    // =========================================================
    
    [Header("Sıralama UI'ı")]
    public GameObject SiralamaPaneli;
    public Image[] PngSlots = new Image[7];
    public TextMeshProUGUI[] SlotMetinleri = new TextMeshProUGUI[7];

    [Header("Görsel Ayarları")]
    public Sprite[] FarkGorselleri = new Sprite[7]; // Görsel Kaynağı
    public Sprite BosSlotGorseli; // (Opsiyonel) Boş slotlar için şeffaf bir görsel

    [Header("Bulmaca Ayarları")]
    private List<int> bulunanFarkIndeksleri = new List<int>(); 
    public int[] DogruSiralamaCevabi = new int[7]; 

    [Header("Oyun Sonu Ayarları")]
    public string SonrakiSahneAdi = "SonrakiBolum";
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
        
        // Slotları temiz ve şeffaf hale getir (Hazırlık)
        TemizleSlotlari(true); 
    }

    /// <summary>
    /// Slotların başlangıç ayarını yapar ve sıfırlar.
    /// </summary>
    private void TemizleSlotlari(bool baslangic)
    {
        bulunanFarkIndeksleri.Clear();

        for (int i = 0; i < 7; i++)
        {
            if (PngSlots[i] != null)
            {
                // Görseli kaldır
                PngSlots[i].sprite = BosSlotGorseli;
                // Şeffaflık (Alpha) ayarı: Tamamen görünmez yap
                PngSlots[i].color = new Color(1, 1, 1, 0); 
            }
            if (SlotMetinleri[i] != null)
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
    //                FARK BULUNDUĞU AN
    // =========================================================
    
    public void FarkBulundu(int farkIndex)
    {
        // Debug kontrolü: Kodun çağrıldığından emin ol
        Debug.Log($"Fark bulundu: İndeks {farkIndex}. Slot {bulunanFarkIndeksleri.Count}'a ekleniyor.");

        // Eğer tüm slotlar dolmadıysa, görseli ekle
        if (bulunanFarkIndeksleri.Count < 7)
        {
            // 1. Paneli aç (Eğer kapalıysa)
            if (SiralamaPaneli != null && !SiralamaPaneli.activeSelf) {
                SiralamaPaneli.SetActive(true);
            }
            
            int slotIndex = bulunanFarkIndeksleri.Count; // 0'dan 6'ya giden boş slot indeksi
            
            // GÖRSELİ EKLE
            if (PngSlots[slotIndex] != null && FarkGorselleri.Length > farkIndex && FarkGorselleri[farkIndex] != null)
            {
                PngSlots[slotIndex].sprite = FarkGorselleri[farkIndex];
                // Görünür yapma (Alpha 1)
                PngSlots[slotIndex].color = new Color(1, 1, 1, 1); 
            }
            
            // METİNİ EKLE (Sıra Numarası)
            if (SlotMetinleri.Length > slotIndex && SlotMetinleri[slotIndex] != null)
            {
                int suankiSira = slotIndex + 1; 
                SlotMetinleri[slotIndex].text = suankiSira.ToString();
            }
            
            bulunanFarkIndeksleri.Add(farkIndex); // Bulunan farkın indeksini listeye ekle

            // Eğer 7 fark da bulunduysa, kontrolü yap
            if (bulunanFarkIndeksleri.Count == 7)
            {
                KontrolEtVeBolumuGec();
            }
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
            // Otomatik temizlemek yerine, temizleme işlemini BasarisizMesajPaneli'ndeki 
            // "Tekrar Dene" butonuna bağlamak daha iyi bir UX sağlar.
        }
    }

    // Tekrar dene butonuna atanacak metot
    public void TemizleVeTekrarDene()
    {
       TemizleSlotlari(false);
    }
}