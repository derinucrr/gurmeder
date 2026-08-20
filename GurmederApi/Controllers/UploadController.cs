using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GurmederApi.Controllers;

/// <summary>
/// Tarif görseli yükleme. Yalnızca Admin — bu, bir tarife serbestçe
/// herhangi bir dosya yükleyebilen açık bir uç nokta değil, içerik
/// yönetimi işlemidir. Yüklenen dosya wwwroot/images altına, çakışmayı
/// önlemek için rastgele (GUID) bir adla kaydedilir; dönen "images/xxx.jpg"
/// yolu doğrudan bir Recipe'nin Image alanına yazılabilir — zaten
/// wwwroot altındaki her şey Program.cs'teki UseStaticFiles() ile
/// otomatik olarak servis ediliyor.
/// </summary>
[ApiController]
[Route("api/upload")]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("image")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Bir dosya seçilmedi." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "Dosya çok büyük (maksimum 5 MB)." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Yalnızca JPG, PNG ya da WEBP dosyaları kabul edilir." });
        }

        // wwwroot yolu her ortamda (yerel/Render) doğru şekilde IWebHostEnvironment
        // üzerinden alınır — sabit kodlanmış bir yol kullanılmaz.
        var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var imagesFolder = Path.Combine(webRootPath, "images");
        Directory.CreateDirectory(imagesFolder);

        // Rastgele dosya adı: hem çakışmayı önler hem de kullanıcının
        // gönderdiği orijinal dosya adının (path traversal riski taşıyan
        // karakterler içerebilir) doğrudan dosya sistemine yazılmasını engeller.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(imagesFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { path = $"images/{fileName}" });
    }
}
