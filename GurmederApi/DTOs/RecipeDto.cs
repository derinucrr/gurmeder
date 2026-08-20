using System.Collections.Generic;

namespace GurmederApi.DTOs;

/// <summary>
/// API'nin dışarı verdiği tarif şekli. System.Text.Json, ASP.NET Core Web
/// API'nin varsayılan yapılandırmasında property adlarını otomatik olarak
/// camelCase'e çevirir (Name -> "name", Category -> "category" gibi) —
/// bu, ön yüzün (js/recipes.js, js/app.js) beklediği alan adlarıyla
/// birebir eşleşir, ekstra bir eşleme/attribute gerekmez.
/// </summary>
public record RecipeDto(
    int Id,
    string Name,
    string Category,
    string Description,
    string? Image,
    int Calories,
    string Difficulty,
    int Prep,
    int Cook,
    int Time,
    int Servings,
    string Pairs,
    List<string> Tags,
    string Chef,
    NutriDto Nutri,
    List<IngredientDto> Ingredients,
    List<StepDto> Steps,
    string Emo
);

public record NutriDto(int Carb, int Protein, int Fat, int Fiber);

public record IngredientDto(double Amount, string Unit, string Name);

public record StepDto(string Title, string Text);
