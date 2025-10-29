// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro için bu satır şart
using System.Collections; // Coroutine için bu satır şart

public class UIManager : MonoBehaviour
{
    // GameManager'a referans
    public CatGameManager gameManager;

    [Header("Diyalog Paneli")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText; // TextMeshPro kullanıyoruz
    public Button nextButton;
    private string[] currentDialogueLines;
    private int currentLineIndex;

    [Header("Not Defteri")]
    public GameObject notepadPanel;
    public TMP_Text task1Text;
    public TMP_Text task2Text;
    public TMP_Text task3Text;
    public TMP_Text task4Text;

    [Header("Geri Bildirim")]
    public TMP_Text feedbackText;
    public float feedbackDisplayTime = 3f;

    [Header("Sınama (Struggle) Paneli")]
    public GameObject strugglePanel;
    public TMP_Text struggleInfoText; // (İsteğe bağlı, metni değiştirmek isterseniz)
    public Button struggleClickButton;
    public Slider struggleTimerSlider;


    // --- DİYALOG SİSTEMİ ---

    // Diyalogu başlatmak için GameManager'dan çağrılacak
    void Start()
    {
        // Butonun tıklandığında GameManager'daki OnStruggleClicked fonksiyonunu çağırmasını ayarla
        if (struggleClickButton != null && gameManager != null)
        {
            struggleClickButton.onClick.AddListener(gameManager.OnStruggleClicked);
        }
        else
        {
            Debug.LogWarning("UIManager'da Struggle Butonu veya GameManager referansı eksik!");
        }
    }

    public void StartDialogue(string[] lines)
    {
        dialoguePanel.SetActive(true);
        currentDialogueLines = lines;
        currentLineIndex = 0;

        // Butonun eski görevlerini temizle, yenisini ekle
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(DisplayNextLine);

        DisplayNextLine(); // İlk satırı göster
    }

    private void DisplayNextLine()
    {
        if (currentLineIndex < currentDialogueLines.Length)
        {
            dialogueText.text = currentDialogueLines[currentLineIndex];
            currentLineIndex++;
        }
        else
        {
            // Diyalog bitti
            dialoguePanel.SetActive(false);

            // Eğer bu GİRİŞ diyaloğuysa, oyunu başlat
            if (gameManager.currentState == CatGameManager.GameState.Intro)
            {
                gameManager.StartGame(); // GameManager'a "artık başla" diyoruz
            }
        }
    }

    // --- NOT DEFTERİ SİSTEMİ ---

    public void SetupNotepad()
    {
        notepadPanel.SetActive(true);
        task1Text.text = "1. Place the cat in the tub.";
        task2Text.text = "2. Soap the cat.";
        task3Text.text = "3. Rinse the cat.";
        task4Text.text = "4. Dry the cat.";
    }

    // Görevin üstünü çizmek için GameManager'dan çağrılacak
    public void CrossOutTask(int taskID)
    {
        switch (taskID)
        {
            case 1:
                task1Text.text = "<s>1. Place the cat in the tub.</s>";
                break;
            case 2:
                task2Text.text = "<s>2. Soap the cat.</s>";
                break;
            case 3:
                task3Text.text = "<s>3. Rinse the cat.</s>";
                break;
            case 4:
                task4Text.text = "<s>4. Dry the cat.</s>";
                break;
        }
    }

    // --- GERİ BİLDİRİM SİSTEMİ ---

    // Altta çıkan bilgilendirme yazısı
    public void ShowFeedback(string message)
    {
        // Eski coroutine'i durdur (yazılar üst üste binmesin)
        StopCoroutine("FadeOutFeedback"); 
        
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        
        // Yazıyı birkaç saniye sonra gizle
        StartCoroutine("FadeOutFeedback");
    }

    private IEnumerator FadeOutFeedback()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
    }
    // --- SINAMA (STRUGGLE) SİSTEMİ ---

    public void ShowStrugglePanel(bool show, float maxTime = 1f)
    {
        strugglePanel.SetActive(show);
        if (show)
        {
            struggleTimerSlider.maxValue = maxTime;
            struggleTimerSlider.value = maxTime;
        }
    }

    public void UpdateStruggleTimer(float currentTime)
    {
        struggleTimerSlider.value = currentTime;
    }
}