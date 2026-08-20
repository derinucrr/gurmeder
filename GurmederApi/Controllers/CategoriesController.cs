using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GurmederApi.Data;
using GurmederApi.DTOs;
using GurmederApi.Models;

namespace GurmederApi.Controllers;

/// <summary>
/// Kategori yönetimi. Okuma (GET) herkese açık — ön yüzün filtre listesi
/// için kullanılabilir. Yazma uçları (POST/PUT/DELETE) yalnızca Admin
/// rolündeki kullanıcılara açık; içerik yönetimi işlemleri olduğundan.
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly GurmederDbContext _context;

    public CategoriesController(GurmederDbContext context)
    {
        _context = context;
    }

    /// <summary>GET /api/categories — her kategoriyi, içindeki tarif sayısıyla birlikte döner.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Recipes.Count))
            .ToListAsync();

        return categories;
    }

    /// <summary>POST /api/categories — yeni kategori oluşturur. Admin gerektirir.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var name = request.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name == name))
        {
            return Conflict(new { message = "Bu isimde bir kategori zaten var." });
        }

        var category = new Category { Name = name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return StatusCode(201, new CategoryDto(category.Id, category.Name, 0));
    }

    /// <summary>PUT /api/categories/{id} — kategoriyi yeniden adlandırır. Admin gerektirir.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Rename(int id, CategoryRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var category = await _context.Categories.Include(c => c.Recipes).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        var name = request.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != id))
        {
            return Conflict(new { message = "Bu isimde bir kategori zaten var." });
        }

        category.Name = name;
        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Recipes.Count);
    }

    /// <summary>
    /// DELETE /api/categories/{id} — kategoriyi siler. Admin gerektirir.
    /// İçinde tarif varsa 409 döner — tarifleri "kategorisiz" bırakmak ya
    /// da sessizce silmek yerine, önce başka bir kategoriye taşınmalarını
    /// zorunlu kılar (veritabanı seviyesinde de Restrict ile korunuyor,
    /// bkz. GurmederDbContext).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.Include(c => c.Recipes).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        if (category.Recipes.Count > 0)
        {
            return Conflict(new { message = $"Bu kategoride {category.Recipes.Count} tarif var — önce onları başka bir kategoriye taşıyın." });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
