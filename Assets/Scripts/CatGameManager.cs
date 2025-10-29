// GameManager.cs
using UnityEngine;
using UnityEngine.UI; // Slider için bu gerekli

public class CatGameManager : MonoBehaviour
{
    // Oyunun hangi aşamada olduğunu takip etmek için enum
    public enum GameState
    {
        Intro,      // Giriş konuşması
        NeedsCat,   // Kedi küvete girmeli
        NeedsSoap,  // Sabunlanmalı
        NeedsRinse, // Durulanmalı
        NeedsDry,   // Kurulanmalı
        CatStruggling, //Kedi kaçmaya çalışıyor
        StageClear  // Aşama bitti
    }

    public GameState currentState;

    [Header("UI Elemanları")]
    public Slider progressBar;
    public float interactionTime = 3f; // Sabunlama/Durulama süresi (saniye)
    private float currentProgress = 0f;

    [Header("Oyun Objeleri")]
    public GameObject normalCat;
    public GameObject wetCat;
    public SpriteRenderer bathtubRenderer; // Küvetin Sprite Renderer'ı

    [Header("Sprite'lar")]
    public Sprite tubNormalSprite;
    public Sprite tubWithCatSprite;

    [Header("UI Referansları")]
    public UIManager uiManager; // UIManager'a referans

    [Header("Diyalog Metinleri")]
    public string[] introLines;
    public string[] outroLines;

    [Header("Sınama (Struggle) Mekaniği")]
    [Range(0, 1)]
    public float struggleChancePerSecond = 0.2f; // Saniyede kaçma şansı (%20)
    public float struggleDuration = 4f; // Sınama kaç saniye sürecek
    public int clicksNeededToWin = 15;  // Kazanmak için gereken tıklama
    
    private GameState stateBeforeStruggle; // Hangi task'tan geldiğimizi hatırlamak için
    private float struggleTimer;
    private int currentClicks;

    private DraggableItem currentDraggedItem;
    private DraggableItem[] allDraggableItems;

    void Start()
    {
        // --- YENİ BAŞLANGIÇ ---
        currentState = GameState.Intro; // Durumu Intro ile başlat
        
        // Eğer UIManager atanmamışsa hata ver (Önemli kontrol)
        if (uiManager == null)
        {
            Debug.LogError("UIManager reference is not assigned to GameManager!");
            return;
        }

        uiManager.StartDialogue(introLines); // UIManager'a "giriş konuşmasını başlat" de
        allDraggableItems = FindObjectsOfType<DraggableItem>();

        // Bunlar kalsın, oyun başlayınca hazır olsunlar
        progressBar.gameObject.SetActive(false);
        progressBar.maxValue = interactionTime;
        wetCat.SetActive(false);
        normalCat.SetActive(true); // Kedinin görünür başladığından emin ol
        bathtubRenderer.gameObject.SetActive(true); // Küvetin görünür başladığından emin ol
        bathtubRenderer.sprite = tubNormalSprite; // Küvetin normal başladığından emin ol
    }

    // Bu fonksiyonu UIManager çağıracak (Diyalog bitince)
    public void StartGame()
    {
        Debug.Log("Game is starting!");
        currentState = GameState.NeedsCat;
    uiManager.SetupNotepad(); // Not defterini göster
    uiManager.ShowFeedback("Let's get started! Drag the cat into the bathtub.");
    }

    void Update()
{
        // Bu durumlarda Update'i çalıştırma
        if (currentState == GameState.Intro || currentState == GameState.StageClear) return;

        // EĞER KEDİ KAÇIYORSA:
        if (currentState == GameState.CatStruggling)
        {
            RunStruggleLogic();
            return; // Diğer Update kodlarını çalıştırma
        }

        // Eğer bir obje sürüklüyorsak (ve bu bir "araç" ise)
        if (currentDraggedItem != null)
        {
            HandleInteraction();
        }
    }

    // Sürükleme başladığında DraggableItem tarafından çağrılır
    public void StartDragging(DraggableItem item)
    {
        currentDraggedItem = item;
    }

    // Sürükleme bittiğinde DraggableItem tarafından çağrılır
    public void StopDragging(DraggableItem item)
    {
        // KEDİYİ KÜVETE KOYMA KONTROLÜ (Bırakma anında)
        if (currentState == GameState.NeedsCat && item.CompareTag("Cat"))
        {
            if (IsOverTarget("Bathtub"))
            {
                PutCatInTub();
            }
            else
            {
                item.SnapToStart(); // Hedefte değilse geri yolla
            }
        }
        // Eğer bir araçsa ve hedefin üstünde değilse, progress'i sıfırla
        else if (currentDraggedItem != null)
        {
            item.SnapToStart(); // Araçları da yerine yolla
        }

        // Sürüklenen objeyi ve progress barı sıfırla
        currentDraggedItem = null;
        progressBar.gameObject.SetActive(false);
        currentProgress = 0;
    }

