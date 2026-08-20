namespace GurmederApi.Models;

/// <summary>
/// Bir tarifin tek bir malzemesi (ör. "300 g kıyma"). Miktarsız malzemeler
/// (tuz, karabiber vb.) Amount=0, Unit="" olarak saklanır — ön yüz bunu
/// miktar etiketini boş bırakarak yorumlar.
/// </summary>
public class RecipeIngredient
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public double Amount { get; set; }
    public string Unit { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>Malzeme listesindeki gösterim sırası (tarifteki orijinal sırayı korumak için).</summary>
    public int SortOrder { get; set; }
}
