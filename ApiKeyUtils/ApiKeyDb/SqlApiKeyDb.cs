using ApiKeyUtils.Interfaces;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class SqlApiKeyDb : HostedApiKeyDbBase
    {
        /// <summary>
        /// Default constructor that initializes the SQL API key database with default settings. Server and 
        /// authentication type are not configured and need to be set after construction.
        /// </summary>
        public SqlApiKeyDb() : this(null) { }

        /// <summary>
        /// Creates a new instance of the SQL API key database with the provided <see cref="IHostedDbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">Database connection.</param>
        public SqlApiKeyDb(IHostedDbConnection? dbConnection) : base(dbConnection, DbServerType.SqlServer) { }

        /// <inheritdoc/>
        public override bool CreateDatabaseAndApiKeysTable()
        {
            try
            {
                // Temporarily set database to master to create the target database, but store the original database
                // name to restore once connected.
                var dbName = DatabaseName;
                DbConnection.Database = "master";

                // Create database if it does not exist
                using (var connection = new SqlConnection(DbConnectionString))
                {
                    connection.Open();

                    // Restore the target database name
                    DbConnection.Database = dbName;

                    // Create the database if it does not already exist
                    var sql = $@"
                        IF DB_ID('{DatabaseName}') IS NULL
                            CREATE DATABASE [{DatabaseName}];
                        ";

                    var createDbCommand = new SqlCommand(sql, connection);
                    createDbCommand.ExecuteNonQuery();
                }

                // Create api_keys table if it does not exist
                using (var connection = new SqlConnection(DbConnectionString))
                {
                    connection.Open();

                    var createTableCommand = new SqlCommand(
                        $@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='api_keys' AND xtype='U')
                        CREATE TABLE api_keys (
                            id INT IDENTITY(1,1) PRIMARY KEY,                       
                            hashed_key NVARCHAR(255) NOT NULL UNIQUE,
                            owner NVARCHAR(100) NOT NULL,
                            key_type INT NOT NULL,
                            is_active BIT NOT NULL DEFAULT 1,
                            created_ts DATETIME NOT NULL DEFAULT GETDATE(),
                            key_id NVARCHAR(100) NULL
                        );", connection);

                    createTableCommand.ExecuteNonQuery();

                    var createProcCommand = new SqlCommand(
                        @"
                        CREATE OR ALTER PROCEDURE dbo.sp_InsertApiKey
                            @hashed_key NVARCHAR(255),
                            @owner NVARCHAR(100),
                            @key_type INT,
                            @is_active BIT,
                            @key_id NVARCHAR(100)
                        AS
                        BEGIN
                            INSERT INTO api_keys (hashed_key, owner, key_type, is_active, key_id)
                            VALUES (@hashed_key, @owner, @key_type, @is_active, @key_id);
                        END
                        ", connection);

                    createProcCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                // Log exception
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override IApiKey? GetApiKeyFromHash(string hashedApiKey)
        {
            try
            {
                using (var connection = new SqlConnection(DbConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "SELECT id, owner, key_type, is_active FROM api_keys WHERE hashed_key = @hashed_key",
                        connection);

                    command.Parameters.AddWithValue("@hashed_key", hashedApiKey);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader?.Read() ?? false)
                        {
                            var owner = reader["owner"].ToString() ?? "Null Owner";
                            var keyType = Convert.ToInt32(reader["key_type"]);
                            var isActive = Convert.ToBoolean(reader["is_active"]);

                            return new ApiKey(owner, keyType, isActive);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error retrieving API key: {e.Message}");
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEnumerable<IApiKey> GetApiKeys(string owner)
        {
            var apiKeys = new List<IApiKey>();

            using (var connection = new SqlConnection(DbConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(
                    "SELECT owner, key_type, is_active FROM api_keys WHERE owner = @owner",
                    connection);

                //command.Parameters.AddWithValue("@database", DatabaseName);
                command.Parameters.AddWithValue("@owner", owner);

                using (var reader = command.ExecuteReader())
                {
                    while (reader?.Read() ?? false)
                    {
                        var keyOwner = reader["owner"].ToString() ?? "Null Owner";
                        var keyType = Convert.ToInt32(reader["key_type"]);
                        var isActive = Convert.ToBoolean(reader["is_active"]);
                        apiKeys.Add(new ApiKey(keyOwner, keyType, isActive));
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

                using (var connection = new SqlConnection(DbConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("dbo.sp_InsertApiKey", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@hashed_key", hashedKey);
                    command.Parameters.AddWithValue("@owner", owner);
                    command.Parameters.AddWithValue("@key_type", keyType);
                    command.Parameters.AddWithValue("@is_active", true);
                    command.Parameters.AddWithValue("@key_id", DBNull.Value);
                    command.ExecuteNonQuery();
                }
                return true;

            }
            catch (Exception e)
            {
                // Log exception
                return false;
            }
        }
    }
}
