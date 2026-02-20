using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public abstract class HostedApiKeyDbBase : IHostedApiKeyDb
    {
        /// <inheritdoc/>
        public string DatabaseName
        {
            get => DbConnection.Database ?? string.Empty;
            set => DbConnection.Database = value;
        }

        /// <inheritdoc/>
        public IHostedDbConnection DbConnection { get; set; }

        /// <inheritdoc/>
        public string DbConnectionString => DbConnection.ConnectionString;

        /// <summary>
        /// Constructor that initializes the hosted API key database with a provided <see cref="IHostedDbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">Database connection.</param>
        /// <param name="type">Database server type.</param>
        /// <exception cref="Exception">Thrown if database is unsupported.</exception>
        public HostedApiKeyDbBase(IHostedDbConnection? dbConnection, DbServerType type)
        {
            if (dbConnection != null)
            {
                DbConnection = dbConnection;
            }
            else
            {
                switch (type)
                {
                    case DbServerType.MySql:
                        DbConnection = new MySqlDbConnection();
                        break;

                    case DbServerType.SqlServer:
                        DbConnection = new SqlDbConnection();
                        break;

                    default:
                        throw new Exception("Database type is not supported hosted API key database");
                }                
            }
        }

        /// <inheritdoc/>
        public abstract bool CreateDatabaseAndApiKeysTable();

        /// <inheritdoc/>
        public abstract IApiKey? GetApiKeyFromHash(string hashedApiKey);

        /// <inheritdoc/>
        public virtual IApiKey? GetApiKey(string apiKey)
        {
            var hashedKey = ApiDbHelper.GetKeyHash(apiKey);
            return GetApiKeyFromHash(hashedKey);
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IApiKey> GetApiKeys(string owner);

        /// <inheritdoc/>
        public abstract bool StoreApiKey(string apiKey, string owner, int keyType);
    }
}
