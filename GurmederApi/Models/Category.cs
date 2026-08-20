using System.Collections.Generic;

namespace GurmederApi.Models;

/// <summary>
/// Bir tarif kategorisi (ör. "Ana Yemekler"). Önceden Recipe üzerinde düz
/// bir metin sütunuydu; artık kendi tablosunda — admin panelinden
/// yönetilebilmesi (ekleme/yeniden adlandırma/silme) için (bkz.
/// Controllers/CategoriesController.cs).
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public List<Recipe> Recipes { get; set; } = new();
}
