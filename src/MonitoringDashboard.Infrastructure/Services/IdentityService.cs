using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private const int IterationCount = 100_000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(128 / 8);
        var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, IterationCount, 256 / 8);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        var parts = hash.Split('.', 2);
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);
        var computedHash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, IterationCount, 256 / 8);
        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
