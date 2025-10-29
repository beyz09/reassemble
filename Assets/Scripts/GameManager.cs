using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // OYUN İSTATİSTİKLERİ
    public int hedefFarkSayisi = 7;
    private int bulunanFarkSayisi = 0;
    private bool oyunBitti = false; 

    // UI BİLEŞENLERİ
    public TextMeshProUGUI farkSayaciText;
    public GameObject OyunBittiPaneli; 
    public TextMeshProUGUI SonucYazisiText; 

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button dialogueNextButton;

    [Header("Dialogues")]
    public string[] introDialogueLines;
    public string[] outroDialogueLines;

    // YENİ EKLENEN: Müzik Yöneticisi Referansı
    private MuzikYoneticisi muzikYoneticisi; 
    private int currentDialogueLine;
    private string[] activeDialogue;
    private System.Action onDialogueEnd;

    void Start()
    {
        // Oyun başladığında zamanı normal hıza (1f) ayarla
        Time.timeScale = 1f;
        
        // YENİ EKLENEN: Sahnede bulunan MuzikYoneticisi'ni bul
        // MuzikYoneticisi DontDestroyOnLoad ile korunur, bu yüzden her zaman bulunur.
        muzikYoneticisi = FindObjectOfType<MuzikYoneticisi>();
        
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

        dialogueNextButton.onClick.AddListener(OnDialogueNext);

        if (introDialogueLines != null && introDialogueLines.Length > 0)
        {
            StartDialogue(introDialogueLines, () => { Time.timeScale = 1f; });
            Time.timeScale = 0f; // Diyalog sırasında oyun dursun
        }
    }

    private void OnDialogueNext()
    {
        currentDialogueLine++;
        if (activeDialogue != null && currentDialogueLine < activeDialogue.Length)
        {
            dialogueText.text = activeDialogue[currentDialogueLine];
        }
        else
        {
            dialoguePanel.SetActive(false);
            activeDialogue = null;
            onDialogueEnd?.Invoke(); // Diyalog bitince ne olacağını çalıştır
        }
    }

    private void StartDialogue(string[] lines, System.Action onEndAction)
    {
        if (lines == null || lines.Length == 0)
        {
            onEndAction?.Invoke();
            return;
        }

        dialoguePanel.SetActive(true);
        activeDialogue = lines;
        currentDialogueLine = 0;
        dialogueText.text = activeDialogue[currentDialogueLine];
        onDialogueEnd = onEndAction;
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
        oyunBitti = true;
        Debug.Log("Tebrikler! Tüm farkları buldunuz! (KAZANDINIZ)");

        StartDialogue(outroDialogueLines, () => {
            OyunuDurdur();
            if (SonucYazisiText != null)
            {
                SonucYazisiText.text = "TEBRİKLER!\nTüm Farkları Buldunuz!";
            }
        });
    }

    // GeriSayimSayaci bu metodu çağırır.
    public void OyunKaybedildi()
    {
        oyunBitti = true;
        Debug.Log("Süre Bitti! (KAYBETTİNİZ)");

        StartDialogue(outroDialogueLines, () => {
            OyunuDurdur();
            if (SonucYazisiText != null)
            {
                SonucYazisiText.text = "SÜRE BİTTİ!\nTekrar Dene.";
            }
        });
    }

    // YENİDEN BAŞLAT BUTONUNA BAĞLANACAK METOT (Aynı sahneyi yeniden yükler)
    public void YenidenBaslat()
    {
        // 1. Oyun zamanını tekrar normale çevir
        Time.timeScale = 1f;

        // 2. Mevcut sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // YENİ EKLENEN: SONUÇ EKRANINA GEÇİŞ VE MÜZİK DEĞİŞTİRME METODU
    // Bu metot, Son Sahneye geçiş butonuna (Eğer varsa) bağlanacaktır.
    public void SonucEkraninaGec(string sahneAdi) 
    {
        // 1. Müzik Değişikliğini Yap (Önce Müziği Değiştiriyoruz)
        if (muzikYoneticisi != null)
        {
            muzikYoneticisi.SonSahneyeGecisMuzik();
        }
        
        // 2. Sahne Geçişini Yap (Sonra sahneye geçiyoruz)
        Time.timeScale = 1f; // Zamanı normale çevirmezseniz sahne geçişi donabilir
        SceneManager.LoadScene(sahneAdi); 
    }
}