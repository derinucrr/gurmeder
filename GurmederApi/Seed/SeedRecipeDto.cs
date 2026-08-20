using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GurmederApi.Seed;

/// <summary>
/// SeedData/recipes.json dosyasını okumak için kullanılan ara model.
/// Bu dosya, projenin önceki (statik JSON) mimarisinden kalma, malzeme ve
/// adımları [miktar, birim, isim] / [başlık, metin] şeklinde düz diziler
/// olarak tutan eski şekli kullanır — bu yüzden Ingredients/Steps burada
/// List&lt;JsonElement&gt; olarak okunup DbSeeder içinde tek tek ayrıştırılır.
/// Veritabanı modelleri (Models/Recipe.cs vb.) bunun yerine adlandırılmış
/// alanlar kullanır; bu dönüşüm yalnızca ilk tohumlama anında yapılır.
/// </summary>
public class SeedRecipeDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("calories")] public int Calories { get; set; }
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = "";
    [JsonPropertyName("time")] public int Time { get; set; }
    [JsonPropertyName("prep")] public int Prep { get; set; }
    [JsonPropertyName("cook")] public int Cook { get; set; }
    [JsonPropertyName("servings")] public int Servings { get; set; }
    [JsonPropertyName("pairs")] public string Pairs { get; set; } = "";
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
    [JsonPropertyName("chef")] public string Chef { get; set; } = "";
    [JsonPropertyName("nutri")] public SeedNutriDto Nutri { get; set; } = new();
    [JsonPropertyName("ingredients")] public List<List<JsonElement>> Ingredients { get; set; } = new();
    [JsonPropertyName("steps")] public List<List<string>> Steps { get; set; } = new();
    [JsonPropertyName("emo")] public string Emo { get; set; } = "";
}

public class SeedNutriDto
{
    [JsonPropertyName("carb")] public int Carb { get; set; }
    [JsonPropertyName("protein")] public int Protein { get; set; }
    [JsonPropertyName("fat")] public int Fat { get; set; }
    [JsonPropertyName("fiber")] public int Fiber { get; set; }
}
