using ApiKeyUtils.Interfaces;
using System.Text;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class MySqlDbConnection : HostedDbConnectionBase, IMySqlHostedDbConnection
    {
        /// <inheritdoc/>
        public string? Port { get; set; }

        /// <inheritdoc/>
        public string User { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Password { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override string ConnectionString
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append($"Server={Server};");

                if (!string.IsNullOrEmpty(Port))
                    sb.Append($"Port={Port};");

                if (!string.IsNullOrEmpty(Database))
                    sb.Append($"Database={Database};");

                sb.Append($"User={User};Password={Password};");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Default constructor that initializes the MySQL database connection with default values for server, database, and port.
        /// </summary>
        public MySqlDbConnection() : base()
        {
            Server = "localhost";
            Database = "ApiCore";
            Port = "3306";
        }

        /// <summary>
        /// Constructor that initializes the MySQL database connection with the provided username and password, while using default 
        /// values for server, database, and port.
        /// </summary>
        /// <param name="user">MySQL instance username.</param>
        /// <param name="password">MySQL instance password.</param>
        public MySqlDbConnection(string user, string password) : this()
        {
            User = user;
            Password = password;
        }

        /// <summary>
        /// Constructor that initializes the MySQL database connection with the provided server, database, port, username, and password.
        /// </summary>
        /// <param name="server">MySQL server (local or remote).</param>
        /// <param name="database">MySQL database to use.</param>
        /// <param name="port">Server port.</param>
        /// <param name="user">MySQL instance username.</param>
        /// <param name="password">MySQL instance password.</param>
        public MySqlDbConnection(string server, string database, string port, string user, string password) : 
            this(user, password)
        {
            Port = port;
            Server = server;
            Database = database;
        }

        /// <summary>
        /// Creates an instance of <see cref="IMySqlHostedDbConnection"/> with the provided username and password, and optional server 
        /// and port parameters.
        /// </summary>
        /// <param name="user">MySQL instance username.</param>
        /// <param name="password">MySQL instance password.</param>
        /// <param name="server">MySQL server (local or remote).</param>
        /// <param name="port">Server port.</param>
        /// <returns>Implementation of IMySqlHostedDbConnection for connecting to the server.</returns>
        /// <remarks>
        /// This method is used for creating a connection to the MySQL server without specifying a database, useful for server-level operations 
        /// such as creating databases or managing users. If the server parameter is not provided, it defaults to "localhost". If the port 
        /// parameter is not provided, it defaults to "3306".
        /// </remarks>
        public static MySqlDbConnection ServerConnection(string user, string password, string? server = null, string? port = null)
        {
            var conn = new MySqlDbConnection { User = user, Password = password, Port = port };

            if (server != null)
                conn.Server = server;

            return conn;
        }
    }
}
