using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GurmederApi.Data;
using GurmederApi.Models;

namespace GurmederApi.Services;

/// <summary>
/// Geçerli HTTP isteğindeki kimlik doğrulama bilgisinden (çerez claim'leri)
/// veritabanındaki gerçek User kaydını bulur. AuthController ve
/// FavoritesController arasında paylaşılan bir servis — bir controller'ı
/// doğrudan başka bir controller'a enjekte etmek yerine (bu, ASP.NET
/// Core'da teknik olarak mümkün olsa da önerilmeyen bir kalıptır).
/// </summary>
public class CurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GurmederDbContext _context;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, GurmederDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<User?> GetUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var idClaim = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !int.TryParse(idClaim, out var id)) return null;
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
}
