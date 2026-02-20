using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <summary>
    /// API key database factory to create instances of <see cref="IApiKeyDb"/> based on the specified database type.
    /// </summary>
    public static class ApiKeyDbFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IApiKeyDb"/> based on the specified <see cref="ApiKeyDbType"/>.
        /// </summary>
        /// <param name="dbType">Database type.</param>
        /// <returns>API key database representation.</returns>
        /// <exception cref="NotSupportedException">Thrown if specified type is unsupported at this time.</exception>
        public static IApiKeyDb Create(ApiKeyDbType dbType)
        {
            return dbType switch
            {
                ApiKeyDbType.Sqlite => new SqliteApiKeyDb(),
                ApiKeyDbType.MySQL => new MySqlApiKeyDb(null),
                ApiKeyDbType.SQL => new SqlApiKeyDb(),
                _ => throw new NotSupportedException($"The specified database type '{dbType}' is not supported."),
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="IHostedApiKeyDb"/> based on the specified <see cref="ApiKeyDbType"/> and 
        /// <see cref="IHostedDbConnection"/>.
        /// </summary>
        /// <param name="dbType">Database type.</param>
        /// <param name="dbConnection">Hosted database connection instance.</param>
        /// <returns>Hosted API key database representation with connection set.</returns>
        /// <exception cref="NotSupportedException">Thrown if specified type is unsupported at this time.</exception>
        public static IHostedApiKeyDb CreateHosted(ApiKeyDbType dbType, IHostedDbConnection dbConnection)
        {
            return dbType switch
            {              
                ApiKeyDbType.MySQL => new MySqlApiKeyDb((IMySqlHostedDbConnection?)dbConnection),
                ApiKeyDbType.SQL => new SqlApiKeyDb(dbConnection),
                _ => throw new NotSupportedException($"The specified hosted database type '{dbType}' is not supported."),
            };
        }
    }

    public enum ApiKeyDbType
    {
        Sqlite,
        MySQL,
        SQL
    }
}
