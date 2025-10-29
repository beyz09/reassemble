// RoomUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUIManager : MonoBehaviour
{
    // EMANET ALINACAK NESNE (Bunu bir önceki adımda eklemiştik, kalsın)
    public ClickableObject activeObjectForChoice;

    [Header("Panels")]
    public GameObject dialoguePanel;
    public GameObject anxietyPanel;
    public GameObject choicePanel;
    public GameObject inventoryPanel; 

    [Header("Dialogue")]
    public TMP_Text dialogueText;
    public Button dialogueNextButton;

    [Header("Anxiety Minigame")]
    public Slider anxietySlider;

    [Header("Choice")]
    public TMP_Text choiceQuestionText;
    public Button sameChoiceButton;
    public Button differentChoiceButton;

    [Header("Inventory")]
    public Image[] inventorySlots; // 4 slotu buraya sürükle
    public Sprite[] collectedItemSprites; // 4 net resim sprite'ı buraya sürükle

    
    // --- DIALOGUE ---
    public void ShowDialogue(bool show)
    {
        dialoguePanel.SetActive(show);
    }
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    // --- ANXIETY ---
    public void ShowAnxietyPanel(bool show)
    {
        anxietyPanel.SetActive(show);
    }
    public void SetAnxietySlider(float value) // Değer 0 ile 1 arasında olmalı
    {
        anxietySlider.value = value;
    }

    // --- CHOICE ---
    public void ShowChoicePanel(bool show, string question = "")
    {
        choicePanel.SetActive(show);
        if(show)
        {
            choiceQuestionText.text = question;
        }
    }
    
    // --- INVENTORY ---

    // DÜZELTİLMİŞ, GÜVENLİ FONKSİYON (BU ARTIK ÇALIŞIYOR)
    public void UpdateInventorySlot(int itemID)
    {
        // FİŞEK 4:
        Debug.LogWarning("--- 4. UIManager.UpdateInventorySlot() ÇAĞRILDI! ID: " + itemID);

        // --- GÜVENLİK KONTROLÜ (ÇÖKMEYİ ENGELLEME) ---
        if (inventorySlots == null || collectedItemSprites == null)
        {
            Debug.LogError("FELAKET: 'inventorySlots' veya 'collectedItemSprites' dizisi 'null' (BOŞ)! RoomUIManager Inspector atamalarını kontrol et.");
            return;
        }
        if (itemID < 0 || itemID >= inventorySlots.Length)
        {
            Debug.LogError("HATA: GEÇERSİZ ITEM ID: " + itemID + "! 'inventorySlots' dizisinde bu ID için yer yok. Dizi boyutu: " + inventorySlots.Length);
            return;
        }
        if (itemID >= collectedItemSprites.Length)
        {
             Debug.LogError("HATA: GEÇERSİZ ITEM ID: " + itemID + "! 'collectedItemSprites' dizisinde bu ID için resim yok. Dizi boyutu: " + collectedItemSprites.Length);
            return;
        }
        // --- GÜVENLİK KONTROLÜ BİTTİ ---

        // Bu satır artık GÜVENLİ
        inventorySlots[itemID].sprite = collectedItemSprites[itemID];
    }

    // ENVANTER BUTONUNUN KULLANDIĞI, KAYBOLAN FONKSİYON
    public void ToggleInventoryPanel()
    {
        // Panel aktifse kapat, kapalıysa aç
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    // EMANET NESNE FONKSİYONU
    public void SetActiveChoiceObject(ClickableObject obj)
    {
        activeObjectForChoice = obj;
    }
}