using System.Collections.Generic;

namespace GurmederApi.Models;

/// <summary>
/// Bir tarifin veritabanı kaydı. Malzemeler ve adımlar ayrı tablolarda
/// (RecipeIngredient, RecipeStep) tutulur, bire-çok ilişkiyle bağlanır.
/// Tags basit bir string listesi olduğu için ayrı bir tabloya gerek
/// duyulmadı; DbContext'te JSON sütun olarak saklanacak şekilde
/// yapılandırılır (bkz. GurmederDbContext.OnModelCreating). Category ise
/// artık düz bir metin değil, gerçek bir ilişki (bkz. Models/Category.cs) —
/// admin panelinden kategori yönetimi yapılabilmesi için.
/// </summary>
public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Description { get; set; } = "";

    /// <summary>Yerel dosya yolu ("images/x.jpg") ya da tam bir URL olabilir. Fotoğrafı olmayan tarifler için null.</summary>
    public string? Image { get; set; }

    public int Calories { get; set; }
    public string Difficulty { get; set; } = "";
    public int Prep { get; set; }
    public int Cook { get; set; }
    public int Time { get; set; }
    public int Servings { get; set; }
    public string Pairs { get; set; } = "";
    public string Chef { get; set; } = "";
    public string Emo { get; set; } = "";

    // Besin değerleri (1 porsiyon başına) — ayrı bir tabloya gerek
    // duyulmayacak kadar basit olduğundan doğrudan sütun olarak tutulur.
    public int NutriCarb { get; set; }
    public int NutriProtein { get; set; }
    public int NutriFat { get; set; }
    public int NutriFiber { get; set; }

    public List<string> Tags { get; set; } = new();

    public List<RecipeIngredient> Ingredients { get; set; } = new();
    public List<RecipeStep> Steps { get; set; } = new();
}
