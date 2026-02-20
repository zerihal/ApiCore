using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public abstract class HostedDbConnectionBase : IHostedDbConnection
    {
        /// <inheritdoc/>
        public string Server { get; set; }

        /// <inheritdoc/>
        public string? Database { get; set; }

        /// <inheritdoc/>
        public abstract string ConnectionString { get; }

        /// <summary>
        /// Default constructor that initializes the server to an empty string. This will be overridden by 
        /// derived classes to provide specific connection string implementations based on the server type. 
        /// </summary>
        public HostedDbConnectionBase()
        {
            Server = string.Empty;
        }
    }
}
