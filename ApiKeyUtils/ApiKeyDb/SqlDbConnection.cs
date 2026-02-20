using ApiKeyUtils.Interfaces;
using System.Text;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class SqlDbConnection : HostedDbConnectionBase, IHostedSqlDbConnection
    {
        /// <inheritdoc/>
        public bool TrustedConnection { get; set; } = true;

        /// <inheritdoc/>
        public bool TrustServerCertificate { get; set; } = true;

        /// <inheritdoc/>
        public SqlDbAuthentication AuthenticationType { get; set; }

        /// <inheritdoc/>
        public Tuple<bool, bool>? UseIntegratedSecurity { get; set; }

        /// <inheritdoc/>
        public Tuple<string, string>? UserCredentials { get; set; }

        /// <inheritdoc/>
        public override string ConnectionString
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append($"Server={Server}");

                // If no database is set, connect to the server instance itself (master).
                if (string.IsNullOrEmpty(Database))
                    sb.Append(";Database=master");
                else
                    sb.Append($";Database={Database}");

                if (AuthenticationType == SqlDbAuthentication.WindowsAuthentication && (UseIntegratedSecurity?.Item1 ?? false))
                    sb.Append($";Integrated Security={UseIntegratedSecurity.Item2}");

                if (AuthenticationType == SqlDbAuthentication.SQLAuthentication)
                {
                    if (UserCredentials == null || string.IsNullOrEmpty(UserCredentials.Item1) || string.IsNullOrEmpty(UserCredentials.Item2))
                        throw new Exception("Invalid user credentials with SQL authentication used");

                    sb.Append($";User Id={UserCredentials.Item1};Password={UserCredentials.Item2}");
                }

                if (AuthenticationType == SqlDbAuthentication.WindowsAuthentication)
                    sb.Append($";Trusted_Connection={TrustedConnection.ToString()}");

                sb.Append($";TrustServerCertificate={TrustServerCertificate.ToString()}");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Default constructor that initializes the database name to "ApiCore". The server and 
        /// authentication type must be set separately after instantiation.
        /// </summary>
        public SqlDbConnection() : base()
        {
            Database = "ApiCore";
        }

        /// <summary>
        /// Constructor that accepts the server name and authentication type. The database name is 
        /// initialized to "ApiCore" by default, but can be changed after instantiation if needed.
        /// </summary>
        /// <param name="server">Server name.</param>
        /// <param name="authentication">Authentication type.</param>
        public SqlDbConnection(string server, SqlDbAuthentication authentication) : this()
        {
            Server = server;
            AuthenticationType = authentication;
        }

        /// <summary>
        /// Constructor that accepts the server name, database name, and authentication type.
        /// </summary>
        /// <param name="server">Server name.</param>
        /// <param name="database">Database name.</param>
        /// <param name="authentication">Authentication type.</param>
        public SqlDbConnection(string server, string database, SqlDbAuthentication authentication) : 
            this(server, authentication)
        {
            Database = database;
        }
    }
}
