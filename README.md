# GURMEDER 🍅

**Ölçüsü tam, lezzeti kesin.**

GURMEDER, tarifleri sabit HTML yerine merkezi bir **veri/API katmanından**
sunan, tamamen statik (build aracı gerektirmeyen, GitHub Pages'te doğrudan
çalışan) bir tarif keşif sitesidir. **89 gerçek tarif** (Türk mutfağı
klasikleri + dünya mutfağından birkaç seçki), **79'u gerçek fotoğrafla**
eşleşiyor (62'si yerel dosya, 17'si benzersiz Unsplash fotoğrafı — bkz.
"Fotoğraflar" bölümü), kategori/zorluk/süre/kalori filtreleri,
sayfalama, "malzemeden tarif bul" motoru, porsiyona göre otomatik ölçeklenen
malzeme listeleri, besin değeri grafikleri ve gerçek tarayıcı geçmişiyle
çalışan bir "geri" deneyimi içerir.

## Klasör yapısı

```
gurmeder-project/
├── index.html          # SPA kabuğu — görünümler JS ile değiştirilir, sayfa yenilenmez
├── css/
│   └── styles.css      # Tasarım token'ları (toz pembe / gri / beyaz) + tüm bileşen stilleri
├── js/
│   ├── api.js           # API katmanı: data/recipes.json'ı fetch eder, hataları fırlatır
│   ├── recipes.js        # Veri katmanı: yükler, önbelleğe alır, id/kategori/arama sorgularını sağlar
│   └── app.js             # Sayfa/UI mantığı — ham veriye asla doğrudan erişmez
├── data/
│   └── recipes.json     # 89 tarifin tamamı, tek kaynak (single source of truth)
├── images/
│   ├── README.md        # Fotoğrafların kaynağı ve yeni fotoğraf ekleme kuralları
│   └── *.jpg             # 7 gerçek tarif fotoğrafı
├── .gitignore
└── README.md
```

## Veri akışı (API mimarisi)

```
data/recipes.json  →  api.js (fetch)  →  recipes.js (önbellek + sorgular)  →  app.js (render)
```

- **`js/api.js`** — tek işi var: `data/recipes.json`'ı `fetch()` ile çekmek. Ağ hatası,
  HTTP hatası veya geçersiz JSON durumunda anlamlı bir `Error` fırlatır; hiçbir zaman
  sessizce başarısız olmaz. Gerçek bir Recipe API'ye geçmek isterseniz değişmesi
  gereken **tek dosya** budur.
- **`js/recipes.js`** — `loadRecipes()` verinin sadece bir kez çekilmesini sağlar
  (bellek içi önbellek + eşzamanlı çağrıları tekilleştirme), sonra `getRecipeById(id)`,
  `getRecipesByCategory(category)`, `searchRecipes(query)`,
  `matchRecipesByIngredients(list, category?)`, `getFeaturedRecipes()` gibi sorgu
  fonksiyonlarını sağlar. `app.js` tarif verisine **hiçbir zaman doğrudan erişmez** —
  her zaman bu fonksiyonlar üzerinden gider.
- **`js/app.js`** — sayfa açılışında `await loadRecipes()` bekler; yüklenirken
  "Tarifler yükleniyor..." gösterilir, başarısız olursa "Tarifler şu anda
  yüklenemiyor. Lütfen tekrar deneyin." + **Tekrar Dene** butonu gösterilir.

### ⚠️ Yerel test için sunucu gerekir

Veri artık `fetch()` ile çekildiği için, `index.html`'i doğrudan çift tıklayıp
(`file://` protokolüyle) açmak **çalışmaz** — tarayıcılar güvenlik nedeniyle
`file://` üzerinden yerel JSON dosyalarının fetch edilmesini engeller. Bunun
yerine basit bir yerel sunucu kullanın:

```bash
# proje klasöründe
python3 -m http.server 8080
# tarayıcıda http://localhost:8080 adresini açın
```

