using ApiKeyUtils.Interfaces;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class SqliteApiKeyDb : ISqliteApiKeyDb
    {
        /// <inheritdoc/>
        private string _databaseDirectory = string.Empty;

        /// <inheritdoc/>
        public string DatabaseDirectory 
        { 
            get => !string.IsNullOrEmpty(_databaseDirectory) ? _databaseDirectory : ApiDbHelper.GetDefaultDataDirectory();
            set => _databaseDirectory = value;
        }

        /// <inheritdoc/>
        public string DatabaseName { get; set; } = "ApiCore.db";

        /// <inheritdoc/>
        public string DbConnectionString
        {
            get
            {
                var dbPath = Path.Combine(DatabaseDirectory, DatabaseName);
                return $"Data Source={dbPath}";
            }
        }

        /// <summary>
        /// Default constructor that initializes the SQLite engine.
        /// </summary>
        public SqliteApiKeyDb()
        {
            SQLitePCL.Batteries.Init();
        }

        /// <inheritdoc/>
        public bool CreateDatabaseAndApiKeysTable()
        {
            try
            {
                // Create the database and ApiKeys table if it does not exist
                using (var conn = new SqliteConnection(DbConnectionString))
                {
                    conn.Open();
                    var createTableCmd = conn.CreateCommand();

                    createTableCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ApiKeys (
                            Id INTEGER PRIMARY KEY,
                            HashedKey TEXT NOT NULL UNIQUE,
                            Owner TEXT,
                            KeyType INTEGER NOT NULL DEFAULT 0,
                            IsActive INTEGER NOT NULL,
                            KeyId TEXT DEFAULT NULL,
                            CreatedUtc TEXT NOT NULL DEFAULT (datetime('now'))
                        );";

                    createTableCmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error creating database or table: {e.Message}");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public IApiKey? GetApiKeyFromHash(string hashedApiKey)
        {
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Owner, KeyType, IsActive, KeyId, CreatedUtc
                FROM ApiKeys
                WHERE HashedKey = $hashedKey;
                ";

            cmd.Parameters.AddWithValue("$hashedKey", hashedApiKey);

            try
            {
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var owner = reader.GetString(0);
                    var keyType = reader.GetInt32(1);
                    var isActive = reader.GetInt32(2) == 1;
                    return new ApiKey(owner, keyType, isActive);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error retrieving API key: {e.Message}");
            }

            return null;
        }

        /// <inheritdoc/>
        public IApiKey? GetApiKey(string apiKey)
        {
            var hashedKey = ApiDbHelper.GetKeyHash(apiKey);
            return GetApiKeyFromHash(hashedKey);
        }

        /// <inheritdoc/>
        public IEnumerable<IApiKey> GetApiKeys(string owner)
        {
            var apiKeys = new List<IApiKey>();
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT KeyType, IsActive, KeyId, CreatedUtc
                FROM ApiKeys
                WHERE Owner = $owner;
                ";

            cmd.Parameters.AddWithValue("$owner", owner);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                try
                {
                    var keyType = reader.GetInt32(0);
                    var isActive = reader.GetInt32(1) == 1;
                    apiKeys.Add(new ApiKey(owner, keyType, isActive));
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error retrieving API key: {e.Message}");
                }
            }

            return apiKeys;
        }

        /// <inheritdoc/>
        public bool StoreApiKey(string apiKey, string owner, int keyType)
        {
            if (GetApiKey(apiKey) != null)
            {
                // API key already exists - return true as already stored
                Debug.WriteLine("API key already exists in the database.");
                return true;
            }

            var hashedKey = ApiDbHelper.GetKeyHash(apiKey);
            using var conn = new SqliteConnection(DbConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ApiKeys (HashedKey, Owner, KeyType, IsActive)
                VALUES ($hashedKey, $owner, $keyType, 1);
                ";

            cmd.Parameters.AddWithValue("$hashedKey", hashedKey);
            cmd.Parameters.AddWithValue("$owner", owner);
            cmd.Parameters.AddWithValue("$keyType", keyType);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
