using System.Text;

namespace ApiCore.Main.ApiKey
{
    public static class ApiKeyHasher
    {
        public static string HashApiKey(string rawKey)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(rawKey)));
        }
    }
}
