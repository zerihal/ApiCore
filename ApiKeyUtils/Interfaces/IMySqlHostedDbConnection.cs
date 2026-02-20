namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// MySQL-specific hosted database connection that extends <see cref="IHostedDbConnection"/> to include
    /// additional properties specific to the connection type.
    /// </summary>
    public interface IMySqlHostedDbConnection : IHostedDbConnection
    {
        /// <summary>
        /// Database server port to connect to (if applicable).
        /// </summary>
        public string? Port { get; set; }

        /// <summary>
        /// Username for database server authentication.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password for database server authentication.
        /// </summary>
        public string Password { get; set; }
    }
}
