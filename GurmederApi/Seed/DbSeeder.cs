using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GurmederApi.Data;
using GurmederApi.Models;
using GurmederApi.Services;

namespace GurmederApi.Seed;

/// <summary>
/// Uygulama ilk kez ayağa kalktığında (veritabanı boşsa) SeedData/recipes.json
/// dosyasındaki 89 gerçek tarifi okuyup veritabanına yazar, kategorileri bu
/// tariflerin isimlerinden çıkarıp ayrı satırlar olarak oluşturur, ve bir
/// demo Admin hesabı ekler. Veritabanında zaten tarif varsa hiçbir şey
/// yapmaz — bu yüzden `dotnet run` her çalıştırıldığında güvenle tekrar
/// çağrılabilir.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Demo admin hesabı bilgileri. ÖNEMLİ: bu proje herkese açık/canlı bir
    /// yere dağıtılıyorsa bu şifre ilk fırsatta değiştirilmeli (ya da bu
    /// seed adımı tamamen kaldırılmalı) — bkz. README.md.
    /// </summary>
    public const string AdminEmail = "admin@gurmeder.com";
    public const string AdminPassword = "Gurmeder2026!";

    public static async Task SeedAsync(GurmederDbContext db, string contentRootPath)
    {
        if (await db.Recipes.AnyAsync())
        {
            return; // zaten tohumlanmış
        }

        var seedPath = Path.Combine(contentRootPath, "SeedData", "recipes.json");
        if (!File.Exists(seedPath))
        {
            Console.WriteLine($"[DbSeeder] Uyarı: tohum dosyası bulunamadı: {seedPath}");
            return;
        }

        var json = await File.ReadAllTextAsync(seedPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var seedRecipes = JsonSerializer.Deserialize<List<SeedRecipeDto>>(json, options);

        if (seedRecipes is null || seedRecipes.Count == 0)
        {
            Console.WriteLine("[DbSeeder] Uyarı: tohum dosyası boş ya da okunamadı.");
            return;
        }

        // Kategoriler önce, ayrı satırlar olarak: tohum verisindeki her
        // benzersiz kategori adı için bir Category satırı oluşturulur ve
        // veritabanına yazılır ki gerçek Id'ler atansın — tarifler bu Id'lere
        // referans vererek bağlanacak (bkz. aşağıdaki categoryLookup).
        var categoryNames = seedRecipes.Select(r => r.Category).Distinct().OrderBy(n => n);
        var categoryLookup = new Dictionary<string, Category>();
        foreach (var name in categoryNames)
        {
            var category = new Category { Name = name };
            db.Categories.Add(category);
            categoryLookup[name] = category;
        }
        await db.SaveChangesAsync(); // Id'lerin atanması için tarifler eklenmeden önce kaydedilir

        foreach (var sr in seedRecipes)
        {
            var recipe = new Recipe
            {
                Name = sr.Name,
                CategoryId = categoryLookup[sr.Category].Id,
                Description = sr.Description,
                Image = sr.Image,
                Calories = sr.Calories,
                Difficulty = sr.Difficulty,
                Prep = sr.Prep,
                Cook = sr.Cook,
                Time = sr.Time,
                Servings = sr.Servings,
                Pairs = sr.Pairs,
                Chef = sr.Chef,
                Emo = sr.Emo,
                Tags = sr.Tags,
                NutriCarb = sr.Nutri.Carb,
                NutriProtein = sr.Nutri.Protein,
                NutriFat = sr.Nutri.Fat,
                NutriFiber = sr.Nutri.Fiber,
                Ingredients = sr.Ingredients.Select((triple, idx) => new RecipeIngredient
                {
                    Amount = triple[0].GetDouble(),
                    Unit = triple[1].GetString() ?? "",
                    Name = triple[2].GetString() ?? "",
                    SortOrder = idx,
                }).ToList(),
                Steps = sr.Steps.Select((pair, idx) => new RecipeStep
                {
                    Title = pair[0],
                    Text = pair[1],
                    SortOrder = idx,
                }).ToList(),
            };

            db.Recipes.Add(recipe);
        }

        // Demo Admin hesabı: kategori/görsel yönetimi uçlarını denemek için.
        // Zaten aynı e-postayla bir kullanıcı varsa (teorik olarak imkansız,
        // çünkü bu blok yalnızca veritabanı tamamen boşken çalışır, ama yine
        // de savunmacı bir kontrol) tekrar eklenmez.
        if (!await db.Users.AnyAsync(u => u.Email == AdminEmail))
        {
            db.Users.Add(new User
            {
                FullName = "Yönetici",
                Email = AdminEmail,
                PasswordHash = PasswordService.Hash(AdminPassword),
                Role = "Admin",
            });
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"[DbSeeder] {seedRecipes.Count} tarif, {categoryLookup.Count} kategori ve 1 demo admin hesabı veritabanına yüklendi.");
    }
}
