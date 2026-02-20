using ApiCore.Main.Interfaces;
using ApiKeyUtils.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public abstract class ApiKeyStoreBase : IApiKeyStore
    {
        /// <summary>
        /// API key database instance (overridden by derived classes to specific interface).
        /// </summary>
        protected abstract IApiKeyDb ApiKeyDb { get; }

        /// <inheritdoc/>
        public virtual async Task<ApiKeyResult> ValidateAsync(string hashedKey)
        {
            var apiKeyInfo = ApiKeyDb.GetApiKeyFromHash(hashedKey);

            if (apiKeyInfo != null)
            {
                return new ApiKeyResult(apiKeyInfo.IsActive, apiKeyInfo.Owner, apiKeyInfo.KeyType);
            }
            else
            {
                return new ApiKeyResult(false, null, 0);
            }
        }
    }
}
