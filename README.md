# GURMEDER API

GURMEDER artık statik bir JSON dosyası yerine gerçek bir **ASP.NET Core
Web API + veritabanı** üzerinde çalışıyor — gerçek kullanıcı girişi,
kategori yönetimi, görsel yükleme ve sunucu tarafında saklanan
favorilerle birlikte. Ön yüz (eski `gurmeder-project`) artık bu API'nin
`wwwroot/` klasöründe yaşıyor — ikisi **tek bir uygulama** olarak
dağıtılıyor.


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


