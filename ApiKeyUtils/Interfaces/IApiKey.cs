namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// API key instance.
    /// </summary>
    public interface IApiKey
    {
        /// <summary>
        /// ID of the API (identifier or description) key (if applicable)
        /// </summary>
        public string? ID { get; }

        /// <summary>
        /// API key owner identifier (e.g., username or email)
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Type of the API key.
        /// </summary>
        /// <remarks>
        /// This is not currently used, with all keys as type 1 (standard), but intended for expansion
        /// to further key types in the future.
        /// </remarks>
        public int KeyType { get; }

        /// <summary>
        /// Flag to indicate if the API key is active or not.
        /// </summary>
        public bool IsActive { get; }
    }
}