VS Code'da **Live Server** eklentisiyle `index.html` üzerine sağ tıklayıp
*Open with Live Server* de diyebilirsiniz. **GitHub Pages üzerinde bu sorun
zaten yoktur** — Pages her zaman http(s) üzerinden servis eder, `fetch()`
sorunsuz çalışır. (Fetch başarısız olsa bile site çökmez — yukarıdaki hata
ekranı ve **Tekrar Dene** butonu devreye girer.)

## Tarif veri şeması

```js
{
  "id": 1,
  "name": "Karnıyarık",
  "category": "Ana Yemekler",
  "description": "…",
  "image": null,
  "calories": 412,
  "difficulty": "Orta",
  "time": 65, "prep": 25, "cook": 40, "servings": 4,
  "pairs": "Pilav, cacık",
  "tags": ["Klasik", "Fırın", "Ev Yapımı", "Ana Yemekler"],
  "chef": "…",
  "nutri": { "carb": 18, "protein": 22, "fat": 27, "fiber": 6 },
  "ingredients": [[6, "adet", "orta boy patlıcan"], [400, "g", "kıyma"], …],
  "steps": [["Patlıcanları hazırlayın", "…"], …],
  "emo": "🍆"
}
```

`ingredients` ve `steps` bilinçli olarak düz string dizisi değil, **yapılandırılmış
çiftler** olarak tutulur (`[miktar, birim, isim]` ve `[başlık, açıklama]`) — bu,
"6 adet orta boy patlıcan" gibi doğru biçimlendirilmiş gösterimi VE porsiyona göre
canlı miktar ölçeklemesini (`js/app.js`'teki servings stepper) mümkün kılar. Düz
string olsaydı bu iki özellik kaybolurdu. Yeni bir tarif eklemek isterseniz
`data/recipes.json`'a bu şemaya uygun bir obje eklemeniz yeterli — kart, filtre,
kategori sayacı ve arama otomatik olarak günceli yansıtır, hiçbir HTML elle
değiştirilmez.

**`rating`/`rc` (yıldız puanı, değerlendirme sayısı) alanları yoktur** — bkz.
aşağıdaki "Neden yıldız puanı yok?" bölümü.

## Fotoğraflar — gerçek, tarife özel, asla kırık görsel yok

Fotoğraf mantığı kasıtlı olarak basittir: bir tarifin `image` alanı doluysa
(yerel bir dosya yolu **veya** tam bir URL), kart ve detay sayfası doğrudan
o gerçek fotoğrafı gösterir — CSS `background-image` ikisini de aynı şekilde
işler. Boşsa (`null`), kategoriye özel gradyan + emoji tasarımına zarifçe
düşülür — **hiçbir zaman kırık görsel simgesi görünmez**, çünkü hiçbir
uzak/otomatik fotoğraf çekme denemesi yoktur; sadece `image` alanına bakılır.

Şu an **88 tarif gerçek fotoğrafla eşleşiyor**, sekiz kaynaktan (dokuzuncu
küçük bir güncelleme daha oldu — bkz. madde 8'in sonu):

1. **7'si** ilk yerel dosya arşivinden (`images/*.jpg`) — kullanıcı tarafından
   yüklenen gerçek bir fotoğraf arşivinden tek tek incelenip doğrulanarak
   seçildi (bkz. `images/README.md`).
2. **17'si benzersiz Unsplash fotoğrafı** — ikinci bir veri kaynağından
   gelen 30 tarifin fotoğraf URL'leri arasında, **5 grup halinde 11 tarifin
   birbiriyle aynı URL'yi paylaştığı** tespit edildi (ör. Karnıyarık ile
   İmam Bayıldı, ya da üç farklı tatlının tamamı aynı fotoğrafı
   kullanıyordu). Bir fotoğraf iki farklı tarife birden gerçekten ait
   olamayacağından, **paylaşılan URL'ler hiçbir tarife atanmadı** —
   yalnızca başka hiçbir tarifle paylaşılmayan, benzersiz URL'ler kullanıldı.
   Bu URL'ler `images.unsplash.com` üzerinde barındığından, göstermek için
   internet bağlantısı gerekir (yerel dosyaların aksine). (İki tanesi —
   Mantı ve Lahmacun — sonradan gerçek yerel dosyalarla değiştirilerek bu
   dış bağımlılıktan kurtarıldı, bkz. madde 4.)
3. **20'si ikinci bir yerel dosya turu** — kullanıcı, tarif adlarıyla
   birebir eşleşen dosya adlarıyla (`karniyarik.jpg`, `humus.jpg`, vb.) 20
   gerçek fotoğraf yükledi; her biri görsel olarak doğrulanıp (ör.
   Karnıyarık'ın et dolgulu, İmam Bayıldı'nın etsiz zeytinyağlı dolgulu
   olması gibi ayırt edici detaylar kontrol edilerek) proje adlandırma
   kuralına uygun şekilde `images/` klasörüne eklendi.
4. **19'u üçüncü bir yerel dosya turu** — aynı şekilde, tarif adlarıyla
   birebir eşleşen 19 gerçek fotoğraf daha eklendi; bunlardan ikisi
   (Mantı, Lahmacun) önceden madde 2'deki Unsplash URL'sini kullanan
   tarifleri, daha güvenilir yerel dosyalarla değiştirdi.
5. **17'si dördüncü bir yerel dosya turu** — yine tarif adlarıyla eşleşen
   20 fotoğraf daha yüklendi ve her biri görsel olarak incelendi:
   - Üç dosya (`pogaca.jpg`, `taze-istiridye.JPG`, `zerdecalli-altin-sut.JPG`)
     önceki turlarda zaten eklenmiş tariflerle aynı isimde geldi. İkisi
     (Poğaça, Taze İstiridye) halihazırda doğru ve iyi bir fotoğrafa sahip
     olduğundan değiştirilmedi; **Zerdeçallı Altın Süt ise yükseltildi** —
     eski fotoğraf yalnızca çiğ zerdeçal kökünü gösteren bir stil çekimiydi,
     yeni fotoğraf tarifin kendisi olan bitmiş içeceği (fincanda altın süt)
     gösteriyor, bu yüzden daha doğru olan yeni dosyayla değiştirildi.
   - Bir dosya (`sehriye-corbasi.jpg` adıyla yüklenen) görsel olarak
     incelendiğinde aslında bir tel şehriye çorbası değil, kremalı bir
     mantar çorbası gösterdiği görüldü — dosya adı yerine **gerçek görsel
     içeriğine göre** "Mantar Çorbası"na atandı, "Şehriye Çorbası"
     fotoğrafsız bırakıldı. *(Sonraki bir turda gerçek şehriye içeren
     doğru bir fotoğraf geldi — bkz. madde 6.)*
   - İki dosya daha, ilk atamada isimlerine göre eklenmiş olsa da sonradan
     yapılan görsel doğrulamada **içerik uyuşmazlığı** tespit edilip geri
     alındı: `salgam-suyu.jpg` aslında pancar suyu gösteriyordu (şalgam
     suyu kırmızı havuç ve şalgamdan yapılır, pancar farklı bir sebzedir);
     `sucuklu-yumurta.jpg` sadece kaşarlı sucuk gösteriyordu, hiç yumurta
     görünmüyordu. İkisi de fotoğrafsız bırakıldı — yanlış görsel
     göstermektense hiç göstermemek tercih edildi. *(Sucuklu Yumurta için
     sonraki bir turda yumurtası net görünen doğru bir fotoğraf geldi —
     bkz. madde 6. Şalgam Suyu için gelen ikinci dosya da yine pancar
     gösterdiğinden dışlanmaya devam ediyor.)*
   - Kalan 15 dosya doğrudan, doğru şekilde eşleşti.
6. **5'i altıncı bir yerel dosya turu** — 11 dosya daha yüklendi (ayrıca
   ana sayfa için ayrı bir banner görseli geldi, bkz. "Ana sayfa tasarımı"
   bölümü); 5'i zaten iyi bir fotoğrafa sahip tariflerle aynı isimde
   geldiği için (mevcutlar zaten doğru ve tarif içeriğiyle daha tutarlı
   olduğundan) değiştirilmeden bırakıldı. Baklava, önceden kullandığı
   Unsplash URL'sinden yerel dosyaya yükseltildi. Kalan 4'ü gerçek
   eksikleri doldurdu — bunlardan ikisi tam olarak madde 5'te fotoğrafsız
   bırakılan **Şehriye Çorbası** (bu kez gerçekten tel şehriye görünüyor)
   ve **Sucuklu Yumurta** (bu kez yumurta net görünüyor) için düzeltilmiş,
   doğru fotoğraflardı. Gelen ikinci `salgam-suyu.jpg` dosyası ise yine
   pancar gösterdiğinden aynı gerekçeyle dışlandı.