    // Sürükleme anında (Update'te) çalışan ana mantık
    private void HandleInteraction()
    {
        bool onTarget = false;

        // SABUNLAMA KONTROLÜ
        if (currentState == GameState.NeedsSoap && currentDraggedItem.CompareTag("Soap"))
        {
            if (IsOverTarget("Bathtub"))
            {
                onTarget = true;
            }
        }
        // DURULAMA KONTROLÜ
        else if (currentState == GameState.NeedsRinse && currentDraggedItem.CompareTag("Shower"))
        {
            if (IsOverTarget("Bathtub"))
            {
                onTarget = true;
            }
        }
        // KURULAMA KONTROLÜ
        else if (currentState == GameState.NeedsDry && currentDraggedItem.CompareTag("Towel"))
        {
            if (IsOverTarget("WetCat")) // Artık hedefimiz ıslak kedi
            {
                onTarget = true;
            }
        }

        // Eğer doğru araç doğru hedefin üzerindeyse
        if (onTarget)
        {
            progressBar.gameObject.SetActive(true);
            currentProgress += Time.deltaTime;
            progressBar.value = currentProgress;

            // --- YENİ KAÇMA KONTROLÜ ---
            // Belirli bir şansla (saniyede %X) kaçma durumunu tetikle
            if (Random.value < (struggleChancePerSecond * Time.deltaTime))
            {
                TriggerStruggle();
                return; // Bu frame'de işlemi bitir
            }
            // --- BİTTİ ---

            if (currentProgress >= interactionTime)
            {
                // İşlem tamamlandı, bir sonraki aşamaya geç
                AdvanceState();
            }
        }
        else
        {
            // Hedefin üstünde değilse progress'i sıfırla
            progressBar.gameObject.SetActive(false);
            currentProgress = 0;
            progressBar.value = 0;
        }
    }

    // Bir sonraki aşamaya geçişi yönetir
    private void AdvanceState()
    {
        progressBar.gameObject.SetActive(false);
        currentProgress = 0;
        currentDraggedItem.SnapToStart(); // Aracı yerine geri yolla
        currentDraggedItem = null; // Sürüklemeyi bitir

        if (currentState == GameState.NeedsSoap)
        {
            CompleteSoaping();
        }
        else if (currentState == GameState.NeedsRinse)
        {
            CompleteRinsing();
        }
        else if (currentState == GameState.NeedsDry)
        {
            CompleteDrying();
        }
    }

