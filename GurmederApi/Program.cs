using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using GurmederApi.Data;
using GurmederApi.Seed;
using GurmederApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Render (ve benzeri PaaS platformları) uygulamanın hangi portu dinleyeceğini
// PORT ortam değişkeniyle bildirir — sabit bir port yerine bunu okumak,
// projeyi Render'a uyumlu kılar. Yerelde bu değişken yoksa launchSettings.json
// zaten http://localhost:5080'i kullanır, burası devreye girmez.
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// ---- servisler ----
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserAccessor>();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=gurmeder.db";

// Render'a bağlı bir Postgres veritabanı varsa, DATABASE_URL ortam
// değişkenini otomatik olarak sağlar (render.yaml'daki fromDatabase
// bağlantısı, bkz. proje kökü). Bu değişken varsa Postgres'e geçilir —
// SQLite'ın Render'ın GEÇİCİ (ephemeral) disk alanında her yeniden
// başlatmada/deploy'da SİLİNMESİ riski (kayıtlı kullanıcılar, favoriler
// kaybolur) böylece ortadan kalkar. Yoksa (yerel geliştirme) hiçbir şey
// değişmez, SQLite kullanılmaya devam eder.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var npgsqlConnectionString = ConvertRenderDatabaseUrl(databaseUrl);
    builder.Services.AddDbContext<GurmederDbContext>(options => options.UseNpgsql(npgsqlConnectionString));
}
else
{
    builder.Services.AddDbContext<GurmederDbContext>(options => options.UseSqlite(connectionString));
}

// Gerçek, çerez tabanlı oturum. Bu bir JSON API olduğundan, kimliği
// doğrulanmamış bir isteği ASP.NET Core'un varsayılan davranışı olan bir
// giriş SAYFASINA yönlendirmek yerine (302), doğrudan 401/403 durum kodu
// dönmesi gerekir — aksi halde ön yüzün fetch() çağrıları bunu bir hata
// olarak değil, farkında olmadan bir HTML sayfası olarak görür.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "gurmeder_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Ön yüz artık normalde bu API ile AYNI origin'den sunulur (wwwroot,
// aşağıya bkz.) — bu durumda CORS gerekmez. Yine de yerel geliştirmede
// ayrı bir statik sunucu (ör. VS Code Live Server :5500) kullanmak
// isteyenler için yaygın portlar serbest bırakıldı; AllowCredentials,
// çerezin bu senaryoda da gönderilebilmesi içindir.
const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080", "http://127.0.0.1:8080",
                "http://localhost:5500", "http://127.0.0.1:5500",
                "http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ---- veritabanını oluştur + tohumla ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GurmederDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db, app.Environment.ContentRootPath);
}

// ---- HTTP boru hattı ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // ham OpenAPI JSON şu adreste: /openapi/v1.json
}

// Ön yüz dosyaları (eski gurmeder-project) artık wwwroot/ altında — bu iki
// satır kök adrese (/) gelen isteklerde wwwroot/index.html'i, /css, /js,
// /images altındaki her şeyi doğrudan sunar. UseDefaultFiles MUTLAKA
// UseStaticFiles'tan ÖNCE gelmeli (istek yolunu index.html'e çevirir,
// sonra UseStaticFiles gerçekten sunar).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Render'ın (ve çoğu PaaS'ın) sağladığı DATABASE_URL, URI biçimindedir:
/// postgres://kullanici:sifre@host:port/veritabani — ama Npgsql'in
/// bağlantı dizesi biçimi anahtar=değer çiftleridir (Host=...;Username=...).
/// Npgsql'in kendi URI ayrıştırması sürüm/duruma göre tutarsız
/// davranabildiğinden, dönüşüm burada elle ve açıkça yapılır — hangi
/// parçanın nereye gittiği net, tahmine dayalı bir davranışa güvenilmiyor.
/// SSL zorunlu kılınır (Render'ın yönetilen Postgres'i bunu gerektirir);
/// TrustServerCertificate, Render'ın sertifika zincirinin yerel güven
/// deposunda olmayabileceği PaaS senaryoları için yaygın, pragmatik bir
/// ayardır.
/// </summary>
static string ConvertRenderDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfoParts = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfoParts[0]);
    var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : "";
    var database = uri.AbsolutePath.TrimStart('/');

    var connStringBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = username,
        Password = password,
        Database = database,
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
    };
    return connStringBuilder.ConnectionString;
}
