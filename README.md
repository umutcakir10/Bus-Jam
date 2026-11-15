# 🚌 Bus Jam

Unity 2022.3.19f1 ile sıfırdan geliştirdiğim, Bus Jam tarzı bir bulmaca oyunu.

Amacım sadece çalışan bir oyun değil, **temiz kurulmuş, test edilmiş ve oynaması keyifli** bir oyun çıkarmaktı. Oyunun oynanışını ve zorluğunu, oyuncuyu oyunda daha uzun süre tutacak şekilde geliştirmeye özen gösterdim. Kuralları Unity'den bağımsız yazdım ve testlerle doğruladım. Üstüne animasyon, ses, arayüz ve çevre ile oyunun hissini kurdum. Sonra da bu temelin üstüne yeni bir mekanik (gizli yolcular) ve bir tutorial ekledim.

---

## İçindekiler

- [Oyun](#-oyun)
- [Kurulum](#-kurulum)
- [Case gereksinimleri](#-case-gereksinimleri)
- [Ekstra eklediklerim](#-ekstra-eklediklerim)
- [Level Editor](#-level-editor)
- [Teknik tarafta neler yaptım](#-teknik-tarafta-neler-yaptım)
- [Testler](#-testler)
- [Nasıl çalıştım](#-nasıl-çalıştım)
- [Bilinen eksikler ve sonraki adımlar](#-bilinen-eksikler-ve-sonraki-adımlar)
- [Ek kaynaklar](#-ek-kaynaklar)

---

## 🎮 Oyun

Yolcular renkli bir grid üzerinde bekliyor, otobüsler de yolda sırayla geliyor. Görevin, süre bitmeden her yolcuyu kendi rengindeki otobüse bindirmek.

- Bir yolcuya dokun. **Önünde ön sıraya giden boş bir yol varsa** yürüyüp çıkar. Yolu kapalıysa titrer ve yerinde kalır.
- Duraktaki otobüs **onun rengindeyse** biner. Değilse **bekleme yerlerinden** birine geçer.
- Otobüs dolunca yola çıkar, sıradaki gelir. Bekleme yerindeki aynı renkli yolcular yeni otobüse **kendiliğinden** biner.

| | |
|---|---|
| 🏆 **Kazanma** | Bütün otobüsler dolup gidince |
| ❌ **Kaybetme** | Süre bitince ya da bütün bekleme yerleri dolunca |
| ⭐ **Yıldız** | Sürenin yarısı ya da fazlası kaldıysa 3, dörtte biri ya da fazlası kaldıysa 2, yoksa 1 |

İşin bulmaca kısmı sırada: Bekleme yerleri az, yanlış sırayla dokunursan onları boşa harcayıp kaybedersin.

---

## 📦 Kurulum

### Sadece oynamak için

| Platform | Adımlar |
|---|---|
| **Windows** | [Releases](TODO-releases-linki) sayfasından zip'i indir, bir klasöre çıkar, `Bus Jam.exe`'yi çalıştır. Uygulama imzasız olduğu için Windows "tanınmayan uygulama" uyarısı verebilir: **Ek bilgi → Yine de çalıştır**. |
| **Android** | Aynı sayfadan `BusJam.apk`'yı indirip telefona yükle. Telefon izin isterse bilinmeyen kaynaklardan yüklemeye izin ver. |

> Oyun telefon için tasarlandığı için Windows'ta pencere bilerek **dikey (450 × 975)** açılıyor.

### Projeyi Unity'de açmak için

1. Repoyu klonla:
   ```
   git clone TODO-repo-linki
   ```
2. **Unity Hub**'da **Add** ile klasörü ekle, **Unity 2022.3.19f1** ile aç.
3. `Assets/_BusJam/Scenes/Game` sahnesini aç ve **Play**'e bas.

Unity'nin üst menüsünde benim eklediğim bir **Bus Jam** menüsü var:

| Komut | Ne yapıyor |
|---|---|
| **Validate Levels** | Bütün level dosyalarını kontrol ediyor, hataları tek seferde listeliyor |
| **Delete Save** | Kaydı silip oyunu Level 1'den başlatıyor |

---

## ✅ Case gereksinimleri

| Gereksinim | Nasıl karşıladım |
|---|---|
| Unity 2022.3.19f1 | Proje bu sürümle geliştirildi |
| **Start ekranı**, dokununca başlama | Tam ekran bir başlık ekranı. Ekranın herhangi bir yerine ya da Play butonuna dokununca level başlıyor. Ekranda hangi level'da olduğun da yazıyor. |
| **Süre ile kaybetme** | Her level'ın kendi süresi var. Süre sağ üstte geri sayıyor, son saniyelerde rengi değişiyor. Bitince kaybetme kartı açılıyor. |
| **Level göstergesi** | Sol üstte "Level 1", "Level 2"... yazıyor |
| **Kaldığı yerden devam** | İlerleme bir kayıt dosyasında tutuluyor. Oyunu kapatıp açınca en son ulaştığın level'dan başlıyorsun. |
| **En az 5 farklı level** | **9 level** var. Zorluk giderek artıyor. |
| **Oyun sonu ekranı** | Kazanınca yıldızlı bir kart ve sonraki level'a geçiş, kaybedince sebebini gösteren bir kart (süre bitti / yer kalmadı) ve tekrar deneme |
| **Level Editor**: yeni level ekleme, var olanı güncelleme, süreyi düzenleme | Ayrı bir `LevelEditor` sahnesi, başlık ekranından açılıyor. Build içinde de çalışıyor. |
| Ücretsiz asset kullanımı | Kullandıklarımın hepsi [Ek kaynaklar](#-ek-kaynaklar) bölümünde |

---

## ✨ Ekstra eklediklerim

### 🕵️ Gizli yolcular (Level 7–9)
Bazı yolcular gri ve başlarında **"?"** ile başlıyor. Ancak önlerindeki yol açıldığında renkleri ortaya çıkıyor ve küçük bir zıplama yapıyorlar. Oyuncuyu sadece önündekini değil, birkaç hamle sonrasını da düşünmeye zorluyor.

### 👆 Tutorial (Level 1–2)
Oyuncu 1,5 saniye hiçbir şeye dokunmazsa bir el belirip **en doğru hamleyi** gösteriyor: duraktaki otobüse binebilecek bir yolcuyu. Hızlı oynayan oyuncu eli hiç görmüyor, takılan oyuncu her hamlede yardım alıyor.

### 📈 Zorluk eğrisi
İlk level'lar küçük ve rahat. Sonrasında grid büyüyor, renk sayısı artıyor, duvarlar ve boşluklar geliyor, yolcu başına düşen süre ve bekleme yeri azalıyor. Son üç level gizli yolcularla bitiyor.

| | Level 7 | Level 8 | Level 9 |
|---|---|---|---|
| Grid | 5×4 | 5×5 | 6×5 |
| Yolcu | 18 | 21 | 27 |
| Renk | 4 | 4 | 5 |
| Gizli sıra | 2 | 3 | 3 |
| Yolcu başına süre | 5,0 sn | 3,8 sn | 3,5 sn |

### 🎨 Oyun hissi
- **Animasyonlar (DOTween):** yolcuların otobüse zıplaması, otobüslerin gelip gitmesi, kartların açılması, butonlar
- **Sesler:** dokunma, kapalı yol, oturma, otobüs kalkışı, kazanma ve kaybetme, buton tıkları
- **Arayüz:** kendi kart, buton, ikon ve kurdele görsellerim, Fredoka fontu
- **Çevre:** lavanta zemin, kaldırımlar, parlayan sokak lambaları, bollard'lar, çalılar, yumuşak gölgeler
- **Başlık ekranı ve uygulama ikonu**

---

## 🛠 Level Editor

Case'in istediği editörü **oyunun içinde** çalışacak şekilde yaptım. Unity editöründe de, build'de de aynı şekilde kullanılıyor.

1. Başlık ekranında sağ üstteki **Level Editor**'a bas.
2. `<` `>` ile var olan level'lar arasında gez ya da **New** ile yeni bir level aç.
3. Üstten ayarla:
   - **Width / Height:** grid boyutu
   - **Time:** süre (5'er saniye)
   - **Slots:** bekleme yeri sayısı
4. Alttaki paletten bir şey seç ve grid'e dokunarak boya:
   - Renkli kareler: o renkte yolcu
   - **Empty:** boş hücre, **Hole:** tile olmayan boşluk, **Wall:** duvar
   - **?** açıkken boyadığın yolcular gizli oluyor
5. **Buses** kutusuna otobüslerin geliş sırasını yaz. Örneğin `RGBR`: kırmızı, yeşil, mavi, kırmızı.
6. Altta her renk için kaç yolcu ve kaç koltuk olduğu yazıyor. Hepsi tutunca **Ready to save** çıkıyor, tutmuyorsa neyin eksik olduğunu söylüyor.
7. **Save** ile kaydet, **Test** ile hemen oyna, **Menu** ile kaydetmeden çık.

Editörde gördüğün görüntü oyundakiyle aynı, çünkü editör oyunun kendi çizim sınıflarını kullanıyor.

---

## 🧠 Teknik tarafta neler yaptım

### Klasör yapısı

```
Assets/_BusJam/Scripts
├── Core                 Renkler, grid koordinatı (GridPos)
├── LevelSystem          Level verisi, okuma/yazma, doğrulama, editör taslağı
├── Gameplay
│   ├── Domain           Oyunun kuralları: board, yol bulma, otobüs sırası, bekleme yerleri, süre
│   └── Presentation     Görüntü: yolcular, otobüsler, kamera, yerleşim, animasyonlar
├── Flow                 Oyun ve editör sahnelerinin yöneticileri, sahneler arası durum
├── UI                   HUD, kartlar, başlık ekranı, tutorial eli, editör arayüzü
├── Save                 Kayıt dosyası
├── Audio                Ses oynatıcı
└── Editor               Unity menüsündeki Bus Jam komutları
```

Kodu üç **assembly**'ye ayırdım: oyun kodu (`BusJam.Runtime`), sadece Unity editöründe çalışan araçlar (`BusJam.Editor`) ve testler (`BusJam.Tests.EditMode`). Böylece kuralların, görüntü ya da editör koduna yanlışlıkla bağımlı olması mümkün değil.

### Kurallar ve görüntü birbirinden ayrı

Bu projenin temel kararı bu.

- **Kurallar (Domain)** saf C#, içinde hiç `MonoBehaviour` yok. Bir dokunuşu **anında ve tamamen** çözüyor: yolcu otobüse mi binecek, bekleme yerine mi gidecek, otobüs doldu mu, sıradaki otobüse bekleyenlerden kim binecek... Zincir ne kadar uzun olursa olsun tek seferde. Sonuç bir **olay listesi**: "şu yolcu şu otobüsün şu koltuğuna", "şu otobüs kalktı" gibi.
- **Görüntü (Presentation)** bu olayları sadece oynatıyor. Yolcuya yürüyeceği yolu veriyor, otobüs de koltukları gerçekten dolunca yola çıkıyor.

Kurallar animasyonları hiç beklemediği için **sahne açmadan test edilebiliyor**. Görüntü tarafını da oynanışı bozmadan istediğim gibi değiştirebildim. Animasyonları ve çevreyi en sona bırakabilmemin sebebi bu.

### Level'lar JSON dosyası

```json
{
  "version": 1,
  "id": "level_008",
  "displayName": "Level 8",
  "timeLimitSeconds": 80,
  "busCapacity": 3,
  "waitingSlotCount": 4,
  "rows": [".GBYB", "RRYBB", "XygbX", "rbggg", "rg.rr"],
  "buses": "BRRBYGG"
}
```

| Karakter | Anlamı |
|---|---|
| `R` `B` `G` `Y` `P` | Kırmızı, mavi, yeşil, sarı, mor yolcu |
| `r` `b` `g` `y` `p` | Aynı renklerin **gizli** yolcusu |
| `.` | Boş hücre |
| `X` | Duvar |
| `#` | Boşluk (tile yok) |

İlk satır yola en yakın sıra. Level'ı bir metin editöründe açıp okuyabiliyorsun.

**Neden ScriptableObject değil de JSON?** Case oyunun içinde çalışan bir level editörü istiyor. ScriptableObject build içinde kalıcı olarak kaydedilemiyor, JSON kaydedilebiliyor. Unity editöründe kaydedilen level'lar `Resources/Levels` klasörüne gidiyor ve oyunla birlikte geliyor. Build'de kaydedilenler ise cihazın kendi klasörüne (`persistentDataPath`) yazılıyor ve aynı isimli level'ın yerine geçiyor.

**Doğrulama:** Her level oynanmadan ve kaydedilmeden önce bir validator'dan geçiyor. Grid düzgün mü, harfler tanınıyor mu, sınırlar aşılmış mı, her renkte tam `otobüs sayısı × 3` yolcu var mı? Bütün hatalar ilk hatada durmadan, tek seferde listeleniyor.

### Yol bulma: BFS

Bir yolcu, ön sıraya giden boş hücrelerden oluşan bir yol varsa çıkabiliyor. Bunu **BFS** ile buluyorum. Hedef tek bir nokta değil, ön sıradaki herhangi bir hücre. Grid'ler de en fazla 10×12, BFS fazlasıyla yetiyor.

### Oyun sırasında sıfır çöp (GC)

Mobilde Garbage Collector çalışması oyunda küçük takılmalara sebep olur. Bu yüzden oyun sırasında **her dokunuşta hiç yeni bellek ayrılmıyor**:
- `GridPos` bir `struct` ve `IEquatable` uyguluyor, hiçbir koleksiyonda boxing'e uğramıyor.
- BFS'in kullandığı diziler level başında bir kez oluşuyor. Her aramada temizlemek yerine hangi hücrelerin bu aramada görüldüğünü ayırıyorum.
- Yol ve olay listeleri dışarıdan veriliyor ve her dokunuşta tekrar kullanılıyor.
- Yolcular bir **object pool**'dan geliyor.
- Süre yazısı her karede yeni string oluşturmak yerine hazır bir `char[]` dizisine yazılıyor.

### Gizli yolcuları kurallara dokunmadan ekledim

Bu, mimarinin işe yaradığını en net gördüğüm yer oldu. Level formatında küçük harf, o rengin gizli hali. Harfleri okuyan tek yer `LevelFormat` ve orada `r` ile `R`'yi aynı kırmızı yolcu olarak okuyorum. Böylece board, validator, yol bulma ve hamle çözümü **hiç değişmedi**.

"Gizli" olmak sadece görüntüyle ilgili: Her hamleden sonra görüntü katmanı, gizli yolcuların her biri için kurallara "bunun yolu açıldı mı?" diye soruyor. Açıldıysa rengini gösteriyor. Yolu kapalı bir yolcu zaten hareket edemediği için oyuncu rengini bilmediği bir yolcuyu hiçbir zaman oynatamıyor. Mevcut testlerin hiçbiri değişmedi, sadece yenileri eklendi.

### Tutorial'ın doğru hamlesi de kurallardan geliyor

"El hangi yolcuyu göstermeli?" bir kural sorusu, o yüzden cevabı kural katmanında ve testli. Önce duraktaki otobüse binebilecek bir yolcu aranıyor, yoksa yolu açık herhangi biri. Arayüz sadece o yolcunun ekrandaki yerini bulup eli oraya taşıyor.

### Diğer kararlar

- **Konumların tek kaynağı:** Hücrelerin, bekleme yerlerinin, durağın ve yolun dünyadaki konumlarını tek bir sınıf (`BoardLayout`) hesaplıyor. Hiçbir görüntü sınıfı kendi kafasına göre konum uydurmuyor.
- **Her ekrana uyan kamera:** Kamera oyun alanını her ekran oranına sığdırıyor. Kaldırımlar ve lambalar da her level'da oyun alanının kenarlarına göre yerleşiyor.
- **Oyun akışı küçük bir durum makinesi:** `Ready → Playing → Ending → Ended`. Örneğin kazanma kartı, son yolcu binip son otobüs gerçekten yola çıkana kadar bekliyor.
- **Bozulmayan kayıt:** Kayıt önce geçici bir dosyaya yazılıyor, sonra asıl dosyanın yerine konuyor. Yazarken oyun kapansa bile ilerleme kaybolmuyor.
- **Ses:** Tek bir ses oynatıcı sahneler arasında yaşıyor, bu yüzden butona basıp sahne değişince tık sesi yarıda kesilmiyor. Sahnede oynatıcı yoksa çağrılar sessizce boşa düşüyor, hiçbir sınıf sese bağımlı değil.

---

## 🧪 Testler

**Window → General → Test Runner → EditMode → Run All**

Kural katmanını **57 EditMode testi** ile doğruladım:

- Grid koordinatının eşitliği ve hash'i
- Level'ın okunması, duvarların etrafından dolaşan ve sonradan açılan yollar
- Otobüs sırası ve bekleme yeri kuralları
- Binme, bekleme, zincirleme otobüs kalkışları, kazanma ve kaybetme
- Süre, kayıt sistemi, level okuma/kaydetme ve editör taslağı
- Gizli yolcular ve tutorial ipucu

Testler Play'e basmadan, birkaç saniyede çalışıyor.

---

## 🧭 Nasıl çalıştım

- **Küçük ve çalışan adımlar:** Her parçayı önce test edip sonra anlamlı bir mesajla commit'ledim. Commit geçmişi projenin katman katman nasıl kurulduğunu anlatıyor: önce veri ve kurallar, sonra görüntü, sonra akış ve editör.
- **Önce kurallar, sonra görüntü:** Yol bulmayı ve hamle çözümünü ekranda tek bir yolcu yokken testlerle doğruladım. Görüntüyü eklediğimde oynanışla ilgili bir hatayla karşılaşmadım.
- **Gerekli testlerin yapılması:** Validator'ın çalıştığını görmek için level dosyalarını bilerek bozdum. Level'ların çözülebilir olduğunu bir solver script'i ile kontrol ettim.

---


## 🙏 Ek kaynaklar

| | |
|---|---|
| **3D modeller** | TODO (adı, yapımcısı, linki, lisansı) |
| **Sesler** | [Kenney](https://kenney.nl): Interface Sounds ve Music Jingles (CC0) |
| **Font** | [Fredoka](https://fonts.google.com/specimen/Fredoka) (SIL Open Font License) |
| **Animasyon** | [DOTween](http://dotween.demigiant.com) (Demigiant) |
| **Model içe aktarma** | [glTFast](https://github.com/atteneder/glTFast) |
| **İllüstrasyonlar, başlık ekranı, uygulama ikonu, tutorial eli** | ChatGPT ile üretildi |
| **Arayüz görselleri** (kartlar, butonlar, ikonlar, kurdeleler) | Bu proje için hazırlandı |
