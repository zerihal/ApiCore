using ApiCore.Main.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public sealed class SqlApiKeyStorecs : IApiKeyStore
    {
        public SqlApiKeyStorecs()
        {
            // Constructor implementation (e.g., initialize connection string) goes here.
        }

        /// <inheritdoc/>
        public Task<ApiKeyResult> ValidateAsync(string hashedKey)
        {
            throw new NotImplementedException();
        }
    }
}
