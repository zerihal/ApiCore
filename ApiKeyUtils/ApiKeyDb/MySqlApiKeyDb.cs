using ApiKeyUtils.Interfaces;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class MySqlApiKeyDb : HostedApiKeyDbBase
    {
        /// <summary>
        /// Default constructor that initializes the database connection using the provided 
        /// <see cref="IMySqlHostedDbConnection"/> instance.
        /// </summary>
        /// <param name="dbConnection">Database connection.</param>
        public MySqlApiKeyDb(IMySqlHostedDbConnection? dbConnection) : base(dbConnection, DbServerType.MySql) { }

        /// <inheritdoc/>
        public override bool CreateDatabaseAndApiKeysTable()
        {
            if (DbConnection is IMySqlHostedDbConnection mySqlDbConnection)
            {
                if (string.IsNullOrWhiteSpace(mySqlDbConnection.User) || string.IsNullOrWhiteSpace(mySqlDbConnection.Password))
                {
                    Debug.WriteLine("Invalid username or password for database connection");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Invalid connection type for MySQL database");
                return false;
            }

            // 1. Create database if it does not exist
            var builder = new MySqlConnectionStringBuilder(DbConnectionString);
            var databaseName = builder.Database;

            builder.Database = string.Empty;

            try
            {
                // Note: The user in DbConnection.User must already exist on the server (even localhost) and
                // must have relevant permission to create the database and table.

                using (var connection = new MySqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    using var createDbCmd = connection.CreateCommand();
                    createDbCmd.CommandText = $"""
                        CREATE DATABASE IF NOT EXISTS `{databaseName}`
                        CHARACTER SET utf8mb4
                        COLLATE utf8mb4_unicode_ci;
                        """;

                    createDbCmd.ExecuteNonQuery();
                }

                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    connection.Open();

                    // 2. Create table if it does not exist
                    using var createTableCmd = connection.CreateCommand();
                    createTableCmd.CommandText = """
                        CREATE TABLE IF NOT EXISTS api_keys (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            hashed_key VARCHAR(255) NOT NULL,
                            owner VARCHAR(100) NOT NULL,
                            key_type INT NOT NULL DEFAULT 0,
                            is_active BOOLEAN NOT NULL,
                            created_ts TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            key_id VARCHAR(100) DEFAULT NULL,
                            UNIQUE KEY uq_api_key (hashed_key)
                        );
                        """;

                    createTableCmd.ExecuteNonQuery();

                    // 3. Create stored procedure to add API key (remove and recreate if already exists)
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = """
                            DROP PROCEDURE IF EXISTS sp_add_api_key;
                            """;

                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = """
                            CREATE PROCEDURE sp_add_api_key (
                                IN p_owner VARCHAR(100),
                                IN p_hashed_key VARCHAR(255),
                                IN p_key_type INT
                            )
                            BEGIN
                                INSERT INTO api_keys (
                                    owner,
                                    hashed_key,
                                    key_type,
                                    is_active
                                )
                                VALUES (
                                    p_owner,
                                    p_hashed_key,
                                    p_key_type,
                                    TRUE
                                )
                                ON DUPLICATE KEY UPDATE
                                    key_type = p_key_type,
                                    is_active = TRUE;
                            END;
                            """;

                        cmd.ExecuteNonQuery();
                    }
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
        public override IApiKey? GetApiKeyFromHash(string hashedApiKey)
        {
            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = """
                    SELECT id, owner, key_type, is_active, created_ts
                    FROM api_keys
                    WHERE hashed_key = @hashedKey
                      AND is_active = TRUE;
                    """;

                cmd.Parameters.AddWithValue("@hashedKey", hashedApiKey);

                try
                {
                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        var owner = reader.GetString("owner");
                        var keyType = reader.GetInt32("key_type");
                        var isActive = reader.GetBoolean("is_active");
                        return new ApiKey(owner, keyType, isActive);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error retrieving API key: {e.Message}");
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEnumerable<IApiKey> GetApiKeys(string owner)
        {
            var apiKeys = new List<IApiKey>();

            using (var connection = new MySqlConnection(DbConnectionString))
            {
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = """
                    SELECT key_type, is_active, created_ts, key_id
                    FROM api_keys
                    WHERE owner = @owner
                      AND is_active = TRUE;
                    """;

                cmd.Parameters.AddWithValue("@owner", owner);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    try
                    {
                        var keyType = reader.GetInt32("key_type");
                        var isActive = reader.GetBoolean("is_active");
                        apiKeys.Add(new ApiKey(owner, keyType, isActive));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error retrieving API key: {e.Message}");
                    }
                }
            }

            return apiKeys;
        }

        /// <inheritdoc/>
        public override bool StoreApiKey(string apiKey, string owner, int keyType)
        {
            try
            {
                var hashedKey = ApiDbHelper.GetKeyHash(apiKey);

                using (var connection = new MySqlConnection(DbConnectionString))
                {
                    connection.Open();

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "sp_add_api_key";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_owner", owner);
                    cmd.Parameters.AddWithValue("p_hashed_key", hashedKey);
                    cmd.Parameters.AddWithValue("p_key_type", keyType);

                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
