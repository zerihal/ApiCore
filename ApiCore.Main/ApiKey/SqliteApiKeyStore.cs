using ApiCore.Main.Interfaces;
using Microsoft.Data.Sqlite;

namespace ApiCore.Main.ApiKey
{
    public sealed class SqliteApiKeyStore : IApiKeyStore
    {
        private readonly string _connectionString;

        public SqliteApiKeyStore(IConfiguration config) 
        {
            var dbPath = string.Empty;

            if (OperatingSystem.IsWindows())
                dbPath = config.GetValue<string>("ApiKeyWinDb:Path");
            else if (OperatingSystem.IsLinux())
                dbPath = config.GetValue<string>("ApiKeyLinuxDb:Path");

            if (string.IsNullOrEmpty(dbPath))
                throw new ArgumentException("ApiKeyDb:Path configuration is missing or empty.");

            _connectionString = $"Data Source={dbPath}";
        }

        /// <inheritdoc/>
        public async Task<ApiKeyResult> ValidateAsync(string hashedKey)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Owner, KeyType FROM ApiKeys WHERE HashedKey = $hash AND IsActive = 1";
            command.Parameters.AddWithValue("$hash", hashedKey);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var owner = reader.GetString(0);
                var keyType = reader.GetInt32(1);
                return new ApiKeyResult(true, owner, keyType);
            }
            else
            {
                return new ApiKeyResult(false, null, 0);
            }
        }
    }
}
