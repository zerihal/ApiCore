using ApiCore.Main.ApiKey;

namespace ApiCore.Main.Interfaces
{
    public interface IApiKeyStore
    {
        /// <summary>
        /// Validates the given hashed API key.
        /// </summary>
        /// <param name="hashedKey">Hashed API key.</param>
        /// <returns>
        /// <see cref="ApiKeyResult"/> to indicate whether the hashed key is valid, and if so, 
        /// the owner and key type.
        /// </returns>
        Task<ApiKeyResult> ValidateAsync(string hashedKey);
    }
}
