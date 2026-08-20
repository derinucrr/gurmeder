# GURMEDER API

GURMEDER artık statik bir JSON dosyası yerine gerçek bir **ASP.NET Core
Web API + veritabanı** üzerinde çalışıyor — gerçek kullanıcı girişi,
kategori yönetimi, görsel yükleme ve sunucu tarafında saklanan
favorilerle birlikte. Ön yüz (eski `gurmeder-project`) artık bu API'nin
`wwwroot/` klasöründe yaşıyor — ikisi **tek bir uygulama** olarak
dağıtılıyor.

> **⚠️ Önemli, dürüst bir not:** Bu proje, .NET SDK'sının kurulu
> *olmadığı* ve internet erişimi *olmayan* bir ortamda yazıldı — yani kod
> hiçbir zaman gerçekten derlenip çalıştırılamadı. Standart, iyi bilinen
> ASP.NET Core + EF Core kalıplarını kullanarak, her satırı elden
> geçirerek yazdım; kullandığım her NuGet paket sürümünün (EF Core
> Sqlite/Design/OpenApi 10.0.10, Npgsql.EntityFrameworkCore.PostgreSQL
> 10.0.3) gerçekten var olduğunu web araması yaparak tek tek doğruladım.
> Yine de **ilk `dotnet build` denemesinde küçük bir hata çıkma ihtimali
> gerçek**. Ön yüz tarafındaki her değişikliği (favoriler, giriş/kayıt
> akışı dahil) gerçek bir tarayıcıda, sahte ama API'nin gerçek şeklini
> birebir taklit eden bir sunucuya karşı uçtan uca test edip doğruladım —
> bu kısımda güvenim yüksek. Bir derleme hatası alırsanız, tam hata
> mesajını paylaşın; birlikte hızlıca düzeltiriz.

## Neler var

- **Gerçek kullanıcı girişi** — kayıt, giriş, çıkış. Şifreler asla düz
  metin saklanmaz (PBKDF2, tuzlanmış, 100.000 iterasyon — bkz.
  `Services/PasswordService.cs`); oturum HttpOnly bir çerezle yönetilir.
  Giriş denemeleri, kayıtlı olmayan bir e-postanın yanıt süresinden
  anlaşılamayacağı şekilde (timing-attack korumalı) yazıldı.
- **Kategoriler** artık gerçek bir tablo (`Categories`) — Admin rolündeki
  kullanıcılar ekleyebilir/yeniden adlandırabilir/silebilir
  (`Controllers/CategoriesController.cs`).
- **Görsel yükleme** — `POST /api/upload/image`, yalnızca Admin.
  JPG/PNG/WEBP, maksimum 5 MB, rastgele (GUID) dosya adıyla
  `wwwroot/images/` altına kaydedilir.
- **Favoriler** giriş yapıldığında sunucuda saklanır (`User.FavoriteRecipeIds`)
  — aynı hesapla farklı cihazlarda senkron kalır. Misafirken hâlâ
  `localStorage` kullanılır; giriş yapıldığında mevcut misafir favorileri
  otomatik olarak hesaba aktarılır.
- **Render'a dağıtıma hazır** — `Dockerfile` + `render.yaml` ile, GitHub'a
  push edip Render'da "New Blueprint" demeniz yeterli (bkz. aşağısı).
  Render'a bağlı bir Postgres veritabanı varsa otomatik olarak
  kullanılır; yoksa (yerel geliştirme) SQLite'a devam edilir.

Demo bir **Admin hesabı** otomatik oluşturulur (`admin@gurmeder.com` /
`Gurmeder2026!`) — kategori/görsel yönetimi uçlarını denemek için.
**Herkese açık bir yere dağıtıyorsanız bu şifreyi hemen değiştirin** ya da
hesabı silin (bkz. `Seed/DbSeeder.cs`).

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) —
  Kasım 2025'te çıkan güncel LTS sürümü (destek Kasım 2028'e kadar).
- Yerel geliştirme için ekstra bir veritabanı sunucusu **gerekmez** —
  SQLite tek bir dosya (`gurmeder.db`), ilk çalıştırmada otomatik
  oluşturulur.

## Yerelde çalıştırma

```bash
cd GurmederApi
dotnet restore
dotnet run
```

İlk çalıştırmada:
1. `gurmeder.db` adında bir SQLite dosyası oluşturulur.
2. `SeedData/recipes.json` içindeki **89 gerçek tarif**, kategorileri ve
   demo Admin hesabı otomatik olarak yüklenir (`Seed/DbSeeder.cs`).
3. Uygulama `http://localhost:5080` adresinde ayağa kalkar — **hem API
   hem ön yüz aynı adreste**: tarayıcıda `http://localhost:5080` açtığınızda
   siteyi, `http://localhost:5080/api/recipes` adresinde ham API
   yanıtını görürsünüz.

