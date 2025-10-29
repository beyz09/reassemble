using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMesh Pro kullanıyorsanız bu gereklidir

public class GeriSayimSayaci : MonoBehaviour
{
    // Inspector'dan ayarlanabilir başlangıç süresi (saniye)
    public float baslangicSuresi = 60f;

    // Şu anki kalan süre
    private float kalanSure;

    // Inspector'dan bağlanacak TextMeshPro bileşeni
    public TextMeshProUGUI sureGostergeText;
    
    // GameManager referansı (Oyunun bitişini yönetmek için)
    private GameManager gameManager; // public yerine private yapıp Start'ta atamak daha güvenli

    void Start()
    {
        // Kalan süreyi başlangıç değerine ayarla
        kalanSure = baslangicSuresi;
        
        // GameManager'ı sahnede bul ve referansı al
        gameManager = FindObjectOfType<GameManager>();

        // Başlangıç süresini göster
        sureGostergeText.text = FormatSure(kalanSure);
    }

    void Update()
    {
        // Time.timeScale 0 ise Update çalışır, ancak Time.deltaTime 0 olur.
        // Ancak GameManager oyunu durdurduğunda bu döngünün de durmasını istiyoruz.
        
        // Süre sıfırdan büyük olduğu ve oyun durdurulmadığı sürece devam et
        // Oyun durdurulduğunda (Time.timeScale=0) Time.deltaTime da 0 olacağı için bu zaten durur.
        if (kalanSure > 0)
        {
            // Süreyi azalt (Time.deltaTime, Time.timeScale 0 ise 0'dır)
            kalanSure -= Time.deltaTime;

            // Sürenin sıfırın altına düşmesini engelle
            kalanSure = Mathf.Max(0, kalanSure); 

            // Süreyi ekranda göster
            sureGostergeText.text = FormatSure(kalanSure);

            // Kalan süre sıfır olduysa
            if (kalanSure <= 0)
            {
                ZamanBitti();
            }
        }
    }

    // Süreyi dakika:saniye formatına çeviren yardımcı fonksiyon
    private string FormatSure(float sure)
    {
        int saniye = Mathf.FloorToInt(sure);
        int dakikalar = saniye / 60;
        int saniyeler = saniye % 60;

        return string.Format("{0:00}:{1:00}", dakikalar, saniyeler);
    }

    void ZamanBitti()
    {
        // GameManager'a sürenin bittiğini ve oyunu kaybetme durumunun olduğunu bildir
        if (gameManager != null)
        {
            gameManager.OyunKaybedildi();
        }
    }
}