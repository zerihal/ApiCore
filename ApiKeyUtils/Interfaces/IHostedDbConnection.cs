namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Hosted database connection that provides necessary information for connecting to a database server, 
    /// such as server address, database name, and connection string.
    /// </summary>
    public interface IHostedDbConnection
    {
        /// <summary>
        /// Gets or sets the name or address of the server to connect to.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Database name to connect to.
        /// </summary>
        public string? Database { get; set; }

        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; }
    }
}
