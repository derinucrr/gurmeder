using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GurmederApi.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Ad soyad gerekli.")]
    [MinLength(2, ErrorMessage = "Ad soyad en az 2 karakter olmalı.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required(ErrorMessage = "E-posta gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    public string Password { get; set; } = "";
}

public record UserDto(int Id, string FullName, string Email, List<int> FavoriteRecipeIds);
