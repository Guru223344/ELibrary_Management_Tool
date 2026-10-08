using System;
using System.Security.Cryptography;

namespace ELibraryManagement.Infrastructure
{
    /// <summary>PBKDF2-SHA256 with a random salt. Stored format: v1$iterations$salt$hash (Base64).</summary>
    public static class PasswordHasher
    {
        const int Iterations = 100000;

        public static string Hash(string password)
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(salt); }
            return "v1$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(Derive(password, salt, Iterations));
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || password == null) return false;
            var parts = stored.Split('$');
            int iterations;
            if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out iterations)) return false;
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Derive(password, Convert.FromBase64String(parts[2]), iterations);
            int diff = expected.Length ^ actual.Length;
            for (int i = 0; i < expected.Length && i < actual.Length; i++) diff |= expected[i] ^ actual[i];
            return diff == 0;   // constant-time comparison
        }

        static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return kdf.GetBytes(32);
            }
        }
    }
}