Veritabanını sıfırlamak isterseniz `gurmeder.db` dosyasını silip
`dotnet run`'ı tekrar çalıştırmanız yeterli.

### Ayrı bir statik sunucuyla çalıştırmak isterseniz

Ön yüz artık `wwwroot/` altında olduğundan normalde ayrı bir sunucuya
gerek yok. Yine de VS Code Live Server gibi bir araç tercih ederseniz,
`wwwroot` klasörünü ayrı sunup API'yi `dotnet run` ile `:5080`'de
çalıştırabilirsiniz — CORS zaten `5500`, `8080`, `3000` portlarına izin
verecek şekilde ayarlı (`Program.cs`, `AllowCredentials` dahil, çerezin
bu senaryoda da gönderilebilmesi için).

## Render'a dağıtım

1. Bu `gurmeder-api` klasörünü bir GitHub reposuna push edin (`Dockerfile`
   ve `render.yaml` kök dizinde olmalı).
2. [Render Dashboard](https://dashboard.render.com)'da **New +** →
   **Blueprint** → reponuzu seçin.
3. Render, `render.yaml`'ı okuyup hem `gurmeder-api` web servisini hem
   `gurmeder-db` Postgres veritabanını otomatik oluşturur ve birbirine
   bağlar (`DATABASE_URL` ortam değişkeni üzerinden — elle bir şey
   girmeniz gerekmez).
4. İlk açılışta uygulama Postgres'e karşı aynı seed işlemini yapar (89
   tarif + demo Admin) — **bu demo şifreyi canlıya çıkar çıkmaz
   değiştirin.**

`render.yaml`'daki `free` plan, Render'ın ücretsiz Postgres'inin **30 gün
sonra süresi dolan geçici bir veritabanı** olduğunu unutmayın — gerçek,
kalıcı bir dağıtım için ücretli bir plana geçmeniz gerekir.

### SQLite → Postgres nasıl işliyor

