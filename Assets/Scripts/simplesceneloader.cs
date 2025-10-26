using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SimpleSceneLoader : MonoBehaviour
{
    // Unity'nin her döngüde kontrol ettiği metot
    void Update()
    {
        // Ekrana tıklandığında (sol fare tuşu) VEYA herhangi bir tuşa basıldığında
        if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
        {
            // Bir sonraki sahneyi yükleme fonksiyonunu çağır
            LoadNextScene();
        }
    }

    // Bir sonraki sahneyi yükleyen fonksiyon
    void LoadNextScene()
    {
        // 1. Şu anki sahnenin Build Index numarasını al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // 2. Bir sonraki sahnenin Index numarasını hesapla
        int nextSceneIndex = currentSceneIndex + 1;

        // 3. Sahne sayısını kontrol et (Oyunun bitip bitmediğini görmek için)
        // Build Settings'teki toplam sahne sayısını al
        int totalSceneCount = SceneManager.sceneCountInBuildSettings;

        // Eğer bir sonraki index toplam sahne sayısına eşitse (son sahneden sonraki index),
        // oyun bitmiştir veya menüye dönülmelidir.
        if (nextSceneIndex < totalSceneCount)
        {
            // Geçiş yapılacak sahne varsa, yükle
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // OPSİYONEL: Son sahneden sonra ilk sahneye (Ana Menü, Index 0) dön
            SceneManager.LoadScene(0);
        }
    }
}