using ApiKeyUtils.Interfaces;
using MySqlConnector;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class MySqlServerAdmin : IHostedDbServerAdmin
    {
        /// <inheritdoc/>
        public IHostedDbConnection AdminConnection { get; set; }

        /// <summary>
        /// Default constructor that accepts an <see cref="IMySqlHostedDbConnection"/> for administrative 
        /// operations.
        /// </summary>
        /// <param name="adminConnection">Administrative hosted DB connection.</param>
        public MySqlServerAdmin(IMySqlHostedDbConnection adminConnection)
        {
            AdminConnection = adminConnection;
        }

        /// <summary>
        /// Constructor that accepts individual connection parameters to create an administrative connection.
        /// </summary>
        /// <param name="server">Server name.</param>
        /// <param name="port">Port (if applicable).</param>
        /// <param name="user">Username for admin connection.</param>
        /// <param name="password">User password.</param>
        /// <remarks>
        /// The user must have the relevant permissions to perform administrative operations such as creating users 
        /// and databases.
        /// </remarks>
        public MySqlServerAdmin(string server, string? port, string user, string password)
        {
            AdminConnection = MySqlDbConnection.ServerConnection(user, password, server, port);
        }

        /// <inheritdoc/>
        public bool CreateUser(string username, string password, UserPrivileges privileges, string? database)
        {
            try
            {
                using (var connection = new MySqlConnection(AdminConnection.ConnectionString))
                {
                    connection.Open();

                    var scope = database == null ? "*.*" : $"`{database}`.*";
                    var host = AdminConnection.Server.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        ? "localhost"
                        : "%";

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"""
                        CREATE USER IF NOT EXISTS '{username}'@'{host}' IDENTIFIED BY '{password}';
                        GRANT {ApiDbHelper.GetMySqlPrivilegesString(privileges)} ON {scope} TO '{username}'@'{host}';
                        FLUSH PRIVILEGES;
                        """;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool DeleteUser(string username)
        {
            try
            {
                using var connection = new MySqlConnection(AdminConnection.ConnectionString);
                connection.Open();

                var host = AdminConnection.Server.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    ? "localhost"
                    : "%";

                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"""
                    DROP USER IF EXISTS '{username}'@'{host}';
                    FLUSH PRIVILEGES;
                    """;

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetDatabaseUsers(string database)
        {
            var users = new List<string>();

            try
            {
                using var connection = new MySqlConnection(AdminConnection.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = """
                SELECT DISTINCT
                    REPLACE(REPLACE(GRANTEE, '''', ''), '@%', '') AS UserName,
                    GRANTEE
                FROM INFORMATION_SCHEMA.SCHEMA_PRIVILEGES
                WHERE TABLE_SCHEMA = @db;                
                """;

                cmd.Parameters.AddWithValue("@db", database);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(reader.GetString(0));
                }
            }
            catch (Exception e)
            {
                // Log
            }

            return users;
        }

        /// <inheritdoc/>
        public bool DeleteDatabase(string databaseName)
        {
            using var conn = new MySqlConnection(AdminConnection.ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP DATABASE IF EXISTS `{databaseName}`;";
            var nqResult = cmd.ExecuteNonQuery();

            return nqResult > 0;
        }
    }
}
