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
    
    // GameManager referansı (Süre bittiğinde GameManager'a haber vermek için)
    public GameManager gameManager; 

    void Start()
    {
        // Kalan süreyi başlangıç değerine ayarla
        kalanSure = baslangicSuresi;
        
        // GameManager'ı otomatik bul (Eğer atanmamışsa)
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void Update()
    {
        // Eğer kalan süre sıfırdan büyükse
        if (kalanSure > 0)
        {
            // Süreyi azalt (Time.deltaTime: son frameden bu yana geçen zaman)
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
        Debug.Log("Süre Bitti! Oyun Kaybedildi.");
        
        // Eğer GameManager varsa, Kaybetme/Süre Bitti fonksiyonunu çağır
        if (gameManager != null)
        {
             // Örneğin GameManager'a bir Kaybettin fonksiyonu ekleyebilirsiniz
             // gameManager.OyunKaybedildi(); 
        }
        
        // Zamanı durdur (Oyunu dondurmak için)
        Time.timeScale = 0f; 
    }
}