using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GurmederApi.Data;
using GurmederApi.DTOs;
using GurmederApi.Models;

namespace GurmederApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly GurmederDbContext _context;

    public RecipesController(GurmederDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET /api/recipes — bütün tarifleri döner. Ön yüzün mevcut mimarisi
    /// (js/recipes.js) tarifleri tek seferde çekip filtrelemeyi/aramayı
    /// tarayıcıda yapacak şekilde tasarlandığından, şu an için tek uç nokta
    /// bu — kategori/arama parametreleri istemci tarafında zaten çalışıyor.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeDto>>> GetAll()
    {
        var recipes = await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Ingredients.OrderBy(i => i.SortOrder))
            .Include(r => r.Steps.OrderBy(s => s.SortOrder))
            .OrderBy(r => r.Id)
            .ToListAsync();

        return recipes.Select(ToDto).ToList();
    }

    /// <summary>GET /api/recipes/{id} — tek bir tarifi döner. Bulunamazsa 404.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeDto>> GetById(int id)
    {
        var recipe = await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Ingredients.OrderBy(i => i.SortOrder))
            .Include(r => r.Steps.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe is null) return NotFound();
        return ToDto(recipe);
    }

    private static RecipeDto ToDto(Recipe r) => new(
        r.Id, r.Name, r.Category.Name, r.Description, r.Image,
        r.Calories, r.Difficulty, r.Prep, r.Cook, r.Time, r.Servings,
        r.Pairs, r.Tags, r.Chef,
        new NutriDto(r.NutriCarb, r.NutriProtein, r.NutriFat, r.NutriFiber),
        r.Ingredients.Select(i => new IngredientDto(i.Amount, i.Unit, i.Name)).ToList(),
        r.Steps.Select(s => new StepDto(s.Title, s.Text)).ToList(),
        r.Emo
    );
}