7. **5'i yedinci bir yerel dosya turu** — kullanıcının ayrı bir HTML/JS
   mini-sitesinden yapıştırdığı 6 tarifle birlikte 7 fotoğraf geldi. Bu
   dosyalardan biri (`fırında_sutlac.JPG`) zaten iyi bir fotoğrafa sahip
   olan Fırın Sütlaç ile aynı isimde geldiği için değiştirilmeden
   bırakıldı. Bir dosya (`zeytinyagli-yaprak-sarma.JPG`) **görünür bir
   "Creative Market" filigranı taşıdığı için kesinlikle kullanılmadı** —
   bu, satın alınmamış, lisanssız bir stok fotoğraf önizlemesi anlamına
   gelir ve kullanımı telif ihlali olurdu; ayrıca bu tarif zaten mevcut
   veri setinde vardı. Kalan 5 dosya, yapıştırılan metinden gelen 5 yeni
   tarife (Ev Yapımı Pizza, Pratik Milföy Börek, Çıtır Çıtır Katmer,
   Damla Çikolatalı Cookie, Çıtır Tavuk Parçaları) doğru şekilde atandı.
8. **5'i sekizinci bir yerel dosya turu** — kullanıcı, halihazırda dış
   Unsplash URL'si kullanan 6 tarif için doğrudan yerel dosyalarla
   **değiştirme** istedi (Zeytinyağlı Yaprak Sarma, Su Böreği, Revani,
   Peynirli Sigara Böreği, İçli Köfte, Tas Kebabı, İskender Kebap — 7
   dosya). İncelemede **iki dosyada görünür filigran/marka tespit edildi
   ve hiçbiri kullanılmadı**: `zeytinyagli-yaprak-sarma.JPG` yine aynı
   "Creative Market" filigranını taşıyordu (bir önceki turda reddedilen
   dosyayla aynı sorun); `iskender-kebap.JPG` ise sol üst köşede "I ❤
   FOOD" marka/logo yazısı içeriyordu — bu da fotoğrafın belirli bir
   içerik üreticisine ait, izinsiz kullanılamayacak bir görsel olduğunu
   gösteriyordu. Bu iki tarifin **mevcut, çalışan Unsplash fotoğrafları
   değiştirilmeden bırakıldı** — hiç fotoğrafsız kalmaktansa var olan
   doğru fotoğrafı korumak tercih edildi. Kalan 5 dosya, mevcut dış
   URL'leri yerel dosyalarla değiştirerek (veya tek eksik olan Peynirli
   Sigara Böreği'nin boşluğunu doldurarak) başarıyla uygulandı.

   *Küçük bir devam notu:* kullanıcı daha sonra "Zeytinyağlı Yaprak
   Sarma" için **üçüncü kez** bir fotoğraf yükledi (ilk ikisi aynı
   filigranlı dosyaydı). Bu sefer dosya hash'i öncekiyle karşılaştırılıp
   gerçekten farklı olduğu doğrulandı, görsel yeniden incelendi ve
   filigran/marka bulunamadı — kabul edilip mevcut Unsplash URL'sinin
   yerine yerel dosya olarak eklendi.

