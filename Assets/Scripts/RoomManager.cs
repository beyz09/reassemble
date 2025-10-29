// RoomManager.cs
using UnityEngine;
using System.Collections;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance; // Singleton (diğer script'lerin erişimi için)

    public RoomUIManager uiManager; // Inspector'dan UIManager'ı sürükle

    [Header("Anxiety Minigame Settings")]
    public GameObject calmThoughtPrefab; // Baloncuk prefab'ı
    public RectTransform anxietySpawnArea; // Baloncukların spawn olacağı panel
    public float anxietyIncreaseRate = 5f; // Anksiyetenin saniyede artış hızı
    public float anxietyDecreasePerClick = 10f; // Tıklama başına anksiyete düşüşü
    public float maxAnxiety = 100f;
    public float bubbleSpawnRate = 0.5f; // Saniyede kaç baloncuk spawn olacağı

    [Header("Game Dialogues")]
    public string[] introDialogueLines;
    public string[] outroDialogueLines;

    private float currentAnxiety;
    private ClickableObject currentObject; // Şu an etkileşimde olunan nesne
    private int currentDialogueLine;
    private string[] activeDialogue; // Mevcut aktif diyalog (nesne veya sistem)
    private static ClickableObject staticCurrentObject;
    private bool[] collectedItems = new bool[4];

    // Oyunun durumunu takip etmek için
    private enum GameState { Browsing, InDialogue, AnxietyMinigame, InChoice, GameOver }
    private GameState currentState;

    void Awake()
    {
        // Singleton kurulumu
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentState = GameState.Browsing;

        // UI panellerinin kapalı başladığından emin ol
        uiManager.ShowDialogue(false);
        uiManager.ShowAnxietyPanel(false);
        uiManager.ShowChoicePanel(false);

        // UI Butonlarına görevlerini ata
        uiManager.dialogueNextButton.onClick.AddListener(OnDialogueNext);
        //uiManager.sameChoiceButton.onClick.AddListener(() => OnChoiceMade(true));
        //uiManager.differentChoiceButton.onClick.AddListener(() => OnChoiceMade(false));

        // --- YENİ: OYUN BAŞLANGIÇ DİYALOĞU ---
        if (introDialogueLines != null && introDialogueLines.Length > 0)
        {
            StartSystemDialogue(introDialogueLines);
        }
    }

    public bool CanInteract()
    {
        // Diyalog sırasında da etkileşimi engelle
        return currentState == GameState.Browsing;
    }

    // 1. ADIM: Nesneye tıklandı
    public void StartObjectInteraction(ClickableObject obj)
    {
        currentObject = obj;
        staticCurrentObject = obj;


        currentState = GameState.InDialogue;
        currentDialogueLine = 0;
        activeDialogue = currentObject.dialogueLines; // Aktif diyaloğu ayarla
        
        uiManager.ShowDialogue(true);
        uiManager.SetDialogueText(activeDialogue[currentDialogueLine]);
    }

    // 2. ADIM: Diyalog "İleri" butonuna basıldı
    private void OnDialogueNext()
    {
        // Eğer oynatılacak bir diyalog yoksa veya oyun bittiyse bir şey yapma
        if (activeDialogue == null || currentState == GameState.GameOver) return;

        currentDialogueLine++;
        if (currentDialogueLine < activeDialogue.Length)
        {
            uiManager.SetDialogueText(activeDialogue[currentDialogueLine]);
        }
        else
        {
            // Mevcut diyalog bittiğinde ne olacağını belirle
            if (currentObject != null)
            {
                // Bu bir nesne diyaloğuydu, mini oyuna geç
                StartAnxietyMinigame();
            }
            else
            {
                // Bu bir sistem (giriş/çıkış) diyaloğuydu, normale dön
                uiManager.ShowDialogue(false);
                currentState = GameState.Browsing;
                activeDialogue = null; // Aktif diyaloğu temizle
            }
        }
    }

    // 3. ADIM: Anksiyete Mini-Oyunu Başlıyor
    private void StartAnxietyMinigame()
    {
        currentState = GameState.AnxietyMinigame;
        uiManager.ShowDialogue(false); // Diyalogu kapat

        currentObject.ghostToActivate.SetActive(true); // Hayaleti göster
        uiManager.ShowAnxietyPanel(true); // Anksiyete panelini aç
        
        currentAnxiety = 30f; // Başlangıç anksiyetesi
        StartCoroutine(AnxietyGameLoop());
        StartCoroutine(SpawnCalmBubbles());
    }

    // Mini-oyun döngüsü (Update gibi çalışır)
    IEnumerator AnxietyGameLoop()
    {
        while(currentState == GameState.AnxietyMinigame)
        {
            currentAnxiety += anxietyIncreaseRate * Time.deltaTime;
            uiManager.SetAnxietySlider(currentAnxiety / maxAnxiety);

            // Kaybetme durumu
            if (currentAnxiety >= maxAnxiety)
            {
                LoseMinigame();
                yield break; // Döngüden çık
            }
            yield return null; // Bir sonraki frame'e kadar bekle
        }
    }

    // Baloncuk spawn döngüsü
    IEnumerator SpawnCalmBubbles()
    {
        while(currentState == GameState.AnxietyMinigame)
        {
            // anxietySpawnArea içinde rastgele bir pozisyon hesapla
            float xPos = Random.Range(anxietySpawnArea.rect.xMin, anxietySpawnArea.rect.xMax);
            float yPos = Random.Range(anxietySpawnArea.rect.yMin, anxietySpawnArea.rect.yMax);
            Vector2 spawnPos = new Vector2(xPos, yPos);
            
            // Prefab'ı panelin altında (child'ı olarak) oluştur
            GameObject bubble = Instantiate(calmThoughtPrefab, anxietySpawnArea.transform);
            bubble.GetComponent<RectTransform>().anchoredPosition = spawnPos;

            yield return new WaitForSeconds(1f / bubbleSpawnRate);
        }
    }

    // 4. ADIM: Baloncuğa tıklandı (CalmThoughtBubble.cs tarafından çağrılır)
    public void OnCalmBubbleClicked()
    {
        if (currentState != GameState.AnxietyMinigame) return;

        currentAnxiety -= anxietyDecreasePerClick;
        
        // Kazanma durumu
        if (currentAnxiety <= 0)
        {
            WinMinigame();
        }
    }

    private void WinMinigame()
    {
        currentState = GameState.InChoice;
        StopAllCoroutines(); // Baloncuk spawn'ı ve anksiyete artışını durdur
        
        uiManager.ShowAnxietyPanel(false);
        // Hayaletin sorusunu göster
        uiManager.ShowChoicePanel(true, currentObject.choiceQuestion); 
    }

    private void LoseMinigame()
    {
        Debug.Log("Mini-oyun kaybedildi! Tekrar başla.");
        // (Şimdilik basitçe tekrar başlatıyoruz)
        StopAllCoroutines();
        currentObject.ghostToActivate.SetActive(false); // Hayaleti gizle
        StartAnxietyMinigame(); // Mini-oyunu tekrar başlat
    }

    // 5. ADIM: Son seçim yapıldı
    private void OnChoiceMade(bool isSameChoice)
    {
        // --- YENİ KORUMA KODU ---
        // Eğer currentObject bir şekilde kaybolduysa, static kopyadan geri yükle
        if (currentObject == null)
        {
            Debug.LogError("currentObject BOŞTU! Static kopyadan geri yüklendi.");
            currentObject = staticCurrentObject;
        }
        // Eğer hala boşsa, bir sorun var demektir
        if (currentObject == null)
        {
             Debug.LogError("STATIC KOPYA DA BOŞ! Nesne toplanamayacak.");
             currentState = GameState.Browsing;
             uiManager.ShowChoicePanel(false);
             return; // Fonksiyondan çık
        }
        // --- KORUMA KODU BİTTİ ---


        currentState = GameState.Browsing; // Oyunu normal akışa döndür
        uiManager.ShowChoicePanel(false); // Seçim panelini kapat
        currentObject.ghostToActivate.SetActive(false); // Hayaleti gizle

        // (Burada oyuncunun seçimine göre bir şey yapabilirsiniz, örn: puan kaydet)
        if (isSameChoice) Debug.Log("Oyuncu 'Aynı Kararı' seçti.");
        else Debug.Log("Oyuncu 'Farklı Kararı' seçti.");

        // Nesneyi envantere ekle
        CollectItem(currentObject.itemID);
    }

    public void HandleSameChoice()
    {
        Debug.LogWarning("--- 2. OnChoiceMade() ÇAĞRILDI! ---");
        OnChoiceMade(true);
    }

    public void HandleDifferentChoice()
    {
        Debug.LogWarning("--- 2. OnChoiceMade() ÇAĞRILDI! ---");
        OnChoiceMade(false);
    }
    // --- YENİ EKLENECEK FONKSİYONLARIN SONU ---

