using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GurmederApi.Data;
using GurmederApi.DTOs;
using GurmederApi.Models;
using GurmederApi.Services;

namespace GurmederApi.Controllers;

/// <summary>
/// Gerçek, backend-doğrulamalı kullanıcı girişi. Şifreler PasswordService
/// ile hash'lenip veritabanında saklanır (bkz. Models/User.cs). Oturum,
/// ASP.NET Core'un yerleşik ÇEREZ (cookie) kimlik doğrulama şemasıyla
/// yönetilir — ön yüz artık bu API ile AYNI origin'den sunulduğundan
/// (bkz. Program.cs static files + wwwroot), tarayıcı çerezi otomatik
/// gönderir; JWT gibi ek bir paket/karmaşıklık gerekmez.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GurmederDbContext _context;
    private readonly CurrentUserAccessor _currentUser;

    public AuthController(GurmederDbContext context, CurrentUserAccessor currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "Bu e-posta adresiyle zaten bir hesap var." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = PasswordService.Hash(request.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInAsync(user);
        return Ok(ToDto(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Kullanıcı bulunamasa BİLE Verify'ı sahte bir hash ile çağırıyoruz
        // (kullanıcı null ise gerçek hash'i yok, o yüzden DummyHashForTiming
        // kullanılır) — böylece "e-posta yok" durumu, Verify'ın kendi PBKDF2
        // hesaplamasını hiç çalıştırmadan anında dönüp "şifre yanlış"tan
        // (hesaplama sonrası döner) daha hızlı yanıt vermez. Bu olmadan,
        // yanıt SÜRESİ tek başına hangi e-postaların kayıtlı olduğunu
        // sızdırabilirdi (user enumeration timing saldırısı).
        var passwordOk = PasswordService.Verify(request.Password, user?.PasswordHash ?? DummyHashForTiming);

        if (user is null || !passwordOk)
        {
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });
        }

        await SignInAsync(user);
        return Ok(ToDto(user));
    }

    // Gerçek bir kaydın PasswordHash'iyle aynı biçimde (salt.hash, Base64)
    // ama hiçbir gerçek şifreye karşılık gelmeyen sabit bir değer. Yukarıdaki
    // Login'de "kullanıcı bulunamadı" durumunda bile PBKDF2'nin tam maliyetini
    // çalıştırmak için kullanılır. Uygulama başına bir kez, PasswordService'in
    // KENDİ üretim yoluyla hesaplanır — elle yazılmış olası bozuk bir Base64
    // riskini de ortadan kaldırır.
    private static readonly string DummyHashForTiming = PasswordService.Hash("timing-safety-dummy-password");

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>GET /api/auth/me — oturum açıksa mevcut kullanıcıyı döner, kapalıysa 401.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await _currentUser.GetUserAsync();
        if (user is null) return Unauthorized();
        return Ok(ToDto(user));
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.FavoriteRecipeIds);
}