Turlar 3-8'de eklenen tüm dosyalar, orijinallerinden ortalama %47 daha
küçük dosya boyutuna sıkıştırıldı (900px uzun kenar, JPEG q80) — sayfa
performansını korumak için.

Kalan tarifler fotoğrafsız — bu, "gerçek olmayan/belirsiz bir fotoğraf
koymaktansa hiç koymamak" tercihinin doğal sonucu. Bir tarife gerçek
fotoğraf eklemek için `image` alanını yerel bir dosya yolu ya da bir URL
ile doldurmanız yeterli — kod değişikliği gerekmez.

## Veri seti nasıl büyüdü (394→59→74→84→89)

Proje başlangıçta kombinatoryal olarak **üretilmiş** 394 sahte tarifle
kuruldu; bunlar tamamen atılıp yerine **59 gerçek, tek tek doğrulanmış**
tarifle değiştirildi. Ardından ikinci bir kaynaktan **30 tarif daha**
sağlandı: bunların **15'i** isim bazında zaten mevcut tarifle aynıydı
(ör. "Mercimek Çorbası") — bu 15'i **tekrar eklemek yerine**, zaten var
olan yapılandırılmış (miktar+birim+isim, adım adım) versiyonları korundu;
yalnızca güvenle atanabilecek fotoğrafları mevcut kayıtlara eklendi.
**15'i gerçekten yeniydi** (ör. "Baklava", "İskender Kebap") — bunlar
sağlanan malzeme/yapılış bilgisi referans alınarak, projenin şemasına
uygun şekilde yeniden yazılıp eklendi. Bir istisna: "Tavuk Göğsü Tatlısı"
adıyla sağlanan malzeme listesinde, bu tarifin adını taşıyan asıl unsur —
didiklenmiş tavuk göğsü — eksikti (verilen tarif aslında sade bir süt
muhallebisiydi); bu, tarifin kendi otantik içeriğiyle düzeltilerek eklendi.

