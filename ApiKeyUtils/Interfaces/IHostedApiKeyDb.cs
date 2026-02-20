namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Hosted API key database interface that extends <see cref="IApiKeyDb"/> to include a property 
    /// for the hosted database connection.
    /// </summary>
    public interface IHostedApiKeyDb : IApiKeyDb
    {
        /// <summary>
        /// Gets or sets the database connection used by the host for executing database operations.
        /// </summary>
        public IHostedDbConnection DbConnection { get; set; }
    }
}
