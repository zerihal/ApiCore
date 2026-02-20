using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class ApiKey : IApiKey
    {
        /// <inheritdoc/>
        public string? ID { get; }

        /// <inheritdoc/>
        public string Owner { get; }

        /// <inheritdoc/>
        public int KeyType { get; }

        /// <inheritdoc/>
        public bool IsActive { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ApiKey"/> class with the specified owner, key type, 
        /// active status, and optional ID. The constructor initializes the properties of the API key 
        /// based on the provided parameters.
        /// </summary>
        /// <param name="owner">Key owner.</param>
        /// <param name="keyType">Key type.</param>
        /// <param name="isActive">Flag to indicate whether or not the key is active.</param>
        /// <param name="id">Optional additional ID.</param>
        public ApiKey(string owner, int keyType, bool isActive, string? id = null)
        {
            Owner = owner;
            KeyType = keyType;
            IsActive = isActive;
            ID = id;
        }

        /// <summary>
        /// ToString override to provide a readable representation of the API key instance.
        /// </summary>
        /// <returns>API key as string.</returns>
        override public string ToString() => $"Key Type: {KeyType}, Owner: {Owner}, Active: {IsActive}";
    }
}
