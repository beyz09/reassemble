using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections.Generic; // Varsayılan Unity kütüphaneleri

public class TetrisManager : MonoBehaviour
{
    // =========================================================
    //                DEĞİŞKEN TANIMLARI (TEMİZ)
    // =========================================================

    [Header("PUAN VE HEDEF")]
    public int scoreGoal = 300;     // Kazanmak için gereken minimum skor
    private int currentScore = 0;   // Skoru takip eden TEK DEĞİŞKEN (private)
    public bool isGameOver = false;

    [Header("UI BAĞLANTILARI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject KaybetmePaneli;       // Skor < 300 ise açılacak
    public GameObject SonrakiBolumButonu;   // Skor >= 300 ise açılacak (Kazanma)

    [Header("OYUN AYARLARI")]
    public float timeLimit = 120f;
    public string SonrakiSahneAdi = "TetrisSahnesi"; 
    
    // Özel
    private float currentTime;


    // =========================================================
    //                UNITY BAŞLANGIÇ/DÖNGÜ METOTLARI
    // =========================================================

    void Start()
    {
        // Başlangıç durumlarını garanti altına al
        Time.timeScale = 1f;
        isGameOver = false;
        currentTime = timeLimit;
        currentScore = 0;

        // UI elemanlarını başlangıçta pasif yap
        if (KaybetmePaneli != null) KaybetmePaneli.SetActive(false);
        if (SonrakiBolumButonu != null) SonrakiBolumButonu.SetActive(false);
        
        UpdateScoreDisplay();
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            // Debug.Log("ZAMAN DOLDU. EndGame çağrılıyor.");
            currentTime = 0f;
            EndGame();
        }

        UpdateTimerDisplay();
    }

    // =========================================================
    //               PUAN (SCORE) SİSTEMİ
    // =========================================================

    /// <summary>
    /// Harici script'lerden çağrılır (Örn: Board.cs)
    /// </summary>
    public void AddScore(int linesCleared)
    {
        int points = 0;
        
        switch (linesCleared)
        {
            case 1: points = 100; break;
            case 2: points = 300; break;
            case 3: points = 500; break;
            case 4: points = 800; break; 
            default: break;
        }

        currentScore += points; // Doğru değişkeni güncelliyoruz
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUAN:\n" + currentScore.ToString("D5"); 
        }
    }

    // =========================================================
    //               SÜRE & OYUN SONU (GAME OVER)
    // =========================================================

    void UpdateTimerDisplay()
    // ... (Zamanı dakika:saniye formatında gösteren kod) ...
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void EndGame()
    {
        if (isGameOver) return; 
        isGameOver = true;
        Time.timeScale = 0f;

        // KRİTİK KONTROL: Doğru değişkeni (currentScore) kullanıyoruz.
        if (currentScore < scoreGoal) // 300'den küçükse
        {
            Debug.Log($"Sonuç: KAYIP. Final Skor: {currentScore}");
            if (KaybetmePaneli != null) 
            {
                KaybetmePaneli.SetActive(true); 
            }
        }
        else // 300 ve üzeriyse
        {
            Debug.Log($"Sonuç: KAZANÇ! Final Skor: {currentScore}");
            if (SonrakiBolumButonu != null)
            {
                SonrakiBolumButonu.SetActive(true);
            }
        }
    }

    // =========================================================
    //                  SAHNE YÖNETİMİ
    // =========================================================
    
    public void SahneGecisiYap()
    {
        Time.timeScale = 1f; 
        
        if (!string.IsNullOrEmpty(SonrakiSahneAdi))
        {
            SceneManager.LoadScene(SonrakiSahneAdi);
        }
        else
        {
            Debug.LogError("Sahne geçişi için SonrakiSahneAdi belirlenmedi!");
        }
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f; 
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}