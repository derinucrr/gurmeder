using System.Collections.Generic;

namespace GurmederApi.Models;

/// <summary>
/// Kayıtlı bir kullanıcı. Şifre KESİNLİKLE düz metin olarak tutulmaz —
/// yalnızca <see cref="PasswordHash"/> saklanır, bu da ASP.NET Core'un
/// kendi <c>PasswordHasher&lt;T&gt;</c> sınıfıyla üretilir (PBKDF2 tabanlı,
/// tuzlanmış/salted, endüstri standardı — ASP.NET Core Identity'nin
/// kullandığı ile birebir aynı algoritma). Bkz. Services/PasswordService.cs.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";

    /// <summary>Giriş kimliği olarak kullanılır; benzersizliği DbContext'te zorunlu kılınır.</summary>
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>"User" (varsayılan) ya da "Admin". Kategori yönetimi ve
    /// görsel yükleme gibi içerik-yönetimi uçları yalnızca Admin'e açık
    /// (bkz. [Authorize(Roles = "Admin")] kullanan controller'lar).</summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// Kullanıcının favori tarif Id'leri. Tags alanındaki gibi (bkz.
    /// GurmederDbContext) basit bir liste olduğundan ayrı bir tabloya
    /// gerek duyulmadı, JSON sütun olarak saklanır.
    /// </summary>
    public List<int> FavoriteRecipeIds { get; set; } = new();
}
