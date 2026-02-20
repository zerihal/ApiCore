using ApiKeyUtils.ApiKeyDb;
using ApiKeyUtils.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public sealed class SqliteApiKeyStore : ApiKeyStoreBase
    {
        /// <inheritdoc/>
        protected override ISqliteApiKeyDb ApiKeyDb { get; }

        public SqliteApiKeyStore(IConfiguration config) 
        {
            var dbPath = string.Empty;

            if (OperatingSystem.IsWindows())
                dbPath = config.GetValue<string>("ApiKeyWinDb:Path");
            else if (OperatingSystem.IsLinux())
                dbPath = config.GetValue<string>("ApiKeyLinuxDb:Path");

            if (string.IsNullOrEmpty(dbPath))
                throw new ArgumentException("ApiKeyDb:Path configuration is missing or empty.");

            //_connectionString = $"Data Source={dbPath}";

            // Create instance of SQLite API key database interface and set the database directory
            if (ApiKeyDbFactory.Create(ApiKeyDbType.Sqlite) is ISqliteApiKeyDb sqLiteApiKeyDb)
            {
                ApiKeyDb = sqLiteApiKeyDb;
                ApiKeyDb.DatabaseDirectory = dbPath;
            }
            else
            {
                throw new InvalidOperationException("Failed to create SQLite API key database instance.");
            }
        }
    }
}