Üçüncü bir kaynaktan **400 tarif** sağlandı. İncelemede, bu 400 girdinin
aslında yalnızca **20 gerçekten farklı tarifin** ("Özel Soslu", "Ev Yapımı",
"Pratik", "Fırında", "Yöresel", "Gourmet", "Saray Usulü" gibi isim
önekleriyle) tam **20'şer kez mekanik olarak çoğaltılmış** hâli olduğu
tespit edildi — malzeme ve yapılış içeriği harfiyen aynıydı, yalnızca bir
"(Aşama N: ...)" sayacı artıyordu (öyle ki "Fırında Mercimek Çorbası" bile
ocak-üstü tarifle birebir aynı talimatları taşıyordu). Bu 20 tarifin 10'u
isim bazında zaten mevcuttu (dokunulmadı), **10'u gerçekten yeniydi** (ör.
"Tiramisu", "Kayseri Mantısı", "San Sebastian Cheesecake") ve eklendi.
Bu dosyadaki fotoğraflar için ayrı bir sorun tespit edildi: yalnızca 34
benzersiz görsel, 400 girdiye neredeyse rastgele dağıtılmıştı — tek bir
tarifin kendi 20 isim-varyasyonu bile birbirinden farklı 9 fotoğrafı
paylaşıyor, üstelik bu fotoğraflar tamamen alakasız başka tariflerle de
ortak kullanılıyordu. Güvenilir bir tarif↔fotoğraf eşleşmesi çıkarılamadığı
için, bu turda eklenen 10 tarifin hiçbirine fotoğraf atanmadı.

İki tarif ismi daha netlik için düzeltildi: "Bruşetta (Domates ve Krem
Peynirli)" → **"Bruschetta (Domates ve Krem Peynirli)"** (İtalyanca
orijinal yazımı) ve "Çilekli Yulaf Kavanozu" → **"Çilekli Yulaf"**
(sadeleştirilmiş isim). İsim değişikliği yapılırken `js/recipes.js`
içindeki `FEATURED_NAMES` listesi de güncellendi — aksi halde bu iki
tarif eski isimleriyle arandığından ana sayfanın "Bu haftanın gözdeleri"
listesinden sessizce düşerlerdi.

Son olarak, bağımsız bir HTML/JS mini-sitesi olarak yapıştırılan 6 tarif
projenin şemasına uyarlandı: kaynakta malzemeler tek satırda virgülle
birleştirilmiş haldeydi (ör. `"500g un, 1 paket kuru maya, 1 tatlı kaşığı
şeker, 1 çay kaşığı tuz"`); bunlar ayrıştırılıp `[miktar, birim, isim]`
üçlülerine bölündü. Talimatlar zaten adım adımdı, her birine kısa bir
başlık eklendi. Kategori, süre, zorluk, porsiyon, kalori ve şef notu
kaynakta hiç yoktu — bunlar gerçek mutfak bilgisiyle eklendi. 6 tariften
5'i gerçekten yeniydi ve eklendi; "Zeytinyağlı Yaprak Sarma" zaten mevcut
veri setinde olduğundan tekrar eklenmedi.

