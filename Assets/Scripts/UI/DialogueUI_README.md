DialogueUI setup and responsive tips

Bu kısa rehber `DialogueUI` bileşenini sahnene düzgün şekilde eklemene yardımcı olur ve Canvas/panel'in çok büyük gelmesi veya yazıların orantısız olması sorunlarını çözmeyi amaçlar.

1) Sahne hazırlığı
- Sahneye bir Canvas ekle. Canvas'ın `Render Mode` genelde `Screen Space - Overlay` veya `Screen Space - Camera` olabilir.
- Canvas üzerinde bir panel (UI > Panel) oluştur. Bu panel dialog panelin olacak (arka plan, border vs.).
- Panel içine bir metin alanı (Text) veya TextMeshPro kullanıyorsan `TMP - Text` ekle. Satırların gözükeceği bir `RectTransform` olmalı.
- Panel içinde bir boş `RectTransform` (ör. `ChoicesContainer`) oluştur; bu, butonların yerleşeceği yer.
- Eğer butonlarını özel tasarım ile istiyorsan bir `Button` prefab'ı oluştur ve `choiceButtonPrefab` alanına sürükle.

2) `DialogueUI` ayarları
- `DialogueUI` komponentini panelin parent'ine veya tek bir persistent UI GameObject'ine ekle.
- `panel` alanına dialog panel GameObject'ini ata.
- `lineText` (veya TextMeshPro kullanıyorsan `tmpLineText`) alanını ata.
- `choicesContainer` alanına butonların ekleneceği RectTransform'u ata.
- (İsteğe bağlı) `autoFixCanvasScaler` true yaparsan, script Awake() içinde Canvas Scaler'ı `Scale With Screen Size` ve `1920x1080` olarak ayarlamaya çalışır. Bu genelde UI'nin farklı çözünürlüklerde tutarlı görünmesine yardımcı olur.

3) Önemli Canvas/Panel ayarları (özellikle "çok büyük" UI sorunu için)
- Canvas -> Canvas Scaler -> UI Scale Mode = "Scale With Screen Size"
- Reference Resolution = 1920 x 1080 (veya projenin hedef çözünürlüğü)
- Match = 0.5 (en-boy dengesi)

- Panel'in RectTransform'unu ekran oranına göre sabit piksel büyüklüğünde tutma; bunun yerine
  - Anchor'ları uygun şekilde (ör. orta alta, orta üst vb.) ayarla
  - Panel boyutunu reference resolution'a göre tasarla (ör. width=1200, height=300)

4) Text sorunları
- Legacy `Text` için `Best Fit` (resizeTextForBestFit) aktif edildi — script bunu dinamik olarak açar.
- TextMeshPro kullanıyorsan `Auto Size` (enableAutoSizing) aktif edilir. TMP daha iyi sonuç verir.

5) Buton yerleşimi
- `ChoicesContainer`'a bir `Horizontal Layout Group` veya `Vertical Layout Group` ve `Content Size Fitter` ekleyerek butonların otomatik boyutlandırılmasını sağla.
- Eğer özel bir buton prefab'ı kullanıyorsan, prefab içindeki Text/TMP öğesinin Auto Size/Best Fit açık olduğundan emin ol.

6) Kısa test
- Oyun sahnesini çalıştır, çözünürlüğü değiştir (Game view), dialog'u tetikle ve yazı/boşluk/butonların nasıl adapte olduğunu kontrol et.

7) Hızlı öneriler
- Eğer UI çok büyük geliyorsa, büyük ihtimalle Canvas Scaler `Constant Pixel Size` modundadır veya referans çözünürlüğü çok düşük seçilmiştir.
- Text orantısızsa TMP'e geç; TMP çoğunlukla en iyi ve en kararlı sonucu verir.

Eğer istersen, sahnendeki `DialogueUI` komponentinin referanslarını (panel, text, choicesContainer) bağlayabilirim ya da `DialogueUI`'yi `autoFixCanvasScaler=true` yapıp sana test kurulumu için küçük bir örnek script bırakabilirim.