`Program.cs`, `DATABASE_URL` ortam değişkenini kontrol eder:
- **Varsa** (Render, Postgres bağlıyken): bu URI (`postgres://kullanici:sifre@host:port/db`)
  Npgsql'in beklediği anahtar-değer biçimine elle dönüştürülüp
  (`ConvertRenderDatabaseUrl`, `Program.cs`'in sonunda) Postgres'e bağlanılır.
- **Yoksa** (yerel geliştirme): eskisi gibi SQLite kullanılır.

Bunun neden önemli olduğu: Render'ın disk alanı **geçicidir** — bir web
servisi her yeniden başlatıldığında/deploy edildiğinde sıfırlanır. Tarif
verisi için bu sorun değildi (her başlangıçta zaten aynı 89 tarifle
yeniden tohumlanıyordu) ama **kayıtlı kullanıcılar ve favorileri için
kabul edilemez** — Postgres'e geçiş bu veriyi kalıcı kılar.

## API dokümantasyonu

.NET 9'dan itibaren ASP.NET Core, Swashbuckle yerine kendi yerleşik
OpenAPI desteğini (`Microsoft.AspNetCore.OpenApi`) önerir; bu proje de
onu kullanıyor. Görsel bir Swagger UI **getirmez** — yalnızca ham OpenAPI
şemasını (yalnızca geliştirme ortamında) `/openapi/v1.json` adresinde
sunar.

## Veritabanı şeması

```
Categories
└─ Id, Name (benzersiz)

Recipes
├─ Id, Name (benzersiz), CategoryId (→ Categories), Description, Image
├─ Calories, Difficulty, Prep, Cook, Time, Servings, Pairs, Chef, Emo
├─ NutriCarb, NutriProtein, NutriFat, NutriFiber
└─ Tags (JSON metin sütunu olarak saklanan string listesi)

RecipeIngredients (Recipes'e RecipeId ile bağlı, 1-e-çok)
└─ Amount, Unit, Name, SortOrder

RecipeSteps (Recipes'e RecipeId ile bağlı, 1-e-çok)
└─ Title, Text, SortOrder

Users
├─ Id, FullName, Email (benzersiz), PasswordHash, Role ("User"/"Admin")
└─ FavoriteRecipeIds (JSON metin sütunu olarak saklanan int listesi)
```

**Neden Tags ve FavoriteRecipeIds ayrı bir tablo değil de JSON sütun?**
İkisi de basit, düz listeler — tarifler/kullanıcılar arası analiz veya
sorgu gerektirmiyorlar (ör. "hangi kullanıcılar bu tarifi favoriledi"
gibi bir sorgu yok). Ayrı bir join tablosu bu basitlik için gereksiz
karmaşıklık olurdu. Her ikisi için de EF Core'un doğru değişiklik takibi
yapabilmesi için özel bir `ValueConverter`/`ValueComparer` çifti
tanımlandı (`Data/GurmederDbContext.cs`) — bu olmadan, örneğin
`user.FavoriteRecipeIds.Add(id)` gibi bir liste mutasyonu EF Core
tarafından fark edilmez, `SaveChangesAsync()` sessizce hiçbir şey
kaydetmezdi.

**Neden Malzemeler/Adımlar ayrı tablo, ama Kategori de öyle?** Malzemeler
ve adımlar doğal olarak bire-çok ilişkiler (bir tarifin çok malzemesi/adımı
var). Kategori ise başlangıçta düz bir metin sütunuydu; admin panelinden
yönetilebilmesi (ekleme/yeniden adlandırma/silme, tarif sayısı takibi)
için gerçek bir ilişkiye (`CategoryId` → `Categories.Id`) taşındı.

## API çıktısı neden değişti (dizi → adlandırılmış alan)

Eski statik JSON'da bir malzeme `[300, "g", "kıyma"]` şeklindeydi
(konumsal). API artık `{"amount": 300, "unit": "g", "name": "kıyma"}`
döner (adlandırılmış) — EF Core + `System.Text.Json`'ın gerçek bir
veritabanından gelen adlandırılmış sütunları serileştirme şeklinin doğal
sonucu. Ön yüzde bunun için gereken değişiklik küçüktü (`js/app.js` ve
`js/recipes.js`'te `i[2]` → `i.name` gibi birkaç satır) ve gerçek bir
tarayıcıda uçtan uca doğrulandı.

## Ön yüz değişiklikleri (bu turda bulunan ve düzeltilen 2 gerçek boşluk)

Kod incelemesi sırasında favoriler akışında iki tutarsızlık bulundu ve
düzeltildi:

1. **Favori butonu sunucuyu hiç çağırmıyordu.** Backend'in
   `/api/favorites` uç noktaları vardı, giriş anında misafir
   favorilerini hesaba aktaran senkronizasyon da vardı — ama kalp
   ikonuna tıklamak (`toggleFav`, `js/app.js`) yalnızca `localStorage`'a
   yazıyordu. Giriş yapmış bir kullanıcı yeni bir tarifi favorileseydi,
   bu değişiklik sunucuya hiç gitmiyordu; başka bir cihazda görünmezdi.
   Düzeltildi: `toggleFav` artık giriş yapılmışsa arka planda ilgili
   `POST`/`DELETE /api/favorites/{id}` isteğini de atıyor.
2. **Sunucudan gelen favoriler `localStorage`'a yazılmıyordu.** Giriş
   sonrası `favorites` değişkeni sunucudaki listeyle güncelleniyordu ama
   `localStorage` eski haliyle kalıyordu — çerez süresi dolup misafir
   moduna dönüldüğünde bayat bir liste görünme riski vardı. Düzeltildi:
   `syncGuestFavoritesToServer` artık `saveFavoritesToStorage()` de
   çağırıyor.

Bu iki düzeltme, sahte ama gerçek API şeklini birebir taklit eden
durumlu (stateful) bir mock sunucuya karşı gerçek bir tarayıcıda test
edildi: giriş yapıldı, yeni bir tarif favorilendi, backend'e gerçekten
`POST /api/favorites/5` isteğinin gittiği doğrulandı, ardından sayfa
yenilenip favori sayısının sunucudan doğru geldiği (`localStorage`
senkron kalarak) teyit edildi — hepsi geçti.

## Klasör yapısı

```
gurmeder-api/
├── Dockerfile              # Render (ve genel Docker) için çok aşamalı build
├── render.yaml             # Render Blueprint — web servisi + Postgres
├── .dockerignore
└── GurmederApi/
    ├── GurmederApi.csproj
    ├── Program.cs           # DI, kimlik doğrulama, DB sağlayıcı seçimi, HTTP pipeline
    ├── Controllers/         # Recipes, Auth, Categories, Favorites, Upload
    ├── Models/               # Recipe, RecipeIngredient, RecipeStep, Category, User
    ├── DTOs/
    ├── Data/                 # GurmederDbContext
    ├── Services/             # PasswordService, CurrentUserAccessor
    ├── Seed/                 # DbSeeder — ilk açılışta veritabanını doldurur
    ├── SeedData/recipes.json # Tohum verisi (89 tarif)
    └── wwwroot/              # Ön yüz — eski gurmeder-project buraya taşındı
        ├── index.html
        ├── css/, js/, images/
```

`gurmeder-project/` klasörü (bu API'nin bir üst dizinindeki bağımsız
proje) hâlâ duruyor ama artık **kullanılmıyor** — `wwwroot/` içindeki
kopya güncel olan. Orijinal `data/recipes.json` da bu API'nin
`SeedData/recipes.json`'ı olarak kopyalandı; iki dosya aynı içeriğe
sahip.
