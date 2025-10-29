using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; // UnityEvent için gerekli

public class SimpleDialogue : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject dialoguePanel; // Diyalog panelin
    public TextMeshProUGUI dialogueText; // Diyalog metni
    public Button nextButton; // İlerleme butonu

    [Header("Diyalog İçeriği")]
    [TextArea(3, 10)] // Inspector'da daha rahat yazmak için
    public string[] dialogueLines; // Buraya diyaloglarını yazacaksın

    [Header("Diyalog Sonrası")]
    public UnityEvent onDialogueEnd; // Diyalog bitince ne olacağını buradan ayarlayabilirsin

    private int currentLineIndex = 0;

    void Start()
    {
        // Butona tıklama olayını ayarla
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextLine);
        }

        // Sahne başladığında diyaloğu otomatik başlat
        StartDialogue();
    }

    // Diyaloğu başlatır
    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Gösterilecek diyalog satırı bulunamadı.");
            return;
        }

        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentLineIndex];
    }

    // Sonraki satırı gösterir
    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            // Diyalog bitti
            EndDialogue();
        }
    }

    // Diyaloğu bitirir
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("Diyalog bitti.");

        // Diyalog bitince tetiklenecek olayları çalıştır
        if (onDialogueEnd != null)
        {
            onDialogueEnd.Invoke();
        }
    }
}