## Ana sayfa tasarımı

Ana sayfanın hero bölümündeki fotoğraf tamamen kaldırılıp yerine düz,
tek renkli bir daire (`var(--pink-tint)`) kondu — hiçbir gradyan,
fotoğraf ya da ona bağlı bilgi kartı yok, kasıtlı olarak sade.

Bu dairenin tamamen boş kalmaması için, içine elle çizilmiş bir çizgi
illüstrasyonu eklendi: markanın "ölçüsü tam" temasına doğrudan bağlı bir
ölçü kabı (üzerinde pembe ölçü çizgileriyle), etrafında yavaşça dönen bir
çelenk — limon dilimi, biber, buğday başağı, ot yaprakları ve tohum
noktalarından oluşan, sıcak renk dolgularıyla zenginleştirilmiş bir
kompozisyon (`.hero-wreath`). Kap sabit kalırken (`.hero-jug`) çelenk
etrafında 100 saniyede bir tam tur atacak şekilde çok yavaş döner
(`heroRotate` animasyonu) — göze batmayan ama sayfayı "canlı" hissettiren
bir hareket. Kabın ağzından da soluk, yükselip kaybolan buhar dalgaları
yükseliyor (`.hero-steam`, `steamRise` animasyonu, iki dalga farklı
gecikmeyle döngüleniyor). Bu bir fotoğraf değil — kasıtlı olarak zarif
ama gösterişli, dekoratif bir SVG; `index.html` içine gömülü
(`.hero-illustration` sınıfı). Her iki animasyon da projenin zaten var
olan `prefers-reduced-motion` kuralına tabi — hareket hassasiyeti olan
kullanıcılarda otomatik olarak durur. *(Uygulama sırasında iki şey fark
edildi ve düzeltildi: 1) `.hero-plate` dairesi kare değil, oval bir kutu
içinde `border-radius:50%` ile çiziliyor; illüstrasyona `width:%` +
`height:%` ile bağımsız yüzde verilince oval şekle göre yatay eksende
gerilip bozuluyordu — `height:%` + `aspect-ratio:1/1` kombinasyonuna
geçilerek illüstrasyonun kendi oranını koruması sağlandı. 2) Buhar
animasyonu ilk yazımda üst gruba statik `opacity:0` koyup alt elemanları
ayrıca animasyonla açmaya çalışıyordu — SVG/CSS'te opacity iç içe
gruplarda çarpımsal olduğundan üst grubun sıfır opaklığı, alt elemanların
kendi animasyonu ne olursa olsun her şeyi görünmez kılıyordu; düzeltme
olarak sıfır opaklık başlangıcı doğrudan animasyonlu elemanların kendi
üzerine taşındı.)*

Ayrıca iki yeni bölüm eklendi, ikisi de mevcut pembe/toz-pembe paletle
uyumlu:

- **Baharat bannerı** (`"Bu haftanın gözdeleri"` ile `"Neden GURMEDER?"`
  arasında): kullanıcının sağladığı bir baharat fotoğrafının üzerine,
  soldan sağa `--paprika-deep` renginden şeffaflaşan bir gradyan
  bindirilerek elde edildi — metin okunabilir kalırken markanın pembe
  tonu görselin üzerinde baskın kalıyor, fotoğrafın kendisi de sağ
  tarafta belirgin şekilde görünüyor. *(Uygulama sırasında bir hata
  yakalandı: CSS dosyası `css/styles.css`'te olduğu için `url('images/...')`
  gibi bir yol, tarayıcı tarafından CSS dosyasının kendi konumuna göre
  çözümlenip `css/images/...`'a bakıyor ve görsel hiç yüklenmiyordu; yol
  `url('../images/...')` olarak düzeltildi.)*