    // Farenin altındaki objenin tag'ini kontrol eder
    private bool IsOverTarget(string targetTag)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag(targetTag))
        {
            return true;
        }
        return false;
    }

    // --- Sınama (Struggle) Mekaniği Fonksiyonları ---

    private void TriggerStruggle()
    {
        Debug.Log("The cat is trying to escape!");
        
        // Mevcut durumu kaydet (örn: NeedsSoap)
        stateBeforeStruggle = currentState; 
        currentState = GameState.CatStruggling;

        // Oyuncunun elindeki aracı geri bırakmasını sağla
        if (currentDraggedItem != null)
        {
            currentDraggedItem.SnapToStart();
            currentDraggedItem = null;
        }

        // Görev ilerlemesini sıfırla (eğer sınamayı geçerse task'a baştan başlasın)
        currentProgress = 0;
        progressBar.gameObject.SetActive(false);

        // Sınama sayaçlarını ayarla
        struggleTimer = struggleDuration;
        currentClicks = 0;

        // UI'ı göster
        uiManager.ShowStrugglePanel(true, struggleDuration);
        uiManager.ShowFeedback("The cat is escaping! Click quickly!");
    }

    // Bu fonksiyon Update'te CatStruggling durumundayken çalışır
    private void RunStruggleLogic()
    {
        struggleTimer -= Time.deltaTime;
        uiManager.UpdateStruggleTimer(struggleTimer);

        // SÜRE BİTTİ
        if (struggleTimer <= 0)
        {
            // Tıklama sayısı yetersizse, başarısız ol
            if (currentClicks < clicksNeededToWin)
            {
                ChallengeFail();
            }
            else
            {
                // Süre bittiğinde yeterince tıklamışsa, başarılı say
                ChallengeSuccess();
            }
        }
    }

    // Bu fonksiyon UIManager'daki Buton tarafından çağrılacak
    public void OnStruggleClicked()
    {
        // Sadece doğru durumda çalışsın
        if (currentState != GameState.CatStruggling) return;

        currentClicks++;
        
        // Yeterli tıklamaya ulaştıysa, süre bitmeden kazan
        if (currentClicks >= clicksNeededToWin)
        {
            ChallengeSuccess();
        }
    }

    private void ChallengeSuccess()
    {
        Debug.Log("Challenge SUCCESS!");
        currentState = stateBeforeStruggle; // Kaldığımız göreve geri dön
        uiManager.ShowStrugglePanel(false); // Paneli gizle
        uiManager.ShowFeedback("Phew! You calmed it down. Continue the task.");
    }

    private void ChallengeFail()
    {
        Debug.Log("Challenge FAILED! The cat escaped!");
        uiManager.ShowStrugglePanel(false); // Paneli gizle
        uiManager.ShowFeedback("Oh no! The cat escaped! You'll need to start over.");
        
        // Tüm görevleri sıfırla
        ResetAllTasks();
    }

    // Bütün görevleri başa saran fonksiyon
    private void ResetAllTasks()
    {
        currentState = GameState.NeedsCat; // En başa dön

        // Dünyayı sıfırla
        normalCat.SetActive(true);
        wetCat.SetActive(false);
        bathtubRenderer.gameObject.SetActive(true);
        bathtubRenderer.sprite = tubNormalSprite;

        // UI'ları sıfırla
        currentProgress = 0;
        progressBar.gameObject.SetActive(false);

        // Sahnede bulduğumuz tüm sürüklenebilir objelerin (Sabun, Duş, Havlu, Kedi)
        // pozisyonunu en baştaki yerine döndür.
        if (allDraggableItems != null)
        {
            foreach (DraggableItem item in allDraggableItems)
            {
                item.SnapToStart();
            }
        }

        // Not defterini sıfırla (üstü çizilenleri kaldır)
        uiManager.SetupNotepad(); 

        // Sahnede bulduğumuz tüm sürüklenebilir objelerin (Sabun, Duş, Havlu, Kedi)
        // pozisyonunu en baştaki yerine döndür.
        if (allDraggableItems != null)
        {
            foreach (DraggableItem item in allDraggableItems)
            {
                item.SnapToStart();
            }
        }
    }

    // --- Aşama Fonksiyonları ---

    void PutCatInTub()
    {
        Debug.Log("The cat entered the bathtub!");
        normalCat.SetActive(false); // Normal kediyi gizle
        bathtubRenderer.sprite = tubWithCatSprite; // Küvetin sprite'ını değiştir
        currentState = GameState.NeedsSoap; // Bir sonraki aşama: Sabunlama
        
        // UI GÜNCELLEMESİ
    uiManager.CrossOutTask(1); // 1. görevin üstünü çiz
    uiManager.ShowFeedback("Great! Now soap the cat.");
    }

    void CompleteSoaping()
    {
        Debug.Log("Soaping finished!");
        currentState = GameState.NeedsRinse; // Bir sonraki aşama: Durulama
        
        // UI GÜNCELLEMESİ
    uiManager.CrossOutTask(2); // 2. görevin üstünü çiz
    uiManager.ShowFeedback("Nice lather! Time to rinse.");
    }

    void CompleteRinsing()
    {
        Debug.Log("Rinsing finished!");

        // KÜVETİ GİZLEME GÜNCELLEMESİ
        bathtubRenderer.gameObject.SetActive(false); // Küvet objesini tamamen gizle
        
        wetCat.SetActive(true); // Islak kediyi göster
        wetCat.transform.position = normalCat.transform.position; 
        currentState = GameState.NeedsDry; // Bir sonraki aşama: Kurulama
        
        // UI GÜNCELLEMESİ
    uiManager.CrossOutTask(3); // 3. görevin üstünü çiz
    uiManager.ShowFeedback("Great! Now dry the cat with the towel.");
    }

    void CompleteDrying()
    {
        Debug.Log("Drying finished!");
        wetCat.SetActive(false); // Islak kediyi gizle
        normalCat.SetActive(true); // Normal kediyi geri getir
        currentState = GameState.StageClear; // Aşama bitti
        Debug.Log("LEVEL COMPLETE!");
        
        // UI GÜNCELLEMESİ
    uiManager.CrossOutTask(4); // 4. görevin üstünü çiz
    uiManager.ShowFeedback("All clean! Task completed.");
        
        // Bitiş konuşmasını başlat
        uiManager.StartDialogue(outroLines);
    }
}