using System;
using System.Security.Cryptography;
using System.Text;

namespace GurmederApi.Services;

/// <summary>
/// Şifre hash'leme: PBKDF2 (RFC 2898), SHA-256, 100.000 iterasyon, 16 baytlık
/// rastgele tuz (salt) — endüstri standardı, kanıtlanmış bir yöntem. Kasıtlı
/// olarak yalnızca System.Security.Cryptography'nin temel .NET kütüphanesinde
/// (BCL) her zaman hazır bulunan sınıfları kullanır — ek bir NuGet paketi
/// (ör. Identity paketleri) gerektirmez, bu yüzden paket sürümü/uyumluluk
/// riski taşımaz. Şifre asla düz metin ya da geri döndürülebilir şekilde
/// saklanmaz; karşılaştırma sabit-zamanlı (timing-attack'a karşı) yapılır.
/// </summary>
public static class PasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt, expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, Iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
