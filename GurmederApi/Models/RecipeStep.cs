namespace GurmederApi.Models;

/// <summary>
/// Bir tarifin tek bir hazırlama adımı — kısa bir başlık ve açıklama metni.
/// </summary>
public class RecipeStep
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public string Title { get; set; } = "";
    public string Text { get; set; } = "";

    /// <summary>Adım sırası (1., 2., 3'üncü adım gibi gösterim sırası).</summary>
    public int SortOrder { get; set; }
}
