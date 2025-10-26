using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    // Bu fonksiyon, bir UI Butonuna tıklandığında çağrılacaktır.
    public void LoadNextScene()
    {
        // 1. Şu anki sahnenin Build Index numarasını al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // 2. Bir sonraki sahnenin Index numarasını hesapla
        int nextSceneIndex = currentSceneIndex + 1;

        // 3. Sahne sayısını kontrol et (Oyunun bitip bitmediğini görmek için)
        int totalSceneCount = SceneManager.sceneCountInBuildSettings;

        // Eğer bir sonraki index toplam sahne sayısından küçükse
        if (nextSceneIndex < totalSceneCount)
        {
            // Geçiş yapılacak sahne varsa, yükle
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Son sahneden sonra Ana Menü'ye (Index 0) dön
            SceneManager.LoadScene(0);
        }
    }
}