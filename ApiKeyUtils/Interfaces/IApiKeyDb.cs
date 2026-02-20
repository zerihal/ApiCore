namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// API key database instance that defines properties and methods for managing API keys, including creating 
    /// the database and API keys table,
    /// </summary>
    public interface IApiKeyDb
    {
        /// <summary>
        /// Gets or sets the name of the database to which the connection applies.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets the database connection string used to establish a connection to the data source.
        /// </summary>
        public string DbConnectionString { get; }

        /// <summary>
        /// Creates the database and the table used to store API keys if they do not already exist.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if the database and API keys table were successfully created 
        /// or already exist, otherwise, <see langword="false"/>.
        /// </returns>
        public bool CreateDatabaseAndApiKeysTable();

        /// <summary>
        /// Stores an API key for the specified owner and key type.
        /// </summary>
        /// <remarks>This method does not validate the format of the API key or owner beyond checking for
        /// null or empty values. If an API key for the specified owner and key type already exists, it may be
        /// overwritten depending on implementation.</remarks>
        /// <param name="apiKey">The API key to store. Cannot be null or empty.</param>
        /// <param name="owner">
        /// The identifier of the owner associated with the API key. Cannot be null or empty.
        /// </param>
        /// <param name="keyType">
        /// An integer representing the type of API key. Must be a valid key type defined by the application.
        /// </param>
        /// <returns>
        /// <see langword="True"/> if the API key was stored successfully; otherwise, <see langword="false"/>.
        /// </returns>
        public bool StoreApiKey(string apiKey, string owner, int keyType);

        /// <summary>
        /// Retrieves the API key information associated with the specified hashed key string.
        /// </summary>
        /// <param name="hashedApiKey">The API key string to look up. Cannot be null or empty.</param>
        /// <returns>
        /// An <see cref="IApiKey"/> instance containing details of the API key if found; otherwise, 
        /// <see langword="null"/>.
        /// </returns>
        public IApiKey? GetApiKeyFromHash(string hashedApiKey);

        /// <summary>
        /// Retrieves the API key information associated with the specified raw key string.
        /// </summary>
        /// <param name="apiKey">The API key string to look up. Cannot be null or empty.</param>
        /// <returns>
        /// An <see cref="IApiKey"/> instance containing details of the API key if found; otherwise, 
        /// <see langword="null"/>.
        /// </returns>
        public IApiKey? GetApiKey(string apiKey);

        /// <summary>
        /// Retrieves all API keys associated with the specified owner.
        /// </summary>
        /// <param name="owner">
        /// The identifier of the owner whose API keys are to be retrieved. Cannot be null or empty.
        /// </param>
        /// <returns>
        /// An enumerable collection of API keys belonging to the specified owner. The collection will 
        /// be empty if the owner has no API keys.
        /// </returns>
        public IEnumerable<IApiKey> GetApiKeys(string owner);
    }
}
