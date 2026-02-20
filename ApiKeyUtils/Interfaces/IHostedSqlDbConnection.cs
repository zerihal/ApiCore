using ApiKeyUtils.ApiKeyDb;

namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Hosted SQL database connection interface that defines properties for trusted connections, server 
    /// certificate trust, authentication type, and user credentials.
    /// </summary>
    public interface IHostedSqlDbConnection : IHostedDbConnection
    {
        /// <summary>
        /// Flag indicating whether to use a trusted connection (Windows Authentication) when connecting 
        /// to the database server.
        /// </summary>
        public bool TrustedConnection { get; set; }

        /// <summary>
        /// Flag indicating whether to trust the server's SSL certificate when establishing a secure connection.
        /// </summary>
        public bool TrustServerCertificate { get; set; }

        /// <summary>
        /// SQL authentication type to use when connecting to the database server (<see cref="SqlDbAuthentication"/>).
        /// </summary>
        public SqlDbAuthentication AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets a tuple indicating whether to use integrated security for authentication and, if so, whether 
        /// to set as true or false.
        /// </summary>
        public Tuple<bool, bool>? UseIntegratedSecurity { get; set; }

        /// <summary>
        /// Gets or sets a tuple containing the username and password to use for SQL authentication when connecting to 
        /// the database server.
        /// </summary>
        public Tuple<string, string>? UserCredentials { get; set; }
    }
}