// 6. ADIM: Envantere Ekle
    private void CollectItem(int id)
    {
        // FİŞEK 3: Artık bu fonksiyonun ÇALIŞTIĞINI göreceğiz.
        Debug.LogWarning("--- 3. CollectItem() ÇAĞRILDI! Toplanmak istenen ID: " + id);

        // --- YENİ GÜVENLİK KONTROLÜ (ÇÖKMEYİ ENGELLEME) ---
        if (collectedItems == null)
        {
            Debug.LogError("FELAKET: 'collectedItems' dizisi (array) 'null' (BOŞ)!");
            return; // Çökmemesi için fonksiyondan çık
        }
        
        if (id < 0 || id >= collectedItems.Length)
        {
            Debug.LogError("HATA: GEÇERSİZ ITEM ID: " + id + "! 'collectedItems' dizisi sadece 0 ile " + (collectedItems.Length - 1) + " arası ID kabul eder. Bu nesnenin Item ID'sini düzelt!");
            return; // Çökmemesi için fonksiyondan çık
        }
        // --- GÜVENLİK KONTROLÜ BİTTİ ---

        // Eğer kontrollerden geçtiyse, bu satır artık GÜVENLİ
        collectedItems[id] = true; 
        
        if (uiManager != null)
        {
            uiManager.UpdateInventorySlot(id);
        }
        else
        {
            Debug.LogError("HATA: uiManager 'null' (BOŞ)! Envanter görüntüsü güncellenemedi.");
        }

        // --- Tüm nesneler toplandı mı diye kontrol et ---
        bool allCollected = true;
        foreach (bool collected in collectedItems)
        {
            if (!collected) allCollected = false;
        }

        if (allCollected)
        {
            Debug.Log("TEBRİKLER! TÜM NESNELER TOPLANDI!");
            // --- YENİ: OYUN SONU DİYALOĞU ---
            if (outroDialogueLines != null && outroDialogueLines.Length > 0)
            {
                StartSystemDialogue(outroDialogueLines);
                currentState = GameState.GameOver; // Oyun bitti olarak işaretle
            }
        }
    }

    // --- YENİ FONKSİYON: NESNEYE BAĞLI OLMAYAN DİYALOGLARI BAŞLATMAK İÇİN ---
    private void StartSystemDialogue(string[] lines)
    {
        currentState = GameState.InDialogue;
        currentObject = null; // Bu bir sistem diyaloğu, nesne yok
        activeDialogue = lines;
        currentDialogueLine = 0;

        uiManager.ShowDialogue(true);
        uiManager.SetDialogueText(activeDialogue[currentDialogueLine]);
    }
}