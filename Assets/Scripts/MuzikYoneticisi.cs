using UnityEngine;
using UnityEngine.SceneManagement;

public class MuzikYoneticisi : MonoBehaviour
{
    // Inspector'dan atanacak Audio Source'lar (SIRAYLA ATAYIN)
    public AudioSource oyunMuzigiSource;
    public AudioSource sonSahneMuzigiSource;
    
    // Sahneler arası silinmemeyi yönetir
    void Awake()
    {
        // Birden fazla MuzikYoneticisi kopyasının oluşmasını engeller.
        MuzikYoneticisi[] yoneticiler = FindObjectsOfType<MuzikYoneticisi>();
        if (yoneticiler.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            // Bu objenin sahne geçişlerinde silinmemesini sağlar.
            DontDestroyOnLoad(gameObject);
        }
    }

    // 1. Start Butonuna bağlanacak metot
    public void OyunuBaslatMuzik()
    {
        if (oyunMuzigiSource != null && !oyunMuzigiSource.isPlaying)
        {
            oyunMuzigiSource.Play();
            Debug.Log("Oyun Müziği Başladı.");
        }
    }

    // 2. Sahne Geçiş Butonuna VEYA GameManager'a bağlanacak metot
    public void SonSahneyeGecisMuzik()
    {
        // Oyun müziğini durdur
        if (oyunMuzigiSource != null && oyunMuzigiSource.isPlaying)
        {
            oyunMuzigiSource.Stop();
        }
        
        // Son sahne müziğini başlat
        if (sonSahneMuzigiSource != null && !sonSahneMuzigiSource.isPlaying)
        {
            sonSahneMuzigiSource.Play();
            Debug.Log("Son Sahne Müziği Başladı.");
        }
    }
}