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




## Çalıştırma

```bash
python3 -m http.server 8080
# http://localhost:8080
```

## Kullanılan teknolojiler

- Saf HTML5 / CSS3 / vanilla JavaScript (framework yok, build adımı yok)
- Google Fonts: [Playfair Display](https://fonts.google.com/specimen/Playfair+Display) (başlıklar — süslü, yüksek kontrastlı bir vitrin serifi), [Inter](https://fonts.google.com/specimen/Inter) (gövde), [JetBrains Mono](https://fonts.google.com/specimen/JetBrains+Mono) (ölçüler)
- Veri: `data/recipes.json` (yerel, statik) — harici bir API veya anahtar gerektirmez


## Lisans

Bu proje bir tasarım/geliştirme örneği olarak sunulmuştur; içerik ve kod
dilediğiniz gibi düzenlenebilir. `images/` altındaki fotoğrafların kaynağı
ve olası kullanım kısıtları için `images/README.md`'yi okuyun — bu site
herkese açık/ticari olarak yayınlanacaksa fotoğrafların lisansını kendiniz
doğrulamanız önerilir.