- **"Neden GURMEDER?" kart ızgarası**: eskiden düz bir şeritte yan yana
  duran 4 özellik (net gramaj, şef notları, vb.), her biri kendi beyaz
  kartına, yumuşak gölgesine ve üzerine gelince hafifçe yukarı kalkan
  geçiş efektine kavuşturuldu; ikon rozetleri markanın pembe tonunda.

## Neden yıldız puanı yok?

Önceki bir sürümde her tarifte sahte bir "★ 4.8 (128 değerlendirme)" etiketi
vardı. Bu projede gerçek kullanıcı değerlendirmesi verisi olmadığından, bu
sayılar tamamen uydurmaydı — kaldırıldı. Ana sayfadaki "Bu haftanın
gözdeleri" bölümü artık sahte bir "popülerlik" puanına göre değil,
`js/recipes.js`'teki `getFeaturedRecipes()` fonksiyonunda tanımlı, editöryel
olarak seçilmiş sabit bir listeye göre çalışır (önce gerçek fotoğrafı olan
7 tarif, ardından fotoğrafsız kalan kategorilerden birer klasik). Tarifler
sayfasındaki sıralama seçenekleri de yalnızca gerçek/objektif verilere
dayanır: süre, kalori, isim (A-Z).

## Görünümler ve tarayıcı geçmişi

`index.html` içinde dört ana görünüm bulunur, `switchView()` ile aralarında
geçiş yapılır: **Ana Sayfa**, **Tarifler** (filtre + arama + sayfalama),
**Malzemeden Tarif Bul** (uyumluluk yüzdesiyle sıralı sonuçlar), **Tarif
Detayı**. Her görünüm değişikliği ve her tarif açılışı `history.pushState`
ile kaydedilir; detay sayfasındaki **← Geri** butonu ve tarayıcının kendi
geri tuşu birebir aynı şekilde çalışır ve sizi geldiğiniz kategoriye/filtreye
geri döndürür — native "back" davranışı bozulmaz.

Favoriler `localStorage`'da saklanır, sayfa yenilense bile kaybolmaz.

## Çalıştırma

```bash
python3 -m http.server 8080
# http://localhost:8080
```

## Kullanılan teknolojiler

- Saf HTML5 / CSS3 / vanilla JavaScript (framework yok, build adımı yok)
- Google Fonts: [Playfair Display](https://fonts.google.com/specimen/Playfair+Display) (başlıklar — süslü, yüksek kontrastlı bir vitrin serifi), [Inter](https://fonts.google.com/specimen/Inter) (gövde), [JetBrains Mono](https://fonts.google.com/specimen/JetBrains+Mono) (ölçüler)
- Veri: `data/recipes.json` (yerel, statik) — harici bir API veya anahtar gerektirmez

## Tasarım notları

- Renk paleti: toz pembe `#C48A93` (birincil vurgu), koyu toz pembe `#9C5B68`,
  sıcak gri `#8C8177` (ikincil vurgu), açık gri `#F1ECEA` ve beyaz zeminler,
  koyu gri `#3A3532` metin.
- Tipografi: başlıklarda Playfair Display'in yüksek kontrastlı, süslü
  vitrin karakteri (700-800 ağırlık, italikte 600) kullanılırken gövde
  metninde okunabilirlik için Inter'de kalındı — dekoratif bir marka
  kimliği ile günlük kullanım metninin okunabilirliği dengelendi.
- "Ölçü" teması: hazırlama/pişirme/toplam süre/kalori/zorluk için dairesel
  gösterge (gauge) bileşenleri ve porsiyona göre canlı ölçeklenen malzeme
  miktarları, markanın "ölçüsü tam, lezzeti kesin" sloganını arayüze taşır.

## Lisans

Bu proje bir tasarım/geliştirme örneği olarak sunulmuştur; içerik ve kod
dilediğiniz gibi düzenlenebilir. `images/` altındaki fotoğrafların kaynağı
ve olası kullanım kısıtları için `images/README.md`'yi okuyun — bu site
herkese açık/ticari olarak yayınlanacaksa fotoğrafların lisansını kendiniz
doğrulamanız önerilir.
