using ApiKeyUtils.Interfaces;
using Microsoft.Data.SqlClient;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class SqlServerAdmin : IHostedDbServerAdmin
    {
        /// <inheritdoc/>
        public IHostedDbConnection AdminConnection { get; set; }

        /// <summary>
        /// Default constructor that accepts an <see cref="IHostedDbConnection"/> for administrative operations. 
        /// The connection's database is set to "master" to ensure it has the necessary context for server-level 
        /// operations.
        /// </summary>
        /// <param name="connection">Hosted DB connection.</param>
        public SqlServerAdmin(IHostedDbConnection connection)
        {
            connection.Database = "master"; // Ensure we're connected to the master database for admin operations
            AdminConnection = connection;
        }

        /// <inheritdoc/>
        public bool CreateUser(string username, string password, UserPrivileges privileges, string? database)
        {
            // Create connection and open
            var connection = GetConnectionAndOpen();

            if (connection == null)
                return false;

            try
            {
                // 1. Create LOGIN (server-level)
                var safeUsername = username.Replace("]", "]]");
                var safePassword = password.Replace("'", "''");

                var sql = $@"
                    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = '{safeUsername}')
                    BEGIN
                        CREATE LOGIN [{safeUsername}] WITH PASSWORD = '{safePassword}';
                    END";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }               

                // 2. Create USER in the target database (if database specified)
                if (!string.IsNullOrWhiteSpace(database))
                {
                    var createUserSql = $@"
                        USE {database};

                        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @login)
                        BEGIN
                            DECLARE @sql nvarchar(max);
                            SET @sql = N'CREATE USER ' + QUOTENAME(@login) +
                                       N' FOR LOGIN ' + QUOTENAME(@login);
                            EXEC(@sql);
                        END";

                    using (var cmd = new SqlCommand(createUserSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", username);
                        cmd.ExecuteNonQuery();
                    }

                    var sqlPrivileges = ApiDbHelper.GetSqlPrivilegesString(privileges, username);

                    var grantSql = $@"
                        USE {database};
                        {sqlPrivileges}
                        ";

                    using var grantCmd = new SqlCommand(grantSql, connection);
                    grantCmd.ExecuteNonQuery();
                }
                else
                {
                    // ToDo: Just log if no DB specified - user login is added but not mapped to any database.
                }

                return true;
            }
            catch (Exception e)
            {
                // Log exception
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <inheritdoc/>
        public bool DeleteDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                // Log invalid database name
                return false;
            }

            // Escape identifier safely
            string safeDb = databaseName.Replace("]", "]]");

            string sql = $@"
                USE master;

                IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{safeDb}')
                BEGIN
                    ALTER DATABASE [{safeDb}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{safeDb}];
                END";

            var connection = GetConnectionAndOpen();

            try
            {
                using var cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                // Log the exception
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }

        /// <inheritdoc/>
        public bool DeleteUser(string username)
        {
            var safeLogin = username.Replace("]", "]]");  // escape identifier

            var connection = GetConnectionAndOpen();

            try
            {
                // 1. Drop the user from all databases
                var dropUsersSql = $@"
                    DECLARE @sql NVARCHAR(MAX) = '';

                    SELECT @sql += '
                    USE ' + QUOTENAME(name) + ';
                    IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''{safeLogin}'')
                        DROP USER [{safeLogin}];'
                    FROM sys.databases
                    WHERE state_desc = 'ONLINE'
                      AND name NOT IN ('master','tempdb','model','msdb');

                    EXEC sp_executesql @sql;
                    ";

                using (var cmd = new SqlCommand(dropUsersSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Drop the server login
                var dropLoginSql = $@"
                    IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = '{safeLogin}')
                        DROP LOGIN [{safeLogin}];
                    ";

                using (var cmd = new SqlCommand(dropLoginSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            
                return true;
            }
            catch (Exception e)
            {
                // Log exception
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetDatabaseUsers(string database)
        {
            var users = new List<string>();

            string safeDb = database.Replace("]", "]]");

            string sql = $@"
                USE [{safeDb}];

                SELECT dp.name
                FROM sys.database_principals dp
                WHERE dp.type IN ('S', 'U')
                  AND dp.principal_id > 4
                ORDER BY dp.name;";

            var connection = GetConnectionAndOpen();

            try
            {
                using var cmd = new SqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(reader.GetString(0));
                }

                return users;
            }
            finally
            {
                connection?.Close();
            }
        }

        /// <inheritdoc/>
        private SqlConnection? GetConnectionAndOpen()
        {
            try
            {
                var connection = new SqlConnection(AdminConnection.ConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                // Log exception
                return null;
            }
        }
    }
}
