using System.ComponentModel.DataAnnotations;

namespace GurmederApi.DTOs;

public record CategoryDto(int Id, string Name, int RecipeCount);

public class CategoryRequest
{
    [Required(ErrorMessage = "Kategori adı gerekli.")]
    [MinLength(2, ErrorMessage = "Kategori adı en az 2 karakter olmalı.")]
    [MaxLength(40, ErrorMessage = "Kategori adı en fazla 40 karakter olabilir.")]
    public string Name { get; set; } = "";
}
